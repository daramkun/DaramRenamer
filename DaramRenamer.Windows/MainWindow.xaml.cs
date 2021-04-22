using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Daramee.Winston.Dialogs;
using DaramRenamer.Commands;

namespace DaramRenamer
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private readonly UndoManager _undoManager = new();
		private readonly ManualEditCommand _manualEditCommand = new();

		private BatchWindow _currentBatchWindow;

		public bool UndoManagerHasUndoStackItem => !_undoManager.IsUndoStackEmpty;
		public bool UndoManagerHasRedoStackItem => !_undoManager.IsRedoStackEmpty;

		private PreferencesWindow _preferencesWindow;

		#region Commands

		public static RoutedCommand CommandOpenFiles = new();
		public static RoutedCommand CommandClearList = new();
		public static RoutedCommand CommandApplyFile = new();
		public static RoutedCommand CommandUndoWorks = new();
		public static RoutedCommand CommandRedoWorks = new();
		public static RoutedCommand CommandUpperItem = new();
		public static RoutedCommand CommandLowerItem = new();
		public static RoutedCommand CommandItemsSort = new();

		public static RoutedCommand CommandCustom0 = new();
		public static RoutedCommand CommandCustom1 = new();
		public static RoutedCommand CommandCustom2 = new();
		public static RoutedCommand CommandCustom3 = new();
		public static RoutedCommand CommandCustom4 = new();
		public static RoutedCommand CommandCustom5 = new();
		public static RoutedCommand CommandCustom6 = new();
		public static RoutedCommand CommandCustom7 = new();
		public static RoutedCommand CommandCustom8 = new();
		public static RoutedCommand CommandCustom9 = new();

		private void OnShortcutChanged0(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding0, Preferences.Instance.Shortcut0);

		private void OnShortcutChanged1(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding1, Preferences.Instance.Shortcut1);

		private void OnShortcutChanged2(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding2, Preferences.Instance.Shortcut2);

		private void OnShortcutChanged3(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding3, Preferences.Instance.Shortcut3);

		private void OnShortcutChanged4(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding4, Preferences.Instance.Shortcut4);

		private void OnShortcutChanged5(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding5, Preferences.Instance.Shortcut5);

		private void OnShortcutChanged6(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding6, Preferences.Instance.Shortcut6);

		private void OnShortcutChanged7(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding7, Preferences.Instance.Shortcut7);

		private void OnShortcutChanged8(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding8, Preferences.Instance.Shortcut8);

		private void OnShortcutChanged9(object sender, PropertyChangedEventArgs e) =>
			OnShortcutChanged(KeyBinding9, Preferences.Instance.Shortcut9);

		private void OnShortcutChanged(KeyBinding keyBinding, KeyBindingInfo info)
		{
			keyBinding.Key = info.KeyBindingKey;
			keyBinding.Modifiers = info.KeyBindingModifierKeys;
		}

		private void CommandOpenFiles_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuFileOpen_Click(sender, e);
		}

		private void CommandClearList_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuFileClear_Click(sender, e);
		}

		private void CommandApplyFile_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuFileApply_Click(sender, e);
		}

		private void CommandUndoWorks_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuEditUndo_Click(sender, e);
		}

		private void CommandRedoWorks_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuEditRedo_Click(sender, e);
		}

		private void CommandUpperItem_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuEditItemToUp_Click(sender, e);
		}

		private void CommandLowerItem_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuEditItemToDown_Click(sender, e);
		}

		private void CommandItemsSort_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			MenuEditSort_Click(sender, e);
		}

		private void CommandCustom0_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut0.CommandObject, null);
		private void CommandCustom1_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut1.CommandObject, null);
		private void CommandCustom2_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut2.CommandObject, null);
		private void CommandCustom3_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut3.CommandObject, null);
		private void CommandCustom4_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut4.CommandObject, null);
		private void CommandCustom5_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut5.CommandObject, null);
		private void CommandCustom6_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut6.CommandObject, null);
		private void CommandCustom7_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut7.CommandObject, null);
		private void CommandCustom8_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut8.CommandObject, null);
		private void CommandCustom9_Executed(object sender, ExecutedRoutedEventArgs e) =>
			CommandMenuItem_Click(Preferences.Instance.Shortcut9.CommandObject, null);

		#endregion

		private static async Task<bool> IsAdministrator()
		{
			return await Task.Run(() => new WindowsPrincipal(WindowsIdentity.GetCurrent())
				.IsInRole(WindowsBuiltInRole.Administrator));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public MainWindow()
		{
			InitializeComponent();
			FileInfo.FileOperator = new WindowsNativeFileOperator();

			Title = $"{Strings.Instance["DaramRenamer"]} - {Strings.Instance["Version"]} {GetVersionString()}";

			if (Preferences.Instance.UseCustomPlugins)
				PluginManager.Instance.LoadPlugins();

			PluginToMenu.InitializeCommands(CommandsMenu.Items);
			PluginToMenu.InitializeCommands(ListViewContextMenu.Items);

			PluginToMenu.InitializeConditions(ConditionsMenu);

			_undoManager.UpdateUndo += (_, _) =>
			{
				OnPropertyChanged(nameof(UndoManagerHasUndoStackItem));
				OnPropertyChanged(nameof(UndoManagerHasRedoStackItem));
			};
			_undoManager.UpdateRedo += (_, _) =>
			{
				OnPropertyChanged(nameof(UndoManagerHasUndoStackItem));
				OnPropertyChanged(nameof(UndoManagerHasRedoStackItem));
			};

			ListViewFiles.ItemsSource = FileInfo.Files;

			if (Preferences.Instance.SaveWindowState)
			{
				Left = Preferences.Instance.Left;
				Top = Preferences.Instance.Top;
				Width = Preferences.Instance.Width;
				Height = Preferences.Instance.Height;
				WindowState = Preferences.Instance.WindowState;
			}

			Preferences.Instance.Shortcut0.PropertyChanged += OnShortcutChanged0;
			Preferences.Instance.Shortcut1.PropertyChanged += OnShortcutChanged1;
			Preferences.Instance.Shortcut2.PropertyChanged += OnShortcutChanged2;
			Preferences.Instance.Shortcut3.PropertyChanged += OnShortcutChanged3;
			Preferences.Instance.Shortcut4.PropertyChanged += OnShortcutChanged4;
			Preferences.Instance.Shortcut5.PropertyChanged += OnShortcutChanged5;
			Preferences.Instance.Shortcut6.PropertyChanged += OnShortcutChanged6;
			Preferences.Instance.Shortcut7.PropertyChanged += OnShortcutChanged7;
			Preferences.Instance.Shortcut8.PropertyChanged += OnShortcutChanged8;
			Preferences.Instance.Shortcut9.PropertyChanged += OnShortcutChanged9;

			OnShortcutChanged0(null, null);
			OnShortcutChanged1(null, null);
			OnShortcutChanged2(null, null);
			OnShortcutChanged3(null, null);
			OnShortcutChanged4(null, null);
			OnShortcutChanged5(null, null);
			OnShortcutChanged6(null, null);
			OnShortcutChanged7(null, null);
			OnShortcutChanged8(null, null);
			OnShortcutChanged9(null, null);
		}

		private async void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (!Environment.Is64BitProcess)
				Title += " - [32-Bit]";
			if (await IsAdministrator())
				Title = $"{Title} - [{Strings.Instance["Administrator"]}]";

			var updateInfo = await CheckUpdate();

			Title = updateInfo switch
			{
				true => $"{Title} - [{Strings.Instance["NewLatestVersionAvailable"]}]",
				null => $"{Title} - [{Strings.Instance["UpdateCheckError"]}]",
				_ => Title
			};
		}

		public async void RefreshTitle()
		{
			Title = $"{Strings.Instance["DaramRenamer"]} - {Strings.Instance["Version"]} {GetVersionString()}";

			if (!Environment.Is64BitProcess)
				Title += " - [32-Bit]";
			if (await IsAdministrator())
				Title = $"{Title} - [{Strings.Instance["Administrator"]}]";

			var updateInfo = await CheckUpdate();

			Title = updateInfo switch
			{
				true => $"{Title} - [{Strings.Instance["NewLatestVersionAvailable"]}]",
				null => $"{Title} - [{Strings.Instance["UpdateCheckError"]}]",
				_ => Title
			};
		}

		internal static async Task<bool?> CheckUpdate()
		{
			return await Task.Run<bool?>(() =>
			{
				var updateInformation = UpdateInformationBank.GetUpdateInformation(TargetPlatform.Windows);
				if (updateInformation != null)
					return updateInformation.Value.StableLatestVersion != GetVersionString();
				return null;
			});
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (Preferences.Instance.SaveWindowState)
			{
				Preferences.Instance.Left = Left;
				Preferences.Instance.Top = Top;
				Preferences.Instance.Width = Width;
				Preferences.Instance.Height = Height;
				Preferences.Instance.WindowState = WindowState;
			}
			Preferences.Instance.Save();
		}

		private void Window_Unloaded(object sender, RoutedEventArgs e)
		{
			_preferencesWindow.Close();
			_preferencesWindow = null;
		}

		internal static string GetVersionString()
		{
			var programVersion = Assembly.GetExecutingAssembly().GetName().Version;
			return programVersion != null
				? $"{programVersion.Major}.{programVersion.Minor}.{programVersion.Build}"
				: "UNKNOWN VERSION";
		}

		internal static string GetCopyrightString()
		{
			var copyrights = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
			return copyrights is {Length: > 0} ? (copyrights[0] as AssemblyCopyrightAttribute)?.Copyright ?? string.Empty : string.Empty;
		}

		public static TaskDialogResult MessageBox(string message, string content, TaskDialogIcon icon,
			TaskDialogCommonButtonFlags commonButtons, params string[] buttons)
		{
			var tdButtons = buttons != null ? TaskDialogButton.Cast(buttons) : null;

			var taskDialog = new TaskDialog
			{
				Title = Strings.Instance["DaramRenamer"],
				MainInstruction = message,
				Content = content,
				MainIcon = icon,
				CommonButtons = commonButtons,
				Buttons = tdButtons,
			};
			return taskDialog.Show(Application.Current.MainWindow);
		}

		public void AddItem(string s, bool directoryMode = false)
		{
			if (FileInfo.FileOperator.FileExists(s) || directoryMode)
			{
				var fileInfo = new FileInfo(s);
				if (!FileInfo.Files.Contains(fileInfo))
					FileInfo.Files.Add(fileInfo);
			}
			else
			{
				foreach (var ss in FileInfo.FileOperator.GetFiles(s, false))
					AddItem(ss);
			}
		}

		private void ListViewFiles_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effects = DragDropEffects.None;
		}

		private void ListViewFiles_Drop(object sender, DragEventArgs e)
		{
			if (!e.Data.GetDataPresent(DataFormats.FileDrop))
				return;

			if (!(e.Data.GetData(DataFormats.FileDrop) is string[] temp))
				return;

			var hasDirectory = false;
			foreach (var filename in temp)
			{
				if (File.GetAttributes(filename).HasFlag(FileAttributes.Directory) && filename.Length > 3)
					hasDirectory = true;
			}

			var directoryMode = false;
			if (hasDirectory)
			{
				var result = MessageBox(Strings.Instance["DragAndDrop_DirectoryQuestion"],
					Strings.Instance["DragAndDrop_DirectoryQuestionDescription"],
					TaskDialogIcon.Warning, TaskDialogCommonButtonFlags.Cancel,
					Strings.Instance["DragAndDrop_ButtonAddDirectory"],
					Strings.Instance["DragAndDrop_ButtonIterateFiles"]);
				if (result.Button == TaskDialogResult.Cancel)
					return;
				directoryMode = result.Button == 101;
			}

			_undoManager.SaveToUndoStack(FileInfo.Files);

			foreach (var s in from b in temp orderby b select b)
				AddItem(s,
					(s.Length > 3 && File.GetAttributes(s).HasFlag(FileAttributes.Directory)) && directoryMode);
		}

		private void ListViewFiles_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Delete)
				return;

			_undoManager.SaveToUndoStack(FileInfo.Files);

			foreach (var fileInfo in ListViewFiles.SelectedItems.Cast<FileInfo>().ToList())
				FileInfo.Files.Remove(fileInfo);
		}

		private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if ((sender as ListViewItem)?.Content is not FileInfo info)
				return;

			_manualEditCommand.ChangeName = info.ChangedFilename;
			_manualEditCommand.ChangePath = info.ChangedPath;

			var commandWindow = new CommandWindow(_manualEditCommand) { Owner = this };

			if (commandWindow.ShowDialog() != true)
				return;

			DoApplyCommand(_manualEditCommand, new DesignatedFilesRoutedEventArgs(info));
		}

		private void MenuFileOpen_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Title = Strings.Instance["FileDialogTitleOpenFiles"],
				Filter = Strings.Instance["FileDialogFilterAllFiles"],
				Multiselect = true
			};
			if (openFileDialog.ShowDialog() == false) return;

			_undoManager.SaveToUndoStack(FileInfo.Files);

			foreach (var s in from s in openFileDialog.FileNames orderby s select s)
				AddItem(s);
		}

		private void MenuFileFolderOpen_Click(object sender, RoutedEventArgs e)
		{
			var openFolderDialog = new OpenFolderDialog
			{
				Title = Strings.Instance["FileDialogTitleOpenFiles"],
				AllowMultiSelection = true
			};
			if (openFolderDialog.ShowDialog() == false) return;

			_undoManager.SaveToUndoStack(FileInfo.Files);

			foreach (var s in from s in openFolderDialog.FileNames orderby s select s)
				AddItem(s, true);
		}

		private void MenuFileClear_Click(object sender, RoutedEventArgs e)
		{
			_undoManager.ClearAll();
			FileInfo.Files.Clear();
		}

		private void MenuFileApply_Click(object sender, RoutedEventArgs e)
		{
			new ApplyWindow(_undoManager) { Owner = this }.ShowDialog();
		}

		private void MenuFileExit_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void MenuEditUndo_Click(object sender, RoutedEventArgs e)
		{
			if (_undoManager.IsUndoStackEmpty)
				return;

			_undoManager.SaveToRedoStack(FileInfo.Files);

			var data = _undoManager.LoadFromUndoStack() ?? throw new Exception();
			ListViewFiles.ItemsSource = FileInfo.Files = data;
		}

		private void MenuEditRedo_Click(object sender, RoutedEventArgs e)
		{
			if (_undoManager.IsRedoStackEmpty)
				return;

			_undoManager.SaveToUndoStack(FileInfo.Files, false);

			var data = _undoManager.LoadFromRedoStack() ?? throw new Exception();
			ListViewFiles.ItemsSource = FileInfo.Files = data;
		}

		private void MenuEditItemToUp_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewFiles.SelectedItems.Count == 0) return;
			_undoManager.SaveToUndoStack(FileInfo.Files);
			foreach (FileInfo fileInfo in ListViewFiles.SelectedItems)
			{
				var lastIndex = FileInfo.Files.IndexOf(fileInfo);
				if (lastIndex == 0) continue;
				FileInfo.Files.Move(lastIndex, lastIndex - 1);
			}
		}

		private void MenuEditItemToDown_Click(object sender, RoutedEventArgs e)
		{
			if (ListViewFiles.SelectedItems.Count == 0) return;
			_undoManager.SaveToUndoStack(FileInfo.Files);
			foreach (FileInfo fileInfo in ListViewFiles.SelectedItems)
			{
				var lastIndex = FileInfo.Files.IndexOf(fileInfo);
				if (lastIndex == FileInfo.Files.Count - 1) continue;
				FileInfo.Files.Move(lastIndex, lastIndex + 1);
			}
		}

		private void MenuEditSort_Click(object sender, RoutedEventArgs e)
		{
			_undoManager.SaveToUndoStack(FileInfo.Files);
			FileInfo.Sort(FileInfo.Files);
		}

		private IEnumerable<ICondition> GetActivedConditions()
		{
			foreach (ICondition condition in ConditionsMenu.ItemsSource)
			{
				var item = ConditionsMenu.ItemContainerGenerator.ContainerFromItem(condition);
				if (item == null || (item is MenuItem menuItem && menuItem.IsChecked != true))
					continue;
				yield return condition;
			}
		}

		private void CommandMenuItem_Click(object sender, RoutedEventArgs e)
		{
			ICommand command;
			if ((sender as MenuItem)?.Header is ICommand asCommandFromMenuItem)
				command = asCommandFromMenuItem;
			else if (sender is Button button)
				command = PluginManager.Instance.FindCommand(button.Tag as string);
			else if (sender is ICommand asCommand)
				command = asCommand;
			else
				return;

			if (command == null)
				return;

			command = Activator.CreateInstance(command.GetType()) as ICommand;

			var currentChanges = _undoManager.SaveTemporary(FileInfo.Files);

			var properties = command?.GetType().GetProperties();
			if (properties?.Length > (command is IOrderBy ? 3 : 2))
			{
				var commandWindow = new CommandWindow(command) { Owner = this };
				commandWindow.ValueChanged += (_, _) =>
				{
					if (Preferences.Instance.VisualCommand)
					{
						FileInfo.Files = _undoManager.LoadTemporary(currentChanges);
						DoApplyCommand(command, e);
						ListViewFiles.ItemsSource = FileInfo.Files;
					}
				};

				try
				{
					if (commandWindow.ShowDialog() != true)
						return;
				}
				finally
				{
					ListViewFiles.ItemsSource = FileInfo.Files = _undoManager.LoadTemporary(currentChanges);
				}
			}

			_undoManager.SaveToUndoStack(FileInfo.Files);

			DoApplyCommand(command, e);

			ListViewFiles.ItemsSource = FileInfo.Files;
		}

		private void DoApplyCommand(ICommand command, RoutedEventArgs e)
		{
			var conditions = GetActivedConditions().ToArray();

			{
				var targets =
					(e is DesignatedFilesRoutedEventArgs designatedFilesRoutedEventArgs
						? designatedFilesRoutedEventArgs.DesignatedFiles
						: FileInfo.Files as IEnumerable<FileInfo>).ToArray();

				if (command is ITargetContains targetContains)
					targetContains.SetTargets(targets);

				if (command.ParallelProcessable && !Preferences.Instance.ForceSingleCoreRunning)
					Parallel.ForEach(targets, file =>
					{
						if (conditions.All(condition => condition.IsSatisfyThisCondition(file)))
							command.DoCommand(file);
					});
				else
					foreach (var file in targets)
						if (conditions.All(condition => condition.IsSatisfyThisCondition(file)))
							command.DoCommand(file);
			}
		}

		private void ConditionMenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (!((MenuItem)sender).IsChecked)
				return;

			if (((MenuItem)sender).Header is not ICondition condition)
				return;

			var properties = condition.GetType().GetProperties();
			if (properties.Length <= (condition is IOrderBy ? 1 : 0))
				return;

			var commandWindow = new CommandWindow(condition) { Owner = this };
			if (commandWindow.ShowDialog() != true)
				((MenuItem)sender).IsChecked = false;
		}

		private class DesignatedFilesRoutedEventArgs : RoutedEventArgs
		{
			public FileInfo[] DesignatedFiles { get; }

			public DesignatedFilesRoutedEventArgs(params FileInfo[] files)
			{
				DesignatedFiles = files;
			}
		}

		private void MenuItemPreferences_Click(object sender, RoutedEventArgs e)
		{
			_preferencesWindow ??= new PreferencesWindow();
			if (_preferencesWindow.IsActive)
				return;

			_preferencesWindow.Owner = this;
			_preferencesWindow.Show();
		}

		private async void MenuToolsCheckUpdate_Click(object sender, RoutedEventArgs e)
		{
			var update = await CheckUpdate();
			if (update == true)
			{
				var result = MessageBox(Strings.Instance["NewLatestVersionAvailableText"], "",
					TaskDialogIcon.Information,
					TaskDialogCommonButtonFlags.OK, Strings.Instance["ButtonUpdate"]);

				if (result.Button == 101)
				{
					var updateInfo = UpdateInformationBank.GetUpdateInformation(TargetPlatform.Windows);
					if (updateInfo != null)
					{
						try
						{
							var psInfo = new ProcessStartInfo("cmd")
							{
								Arguments =
									$"/C start DaramRenamer.UpdateAgent.exe {updateInfo.Value.StableLatestVersion} {updateInfo.Value.StableLatestUrl} {Process.GetCurrentProcess().Id}",
								UseShellExecute = true,
							};
							Process.Start(psInfo);
							Application.Current.Shutdown();
						}
						catch
						{
							var psInfo = new ProcessStartInfo("https://github.com/daramkun/DaramRenamer/releases")
							{
								UseShellExecute = true,
							};
							Process.Start(psInfo);
						}
					}
					else
					{

					}
				}
			}
			else
			{
				MessageBox(Strings.Instance[update == false ? "ThisIsLatestVersionText" : "UpdateCheckErrorText"], "",
					update == null ? TaskDialogIcon.Error : null, TaskDialogCommonButtonFlags.OK);
			}
		}

		private void MenuHelpLicenses_Click(object sender, RoutedEventArgs e)
		{
			new LicenseWindow() { Owner = this }.ShowDialog();
		}

		private void MenuHelpAbout_Click(object sender, RoutedEventArgs e)
		{
			new AboutWindow() { Owner = this }.ShowDialog();
		}

		private void MenuItemBatch_Click(object sender, RoutedEventArgs e)
		{
			if (_currentBatchWindow is null or {IsVisible: false})
				_currentBatchWindow = new BatchWindow(this);

			_currentBatchWindow.Show();
		}
	}
}
