using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DaramRenamer.Commands;
using DaramRenamer.FileOperators;
using DaramRenamer.Registry;

namespace DaramRenamer.Avalonia;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly ManualEditCommand _manualEditCommand = new();
    private readonly UndoManager _undoManager = new();
    private readonly List<MenuItem> _conditionItems = [];
    private readonly List<NativeMenuItem> _nativeConditionItems = [];
    private BatchWindow? _batchWindow;
    private ObservableCollection<FileItem> _items = FileItem.Files;
    private ListBox _fileList = null!;
    private readonly NativeTheme _look = NativeTheme.Current;

    public MainWindow()
    {
        InitializeComponent();
        FileItem.FileOperator = new DefaultFileOperator();
        Title = $"{Strings.Instance["DaramRenamer"]} - {Strings.Instance["Version"]} {GetVersionString()}";
        Icon = LoadWindowIcon();
        Background = _look.AppBackground;
        Width = AvaloniaPreferences.Instance.SaveWindowState ? AvaloniaPreferences.Instance.Width : 960;
        Height = AvaloniaPreferences.Instance.SaveWindowState ? AvaloniaPreferences.Instance.Height : 640;
        MinWidth = 760;
        MinHeight = 480;
        if (AvaloniaPreferences.Instance.SaveWindowState)
            Position = new PixelPoint((int)AvaloniaPreferences.Instance.Left, (int)AvaloniaPreferences.Instance.Top);

        _undoManager.UpdateUndo += (_, _) => OnHistoryChanged();
        _undoManager.UpdateRedo += (_, _) => OnHistoryChanged();

        BuildUi();
        KeyDown += async (_, e) => await HandleShortcut(e);
        Closing += (_, _) => SaveWindowState();
    }

    public bool UndoManagerHasUndoStackItem => !_undoManager.IsUndoStackEmpty;
    public bool UndoManagerHasRedoStackItem => !_undoManager.IsRedoStackEmpty;

    public new event PropertyChangedEventHandler? PropertyChanged;

    private void BuildUi()
    {
        var root = new Grid
        {
            Background = _look.AppBackground,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        if (UseNativeMenuBar)
        {
            NativeMenu.SetMenu(this, BuildNativeMenu());
        }
        else
        {
            var menu = BuildMenu();
            Grid.SetRow(menu, 0);
            root.Children.Add(menu);
        }

        var toolbar = BuildToolBar();
        Grid.SetRow(toolbar, 1);
        root.Children.Add(toolbar);

        var listArea = BuildFileList();
        Grid.SetRow(listArea, 2);
        root.Children.Add(listArea);

        Content = root;
        DataContext = this;
        RefreshStatus();
    }

    private Menu BuildMenu()
    {
        var menu = new Menu();
        menu.Items.Add(Menu(Strings.Instance["Menu_File_File"],
            Item(Strings.Instance["Menu_File_Open"], async (_, _) => await OpenFiles(), "Ctrl+O"),
            Item(Strings.Instance["Menu_File_FolderOpen"], async (_, _) => await OpenFolders(), "Ctrl+P"),
            new Separator(),
            Item(Strings.Instance["Menu_File_Clear"], (_, _) => ClearFiles(), "Ctrl+Del"),
            Item(Strings.Instance["Menu_File_Apply"], async (_, _) => await ApplyFiles(), "Ctrl+S"),
            new Separator(),
            Item(Strings.Instance["Menu_File_Exit"], (_, _) => Close(), "Alt+F4")));

        menu.Items.Add(Menu(Strings.Instance["Menu_Edit_Edit"],
            Item(Strings.Instance["Menu_Edit_Undo"], (_, _) => Undo(), "Ctrl+Z"),
            Item(Strings.Instance["Menu_Edit_Redo"], (_, _) => Redo(), "Ctrl+Y"),
            new Separator(),
            Item(Strings.Instance["Menu_Edit_ItemToUp"], (_, _) => MoveSelected(-1), "Ctrl+Up"),
            Item(Strings.Instance["Menu_Edit_ItemToDown"], (_, _) => MoveSelected(1), "Ctrl+Down"),
            Item(Strings.Instance["Menu_Edit_Sort"], (_, _) => SortFiles(), "Ctrl+Shift+S"),
            new Separator(),
            Item(Strings.Instance["Menu_Edit_RestoreSelected"], (_, _) => RestoreSelected())));

        var commandMenu = BuildCommandsMenu(Strings.Instance["Menu_Command_Command"]);
        commandMenu.Items.Add(new Separator());
        commandMenu.Items.Add(Item(Strings.Instance["Menu_Command_Batch"], (_, _) => ShowBatchWindow()));
        menu.Items.Add(commandMenu);
        menu.Items.Add(BuildConditionsMenu());
        menu.Items.Add(Menu(Strings.Instance["Menu_Tools_Tools"],
            Item(Strings.Instance["Menu_Tools_Preferences"], async (_, _) => await ShowPreferences()),
            Item(Strings.Instance["Menu_Tools_CheckUpdate"], async (_, _) => await CheckUpdateAndNotify())));
        menu.Items.Add(Menu(Strings.Instance["Menu_Help_Help"],
            Item(Strings.Instance["Menu_Help_License"], async (_, _) => await ShowLicense()),
            Item(Strings.Instance["Menu_Help_About"], async (_, _) => await ShowAbout())));
        return menu;
    }

    private NativeMenu BuildNativeMenu()
    {
        var menu = new NativeMenu();
        menu.Add(NativeRoot(Strings.Instance["Menu_File_File"],
            NativeItem(Strings.Instance["Menu_File_Open"], async (_, _) => await OpenFiles(), "Ctrl+O"),
            NativeItem(Strings.Instance["Menu_File_FolderOpen"], async (_, _) => await OpenFolders(), "Ctrl+P"),
            new NativeMenuItemSeparator(),
            NativeItem(Strings.Instance["Menu_File_Clear"], (_, _) => ClearFiles(), "Ctrl+Del"),
            NativeItem(Strings.Instance["Menu_File_Apply"], async (_, _) => await ApplyFiles(), "Ctrl+S"),
            new NativeMenuItemSeparator(),
            NativeItem(Strings.Instance["Menu_File_Exit"], (_, _) => Close(), "Alt+F4")));

        menu.Add(NativeRoot(Strings.Instance["Menu_Edit_Edit"],
            NativeItem(Strings.Instance["Menu_Edit_Undo"], (_, _) => Undo(), "Ctrl+Z"),
            NativeItem(Strings.Instance["Menu_Edit_Redo"], (_, _) => Redo(), "Ctrl+Y"),
            new NativeMenuItemSeparator(),
            NativeItem(Strings.Instance["Menu_Edit_ItemToUp"], (_, _) => MoveSelected(-1), "Ctrl+Up"),
            NativeItem(Strings.Instance["Menu_Edit_ItemToDown"], (_, _) => MoveSelected(1), "Ctrl+Down"),
            NativeItem(Strings.Instance["Menu_Edit_Sort"], (_, _) => SortFiles(), "Ctrl+Shift+S"),
            new NativeMenuItemSeparator(),
            NativeItem(Strings.Instance["Menu_Edit_RestoreSelected"], (_, _) => RestoreSelected())));

        var commandMenu = BuildNativeCommandCategoryMenu(Strings.Instance["Menu_Command_Command"]);
        commandMenu.Menu!.Add(new NativeMenuItemSeparator());
        commandMenu.Menu.Add(NativeItem(Strings.Instance["Menu_Command_Batch"], (_, _) => ShowBatchWindow()));
        menu.Add(commandMenu);
        menu.Add(BuildNativeConditionsMenu());
        menu.Add(NativeRoot(Strings.Instance["Menu_Tools_Tools"],
            NativeItem(Strings.Instance["Menu_Tools_Preferences"], async (_, _) => await ShowPreferences()),
            NativeItem(Strings.Instance["Menu_Tools_CheckUpdate"], async (_, _) => await CheckUpdateAndNotify())));
        menu.Add(NativeRoot(Strings.Instance["Menu_Help_Help"],
            NativeItem(Strings.Instance["Menu_Help_License"], async (_, _) => await ShowLicense()),
            NativeItem(Strings.Instance["Menu_Help_About"], async (_, _) => await ShowAbout())));
        return menu;
    }

    private Control BuildToolBar()
    {
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = _look.IsMacOS ? 5 : 4,
            Margin = _look.ToolbarPadding,
            VerticalAlignment = VerticalAlignment.Center
        };

        toolbar.Children.Add(ToolButton("open", Strings.Instance["ToolTip_Open"], async (_, _) => await OpenFiles()));
        toolbar.Children.Add(ToolButton("clear", Strings.Instance["ToolTip_Clear"], (_, _) => ClearFiles()));
        toolbar.Children.Add(ToolButton("apply", Strings.Instance["ToolTip_Apply"], async (_, _) => await ApplyFiles()));
        toolbar.Children.Add(SeparatorLine());
        toolbar.Children.Add(ToolButton("undo", Strings.Instance["ToolTip_Undo"], (_, _) => Undo()));
        toolbar.Children.Add(ToolButton("redo", Strings.Instance["ToolTip_Redo"], (_, _) => Redo()));
        toolbar.Children.Add(SeparatorLine());
        toolbar.Children.Add(ToolButton("item_up", Strings.Instance["ToolTip_ItemUp"], (_, _) => MoveSelected(-1)));
        toolbar.Children.Add(ToolButton("item_down", Strings.Instance["ToolTip_ItemDown"], (_, _) => MoveSelected(1)));
        toolbar.Children.Add(ToolButton("item_sort", Strings.Instance["ToolTip_ItemSort"], (_, _) => SortFiles()));
        toolbar.Children.Add(SeparatorLine());

        AddCommandButton(toolbar, "replace_text", "Command_Name_ReplacePlain");
        AddCommandButton(toolbar, "concat_text", "Command_Name_Concatenate");
        AddCommandButton(toolbar, "trim_text", "Command_Name_Trim");
        AddCommandButton(toolbar, "delete_block", "Command_Name_DeleteBlock");
        AddCommandButton(toolbar, "delete_text", "Command_Name_DeleteFilename");
        AddCommandButton(toolbar, "substring", "Command_Name_Substring");
        AddCommandButton(toolbar, "casecast_text", "Command_Name_Casecast");
        toolbar.Children.Add(SeparatorLine());
        AddCommandButton(toolbar, "add_ext", "Command_Name_AddExtension");
        AddCommandButton(toolbar, "delete_ext", "Command_Name_DeleteExtension");
        AddCommandButton(toolbar, "replace_ext", "Command_Name_ReplaceExtension");
        AddCommandButton(toolbar, "casecast_ext", "Command_Name_CasecastExtension");
        toolbar.Children.Add(SeparatorLine());
        AddCommandButton(toolbar, "del_without_num", "Command_Name_DeleteNoNumber");
        AddCommandButton(toolbar, "match_num_count", "Command_Name_SameNumberCount");
        AddCommandButton(toolbar, "add_index", "Command_Name_AddIndex");
        AddCommandButton(toolbar, "inc_dec_num", "Command_Name_Increase");
        toolbar.Children.Add(SeparatorLine());
        AddCommandButton(toolbar, "add_date", "Command_Name_AddDate");

        var viewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = global::Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            Content = toolbar
        };
        return new Border
        {
            Background = _look.Surface,
            BorderBrush = _look.Border,
            BorderThickness = new Thickness(0, 1, 0, 1),
            Child = viewer
        };
    }

    private Control BuildFileList()
    {
        var root = new Grid
        {
            Margin = _look.ContentPadding,
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        var header = FileRow(
            Strings.Instance["OriginalName"],
            Strings.Instance["ChangedName"],
            Strings.Instance["OriginalPath"],
            Strings.Instance["ChangedPath"],
            false,
            true);
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        _fileList = new ListBox
        {
            ItemsSource = _items,
            SelectionMode = SelectionMode.Multiple,
            ItemTemplate = new FuncDataTemplate<FileItem>((item, _) => FileRow(
                item?.OriginalFilename ?? string.Empty,
                item?.ChangedFilename ?? string.Empty,
                item?.OriginalPath ?? string.Empty,
                item?.ChangedPath ?? string.Empty,
                item?.OriginalFullPath != item?.ChangedFullPath,
                false), true),
            ContextMenu = BuildCommandsContextMenu()
        };
        _fileList.DoubleTapped += async (_, _) => await EditSelectedFile();
        _fileList.KeyUp += (_, e) =>
        {
            if (e.Key == Key.Delete)
                RemoveSelected();
        };
        DragDrop.SetAllowDrop(_fileList, true);
        _fileList.AddHandler(DragDrop.DropEvent, async (_, e) => await DropFiles(e));

        Grid.SetRow(_fileList, 1);
        root.Children.Add(_fileList);
        return new Border
        {
            Background = _look.Surface,
            BorderBrush = _look.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = _look.SurfaceRadius,
            ClipToBounds = true,
            Child = root
        };
    }

    private MenuItem BuildCommandsMenu(string header) => BuildCommandCategoryMenu(header);
    private ContextMenu BuildCommandsContextMenu() => new() { ItemsSource = BuildCommandCategoryItems() };

    private MenuItem BuildCommandCategoryMenu(string header) =>
        Menu(header, BuildCommandCategoryItems().Cast<object>().ToArray());

    private IEnumerable<MenuItem> BuildCommandCategoryItems()
    {
        yield return CommandCategoryMenu(CommandCategory.Filename, Strings.Instance["Menu_Command_Filename"]);
        yield return CommandCategoryMenu(CommandCategory.Extension, Strings.Instance["Menu_Command_Extension"]);
        yield return CommandCategoryMenu(CommandCategory.Path, Strings.Instance["Menu_Command_Path"]);
        yield return CommandCategoryMenu(CommandCategory.Number, Strings.Instance["Menu_Command_Number"]);
        yield return CommandCategoryMenu(CommandCategory.Date, Strings.Instance["Menu_Command_Date"]);
        yield return CommandCategoryMenu(CommandCategory.Tag, Strings.Instance["Menu_Command_Tag"]);
        yield return CommandCategoryMenu(CommandCategory.Etc, Strings.Instance["Menu_Command_Etc"]);
    }

    private MenuItem CommandCategoryMenu(CommandCategory category, string header)
    {
        var item = new MenuItem { Header = header };
        foreach (var descriptor in DaramRenamerRegistry.GetCommands(category))
            item.Items.Add(CommandItem(descriptor));
        return item;
    }

    private MenuItem BuildConditionsMenu()
    {
        var menu = new MenuItem { Header = Strings.Instance["Menu_Condition_Condition"] };
        foreach (var descriptor in DaramRenamerRegistry.Conditions.OrderBy(condition => condition.Order))
        {
            var condition = descriptor.Create();
            var item = new MenuItem
            {
                Header = Strings.Instance[descriptor.LocalizationKey],
                Tag = condition,
                ToggleType = MenuItemToggleType.CheckBox
            };
            item.Click += async (_, _) => await ConfigureCondition(item);
            _conditionItems.Add(item);
            menu.Items.Add(item);
        }
        return menu;
    }

    private NativeMenuItem BuildNativeCommandCategoryMenu(string header)
    {
        var item = new NativeMenuItem(header) { Menu = new NativeMenu() };
        foreach (var category in BuildNativeCommandCategoryItems())
            item.Menu!.Add(category);
        return item;
    }

    private IEnumerable<NativeMenuItem> BuildNativeCommandCategoryItems()
    {
        yield return NativeCommandCategoryMenu(CommandCategory.Filename, Strings.Instance["Menu_Command_Filename"]);
        yield return NativeCommandCategoryMenu(CommandCategory.Extension, Strings.Instance["Menu_Command_Extension"]);
        yield return NativeCommandCategoryMenu(CommandCategory.Path, Strings.Instance["Menu_Command_Path"]);
        yield return NativeCommandCategoryMenu(CommandCategory.Number, Strings.Instance["Menu_Command_Number"]);
        yield return NativeCommandCategoryMenu(CommandCategory.Date, Strings.Instance["Menu_Command_Date"]);
        yield return NativeCommandCategoryMenu(CommandCategory.Tag, Strings.Instance["Menu_Command_Tag"]);
        yield return NativeCommandCategoryMenu(CommandCategory.Etc, Strings.Instance["Menu_Command_Etc"]);
    }

    private NativeMenuItem NativeCommandCategoryMenu(CommandCategory category, string header)
    {
        var item = new NativeMenuItem(header) { Menu = new NativeMenu() };
        foreach (var descriptor in DaramRenamerRegistry.GetCommands(category))
            item.Menu!.Add(NativeCommandItem(descriptor));
        return item;
    }

    private NativeMenuItem BuildNativeConditionsMenu()
    {
        var menu = new NativeMenuItem(Strings.Instance["Menu_Condition_Condition"]) { Menu = new NativeMenu() };
        foreach (var descriptor in DaramRenamerRegistry.Conditions.OrderBy(condition => condition.Order))
        {
            var condition = descriptor.Create();
            var item = new NativeMenuItem(Strings.Instance[descriptor.LocalizationKey])
            {
                CommandParameter = condition,
                ToggleType = NativeMenuItemToggleType.CheckBox
            };
            item.Click += async (_, _) => await ConfigureNativeCondition(item);
            _nativeConditionItems.Add(item);
            menu.Menu!.Add(item);
        }
        return menu;
    }

    private async Task OpenFiles()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = true,
            Title = Strings.Instance["FileDialogTitleOpenFiles"]
        });

        if (files.Count == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var path in files.Select(file => file.TryGetLocalPath()).Where(path => path != null).OrderBy(path => path))
            AddPath(path!);
        RefreshStatus();
    }

    private async Task OpenFolders()
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = true,
            Title = Strings.Instance["FileDialogTitleOpenFiles"]
        });

        if (folders.Count == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var path in folders.Select(folder => folder.TryGetLocalPath()).Where(path => path != null).OrderBy(path => path))
            AddPath(path!, true);
        RefreshStatus();
    }

    private async Task DropFiles(DragEventArgs e)
    {
#pragma warning disable CS0618
        var files = e.Data.GetFiles()?.ToArray();
#pragma warning restore CS0618
        if (files is not { Length: > 0 })
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var path in files.Select(file => file.TryGetLocalPath()).Where(path => path != null).OrderBy(path => path))
            AddPath(path!, Directory.Exists(path));
        RefreshStatus();
    }

    private void AddPath(string path, bool directoryMode = false)
    {
        if (FileItem.FileOperator.FileExists(path) || directoryMode)
        {
            var item = new FileItem(path);
            if (!FileItem.Files.Contains(item))
                FileItem.Files.Add(item);
            return;
        }

        foreach (var file in FileItem.FileOperator.GetFiles(path, false))
            AddPath(file);
    }

    private async Task RunCommand(CommandDescriptor descriptor)
    {
        var command = descriptor.Create();
        var current = _undoManager.SaveTemporary(FileItem.Files);

        if (descriptor.Options.Count > 0)
        {
            var dialog = new OptionDialog(descriptor, command)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                PreviewChanged = AvaloniaPreferences.Instance.VisualCommand
                    ? () =>
                    {
                        SetItems(_undoManager.LoadTemporary(current));
                        ApplyCommand(command, FileItem.Files);
                        RefreshList();
                    }
                    : null
            };
            if (await dialog.ShowDialog<bool>(this) != true)
            {
                SetItems(_undoManager.LoadTemporary(current));
                return;
            }

            SetItems(_undoManager.LoadTemporary(current));
        }

        _undoManager.SaveToUndoStack(FileItem.Files);
        ApplyCommand(command, FileItem.Files);
        RefreshList();
    }

    private void ApplyCommand(ICommand command, IEnumerable<FileItem> targets)
    {
        var conditions = GetActiveConditions().ToArray();
        var targetArray = targets.ToArray();
        if (command is ITargetContains targetContains)
            targetContains.SetTargets(targetArray);

        var index = 0;
        if (command.ParallelProcessable && !AvaloniaPreferences.Instance.ForceSingleCoreRunning)
        {
            Parallel.ForEach(targetArray, file =>
            {
                if (conditions.All(condition => condition.IsSatisfyThisCondition(file)))
                    command.DoCommand(file);
            });
            return;
        }

        foreach (var file in targetArray)
        {
            if (conditions.All(condition => condition.IsSatisfyThisCondition(file)))
                command.DoCommand(file, index);
            ++index;
        }
    }

    private IEnumerable<ICondition> GetActiveConditions() =>
        UseNativeMenuBar
            ? _nativeConditionItems
                .Where(item => item.IsChecked)
                .Select(item => item.CommandParameter)
                .OfType<ICondition>()
            : _conditionItems
                .Where(item => item.IsChecked)
                .Select(item => item.Tag)
                .OfType<ICondition>();

    private async Task ConfigureCondition(MenuItem item)
    {
        if (!item.IsChecked || item.Tag is not ICondition condition)
            return;

        var descriptor = DaramRenamerRegistry.GetDescriptor(condition);
        if (descriptor?.Options.Count > 0)
        {
            var dialog = new OptionDialog(descriptor, condition)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (await dialog.ShowDialog<bool>(this) != true)
                item.IsChecked = false;
        }
    }

    private async Task ConfigureNativeCondition(NativeMenuItem item)
    {
        if (!item.IsChecked || item.CommandParameter is not ICondition condition)
            return;

        var descriptor = DaramRenamerRegistry.GetDescriptor(condition);
        if (descriptor?.Options.Count > 0)
        {
            var dialog = new OptionDialog(descriptor, condition)
            {
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            if (await dialog.ShowDialog<bool>(this) != true)
                item.IsChecked = false;
        }
    }

    private async Task EditSelectedFile()
    {
        if (_fileList.SelectedItem is not FileItem item)
            return;

        _manualEditCommand.ChangeName = item.ChangedFilename;
        _manualEditCommand.ChangePath = item.ChangedPath;
        var descriptor = DaramRenamerRegistry.GetDescriptor(_manualEditCommand);
        if (descriptor == null)
            return;

        var dialog = new OptionDialog(descriptor, _manualEditCommand)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        if (await dialog.ShowDialog<bool>(this) != true)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        ApplyCommand(_manualEditCommand, [item]);
        RefreshList();
    }

    private async Task ApplyFiles()
    {
        if (FileItem.Files.Count == 0)
            return;
        if (!await ConfirmApplyWarnings())
            return;

        var dialog = new ApplyDialog(_undoManager)
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        await dialog.ShowDialog(this);
        RefreshList();
    }

    private void ClearFiles()
    {
        _undoManager.ClearAll();
        FileItem.Files.Clear();
        RefreshStatus();
    }

    private void RemoveSelected()
    {
        var selected = _fileList.SelectedItems?.OfType<FileItem>().ToArray() ?? [];
        if (selected.Length == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var item in selected)
            FileItem.Files.Remove(item);
        RefreshStatus();
    }

    private void Undo()
    {
        if (_undoManager.IsUndoStackEmpty)
            return;

        _undoManager.SaveToRedoStack(FileItem.Files);
        SetItems(_undoManager.LoadFromUndoStack());
    }

    private void Redo()
    {
        if (_undoManager.IsRedoStackEmpty)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files, false);
        SetItems(_undoManager.LoadFromRedoStack());
    }

    private void MoveSelected(int direction)
    {
        var selected = _fileList.SelectedItems?.OfType<FileItem>().ToArray() ?? [];
        if (selected.Length == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var item in direction < 0 ? selected : selected.Reverse())
        {
            var oldIndex = FileItem.Files.IndexOf(item);
            var newIndex = oldIndex + direction;
            if (newIndex < 0 || newIndex >= FileItem.Files.Count)
                continue;
            FileItem.Files.Move(oldIndex, newIndex);
        }
    }

    private void SortFiles()
    {
        _undoManager.SaveToUndoStack(FileItem.Files);
        FileItem.Sort(FileItem.Files);
    }

    private void RestoreSelected()
    {
        var selected = _fileList.SelectedItems?.OfType<FileItem>().ToArray() ?? [];
        if (selected.Length == 0)
            return;

        _undoManager.SaveToUndoStack(FileItem.Files);
        foreach (var item in selected)
            item.Reset();
        RefreshList();
    }

    private async Task CheckUpdateAndNotify()
    {
        if (AvaloniaPreferences.Instance.DisableCheckUpdate)
        {
            await ShowMessage(Strings.Instance["DaramRenamer"], Strings.Instance["PreferencesDisableCheckUpdate"]);
            return;
        }

        var updateInformation = await UpdateInformationBank.GetUpdateInformationAsync(TargetPlatform.Windows);
        var message = updateInformation is null
            ? Strings.Instance["UpdateCheckErrorText"]
            : updateInformation.Value.StableLatestVersion == GetVersionString()
                ? Strings.Instance["ThisIsLatestVersionText"]
                : Strings.Instance["NewLatestVersionAvailableText"];
        await ShowMessage(Strings.Instance["DaramRenamer"], message);
    }

    private async Task ShowPreferences()
    {
        var dialog = new PreferencesDialog { WindowStartupLocation = WindowStartupLocation.CenterOwner };
        await dialog.ShowDialog<bool>(this);
    }

    private void ShowBatchWindow()
    {
        if (_batchWindow is null or { IsVisible: false })
            _batchWindow = new BatchWindow { WindowStartupLocation = WindowStartupLocation.CenterOwner };
        _batchWindow.Show(this);
    }

    private async Task ShowLicense()
    {
        var license = await LoadLicense();
        var dialog = new Window
        {
            Title = Strings.Instance["LicenseWindowTitle"],
            Width = 800,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = new TextBox
            {
                Text = license,
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap
            }
        };
        await dialog.ShowDialog(this);
    }

    private async Task ShowAbout()
    {
        await ShowMessage(Strings.Instance["AboutWindowTitle"],
            $"{Strings.Instance["DaramRenamer"]}\n{Strings.Instance["Version"]} {GetVersionString()}");
    }

    private static async Task<string> LoadLicense()
    {
        var assembly = typeof(MainWindow).Assembly;
        await using var stream = assembly.GetManifestResourceStream("DaramRenamer.Avalonia.Resources.LICENSE.md");
        if (stream == null)
            return string.Empty;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private void SetItems(ObservableCollection<FileItem> items)
    {
        FileItem.Files = items;
        _items = items;
        _fileList.ItemsSource = _items;
        RefreshStatus();
    }

    private void RefreshList()
    {
        _fileList.ItemsSource = null;
        _fileList.ItemsSource = _items;
        RefreshStatus();
    }

    private static void RefreshStatus()
    {
    }

    private async Task HandleShortcut(KeyEventArgs e)
    {
        if (await HandleDefaultShortcut(e))
            return;

        foreach (var shortcut in AvaloniaPreferences.Instance.Shortcuts)
        {
            if (!shortcut.Matches(e))
                continue;
            var command = shortcut.CreateCommand();
            var descriptor = command == null ? null : DaramRenamerRegistry.GetDescriptor(command);
            if (descriptor != null)
            {
                await RunCommand(descriptor);
                e.Handled = true;
            }
            return;
        }
    }

    private async Task<bool> HandleDefaultShortcut(KeyEventArgs e)
    {
        if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
            return false;

        if (e.Key == Key.O)
            await OpenFiles();
        else if (e.Key == Key.P)
            await OpenFolders();
        else if (e.Key == Key.Delete)
            ClearFiles();
        else if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
            await ApplyFiles();
        else if (e.Key == Key.Z)
            Undo();
        else if (e.Key == Key.Y)
            Redo();
        else if (e.Key == Key.Up)
            MoveSelected(-1);
        else if (e.Key == Key.Down)
            MoveSelected(1);
        else if (e.Key == Key.S && e.KeyModifiers.HasFlag(KeyModifiers.Shift))
            SortFiles();
        else
            return false;

        e.Handled = true;
        return true;
    }

    private async Task<bool> ConfirmApplyWarnings()
    {
        var changed = FileItem.Files
            .Where(item => item.OriginalFullPath != item.ChangedFullPath)
            .ToArray();
        var duplicated = changed
            .GroupBy(item => item.ChangedFullPath, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Take(5)
            .ToArray();
        var existing = changed
            .Where(item => !AvaloniaPreferences.Instance.Overwrite &&
                           item.OriginalFullPath != item.ChangedFullPath &&
                           (File.Exists(item.ChangedFullPath) || Directory.Exists(item.ChangedFullPath)))
            .Select(item => item.ChangedFullPath)
            .Take(5)
            .ToArray();

        if (duplicated.Length == 0 && existing.Length == 0)
            return true;

        var message = new List<string>();
        if (duplicated.Length > 0)
        {
            message.Add("Duplicated target paths:");
            message.AddRange(duplicated.Select(path => $"  {path}"));
        }
        if (existing.Length > 0)
        {
            message.Add("Existing target paths:");
            message.AddRange(existing.Select(path => $"  {path}"));
        }
        message.Add("");
        message.Add("Continue applying?");

        return await ShowConfirmation(Strings.Instance["DaramRenamer"], string.Join(Environment.NewLine, message));
    }

    private async Task<bool> ShowConfirmation(string title, string message)
    {
        var look = NativeTheme.Current;
        var dialog = new Window
        {
            Title = title,
            Width = 520,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = look.AppBackground
        };
        var yes = new Button { Content = "OK", MinWidth = 76 };
        yes.Click += (_, _) => dialog.Close(true);
        var cancel = new Button { Content = "Cancel", MinWidth = 76 };
        cancel.Click += (_, _) => dialog.Close(false);
        dialog.Content = new StackPanel
        {
            Margin = look.IsMacOS ? new Thickness(18) : new Thickness(16),
            Spacing = 12,
            Children =
            {
                new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, Foreground = look.Text },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Spacing = 6,
                    Children = { yes, cancel }
                }
            }
        };
        return await dialog.ShowDialog<bool>(this);
    }

    private void OnHistoryChanged()
    {
        OnPropertyChanged(nameof(UndoManagerHasUndoStackItem));
        OnPropertyChanged(nameof(UndoManagerHasRedoStackItem));
    }

    private static string GetVersionString()
    {
        var programVersion = typeof(MainWindow).Assembly.GetName().Version;
        return programVersion != null
            ? $"{programVersion.Major}.{programVersion.Minor}.{programVersion.Build}"
            : "UNKNOWN VERSION";
    }

    private void SaveWindowState()
    {
        var prefs = AvaloniaPreferences.Instance;
        if (!prefs.SaveWindowState)
        {
            prefs.Save();
            return;
        }

        prefs.Left = Position.X;
        prefs.Top = Position.Y;
        prefs.Width = Width;
        prefs.Height = Height;
        prefs.Save();
    }

    private async Task ShowMessage(string title, string message)
    {
        var look = NativeTheme.Current;
        var dialog = new Window
        {
            Title = title,
            Width = 420,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = look.AppBackground,
            Content = new StackPanel
            {
                Margin = look.IsMacOS ? new Thickness(18) : new Thickness(16),
                Spacing = 12,
                Children =
                {
                    new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, Foreground = look.Text },
                    new Button
                    {
                        Content = "OK",
                        MinWidth = 76,
                        HorizontalAlignment = HorizontalAlignment.Right
                    }
                }
            }
        };
        ((Button)((StackPanel)dialog.Content!).Children[1]).Click += (_, _) => dialog.Close();
        await dialog.ShowDialog(this);
    }

    private static MenuItem Menu(string header, params object[] items)
    {
        var menu = new MenuItem { Header = header };
        foreach (var item in items)
            menu.Items.Add(item);
        return menu;
    }

    private static MenuItem Item(string header, EventHandler<RoutedEventArgs> click, string? gesture = null)
    {
        var item = new MenuItem { Header = gesture == null ? header : $"{header}\t{gesture}" };
        item.Click += click;
        return item;
    }

    private static NativeMenuItem NativeRoot(string header, params NativeMenuItemBase[] items)
    {
        var menu = new NativeMenuItem(header) { Menu = new NativeMenu() };
        foreach (var item in items)
            menu.Menu!.Add(item);
        return menu;
    }

    private static NativeMenuItem NativeItem(string header, EventHandler click, string? gesture = null)
    {
        var item = new NativeMenuItem(header);
        if (gesture != null)
        {
            try
            {
                item.Gesture = KeyGesture.Parse(gesture);
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                // Keep the menu item usable even if a platform cannot parse the gesture text.
            }
        }
        item.Click += click;
        return item;
    }

    private MenuItem CommandItem(CommandDescriptor descriptor)
    {
        var item = new MenuItem { Header = Strings.Instance[descriptor.LocalizationKey] };
        item.Click += async (_, _) => await RunCommand(descriptor);
        return item;
    }

    private NativeMenuItem NativeCommandItem(CommandDescriptor descriptor)
    {
        var item = new NativeMenuItem(Strings.Instance[descriptor.LocalizationKey]);
        item.Click += async (_, _) => await RunCommand(descriptor);
        return item;
    }

    private static bool UseNativeMenuBar =>
        OperatingSystem.IsMacOS() || OperatingSystem.IsLinux();

    private void AddCommandButton(StackPanel toolbar, string icon, string localizationKey)
    {
        var descriptor = DaramRenamerRegistry.FindCommandDescriptor(localizationKey);
        if (descriptor != null)
            toolbar.Children.Add(ToolButton(icon, Strings.Instance[localizationKey], async (_, _) => await RunCommand(descriptor)));
    }

    private Button ToolButton(string icon, string tooltip, EventHandler<RoutedEventArgs> click)
    {
        var button = new Button
        {
            Width = _look.ToolbarButtonSize,
            Height = _look.ToolbarButtonSize,
            Padding = _look.IsMacOS ? new Thickness(5) : new Thickness(4),
            Background = icon == "apply" ? _look.AccentSoft : Brushes.Transparent,
            BorderBrush = icon == "apply" ? _look.Accent : Brushes.Transparent,
            CornerRadius = _look.ControlRadius,
            Content = new Image
            {
                Source = LoadIcon(icon),
                Width = _look.ToolbarIconSize,
                Height = _look.ToolbarIconSize,
                Stretch = Stretch.Uniform
            }
        };
        ToolTip.SetTip(button, tooltip);
        button.Click += click;
        return button;
    }

    private Control SeparatorLine() =>
        new Border
        {
            Width = 1,
            Height = _look.IsMacOS ? 24 : 22,
            Margin = _look.IsMacOS ? new Thickness(5, 3) : new Thickness(4, 3),
            Background = _look.Border
        };

    private static IImage? LoadIcon(string name)
    {
        var uri = new Uri($"avares://DaramRenamer.Avalonia/Assets/ToolBarIcons/{name}.png");
        return AssetLoader.Exists(uri) ? new Bitmap(AssetLoader.Open(uri)) : null;
    }

    private static WindowIcon? LoadWindowIcon()
    {
        var uri = new Uri("avares://DaramRenamer.Avalonia/Assets/Icon.jpg");
        return AssetLoader.Exists(uri) ? new WindowIcon(AssetLoader.Open(uri)) : null;
    }

    private Grid FileRow(string originalName, string changedName, string originalPath, string changedPath, bool changed, bool header)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(2.2, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(2.2, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1.6, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1.6, GridUnitType.Star))
            },
            MinHeight = header ? (_look.IsMacOS ? 34 : 31) : (_look.IsMacOS ? 31 : 29),
            Background = header ? _look.HeaderBackground : Brushes.Transparent
        };

        AddCell(row, originalName, 0, header, header ? _look.Text : _look.Text);
        AddCell(row, changedName, 1, header, header ? _look.Text : changed ? _look.ChangedText : _look.Text);
        AddCell(row, originalPath, 2, header, header ? _look.Text : _look.MutedText);
        AddCell(row, changedPath, 3, header, header ? _look.Text : changed ? _look.ChangedText : _look.MutedText);
        return row;
    }

    private void AddCell(Grid row, string text, int column, bool header, IBrush foreground)
    {
        var block = new TextBlock
        {
            Text = text,
            FontWeight = header ? FontWeight.SemiBold : FontWeight.Normal,
            Foreground = foreground,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = _look.IsMacOS ? new Thickness(10, 0) : new Thickness(8, 0)
        };
        ToolTip.SetTip(block, text);
        Grid.SetColumn(block, column);
        row.Children.Add(block);
    }

    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal sealed class OptionDialog : Window
{
    public Action? PreviewChanged { get; set; }

    public OptionDialog(ItemDescriptor descriptor, object target)
    {
        var look = NativeTheme.Current;
        Title = Strings.Instance[descriptor.LocalizationKey];
        Width = 440;
        SizeToContent = SizeToContent.Height;
        Background = look.AppBackground;

        var root = new DockPanel { Margin = look.IsMacOS ? new Thickness(18) : new Thickness(14) };
        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 6,
            Margin = new Thickness(0, 12, 0, 0)
        };
        DockPanel.SetDock(buttons, Dock.Bottom);

        var ok = new Button { Content = "OK", MinWidth = 76 };
        ok.Click += (_, _) => Close(true);
        buttons.Children.Add(ok);

        var cancel = new Button { Content = "Cancel", MinWidth = 76 };
        cancel.Click += (_, _) => Close(false);
        buttons.Children.Add(cancel);
        root.Children.Add(buttons);

        var form = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            RowDefinitions = new RowDefinitions(string.Join(",", descriptor.Options.Select(_ => "Auto")))
        };

        for (var row = 0; row < descriptor.Options.Count; ++row)
        {
            var option = descriptor.Options[row];
            var label = new TextBlock
            {
                Text = Strings.Instance[option.LocalizationKey],
                Foreground = look.Text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 8)
            };
            Grid.SetRow(label, row);
            form.Children.Add(label);

            var control = CreateControl(option, target);
            control.Margin = new Thickness(0, 0, 0, 8);
            Grid.SetRow(control, row);
            Grid.SetColumn(control, 1);
            form.Children.Add(control);
        }

        root.Children.Add(form);
        Content = root;
    }

    private static Control CreateControl(IOptionDescriptor option, object target)
    {
        if (option.ValueKind is OptionValueKind.Boolean or OptionValueKind.NullableBoolean)
        {
            var checkBox = new CheckBox
            {
                IsThreeState = option.ValueKind == OptionValueKind.NullableBoolean,
                IsChecked = option.GetValue(target) as bool?
            };
            checkBox.IsCheckedChanged += (_, _) => option.SetValue(target, checkBox.IsChecked);
            checkBox.IsCheckedChanged += (_, _) => (TopLevel.GetTopLevel(checkBox) as OptionDialog)?.PreviewChanged?.Invoke();
            return checkBox;
        }

        if (option.ValueKind == OptionValueKind.Enum)
        {
            var values = Enum.GetValues(option.ValueType);
            var comboBox = new ComboBox
            {
                ItemsSource = values.Cast<object>().Select(value => new EnumItem(value)).ToArray(),
                SelectedIndex = Array.IndexOf(values, option.GetValue(target)),
                MinWidth = 180
            };
            comboBox.SelectionChanged += (_, _) =>
            {
                if (comboBox.SelectedItem is EnumItem item)
                    option.SetValue(target, item.Value);
                (TopLevel.GetTopLevel(comboBox) as OptionDialog)?.PreviewChanged?.Invoke();
            };
            return comboBox;
        }

        var textBox = new TextBox { Text = option.SerializeValue(target), MinWidth = 220 };
        textBox.TextChanged += (_, _) =>
        {
            try
            {
                option.DeserializeValue(target, textBox.Text ?? string.Empty);
                (TopLevel.GetTopLevel(textBox) as OptionDialog)?.PreviewChanged?.Invoke();
            }
            catch
            {
                // Intermediate text can be invalid while the user is typing.
            }
        };
        return textBox;
    }

    private sealed record EnumItem(object Value)
    {
        public override string ToString() => Strings.Instance[Value.ToString() ?? string.Empty];
    }
}

