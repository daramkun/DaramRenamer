using System.Reflection;
using System.Text;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal sealed class LicenseWindow : Window
{
    public LicenseWindow()
    {
        this.Title(Strings.Instance["LicenseWindowTitle"]);
        this.Resizable(800, 600);
        this.Content(BuildLicenseContent());
    }

    private static ScrollViewer BuildLicenseContent()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                               "DaramRenamer.MewUI.Resources.LICENSE.md") ??
                           throw new InvalidOperationException("The embedded license resource is missing.");
        using var reader = new StreamReader(stream, Encoding.UTF8, true);

        var blocks = ParseMarkdown(reader.ReadToEnd());
        return new ScrollViewer()
            .AutoVerticalScroll()
            .NoHorizontalScroll()
            .Content(new StackPanel()
                .Vertical()
                .Spacing(4)
                .Padding(16)
                .Children(blocks.ToArray()));
    }

    private static List<Element> ParseMarkdown(string markdown)
    {
        var blocks = new List<Element>();
        var paragraph = new List<string>();
        var codeParagraph = new List<string>();
        var codeBlock = new List<Element>();
        var inCodeBlock = false;

        void FlushParagraph()
        {
            if (paragraph.Count == 0)
                return;

            blocks.Add(new Label()
                .Text(string.Join(" ", paragraph.Select(line => line.Trim())))
                .TextWrapping(TextWrapping.Wrap)
                .FontFamily("Segoe UI")
                .FontSize(12));
            paragraph.Clear();
        }

        void FlushCodeParagraph()
        {
            if (codeParagraph.Count == 0)
                return;

            codeBlock.Add(new Label()
                .Text(string.Join(Environment.NewLine, codeParagraph))
                .TextWrapping(TextWrapping.Wrap)
                .FontFamily("Consolas")
                .FontSize(12));
            codeParagraph.Clear();
        }

        void FlushCodeBlock()
        {
            FlushCodeParagraph();
            if (codeBlock.Count == 0)
                return;

            blocks.Add(new Border()
                .Background(new Color(238, 238, 238))
                .Padding(8)
                .Margin(0, 4)
                .WithTheme((theme, border) =>
                {
                    border.Background(theme.IsDark ? new Color(43, 43, 43) : new Color(238, 238, 238));
                })
                .Child(new StackPanel()
                    .Vertical()
                    .Spacing(4)
                    .Children(codeBlock.ToArray())));
            codeBlock.Clear();
        }

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');
            var trimmed = line.Trim();

            if (trimmed.StartsWith("```", StringComparison.Ordinal))
            {
                if (inCodeBlock)
                {
                    FlushCodeBlock();
                    inCodeBlock = false;
                }
                else
                {
                    FlushParagraph();
                    inCodeBlock = true;
                }

                continue;
            }

            if (inCodeBlock)
            {
                if (string.IsNullOrWhiteSpace(line))
                    FlushCodeParagraph();
                else
                    codeParagraph.Add(line);
                continue;
            }

            if (trimmed is "<details>" or "</details>" ||
                (trimmed.StartsWith("<summary>", StringComparison.Ordinal) &&
                 trimmed.EndsWith("</summary>", StringComparison.Ordinal)))
            {
                FlushParagraph();
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                FlushParagraph();
                continue;
            }

            var headingLevel = GetHeadingLevel(trimmed);
            if (headingLevel > 0)
            {
                FlushParagraph();
                blocks.Add(Heading(trimmed[(headingLevel + 1)..], headingLevel));
                continue;
            }

            if (TryGetListItem(trimmed, out var marker, out var itemText))
            {
                FlushParagraph();
                blocks.Add(new Label()
                    .Text($"{marker} {itemText}")
                    .TextWrapping(TextWrapping.Wrap)
                    .FontFamily("Segoe UI")
                    .FontSize(12)
                    .Margin(16, 0, 0, 0));
                continue;
            }

            paragraph.Add(line);
        }

        if (inCodeBlock)
            FlushCodeBlock();
        FlushParagraph();
        return blocks;
    }

    private static int GetHeadingLevel(string line)
    {
        var markerCount = 0;
        while (markerCount < line.Length && markerCount < 6 && line[markerCount] == '#')
            markerCount++;

        return markerCount > 0 && markerCount < line.Length && line[markerCount] == ' '
            ? markerCount
            : 0;
    }

    private static bool TryGetListItem(string line, out string marker, out string text)
    {
        if (line.Length >= 2 && line[1] == ' ' && line[0] is '-' or '*' or '+')
        {
            marker = "•";
            text = line[2..].Trim();
            return true;
        }

        var index = 0;
        while (index < line.Length && char.IsDigit(line[index]))
            index++;

        if (index > 0 && index + 1 < line.Length && line[index] == '.' && line[index + 1] == ' ')
        {
            marker = line[..(index + 1)];
            text = line[(index + 2)..].Trim();
            return true;
        }

        marker = string.Empty;
        text = string.Empty;
        return false;
    }

    private static Label Heading(string text, int level)
    {
        var fontSize = level switch
        {
            1 => 24,
            2 => 18,
            3 => 14,
            _ => 12
        };

        return new Label()
            .Text(text)
            .TextWrapping(TextWrapping.Wrap)
            .FontFamily("Segoe UI")
            .FontSize(fontSize)
            .Bold()
            .Margin(0, level == 1 ? 0 : 8, 0, 2);
    }
}
