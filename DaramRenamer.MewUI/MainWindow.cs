using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.Security.Principal;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Platform;
using DaramRenamer.Commands;
using DaramRenamer.FileOperators;
using DaramRenamer.Registry;
using MewUIWindowState = Aprillz.MewUI.Controls.WindowState;

namespace DaramRenamer.MewUI;

internal sealed class MainWindow : Window
{
    private const double MenuBarHeight = 24;
    private const double ToolbarToListSpacing = 4;

    private static readonly CommandCategory[] CommandCategories =
    [
        CommandCategory.Filename,
        CommandCategory.Extension,
        CommandCategory.Path,
        CommandCategory.Number,
        CommandCategory.Date,
        CommandCategory.Tag,
        CommandCategory.Etc
    ];

    private readonly ManualEditCommand _manualEditCommand = new();
    private readonly UndoManager _undoManager = new();
    private readonly GridView _files = new();
    private readonly Dictionary<string, Command> _commands = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ICondition> _conditions = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ICondition> _activeConditions = new(StringComparer.Ordinal);
    private readonly List<(ItemDescriptor Descriptor, MenuItem MenuItem)> _conditionMenuItems = [];

    private FileItem? _selectedFile;
    private BatchWindow? _batchWindow;
    private PreferencesWindow? _preferencesWindow;
    private bool _refreshingFiles;
    private bool _isAdministrator;
    private bool? _updateAvailable = false;

    public MainWindow()
    {
        var preferences = MewUIPreferences.Instance;
        FileItem.FileOperator = OperatingSystem.IsWindows()
            ? new WindowsNativeFileOperator()
            : new DefaultFileOperator();
        foreach (var descriptor in DaramRenamerRegistry.Conditions.OrderBy(item => item.Order))
            if (descriptor.Create() is ICondition condition)
                _conditions[descriptor.Id] = condition;

        this.Title(BuildTitle());
        if (OperatingSystem.IsWindows())
            this.Icon(IconSource.FromResource(
                typeof(MainWindow).Assembly,
                "DaramRenamer.MewUI.Resources.ProgramIcon.ico"));
        this.Padding(0);
        this.Resizable(
            preferences.SaveWindowState && preferences.Width >= 240 ? preferences.Width : 720,
            preferences.SaveWindowState && preferences.Height >= 180 ? preferences.Height : 480);
        if (preferences.SaveWindowState)
        {
            this.StartManualPosition(preferences.Left, preferences.Top);
            this.WindowState(preferences.WindowState switch
            {
                1 => MewUIWindowState.Minimized,
                2 => MewUIWindowState.Maximized,
                _ => MewUIWindowState.Normal
            });
        }
        this.OnSizeChanged(size =>
        {
            if (preferences.SaveWindowState && this.WindowState == MewUIWindowState.Normal)
            {
                preferences.Width = size.Width;
                preferences.Height = size.Height;
            }
        });

        _files
            .RowHeight(22)
            .HeaderHeight(24)
            .CellPadding(new Thickness(3, 2))
            .FontFamily("Segoe UI")
            .FontSize(12)
            .OnSelectionChanged(OnFileSelectionChanged)
            .OnMouseDoubleClick(OnFileMouseDoubleClick)
            .OnKeyUp(OnFileKeyUp)
            .AllowDrop(true)
            .OnDrop(OnFilesDropped);
        // WPF ListView defaults to Extended selection. The main window relies on
        // selecting several rows for restore and remove operations.
        _files.SelectionMode = ItemsSelectionMode.Extended;
        ConfigureFileColumnsAndContextMenu();

        this.Content(BuildUi());
        this.OnLoaded(OnMainWindowLoaded)
            .OnPreviewKeyDown(OnShortcutKeyDown)
            .OnClosed(OnMainWindowClosed);
        Strings.Instance.Loaded += OnStringsLoaded;
        RefreshFiles();
    }

    private void OnStringsLoaded(object? sender, EventArgs args)
    {
        ConfigureFileColumnsAndContextMenu();
        this.Title(BuildTitle(_updateAvailable, _isAdministrator));
        this.Content(BuildUi());
        foreach (var (descriptor, _) in _conditionMenuItems)
            UpdateConditionLabel(descriptor);
        RefreshFiles(_selectedFile);
    }

    private async void OnMainWindowLoaded()
    {
        _isAdministrator = await IsAdministrator();
        _updateAvailable = false;
        if (!MewUIPreferences.Instance.DisableCheckUpdate)
            _updateAvailable = await CheckUpdateAsync();

        this.Title(BuildTitle(_updateAvailable, _isAdministrator));
    }

    private static string BuildTitle(bool? update = false, bool isAdministrator = false)
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "UNKNOWN VERSION";
        var title = $"{Strings.Instance["DaramRenamer"]} - {Strings.Instance["Version"]} {version}";
        if (!Environment.Is64BitProcess)
            title += " - [32-Bit]";
        if (isAdministrator)
            title += $" - [{Strings.Instance["Administrator"]}]";

