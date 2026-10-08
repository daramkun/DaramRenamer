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

    private static Element BuildLicenseContent()
    {
        var lines = new List<string>();
        using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                   "DaramRenamer.MewUI.Resources.LICENSE.md") ??
               throw new InvalidOperationException("The embedded license resource is missing."))
        using (var reader = new StreamReader(stream, Encoding.UTF8, true))
            while (reader.ReadLine() is { } line)
                lines.Add(line);

        var blocks = new List<Element>();
        var codeLines = new List<string>();
        var inCodeBlock = false;

        foreach (var line in lines)
        {
            if (line == "```")
            {
                if (inCodeBlock)
                {
                    var text = string.Join(Environment.NewLine, codeLines) + Environment.NewLine;
                    blocks.Add(new Label()
                        .Text(text)
                        .TextWrapping(TextWrapping.Wrap)
                        .FontFamily("Consolas")
                        .FontSize(14)
                        .Background(new Color(192, 192, 192))
                        .Padding(8)
                        .Margin(0, 4));
                    codeLines.Clear();
                }
                inCodeBlock = !inCodeBlock;
                continue;
            }

            if (inCodeBlock)
            {
                codeLines.Add(line);
                continue;
            }

            if (line.StartsWith("### ", StringComparison.Ordinal))
                blocks.Add(Heading(line[4..], 14, center: false));
            else if (line.StartsWith("## ", StringComparison.Ordinal))
                blocks.Add(Heading(line[3..], 18, center: false));
            else if (line.StartsWith("# ", StringComparison.Ordinal))
                blocks.Add(Heading(line[2..], 22, center: true));
            else if (line.StartsWith("- ", StringComparison.Ordinal))
                blocks.Add(new Label()
                    .Text("ㆍ " + line[2..].Trim())
                    .FontSize(10)
                    .TextWrapping(TextWrapping.Wrap));
        }

        return new ScrollViewer()
            .AutoVerticalScroll()
            .NoHorizontalScroll()
            .Content(new StackPanel()
                .Vertical()
                .Padding(16)
                .Children(blocks.ToArray()));
    }

    private static Label Heading(string text, double fontSize, bool center) => new Label()
        .Text(text)
        .FontSize(fontSize)
        .TextWrapping(TextWrapping.Wrap)
        .HorizontalAlignment(center ? HorizontalAlignment.Center : HorizontalAlignment.Stretch);
}
