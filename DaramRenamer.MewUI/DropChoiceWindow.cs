using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal sealed class DropChoiceWindow : Window
{
    public int SelectedChoice { get; private set; } = -1;

    public DropChoiceWindow(
        string title,
        string description,
        IReadOnlyList<string> choices)
    {
        this.Title(Strings.Instance["DaramRenamer"]);
        this.Fixed(500, 172);
        this.ShowInTaskbar(false);
        this.IsToolWindow(true);
        this.StartCenterOwner();
        this.OnPreviewKeyDown(args =>
        {
            if (args.Key == Key.Escape)
            {
                CloseWithChoice(-1);
                args.Handled = true;
            }
            else if (args.Key == Key.Enter)
            {
                CloseWithChoice(0);
                args.Handled = true;
            }
        });
        var buttons = choices.Select((choice, index) =>
            new Button()
                .Content(choice)
                .Width(138)
                .Height(26)
                .Margin(index == 0 ? 0 : 6, 0, 0, 0)
                .OnClick(() => CloseWithChoice(index)))
            .Append(new Button()
                .Content(Strings.Instance["ButtonCancel"])
                .Width(84)
                .Height(26)
                .Margin(6, 0, 0, 0)
                .OnClick(() => CloseWithChoice(-1)))
            .ToArray();

        this.Content(new Grid()
            .Rows("Auto,*,Auto")
            .Padding(12)
            .Children(
                new Label()
                    .Text(title)
                    .FontSize(14)
                    .Bold()
                    .Margin(0, 0, 0, 6)
                    .Row(0),
                new Label()
                    .Text(description)
                    .Row(1),
                new StackPanel()
                    .Horizontal()
                    .Right()
                    .Children(buttons)
                    .Margin(0, 8, 0, 0)
                    .Row(2)));
    }

    private void CloseWithChoice(int choice)
    {
        SelectedChoice = choice;
        Close();
    }
}