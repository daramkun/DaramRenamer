using System.Collections.Concurrent;
using Aprillz.MewUI;
using Aprillz.MewUI.Controls;

namespace DaramRenamer.MewUI;

internal sealed class ApplyWindow : Window
{
    private readonly UndoManager _undoManager;
    private readonly Action _refreshMainWindow;
    private readonly ConcurrentQueue<(FileItem Item, ErrorCode Error)> _pendingProgress = new();
    private readonly List<string> _failureItems = [];
    private readonly Label _currentPath = new();
    private readonly Label _progressText = new();
    private readonly ProgressBar _progress = new();
    private readonly ListBox _failures = new();
    private readonly Button _closeButton;
    private readonly DispatcherTimer _progressTimer;
    private int _totalCount;
    private int _processedCount;
    private bool _finished;

    public ApplyWindow(UndoManager undoManager, Action refreshMainWindow)
    {
        _undoManager = undoManager;
        _refreshMainWindow = refreshMainWindow;

        this.Title(Strings.Instance["ApplyWindowTitle"]);
        this.FitContentHeight(320, 280);
        this.ShowInTaskbar(false);
        this.IsToolWindow(true);
        this.StartCenterOwner();
        this.CanClose(false);

        _progressTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100));
        _progressTimer.Tick += FlushProgress;
        this.OnClosed(_progressTimer.Dispose);

        _failures.ItemHeight(20)
            .MaxHeight(120)
            .ItemPadding(new Thickness(4, 1));

        _closeButton = new Button()
            .Content(Strings.Instance["ButtonClose"])
            .Width(64)
            .Disable()
            .OnClick(CloseWhenFinished);

        _progress.Minimum(0).Maximum(Math.Max(FileItem.Files.Count, 1)).Value(0)
            .Foreground(new Color(0, 128, 0));

        this.Content(new Grid()
            .Padding(8)
            .Rows("Auto,Auto,Auto,*,Auto")
            .Children(
                new StackPanel()
                    .Horizontal()
                    .Children(
                        new Label().Text(Strings.Instance["ApplyWindowLabelProcessing"]),
                        _currentPath)
                    .Row(0),
                new Grid()
                    .Rows("*")
                    .Children(
                        _progress,
                        _progressText.CenterHorizontal().CenterVertical())
                    .Height(16)
                    .Margin(0, 4, 0, 0)
                    .Row(1),
                _failures.Margin(0, 8, 0, 0).Row(3),
                _closeButton.CenterHorizontal().Margin(0, 8, 0, 0).Row(4)));

        this.OnLoaded(() =>
        {
            _progressTimer.Start();
            _ = RunAsync();
        });
    }

    private async Task RunAsync()
    {
        if (_finished)
            return;

        var failedItems = new ConcurrentQueue<(FileItem Item, ErrorCode Error)>();
        var oldPaths = FileItem.Files.Select(file => file.OriginalPath).ToArray();
        _totalCount = FileItem.Files.Count;
        _progressText.Text($"0/{_totalCount}");
        _undoManager.ClearUndoStack();

        try
        {
            await Task.Run(() => FileItem.Apply(
                MewUIPreferences.Instance.AutomaticFixingFilename,
                MewUIPreferences.Instance.RenameMode,
                MewUIPreferences.Instance.Overwrite,
                (item, error) =>
                {
                    _pendingProgress.Enqueue((item, error));
                    if (error != ErrorCode.NoError)
                        failedItems.Enqueue((item, error));
                }));

            FlushProgress();

            if (MewUIPreferences.Instance.RemoveEmptyDirectory)
            {
                await Task.Run(() =>
                {
                    foreach (var path in oldPaths)
                    {
                        if (GetTotalFileCount(path) != 0)
                            continue;

                        try
                        {
                            Directory.Delete(path, recursive: true);
                        }
                        catch
                        {
                            // Match the WPF apply window: empty-directory cleanup failures are ignored.
                        }
                    }
                });
            }

            if (failedItems.IsEmpty && MewUIPreferences.Instance.AutomaticListCleaning)
            {
                _undoManager.SaveToUndoStack(FileItem.Files);
                FileItem.Files.Clear();
            }
        }
        catch
        {
            MessageBox.Confirm("An Error occured.", owner: this);
        }
        finally
        {
            FlushProgress();
            _finished = true;
            _closeButton.IsEnabled = true;
            this.CanClose(true);
            _progressTimer.Stop();
            _refreshMainWindow();

            if (failedItems.IsEmpty && MewUIPreferences.Instance.CloseApplyWindowWhenSuccessfullyDone)
                Close();
        }
    }

    private void CloseWhenFinished()
    {
        if (_finished)
            Close();
    }

    private void FlushProgress()
    {
        while (_pendingProgress.TryDequeue(out var update))
        {
            ++_processedCount;
            _progress.Value(_processedCount);
            _progressText.Text($"{_processedCount}/{_totalCount}");
            _currentPath.Text(update.Item.OriginalFullPath);

            if (update.Error != ErrorCode.NoError)
            {
                _progress.Foreground(new Color(255, 0, 0));
                _failureItems.Add($"{update.Item.OriginalFilename} -> {update.Item.ChangedFilename}");
                _failures.Items(_failureItems.ToArray());
            }
        }
    }

    private static int GetTotalFileCount(string path) =>
        Directory.GetDirectories(path).Sum(GetTotalFileCount) + Directory.GetFiles(path).Length;
}
