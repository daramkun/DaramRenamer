using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DaramRenamer
{
	/// <summary>
	/// ApplyWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ApplyWindow : Window
	{
		private readonly UndoManager<ObservableCollection<FileInfo>> undoManager;
		private bool isComplete;

		public ApplyWindow(UndoManager<ObservableCollection<FileInfo>> undoManager)
		{
			InitializeComponent();

			this.undoManager = undoManager;
		}

		private bool isShown;
		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			if (isShown)
				return;

			isShown = true;
			OnShown (e);
		}

		private async void OnShown(EventArgs e)
		{
			try
			{
				undoManager.ClearUndoStack();

				ApplyProgressBar.Foreground = Brushes.Green;
				ApplyProgressBar.Value = 0;
				ApplyProgressBar.Maximum = FileInfo.Files.Count;

				ProceedTextBlock.Text = "0";
				TotalTextBlock.Text = FileInfo.Files.Count.ToString();

				var failed = false;
				await Task.Run(() =>
				{
					FileInfo.Apply(Preferences.Instance.AutomaticFixingFilename, Preferences.Instance.RenameMode,
						Preferences.Instance.Overwrite, (fileInfo, errorCode) =>
						{
							Dispatcher.BeginInvoke((Action) (() =>
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

				if (failed)
					ApplyProgressBar.Foreground = Brushes.Red;

				if (failed || !Preferences.Instance.AutomaticListCleaning)
					return;

				undoManager.SaveToUndoStack(FileInfo.Files);
				FileInfo.Files.Clear();
			}
			catch
			{
				MessageBox.Show("An Error occured.");
			}
			finally
			{
				isComplete = true;
				ApplyClose.IsEnabled = true;
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
}
