using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace DaramRenamer;

/// <summary>
///     LicenseWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class LicenseWindow : Window
{
    public LicenseWindow()
    {
        InitializeComponent();

        var isCodeMode = false;
        var codeStore = new StringBuilder();

        var document = new FlowDocument
        {
            PagePadding = new Thickness(16, 16, 16, 16)
        };

        using var licenseStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("DaramRenamer.Resources.LICENSE.md");
        using var licenseStreamReader = new StreamReader(licenseStream!, Encoding.UTF8, true, 4096, true);

        string line;
        while ((line = licenseStreamReader.ReadLine()) != null)
            if (line != "```" && isCodeMode)
            {
                codeStore.Append(line).Append('\n');
            }
            else if (line.Length > 0 && line[0] == '#')
            {
                if (line.Length > 1 && line[1] == '#')
                    document.Blocks.Add(new Paragraph(new Run(line.Substring(2).Trim()))
                    {
                        FontSize = 18
                    });
                else
                    document.Blocks.Add(new Paragraph(new Run(line.Substring(1).Trim()))
                    {
                        FontSize = 22,
                        TextAlignment = TextAlignment.Center
                    });
            }
            else if (line == "```")
            {
                isCodeMode = !isCodeMode;
                if (!isCodeMode)
                {
                    var table = new Table();
                    var rowGroup = new TableRowGroup();
                    var row = new TableRow();
                    table.RowGroups.Add(rowGroup);
                    table.RowGroups[0].Rows.Add(row);
                    row.Background = new SolidColorBrush(Colors.Silver);
                    row.FontSize = 14;
                    row.FontFamily = new FontFamily("Consolas");
                    row.Cells.Add(new TableCell(new Paragraph(new Run(codeStore.ToString())))
                    {
                        Padding = new Thickness(8, 8, 8, 8)
                    });
                    document.Blocks.Add(table);
                }

                codeStore.Clear();
            }

        LicenseTextBox.Document = document;
    }
}