internal sealed class ApplyDialog : Window
{
    private readonly UndoManager _undoManager;
    private readonly ProgressBar _progress = new();
    private readonly TextBlock _processing = new();
    private readonly TextBlock _count = new();
    private readonly ListBox _failures = new();
    private readonly CheckBox _autoFix = new() { IsChecked = AvaloniaPreferences.Instance.AutomaticFixingFilename };
    private readonly CheckBox _overwrite = new();
    private readonly ComboBox _mode = new();
    private bool _complete;

    public ApplyDialog(UndoManager undoManager)
    {
        _undoManager = undoManager;
        Title = Strings.Instance["ApplyWindowTitle"];
        Width = 420;
        SizeToContent = SizeToContent.Height;
        Background = NativeTheme.Current.AppBackground;

        _mode.ItemsSource = new[]
        {
            new RenameModeItem(RenameMode.Move),
            new RenameModeItem(RenameMode.Copy)
        };
        _mode.SelectedIndex = AvaloniaPreferences.Instance.RenameMode == RenameMode.Copy ? 1 : 0;

        var close = new Button
        {
            Content = Strings.Instance["ButtonClose"],
            IsEnabled = false,
            MinWidth = 76,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        close.Click += (_, _) => Close();

        Content = new StackPanel
        {
            Margin = NativeTheme.Current.IsMacOS ? new Thickness(18) : new Thickness(14),
            Spacing = 8,
            Children =
            {
                new TextBlock { Text = Strings.Instance["PreferencesRenameMode"] },
                _mode,
                _autoFix,
                _overwrite,
                _processing,
                _progress,
                _count,
                _failures,
                close
            }
        };

        _overwrite.Content = Strings.Instance["PreferencesOverwrite"];
        _overwrite.IsChecked = AvaloniaPreferences.Instance.Overwrite;

        Opened += async (_, _) =>
        {
            await Run();
            close.IsEnabled = true;
        };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = !_complete;
        base.OnClosing(e);
    }

    private async Task Run()
    {
        var failed = false;
        _undoManager.ClearUndoStack();
        _progress.Minimum = 0;
        _progress.Maximum = FileItem.Files.Count;
        _progress.Value = 0;
        _count.Text = $"0/{FileItem.Files.Count}";

        var mode = _mode.SelectedItem is RenameModeItem item ? item.Value : RenameMode.Move;
        var autoFix = _autoFix.IsChecked == true;
        var overwrite = _overwrite.IsChecked == true;

        await Task.Run(() =>
        {
            FileItem.Apply(autoFix, mode, overwrite, (file, errorCode) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ++_progress.Value;
                    _processing.Text = $"{Strings.Instance["ApplyWindowLabelProcessing"]}{file.OriginalFullPath}";
                    _count.Text = $"{(int)_progress.Value}/{FileItem.Files.Count}";
                    if (errorCode != ErrorCode.NoError)
                        _failures.Items.Add($"{file.OriginalFilename} -> {file.ChangedFilename} ({errorCode})");
                });
                if (errorCode != ErrorCode.NoError)
                    failed = true;
            });
        });

        if (!failed)
            _undoManager.SaveToUndoStack(FileItem.Files);
        if (!failed && AvaloniaPreferences.Instance.AutomaticListCleaning)
            FileItem.Files.Clear();
        if (AvaloniaPreferences.Instance.RemoveEmptyDirectory)
            RemoveEmptyDirectories();
        _complete = true;
        if (!failed && AvaloniaPreferences.Instance.CloseApplyWindowWhenSuccessfullyDone)
            Dispatcher.UIThread.Post(Close);
    }

    private static void RemoveEmptyDirectories()
    {
        foreach (var path in FileItem.Files.Select(item => item.OriginalPath).Distinct().ToArray())
        {
            try
            {
                if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
                    Directory.Delete(path, recursive: true);
            }
            catch
            {
                // Ignore cleanup failures.
            }
        }
    }

    private sealed record RenameModeItem(RenameMode Value)
    {
        public override string ToString() => Strings.Instance[Value.ToString()];
    }
}