        return title + (update switch
        {
            true => $" - [{Strings.Instance["NewLatestVersionAvailable"]}]",
            null => $" - [{Strings.Instance["UpdateCheckError"]}]",
            _ => string.Empty
        });
    }

    private static Task<bool> IsAdministrator() => Task.Run(() => OperatingSystem.IsWindows()
        ? new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
        : string.Equals(Environment.UserName, "root", StringComparison.Ordinal));

    private void OnShortcutKeyDown(KeyEventArgs args)
    {
        if (IsBuiltInShortcut(args.Key, args.Modifiers))
            return;

        foreach (var shortcut in MewUIPreferences.Instance.Shortcuts)
        {
            if (!TryParseShortcut(shortcut.KeyBinding, out var key, out var modifiers) ||
                args.Key != key || args.Modifiers != modifiers)
                continue;

            var descriptor = DaramRenamerRegistry.FindCommandDescriptor(shortcut.Command);
            if (descriptor is null)
                continue;

            RunCommand(descriptor.Create() as ICommand);
            args.Handled = true;
            return;
        }
    }

    private static bool IsBuiltInShortcut(Key key, ModifierKeys modifiers) =>
        modifiers == ModifierKeys.Control && key is Key.O or Key.Delete or Key.S or Key.Z or Key.Y or Key.Up or Key.Down ||
        modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.S;

    private static bool TryParseShortcut(string binding, out Key key, out ModifierKeys modifiers)
    {
        key = Key.None;
        modifiers = ModifierKeys.None;
        if (string.IsNullOrWhiteSpace(binding))
            return false;

        var parts = binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0 || !Enum.TryParse(parts[^1], true, out key) || key == Key.None)
            return false;

        foreach (var part in parts.Take(parts.Length - 1))
        {
            if (part.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Control", StringComparison.OrdinalIgnoreCase))
                modifiers |= ModifierKeys.Control;
            else if (part.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                modifiers |= ModifierKeys.Alt;
            else if (part.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                modifiers |= ModifierKeys.Shift;
        }

        return true;
    }

    private Grid BuildUi()
    {
        return new Grid()
            .Rows($"{MenuBarHeight},Auto,*")
            .Children(
                BuildMenuBar().Row(0),
                BuildToolBar().Row(1),
                _files.Margin(0, ToolbarToListSpacing, 0, 0).Row(2));
    }

    private void ConfigureFileColumnsAndContextMenu()
    {
        _files.Columns<FileItem>(
            FileColumn(Strings.Instance["OriginalName"], 225, item => item.OriginalFilename,
                showIcon: OperatingSystem.IsWindows()),
            FileColumn(Strings.Instance["ChangedName"], 230, item => item.ChangedFilename),
            FileColumn(Strings.Instance["OriginalPath"], 105, item => item.OriginalPath),
            FileColumn(Strings.Instance["ChangedPath"], 110, item => item.ChangedPath));
        _files.ContextMenu(BuildFileContextMenu());
    }

    private MenuBar BuildMenuBar()
    {
        var fileMenu = new Menu()
            .Item(RegisterAction("file.open", WithGesture("Menu_File_Open", "Ctrl+O"), OpenFiles,
                new KeyGesture(Key.O, ModifierKeys.Control)))
            .Item(RegisterAction("file.folder", WithGesture("Menu_File_FolderOpen", "Ctrl+P"), OpenFolder))
            .Separator()
            .Item(RegisterAction("file.clear", WithGesture("Menu_File_Clear", "Ctrl+Del"), ClearFiles,
                new KeyGesture(Key.Delete, ModifierKeys.Control)))
            .Item(RegisterAction("file.apply", WithGesture("Menu_File_Apply", "Ctrl+S"), ApplyFiles,
                new KeyGesture(Key.S, ModifierKeys.Control)))
            .Separator()
            .Item(RegisterAction("file.exit", WithGesture("Menu_File_Exit", "Alt+F4"), Application.Shutdown));

        var editMenu = new Menu()
            .Item(RegisterAction("edit.undo", WithGesture("Menu_Edit_Undo", "Ctrl+Z"), Undo,
                new KeyGesture(Key.Z, ModifierKeys.Control)))
            .Item(RegisterAction("edit.redo", WithGesture("Menu_Edit_Redo", "Ctrl+Y"), Redo,
                new KeyGesture(Key.Y, ModifierKeys.Control)))
            .Separator()
            .Item(RegisterAction("edit.up", WithGesture("Menu_Edit_ItemToUp", "Ctrl+Up"), MoveSelectedUp,
                new KeyGesture(Key.Up, ModifierKeys.Control)))
            .Item(RegisterAction("edit.down", WithGesture("Menu_Edit_ItemToDown", "Ctrl+Down"), MoveSelectedDown,
                new KeyGesture(Key.Down, ModifierKeys.Control)))
            .Item(RegisterAction("edit.sort", WithGesture("Menu_Edit_Sort", "Ctrl+Shift+S"), SortFiles,
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)))
            .Separator()
            .Item(RegisterAction("edit.restore", Strings.Instance["Menu_Edit_RestoreSelected"], RestoreSelected));

        var commandsMenu = BuildCommandCategories()
            .Item(RegisterAction("batch.open", Strings.Instance["Menu_Command_Batch"], OpenBatchWindow));

        var conditionsMenu = BuildConditionsMenu();
        var toolsMenu = new Menu()
            .Item(RegisterAction("tools.preferences", Strings.Instance["Menu_Tools_Preferences"], ShowPreferences));
        if (OperatingSystem.IsWindows())
            toolsMenu.Separator()
                .Item(RegisterAction("tools.update", Strings.Instance["Menu_Tools_CheckUpdate"], CheckForUpdates));

        var helpMenu = new Menu()
            .Item(RegisterAction("help.license", Strings.Instance["Menu_Help_License"], ShowLicenses))
            .Item(RegisterAction("help.about", Strings.Instance["Menu_Help_About"], ShowAbout));

        return new MenuBar()
            .Height(MenuBarHeight)
            .FontFamily("Segoe UI")
            .FontSize(12)
            .Items(
                new MenuItem(Strings.Instance["Menu_File_File"]).Menu(fileMenu),
                new MenuItem(Strings.Instance["Menu_Edit_Edit"]).Menu(editMenu),
                new MenuItem(Strings.Instance["Menu_Command_Command"]).Menu(commandsMenu),
                new MenuItem(Strings.Instance["Menu_Condition_Condition"]).Menu(conditionsMenu),
                new MenuItem(Strings.Instance["Menu_Tools_Tools"]).Menu(toolsMenu),
                new MenuItem(Strings.Instance["Menu_Help_Help"]).Menu(helpMenu));
    }

    private Menu BuildConditionsMenu()
    {
        var menu = new Menu();
        _conditionMenuItems.Clear();

        foreach (var descriptor in DaramRenamerRegistry.Conditions.OrderBy(item => item.Order))
        {
            var id = descriptor.Id;
            if (!_conditions.TryGetValue(descriptor.Id, out var condition))
                continue;

            var command = RegisterAction(
                $"condition.{id}",
                Strings.Instance[descriptor.LocalizationKey],
                () => ToggleCondition(descriptor, condition));

            var menuItem = new MenuItem($"[ ] {Strings.Instance[descriptor.LocalizationKey]}")
                .Command(command);
            menu.Add(menuItem);
            _conditionMenuItems.Add((descriptor, menuItem));
        }

        return menu;
    }

    private Menu BuildCommandCategories()
    {
        var menu = new Menu();
        foreach (var category in CommandCategories)
        {
            var categoryMenu = new Menu();
            foreach (var descriptor in DaramRenamerRegistry.GetCommands(category))
            {
                var itemDescriptor = descriptor;
                categoryMenu.Item(RegisterAction(
                    $"command.{itemDescriptor.Id}",
                    Strings.Instance[itemDescriptor.LocalizationKey],
                    () => RunCommand(itemDescriptor.Create() as ICommand)));
            }

            menu.SubMenu(Strings.Instance[CategoryLocalizationKey(category)], categoryMenu);
        }

        return menu;
    }

    private ContextMenu BuildFileContextMenu()
    {
        var menu = new ContextMenu();
        foreach (var category in CommandCategories)
        {
            menu.Items.Add(new MenuItem(Strings.Instance[CategoryLocalizationKey(category)])
                .Menu(BuildCategoryMenu(category, "context")));
        }
        return menu;
    }

    private Menu BuildCategoryMenu(CommandCategory category, string keyPrefix)
    {
        var menu = new Menu();
        foreach (var descriptor in DaramRenamerRegistry.GetCommands(category))
        {
            var itemDescriptor = descriptor;
            menu.Item(RegisterAction(
                $"{keyPrefix}.command.{itemDescriptor.Id}",
                Strings.Instance[itemDescriptor.LocalizationKey],
                () => RunCommand(itemDescriptor.Create() as ICommand)));
        }

        return menu;
    }

    private ToolBar BuildToolBar()
    {
        var filenameCommands = new List<Element>();
        AddToolbarCommand(filenameCommands, "replace_text.png", "Command_Name_ReplacePlain");
        AddToolbarCommand(filenameCommands, "concat_text.png", "Command_Name_Concatenate");
        AddToolbarCommand(filenameCommands, "trim_text.png", "Command_Name_Trim");
        AddToolbarCommand(filenameCommands, "delete_block.png", "Command_Name_DeleteBlock");
        AddToolbarCommand(filenameCommands, "delete_text.png", "Command_Name_DeleteFilename");
        AddToolbarCommand(filenameCommands, "substring.png", "Command_Name_Substring");
        AddToolbarCommand(filenameCommands, "casecast_text.png", "Command_Name_Casecast");

        var extensionCommands = new List<Element>();
        AddToolbarCommand(extensionCommands, "add_ext.png", "Command_Name_AddExtension");
        AddToolbarCommand(extensionCommands, "delete_ext.png", "Command_Name_DeleteExtension");
        AddToolbarCommand(extensionCommands, "replace_ext.png", "Command_Name_ReplaceExtension");
        AddToolbarCommand(extensionCommands, "casecast_ext.png", "Command_Name_CasecastExtension");

        var numberCommands = new List<Element>();
        AddToolbarCommand(numberCommands, "del_without_num.png", "Command_Name_DeleteNoNumber");
        AddToolbarCommand(numberCommands, "match_num_count.png", "Command_Name_SameNumberCount");
        AddToolbarCommand(numberCommands, "add_index.png", "Command_Name_AddIndex");
        AddToolbarCommand(numberCommands, "inc_dec_num.png", "Command_Name_Increase");

        var dateCommands = new List<Element>();
        AddToolbarCommand(dateCommands, "add_date.png", "Command_Name_AddDate");

        var fileGroup = new ToolBarGroup().Items(
            ToolbarButton("open.png", "ToolTip_Open", OpenFiles),
            ToolbarButton("clear.png", "ToolTip_Clear", ClearFiles),
            ToolbarButton("apply.png", "ToolTip_Apply", ApplyFiles));
        var historyGroup = new ToolBarGroup().Items(
            ToolbarButton("undo.png", "ToolTip_Undo", Undo),
            ToolbarButton("redo.png", "ToolTip_Redo", Redo));
        var orderGroup = new ToolBarGroup().Items(
            ToolbarButton("item_up.png", "ToolTip_ItemUp", MoveSelectedUp),
            ToolbarButton("item_down.png", "ToolTip_ItemDown", MoveSelectedDown),
            ToolbarButton("item_sort.png", "ToolTip_ItemSort", SortFiles));
        var filenameGroup = new ToolBarGroup().Items(filenameCommands.ToArray());
        var extensionGroup = new ToolBarGroup().Items(extensionCommands.ToArray());
        var numberGroup = new ToolBarGroup().Items(numberCommands.ToArray());
        var dateGroup = new ToolBarGroup().Items(dateCommands.ToArray());

        return new ToolBar()
            .CanReorderGroups(false)
            .StretchHorizontal()
            .Band(fileGroup, historyGroup, orderGroup, filenameGroup, extensionGroup, numberGroup, dateGroup);
    }

    private void AddToolbarCommand(List<Element> children, string icon, string commandKey)
    {
        var descriptor = DaramRenamerRegistry.FindCommandDescriptor(commandKey);
        if (descriptor is null)
            return;

        children.Add(ToolbarButton(icon, commandKey, () => RunCommand(descriptor.Create() as ICommand)));
    }

    private static Button ToolbarButton(string icon, string tooltipKey, Action action) => new Button()
        .Content(new Image()
            .SourceResource(
                typeof(MainWindow).Assembly,
                $"DaramRenamer.MewUI.Resources.ToolBarIcons.{icon}")
            .Size(16, 16))
        .ToolTip(Strings.Instance[tooltipKey])
        .OnClick(action);

    private static GridViewColumn<FileItem> FileColumn(
        string header,
        double width,
        Func<FileItem, string> value,
        bool showIcon = false)
    {
        return new GridViewColumn<FileItem>
        {
            CellTemplate = new DelegateTemplate<FileItem>(
                _ => showIcon
                    ? new StackPanel()
                        .Horizontal()
                        .Spacing(4)
                        .Children(
                            new Image().Size(16, 16),
                            MewUITheme.CellText())
                    : MewUITheme.CellText(),
                (element, item, _, _) =>
                {
                    var text = value(item);
                    if (showIcon)
                    {
                        var children = ((StackPanel)element).Children;
                        var iconPath = OperatingSystem.IsWindows()
                            ? WindowsShellIconCache.GetIconFile(item.OriginalFullPath, item.IsDirectory)
                            : string.Empty;
                        if (!string.IsNullOrEmpty(iconPath))
                            ((Image)children[0]).SourceFile(iconPath);
                        ((Label)children[1]).Text(text).ToolTip(text);
                    }
                    else
                        ((Label)element).Text(text).ToolTip(text);
                })
        }
            .Header(header)
            .Width(width);
    }

    private static string CategoryLocalizationKey(CommandCategory category) => category switch
    {
        CommandCategory.Filename => "Menu_Command_Filename",
        CommandCategory.Extension => "Menu_Command_Extension",
        CommandCategory.Path => "Menu_Command_Path",
        CommandCategory.Number => "Menu_Command_Number",
        CommandCategory.Date => "Menu_Command_Date",
        CommandCategory.Tag => "Menu_Command_Tag",
        _ => "Menu_Command_Etc"
    };

    private static string WithGesture(string localizationKey, string gesture) =>
        $"{Strings.Instance[localizationKey]}    {gesture}";

    private Command RegisterAction(string id, string text, Action action, KeyGesture? gesture = null)
    {
        if (_commands.TryGetValue(id, out var existing))
        {
            existing.Presentation.AccessText = text;
            if (gesture is KeyGesture existingGesture)
                this.InputMap.Map(existing, existingGesture);
            return existing;
        }

        var command = new Command($"DaramRenamer.{id}", text);
        this.Commands.Register(command, action);
        if (gesture is KeyGesture keyGesture)
            this.InputMap.Map(command, keyGesture);
        _commands.Add(id, command);
        return command;
    }

    private void OpenFiles()
    {
        var paths = ModalDialog.RunWithOwner(this, () => MultiFileOpenDialog.Show(
            this,
            Strings.Instance["FileDialogTitleOpenFiles"],
            Strings.Instance["FileDialogFilterAllFiles"]));
        if (paths.Count == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var path in paths.OrderBy(path => path, StringComparer.CurrentCulture))
            AddPath(path, directoryMode: false);
        RefreshFiles(_selectedFile);
    }

    private void OpenFolder()
    {
        var paths = ModalDialog.RunWithOwner(this,
            () => MultiFolderOpenDialog.Show(this, Strings.Instance["FileDialogTitleOpenFiles"]));
        if (paths.Count == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var path in paths.OrderBy(path => path, StringComparer.CurrentCulture))
            AddPath(path, directoryMode: true);
        RefreshFiles(_selectedFile);
    }

    private static void AddPath(string path, bool directoryMode)
    {
        if (File.Exists(path))
        {
            AddFileItem(path, checkIsDirectory: false);
            return;
        }

        if (!Directory.Exists(path))
            return;

        if (directoryMode)
        {
            AddFileItem(path, checkIsDirectory: true);
            return;
        }

        foreach (var file in FileItem.FileOperator.GetFiles(path, topDirectoryOnly: false))
            AddFileItem(file, checkIsDirectory: false);
    }

    private static void AddFileItem(string path, bool checkIsDirectory)
    {
        var item = new FileItem(path, checkIsDirectory);
        if (!FileItem.Files.Contains(item))
            FileItem.Files.Add(item);
    }

    private void ClearFiles()
    {
        _undoManager.ClearAll();
        FileItem.Files.Clear();
        _selectedFile = null;
        RefreshFiles();
    }

    private void ApplyFiles()
    {
        var window = new ApplyWindow(_undoManager, () => RefreshFiles());
        ModalDialog.Show(window, this);
    }

    private void Undo()
    {
        if (_undoManager.IsUndoStackEmpty)
            return;

        _undoManager.SaveToRedoStack(FileItem.Files);
        FileItem.Files = _undoManager.LoadFromUndoStack();
        RefreshFiles(preserveSelection: false);
    }

    private void Redo()
    {
        if (_undoManager.IsRedoStackEmpty)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files, clearRedoStack: false);
        FileItem.Files = _undoManager.LoadFromRedoStack();
        RefreshFiles(preserveSelection: false);
    }

    private void MoveSelectedUp() => MoveSelected(-1);

    private void MoveSelectedDown() => MoveSelected(1);

    private void MoveSelected(int offset)
    {
        var selectedFiles = GetSelectedFiles();
        if (selectedFiles.Length == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var item in selectedFiles)
        {
            var index = FileItem.Files.IndexOf(item);
            var targetIndex = index + offset;
            if (index >= 0 && targetIndex >= 0 && targetIndex < FileItem.Files.Count)
                FileItem.Files.Move(index, targetIndex);
        }
        RefreshFiles(_selectedFile);
    }

    private void SortFiles()
    {
        _undoManager.SaveToUndoStack(FileItem.Files);
        FileItem.Sort(FileItem.Files);
        RefreshFiles(preserveSelection: false);
    }

    private void RestoreSelected()
    {
        var selectedFiles = GetSelectedFiles();
        if (selectedFiles.Length == 0)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(
                $"{Strings.Instance["RestoreSelectedErrorMessage"]}{Environment.NewLine}{Strings.Instance["RestoreSelectedErrorMessageDetail"]}",
                owner: this));
            return;
        }

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var file in selectedFiles)
            file.Reset();
        RefreshFiles(_selectedFile);
    }

    private void RemoveSelected()
    {
        var selectedFiles = GetSelectedFiles();
        if (selectedFiles.Length == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var file in selectedFiles)
            FileItem.Files.Remove(file);
        _selectedFile = null;
        RefreshFiles();
    }

    private void OnFileKeyUp(KeyEventArgs args)
    {
        if (args.Key != Key.Delete)
            return;

        var selectedFiles = GetSelectedFiles();
        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var file in selectedFiles)
            FileItem.Files.Remove(file);

        if (selectedFiles.Length > 0)
        {
            _selectedFile = null;
            RefreshFiles(preserveSelection: false);
        }

        args.Handled = true;
    }

    private void OnFilesDropped(DragEventArgs args)
    {
        if (!args.Data.Contains(StandardDataFormats.StorageItems))
            return;

        var paths = GetDroppedPaths(args.Data.GetData(StandardDataFormats.StorageItems))
            .Where(path => File.Exists(path) || Directory.Exists(path))
            .OrderBy(path => path, StringComparer.CurrentCulture)
            .ToArray();
        if (paths.Length == 0)
            return;

        var hasDirectory = paths.Any(path => Directory.Exists(path) && path.Length > 3);
        var hasBatchFile = paths.Any(path => File.Exists(path) && path.EndsWith(".drb", StringComparison.Ordinal));

        void AddDroppedPaths(bool directoryMode, bool executeBatchFiles)
        {
            _undoManager.SaveToUndoStack(FileItem.Files);
            var batchRoots = new List<RootBatchNode>();

            foreach (var path in paths)
            {
                // Match WPF's drop behavior: once a .drb is present and execution is selected,
                // it attempts to deserialize each dropped file; files that fail are added normally.
                if (executeBatchFiles && hasBatchFile && File.Exists(path))
                {
                    try
                    {
                        using var reader = new StreamReader(path, Encoding.UTF8, false);
                        var root = new RootBatchNode();
                        root.Deserializer(reader);
                        batchRoots.Add(root);
                        continue;
                    }
                    catch
                    {
                        // If a batch file cannot be read, keep it in the file list as WPF does.
                    }
                }

                AddPath(path, directoryMode && Directory.Exists(path) && path.Length > 3);
            }

            foreach (var root in batchRoots)
                Parallel.ForEach(FileItem.Files, item => root.Execute(item));

            RefreshFiles(_selectedFile);
        }

        void ChooseBatchFiles(bool directoryMode)
        {
            if (!hasBatchFile)
            {
                AddDroppedPaths(directoryMode, executeBatchFiles: false);
                return;
            }

            ShowDropChoice(
                Strings.Instance["DragAndDrop_DrbQuestion"],
                Strings.Instance["DragAndDrop_DrbQuestionDescription"],
                [Strings.Instance["DragAndDrop_ButtonExecuteDRB"], Strings.Instance["DragAndDrop_ButtonJustAddDRB"]],
                choice =>
                {
                    if (choice < 0)
                        return;

                    AddDroppedPaths(directoryMode, executeBatchFiles: choice == 0);
                });
        }

        if (!hasDirectory)
        {
            ChooseBatchFiles(directoryMode: false);
            return;
        }

        ShowDropChoice(
            Strings.Instance["DragAndDrop_DirectoryQuestion"],
            Strings.Instance["DragAndDrop_DirectoryQuestionDescription"],
            [Strings.Instance["DragAndDrop_ButtonAddDirectory"], Strings.Instance["DragAndDrop_ButtonIterateFiles"]],
            choice =>
            {
                if (choice < 0)
                    return;

                ChooseBatchFiles(directoryMode: choice == 0);
            });
    }

    private int ShowDropChoice(string title, string description, IReadOnlyList<string> choices)
    {
        var window = new DropChoiceWindow(title, description, choices);
        ModalDialog.Show(window, this);
        return window.SelectedChoice;
    }

    private void ShowDropChoice(
        string title,
        string description,
        IReadOnlyList<string> choices,
        Action<int> completed)
    {
        completed(ShowDropChoice(title, description, choices));
    }

    private static IReadOnlyList<string> GetDroppedPaths(object? data) => data switch
    {
        IEnumerable<string> paths => paths.ToArray(),
        IEnumerable<Uri> uris => uris.Where(uri => uri.IsFile).Select(uri => uri.LocalPath).ToArray(),
        string path => [path],
        _ => []
    };

    private FileItem[] GetSelectedFiles() => _files.SelectedItems.OfType<FileItem>().ToArray();

    private void OnFileSelectionChanged(object? selected)
    {
        if (_refreshingFiles)
            return;

        _selectedFile = selected as FileItem;
    }

    private void RefreshFiles(FileItem? selected = null, bool preserveSelection = true)
    {
        var files = FileItem.Files.ToArray();
        var selectedItems = preserveSelection
            ? _files.SelectedItems.OfType<FileItem>()
                .Where(item => Array.IndexOf(files, item) >= 0)
                .ToList()
            : [];
        if (selected is not null && Array.IndexOf(files, selected) >= 0 && !selectedItems.Contains(selected))
            selectedItems.Add(selected);

        _refreshingFiles = true;
        _files.ItemsSource<FileItem>(files);
        _files.ClearSelection();
        foreach (var item in selectedItems)
        {
            var index = Array.IndexOf(files, item);
            if (index >= 0)
            {
                if (_files.ItemsSource is IMultiSelectableItemsView multiSelection)
                    multiSelection.SelectRange(index, index, clearExisting: false);
                else
                    _files.SelectRange(index, index);
            }
        }
        _selectedFile = selected is not null && Array.IndexOf(files, selected) >= 0
            ? selected
            : selectedItems.LastOrDefault();
        _refreshingFiles = false;
    }

    private void OnFileMouseDoubleClick(MouseEventArgs args)
    {
        if (!_files.TryGetItemIndexAt(args, out var index) ||
            (uint)index >= (uint)FileItem.Files.Count)
            return;

        OpenManualEditor(FileItem.Files[index]);
    }

    private void OpenManualEditor(FileItem? file = null)
    {
        file ??= _files.SelectedItem as FileItem;
        if (file is null)
            return;

        _manualEditCommand.ChangeName = file.ChangedFilename;
        _manualEditCommand.ChangePath = file.ChangedPath;
        var descriptor = DaramRenamerRegistry.GetDescriptor(_manualEditCommand);
        if (descriptor is null)
            return;

        var editor = new CommandEditorWindow(_manualEditCommand, descriptor, accepted =>
        {
            if (!accepted)
                return;

            ApplyCommand(_manualEditCommand, file);
            RefreshFiles(file);
        });
        ModalDialog.Show(editor, this);
    }

    private void RunCommand(ICommand? command, FileItem? designatedFile = null)
    {
        if (command is null)
            return;

        var descriptor = DaramRenamerRegistry.GetDescriptor(command);
        if (descriptor is not null)
            command = descriptor.Clone(command) as ICommand ?? command;

        var targetCommand = command;
        var hasOptions = descriptor?.Options.Count > 0;
        var snapshot = _undoManager.SaveTemporary(FileItem.Files);
        Action<bool> complete = accepted =>
        {
            FileItem.Files = _undoManager.LoadTemporary(snapshot);
            if (accepted)
            {
                _undoManager.SaveToUndoStack(FileItem.Files);
                ApplyCommand(targetCommand, designatedFile);
            }
            if (hasOptions)
                RefreshFiles(preserveSelection: false);
            else
                RefreshFiles(designatedFile ?? _selectedFile);
        };

        if (descriptor?.Options.Count > 0)
        {
            var editor = new CommandEditorWindow(
                targetCommand,
                descriptor,
                accepted => complete(accepted),
                () =>
                {
                    if (!MewUIPreferences.Instance.VisualCommand)
                        return;

                    FileItem.Files = _undoManager.LoadTemporary(snapshot);
                    ApplyCommand(targetCommand, designatedFile);
                    RefreshFiles(preserveSelection: false);
                });
            ModalDialog.Show(editor, this);
            return;
        }

        complete(true);
    }

    private void ApplyCommand(ICommand command, FileItem? designatedFile)
    {
        var targets = designatedFile is null ? FileItem.Files.ToArray() : [designatedFile];
        if (targets.Length == 0)
            return;

        if (command is ITargetContains targetContains)
            targetContains.SetTargets(targets);

        var conditions = _conditions
            .Where(pair => _activeConditions.ContainsKey(pair.Key))
            .Select(pair => pair.Value)
            .ToArray();
        if (command.ParallelProcessable && !MewUIPreferences.Instance.ForceSingleCoreRunning)
        {
            Parallel.ForEach(targets, (item, state, index) =>
            {
                if (conditions.All(condition => condition.IsSatisfyThisCondition(item)))
                    command.DoCommand(item);
            });
            return;
        }

        for (var index = 0; index < targets.Length; ++index)
            if (conditions.All(condition => condition.IsSatisfyThisCondition(targets[index])))
                command.DoCommand(targets[index]);
    }

    private void ToggleCondition(ItemDescriptor descriptor, ICondition condition)
    {
        if (_activeConditions.Remove(descriptor.Id))
        {
            UpdateConditionLabel(descriptor);
            return;
        }

        var created = _conditions.TryGetValue(descriptor.Id, out var cachedCondition)
            ? cachedCondition
            : condition;
        if (descriptor.Options.Count == 0)
        {
            _activeConditions.Add(descriptor.Id, created);
            UpdateConditionLabel(descriptor);
            return;
        }

        var editor = new CommandEditorWindow(created, descriptor, accepted =>
        {
            if (accepted)
                _activeConditions[descriptor.Id] = created;
            UpdateConditionLabel(descriptor);
        });
        ModalDialog.Show(editor, this);
    }

    private void UpdateConditionLabel(ItemDescriptor descriptor)
    {
        var entry = _conditionMenuItems.FirstOrDefault(item => item.Descriptor.Id == descriptor.Id);
        if (entry.MenuItem is null)
            return;

        var active = _activeConditions.ContainsKey(descriptor.Id);
        entry.MenuItem.Text($"{(active ? "[✓] " : "[ ] ")}{Strings.Instance[descriptor.LocalizationKey]}");
    }

    private void OpenBatchWindow()
    {
        if (_batchWindow is not null)
        {
            _batchWindow.Show();
            return;
        }

        _batchWindow = new BatchWindow(ExecuteBatch, _conditions);
        _batchWindow.OnClosed(() => _batchWindow = null);
        _batchWindow.Show();
    }

    private void ExecuteBatch(RootBatchNode root)
    {
        if (FileItem.Files.Count == 0)
            return;

        Parallel.ForEach(FileItem.Files, item => root.Execute(item));
        RefreshFiles(_selectedFile);
    }

    private void ShowPreferences()
    {
        if (_preferencesWindow is null)
        {
            _preferencesWindow = new PreferencesWindow();
            _preferencesWindow.OnClosed(() => _preferencesWindow = null);
        }

        _preferencesWindow.Show(this);
    }

    private async void CheckForUpdates()
    {
        var information = await UpdateInformationBank.GetUpdateInformationAsync(
            System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier);
        if (information is null)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(Strings.Instance["UpdateCheckErrorText"], owner: this));
            return;
        }

        var versionComparison = CompareStableVersionToCurrent(information.Value.StableLatestVersion);
        if (versionComparison is null)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(Strings.Instance["UpdateCheckErrorText"], owner: this));
            return;
        }

        if (versionComparison <= 0)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(Strings.Instance["ThisIsLatestVersionText"], owner: this));
            return;
        }

        if (string.IsNullOrWhiteSpace(information.Value.StableLatestUrl))
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(Strings.Instance["UpdateCheckErrorText"], owner: this));
            return;
        }

        if (SelfUpdater.IsManagedByWinget)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(
                Strings.Instance["UpdateViaWingetText"], owner: this));
            return;
        }

        var choice = ShowDropChoice(
            Strings.Instance["NewLatestVersionAvailableText"],
            information.Value.StableLatestVersion,
            [Strings.Instance["ButtonUpdate"]]);
        if (choice != 0)
            return;

        try
        {
            if (string.IsNullOrWhiteSpace(information.Value.StableLatestUrl))
                throw new InvalidOperationException("The update package URL is empty.");

            await SelfUpdater.DownloadAndStartAsync(
                information.Value,
                System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier,
                Environment.ProcessId);
            Close();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to install update: {ex}");
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(Strings.Instance["UpdateCheckErrorText"], owner: this));
        }
    }

    private static async Task<bool?> CheckUpdateAsync()
    {
        var information = await UpdateInformationBank.GetUpdateInformationAsync(
            System.Runtime.InteropServices.RuntimeInformation.RuntimeIdentifier);
        if (information is null)
            return null;

        var versionComparison = CompareStableVersionToCurrent(information.Value.StableLatestVersion);
        if (versionComparison is null)
            return null;
        if (versionComparison <= 0)
            return false;

        return string.IsNullOrWhiteSpace(information.Value.StableLatestUrl) ? null : true;
    }

    private static int? CompareStableVersionToCurrent(string stableLatestVersion)
    {
        var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
        if (currentVersion is null || currentVersion.Build < 0 ||
            !Version.TryParse(stableLatestVersion, out var latestVersion) ||
            latestVersion.Build < 0 || latestVersion.Revision >= 0)
            return null;

        var currentReleaseVersion = new Version(
            currentVersion.Major,
            currentVersion.Minor,
            currentVersion.Build);
        return latestVersion.CompareTo(currentReleaseVersion);
    }

    private void ShowLicenses() => ShowOwnedWindow(new LicenseWindow());

    private void ShowAbout() => ShowOwnedWindow(new AboutWindow());

    private void ShowOwnedWindow(Window window)
    {
        ModalDialog.Show(window, this);
    }

    private void SaveWindowState()
    {
        var preferences = MewUIPreferences.Instance;
        if (preferences.SaveWindowState)
        {
            var bounds = this.RestoreBounds;
            var position = this.Position;
            if (bounds.Width > 0)
                preferences.Width = bounds.Width;
            if (bounds.Height > 0)
                preferences.Height = bounds.Height;
            preferences.Left = this.WindowState == MewUIWindowState.Normal ? position.X : bounds.X;
            preferences.Top = this.WindowState == MewUIWindowState.Normal ? position.Y : bounds.Y;
            preferences.WindowState = this.WindowState switch
            {
                MewUIWindowState.Minimized => 1,
                MewUIWindowState.Maximized => 2,
                _ => 0
            };
        }

        preferences.Save();
    }

    private void OnMainWindowClosed()
    {
        Strings.Instance.Loaded -= OnStringsLoaded;
        SaveWindowState();
        _preferencesWindow?.CloseForShutdown();
        _batchWindow?.Close();
    }
}
