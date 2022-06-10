using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DaramRenamer;

/// <summary>
///     ApplyWindow.xaml에 대한 상호 작용 논리
/// </summary>
public partial class ApplyWindow : Window
{
    private readonly UndoManager undoManager;
    private bool isComplete;

    private bool isShown;

    public ApplyWindow(UndoManager undoManager)
    {
        InitializeComponent();

        this.undoManager = undoManager;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (isShown)
            return;

        isShown = true;
        OnShown(e);
    }

    private async void OnShown(EventArgs e)
    {
        var failed = false;
        try
        {
            undoManager.ClearUndoStack();

            ApplyProgressBar.Foreground = Brushes.Green;
            ApplyProgressBar.Value = 0;
            ApplyProgressBar.Maximum = FileInfo.Files.Count;

            ProceedTextBlock.Text = "0";
            TotalTextBlock.Text = FileInfo.Files.Count.ToString();

            await Task.Run(() =>
            {
                FileInfo.Apply(Preferences.Instance.AutomaticFixingFilename, Preferences.Instance.RenameMode,
                    Preferences.Instance.Overwrite, async (fileInfo, errorCode) =>
                    {
                        await Dispatcher.BeginInvoke((Action) (() =>
                        {
                            ++ApplyProgressBar.Value;

                            ProcessingTextBlock.Text = fileInfo.OriginalFullPath;
                            ProceedTextBlock.Text = ((int) ApplyProgressBar.Value).ToString();

                            if (errorCode != ErrorCode.NoError)
                                ListBoxFailure.Items.Add(
                                    $"{fileInfo.OriginalFilename} -> {fileInfo.ChangedFilename}");
                        }));

                        if (errorCode != ErrorCode.NoError)
                            failed = true;
                    });
            });

            switch (failed)
            {
                case true:
                {
                    ApplyProgressBar.Foreground = Brushes.Red;
                    break;
                }

                case false when Preferences.Instance.AutomaticListCleaning:
                {
                    undoManager.SaveToUndoStack(FileInfo.Files);
                    FileInfo.Files.Clear();
                    break;
                }
            }
        }
        catch
        {
            await Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("An Error occured.")));
        }
        finally
        {
            isComplete = true;
            ApplyClose.IsEnabled = true;

            if (!failed && Preferences.Instance.CloseApplyWindowWhenSuccessfullyDone)
                await Dispatcher.BeginInvoke(new Action(Close));
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = !isComplete;
        base.OnClosing(e);
    }

    private void ApplyClose_Click(object sender, RoutedEventArgs e)
    {
        if (!isComplete) return;
        Close();
    }
}