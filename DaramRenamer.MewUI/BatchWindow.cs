using System.Text;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Platform;
using DaramRenamer.Registry;

namespace DaramRenamer.MewUI;

internal sealed class BatchWindow : Window
{
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

    private readonly Action<RootBatchNode> _executeBatch;
    private readonly IReadOnlyDictionary<string, ICondition> _conditions;
    private readonly RootBatchNode _rootNode = new();
    private readonly TreeView _tree = new();
    private readonly Dictionary<string, Command> _commands = new(StringComparer.Ordinal);
    private readonly Dictionary<BatchNode, TreeViewNode> _treeNodes = new(ReferenceEqualityComparer.Instance);
    private readonly List<TreeViewNode> _visibleNodes = [];
    private MenuItem? _removeMenuItem;

    private BatchNode? _selectedNode;

    public BatchWindow(Action<RootBatchNode> executeBatch, IReadOnlyDictionary<string, ICondition> conditions)
    {
        _executeBatch = executeBatch;
        _conditions = conditions;

        this.Title(Strings.Instance["BatchWindow_Title"]);
        if (OperatingSystem.IsWindows())
            this.Icon(IconSource.FromResource(
                typeof(BatchWindow).Assembly,
                "DaramRenamer.MewUI.Resources.ProgramIcon.ico"));
        this.Resizable(800, 450);

        _tree
            .ItemHeight(22)
            .ItemPadding(new Thickness(2, 1))
            .FontFamily("Segoe UI")
            .FontSize(12)
            .OnSelectedNodeChanged(OnSelectedNodeChanged)
            .OnMouseDown(OnTreeMouseDown)
            .AllowDrop(true)
            .OnDrop(OnBatchFileDropped)
            .ContextMenu(BuildItemContextMenu());

        this.Content(BuildUi());
        RefreshTree();
        Strings.Instance.Loaded += OnStringsLoaded;
        this.OnClosed(() => Strings.Instance.Loaded -= OnStringsLoaded);
    }

    private void OnStringsLoaded(object? sender, EventArgs args)
    {
        this.Title(Strings.Instance["BatchWindow_Title"]);
        _tree.ContextMenu(BuildItemContextMenu());
        this.Content(BuildUi());
        RefreshTree(_selectedNode);
    }

    private Grid BuildUi()
    {
        var toolbar = new Grid()
            .Columns("*,Auto")
            .Margin(0, 0, 0, 8)
            .Children(
                new StackPanel()
                    .Horizontal()
                    .Spacing(8)
                    .Children(
                        ActionButton(Strings.Instance["BatchWindow_LoadFile"], LoadBatch, 72),
                        ActionButton(Strings.Instance["BatchWindow_SaveFile"], SaveBatch, 72))
                    .Column(1));

        var footer = new StackPanel()
            .Horizontal()
            .CenterHorizontal()
            .Children(
                ActionButton(Strings.Instance["BatchWindow_DoBatch"], ExecuteBatch, 96),
                ActionButton(Strings.Instance["BatchWindow_Close"], Close, 96).Margin(8, 0, 0, 0));

        return new Grid()
            .Rows("32,*,24")
            .Padding(8)
            .Children(
                toolbar.Row(0),
                _tree.Margin(0, 0, 0, 8).Row(1),
                footer.Row(2));
    }

