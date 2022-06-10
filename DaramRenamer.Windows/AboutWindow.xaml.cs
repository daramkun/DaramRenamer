using System.Diagnostics;
using System.Windows;

namespace DaramRenamer;

/// <summary>
///     AboutWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionTextBlock.Text = MainWindow.GetVersionString();
        CopyrightTextBlock.Text = MainWindow.GetCopyrightString();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/daramkun/DaramRenamer",
            UseShellExecute = true
        });
    }
}