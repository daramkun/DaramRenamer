using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DaramRenamer.Registry;

namespace DaramRenamer.Avalonia;

internal sealed class BatchWindow : Window
{
    private readonly RootBatchNode _rootNode = new();
    private readonly ListBox _nodes = new();

    public BatchWindow()
    {
        var look = NativeTheme.Current;
        Title = Strings.Instance["BatchWindow_Title"];
        Width = 820;
        Height = 520;
        MinWidth = 640;
        MinHeight = 420;
        Background = look.AppBackground;

        BuildUi();
        RefreshTree();
    }

    private void BuildUi()
    {
        var look = NativeTheme.Current;
        var root = new Grid
        {
            Margin = look.ContentPadding,
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        _nodes.Background = look.Surface;
        _nodes.BorderBrush = Brushes.Transparent;
        _nodes.ItemTemplate = new FuncDataTemplate<BatchNodeItem>((item, _) => new TextBlock
        {
            Text = item?.Node.ToString() ?? string.Empty,
            Margin = new Thickness((item?.Depth ?? 0) * 18 + 8, 5, 8, 5),
            Foreground = look.Text,
            TextTrimming = TextTrimming.CharacterEllipsis
        }, true);

        var nodesPanel = new Border
        {
            Background = look.Surface,
            BorderBrush = look.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = look.SurfaceRadius,
            ClipToBounds = true,
            Child = _nodes
        };
        Grid.SetColumn(nodesPanel, 0);
        Grid.SetRow(nodesPanel, 0);
        root.Children.Add(nodesPanel);

        var side = new StackPanel
        {
            Width = look.IsMacOS ? 240 : 224,
            Spacing = look.IsMacOS ? 7 : 6,
            Margin = new Thickness(12, 10)
        };
        side.Children.Add(new TextBlock
        {
            Text = Strings.Instance["Menu_Command_Command"],
            FontWeight = FontWeight.SemiBold,
            Foreground = look.Text
        });
        foreach (var descriptor in DaramRenamerRegistry.Commands
                     .Where(command => command.Category != CommandCategory.NoCategorized)
                     .OrderBy(command => command.Category)
                     .ThenBy(command => command.Order))
        {
            var button = SideButton(Strings.Instance[descriptor.LocalizationKey]);
            button.Click += async (_, _) => await AddCommand(descriptor);
            side.Children.Add(button);
        }

        side.Children.Add(new Separator());
        side.Children.Add(new TextBlock
        {
            Text = Strings.Instance["Menu_Condition_Condition"],
            FontWeight = FontWeight.SemiBold,
            Foreground = look.Text
        });
        foreach (var descriptor in DaramRenamerRegistry.Conditions.OrderBy(condition => condition.Order))
        {
            var button = SideButton(Strings.Instance[descriptor.LocalizationKey]);
            button.Click += async (_, _) => await AddCondition(descriptor);
            side.Children.Add(button);
        }

        var sideScroll = new ScrollViewer
        {
            Content = side,
            Background = look.SurfaceElevated
        };
        var sidePanel = new Border
        {
            Background = look.SurfaceElevated,
            BorderBrush = look.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = look.SurfaceRadius,
            ClipToBounds = true,
            Child = sideScroll
        };
        Grid.SetColumn(sidePanel, 1);
        Grid.SetRow(sidePanel, 0);
        root.Children.Add(sidePanel);

        var bottom = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 6,
            Margin = new Thickness(0, 10, 0, 0)
        };
        bottom.Children.Add(Button(Strings.Instance["BatchWindow_LoadFile"], async () => await Load()));
        bottom.Children.Add(Button(Strings.Instance["BatchWindow_SaveFile"], async () => await Save()));
        bottom.Children.Add(Button("Up", () => MoveSelected(-1)));
        bottom.Children.Add(Button("Down", () => MoveSelected(1)));
        bottom.Children.Add(Button("Indent", IndentSelected));
        bottom.Children.Add(Button("Outdent", OutdentSelected));
        bottom.Children.Add(Button(Strings.Instance["BatchWindow_Remove"], RemoveSelected));
        bottom.Children.Add(Button(Strings.Instance["BatchWindow_DoBatch"], Execute));
        bottom.Children.Add(Button(Strings.Instance["BatchWindow_Close"], Close));
        Grid.SetColumnSpan(bottom, 2);
        Grid.SetRow(bottom, 1);
        root.Children.Add(bottom);

        Content = root;
    }

    private async Task AddCommand(CommandDescriptor descriptor)
    {
        var command = descriptor.Create();
        if (descriptor.Options.Count > 0)
        {
            var dialog = new OptionDialog(descriptor, command) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            if (await dialog.ShowDialog<bool>(this) != true)
                return;
        }

        AddNode(new BatchNode { Command = command });
    }