    private ContextMenu BuildItemContextMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem(Strings.Instance["Menu_Command_Command"]).Menu(BuildCommandCategories()));
        menu.Items.Add(new MenuItem(Strings.Instance["Menu_Condition_Condition"]).Menu(BuildConditionsMenu()));
        menu.Separator();
        _removeMenuItem = new MenuItem(Strings.Instance["BatchWindow_Remove"])
            .Command(RegisterAction("remove", Strings.Instance["BatchWindow_Remove"], RemoveSelected))
            .IsEnabled(false);
        menu.Items.Add(_removeMenuItem);

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
                    () => AddCommand(itemDescriptor)));
            }

            menu.SubMenu(Strings.Instance[CategoryLocalizationKey(category)], categoryMenu);
        }

        return menu;
    }

    private Menu BuildConditionsMenu()
    {
        var menu = new Menu();
        foreach (var descriptor in DaramRenamerRegistry.Conditions.OrderBy(item => item.Order))
        {
            var itemDescriptor = descriptor;
            menu.Item(RegisterAction(
                $"condition.{itemDescriptor.Id}",
                Strings.Instance[itemDescriptor.LocalizationKey],
                () => AddCondition(itemDescriptor)));
        }

        return menu;
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

    private Command RegisterAction(string id, string text, Action action)
    {
        if (_commands.TryGetValue(id, out var existing))
        {
            existing.Presentation.AccessText = text;
            return existing;
        }

        var command = new Command($"DaramRenamer.Batch.{id}", text);
        this.Commands.Register(command, action);
        _commands.Add(id, command);
        return command;
    }

    private void AddCommand(ItemDescriptor descriptor)
    {
        if (_selectedNode is null)
            return;

        var command = descriptor.Create() as ICommand;
        if (command is null)
            return;
        command = descriptor.Clone(command) as ICommand ?? command;

        void Add(bool accepted)
        {
            if (!accepted)
                return;

            _selectedNode.Children.Add(new BatchNode { Command = command });
            RefreshTree(_selectedNode);
        }

        if (descriptor.Options.Count == 0)
        {
            Add(true);
            return;
        }

        var editor = new CommandEditorWindow(command, descriptor, Add);
        ModalDialog.Show(editor, this);
    }

    private void AddCondition(ItemDescriptor descriptor)
    {
        if (_selectedNode is null)
            return;

        if (!_conditions.TryGetValue(descriptor.Id, out var condition))
            return;

        void Add(bool accepted)
        {
            if (!accepted)
                return;

            _selectedNode.Children.Add(new BatchNode { Condition = condition });
            RefreshTree(_selectedNode);
        }

        if (descriptor.Options.Count == 0)
        {
            Add(true);
            return;
        }

        var editor = new CommandEditorWindow(condition, descriptor, Add);
        ModalDialog.Show(editor, this);
    }

    private void OnSelectedNodeChanged(TreeViewNode? selectedNode)
    {
        _selectedNode = selectedNode?.Tag as BatchNode;
        _removeMenuItem?.IsEnabled(_selectedNode is not null and not RootBatchNode);
    }

    private void OnTreeMouseDown(MouseEventArgs args)
    {
        if (args.Button != MouseButton.Right ||
            !_tree.TryGetItemIndexAt(args, out var index) ||
            (uint)index >= (uint)_visibleNodes.Count)
            return;

        // WPF selects the row under the pointer before opening its item context menu.
        // Keep that behavior so Add and Remove always target the row that was right-clicked.
        _tree.SelectedNode = _visibleNodes[index];
    }

    private void OnBatchFileDropped(DragEventArgs args)
    {
        if (!args.Data.Contains(StandardDataFormats.StorageItems))
            return;

        var paths = args.Data.GetData(StandardDataFormats.StorageItems) switch
        {
            IEnumerable<string> items => items.ToArray(),
            IEnumerable<Uri> items => items.Where(uri => uri.IsFile).Select(uri => uri.LocalPath).ToArray(),
            string path => [path],
            _ => []
        };
        if (paths.Length == 0)
            return;

        try
        {
            using var reader = new StreamReader(paths[0], Encoding.UTF8, false);
            _rootNode.Deserializer(reader);
            RefreshTree();
        }
        catch (Exception exception)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(exception.Message, owner: this));
        }
    }

    private void RefreshTree(BatchNode? selected = null)
    {
        var selectedNode = selected ?? _selectedNode;
        _treeNodes.Clear();
        _visibleNodes.Clear();

        var root = BuildTreeNode(_rootNode);
        AppendVisibleNodes(_rootNode);
        _tree.ItemsSource(new[] { root });

        foreach (var node in _treeNodes.Values)
            _tree.Expand(node);

        TreeViewNode? selectedView = null;
        if (selectedNode is not null)
            _treeNodes.TryGetValue(selectedNode, out selectedView);

        _selectedNode = selectedView?.Tag as BatchNode;
        _tree.SelectedNode(selectedView);
        _removeMenuItem?.IsEnabled(_selectedNode is not null and not RootBatchNode);
    }

    private void AppendVisibleNodes(BatchNode node)
    {
        _visibleNodes.Add(_treeNodes[node]);
        foreach (var child in node.Children)
            AppendVisibleNodes(child);
    }

    private TreeViewNode BuildTreeNode(BatchNode node)
    {
        var children = node.Children.Select(BuildTreeNode).ToArray();
        var treeNode = new TreeViewNode(node.ToString(), children, node);
        _treeNodes[node] = treeNode;
        return treeNode;
    }

    private void RemoveSelected()
    {
        if (_selectedNode is null || _selectedNode is RootBatchNode)
            return;

        if (DeleteItem(_rootNode, _selectedNode))
            RefreshTree();
    }

    private static bool DeleteItem(BatchNode parent, BatchNode target)
    {
        if (parent.Children.Remove(target))
            return true;

        return parent.Children.Any(child => DeleteItem(child, target));
    }

    private void LoadBatch()
    {
        var path = ModalDialog.RunWithOwner(this, () => FileDialog.OpenFile(new OpenFileDialogOptions
        {
            Owner = this,
            Filters = FileFilter.Parse(Strings.Instance["BatchWindow_File_Filter"])
        }));
        if (path is null)
            return;

        try
        {
            using var reader = new StreamReader(path, Encoding.UTF8, false);
            _rootNode.Deserializer(reader);
            RefreshTree();
        }
        catch (Exception exception)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(exception.Message, owner: this));
        }
    }

    private void SaveBatch()
    {
        var path = ModalDialog.RunWithOwner(this, () => FileDialog.SaveFile(new SaveFileDialogOptions
        {
            Owner = this,
            Filters = FileFilter.Parse(Strings.Instance["BatchWindow_File_Filter"])
        }));
        if (path is null)
            return;

        try
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);
            _rootNode.Serialize(writer);
        }
        catch (Exception exception)
        {
            ModalDialog.RunWithOwner(this, () => MessageBox.Confirm(exception.Message, owner: this));
        }
    }

    private void ExecuteBatch()
    {
        _executeBatch(_rootNode);
    }

    private static Button ActionButton(string text, Action action, double width) => new Button()
        .Content(text)
        .Width(width)
        .FontFamily("Segoe UI")
        .FontSize(12)
        .OnClick(action);
}