    private async Task AddCondition(ConditionDescriptor descriptor)
    {
        var condition = descriptor.Create();
        if (descriptor.Options.Count > 0)
        {
            var dialog = new OptionDialog(descriptor, condition) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
            if (await dialog.ShowDialog<bool>(this) != true)
                return;
        }

        AddNode(new BatchNode { Condition = condition });
    }

    private void AddNode(BatchNode node)
    {
        var parent = _nodes.SelectedItem is BatchNodeItem item ? item.Node : _rootNode;
        parent.Children.Add(node);
        RefreshTree();
    }

    private void RemoveSelected()
    {
        if (_nodes.SelectedItem is not BatchNodeItem item || item.Node == _rootNode)
            return;

        DeleteItem(_rootNode, item.Node);
        RefreshTree();
    }

    private static bool DeleteItem(BatchNode from, BatchNode target) =>
        from.Children.Remove(target) || from.Children.Any(child => DeleteItem(child, target));

    private void MoveSelected(int direction)
    {
        if (_nodes.SelectedItem is not BatchNodeItem item || item.Parent == null)
            return;

        var index = item.Parent.Children.IndexOf(item.Node);
        var newIndex = index + direction;
        if (index < 0 || newIndex < 0 || newIndex >= item.Parent.Children.Count)
            return;

        item.Parent.Children.RemoveAt(index);
        item.Parent.Children.Insert(newIndex, item.Node);
        RefreshTree(item.Node);
    }

    private void IndentSelected()
    {
        if (_nodes.SelectedItem is not BatchNodeItem item || item.Parent == null)
            return;

        var index = item.Parent.Children.IndexOf(item.Node);
        if (index <= 0)
            return;

        var newParent = item.Parent.Children[index - 1];
        item.Parent.Children.RemoveAt(index);
        newParent.Children.Add(item.Node);
        RefreshTree(item.Node);
    }

    private void OutdentSelected()
    {
        if (_nodes.SelectedItem is not BatchNodeItem item || item.Parent is null or RootBatchNode)
            return;

        if (!TryFindParent(_rootNode, item.Parent, out var grandParent))
            return;

        var parentIndex = grandParent.Children.IndexOf(item.Parent);
        item.Parent.Children.Remove(item.Node);
        grandParent.Children.Insert(parentIndex + 1, item.Node);
        RefreshTree(item.Node);
    }

    private static bool TryFindParent(BatchNode current, BatchNode target, out BatchNode parent)
    {
        foreach (var child in current.Children)
        {
            if (child == target)
            {
                parent = current;
                return true;
            }

            if (TryFindParent(child, target, out parent))
                return true;
        }

        parent = null!;
        return false;
    }

    private void Execute()
    {
        var index = 0;
        foreach (var item in FileItem.Files)
            _rootNode.Execute(item, index++);
    }

    private async Task Load()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("DaramRenamer Batch") { Patterns = ["*.drb"] }]
        });
        if (files.Count == 0 || files[0].TryGetLocalPath() is not { } path)
            return;

        await using var stream = File.OpenRead(path);
        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        _rootNode.Deserializer(reader);
        RefreshTree();
    }

    private async Task Save()
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            DefaultExtension = "drb",
            FileTypeChoices = [new FilePickerFileType("DaramRenamer Batch") { Patterns = ["*.drb"] }]
        });
        if (file?.TryGetLocalPath() is not { } path)
            return;

        await using var stream = File.Create(path);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        _rootNode.Serialize(writer);
    }

    private void RefreshTree()
    {
        _nodes.ItemsSource = null;
        _nodes.ItemsSource = Flatten(_rootNode, null, 0).ToArray();
    }

    private void RefreshTree(BatchNode selected)
    {
        var items = Flatten(_rootNode, null, 0).ToArray();
        _nodes.ItemsSource = null;
        _nodes.ItemsSource = items;
        _nodes.SelectedItem = items.FirstOrDefault(item => item.Node == selected);
    }

    private static IEnumerable<BatchNodeItem> Flatten(BatchNode node, BatchNode? parent, int depth)
    {
        yield return new BatchNodeItem(node, parent, depth);
        foreach (var child in node.Children)
        foreach (var item in Flatten(child, node, depth + 1))
            yield return item;
    }

    private sealed record BatchNodeItem(BatchNode Node, BatchNode? Parent, int Depth)
    {
        public override string ToString() => $"{new string(' ', Depth * 4)}{Node}";
    }

    private static Button Button(string text, Action click)
    {
        var button = new Button { Content = text, MinWidth = 84 };
        button.Click += (_, _) => click();
        return button;
    }

    private static Button Button(string text, Func<Task> click)
    {
        var button = new Button { Content = text, MinWidth = 84 };
        button.Click += async (_, _) => await click();
        return button;
    }

    private static Button SideButton(string text) =>
        new()
        {
            Content = text,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            MinHeight = NativeTheme.Current.IsMacOS ? 30 : 28,
            Padding = new Thickness(10, 4),
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            CornerRadius = NativeTheme.Current.ControlRadius
        };
}
