using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Daramkun.DaramRenamer.Processors.Extension;
using Daramkun.DaramRenamer.Processors.Filename;
using Daramkun.DaramRenamer.Processors.Number;
using Daramkun.DaramRenamer.Processors.FilePath;
using TaskDialogInterop;
using Daramkun.DaramRenamer.Processors.Date;
using Daramkun.DaramRenamer.Processors.Tag;
using System.Threading;
using System.Windows.Media;
using Daramkun.DaramRenamer.Processors;
using System.Windows.Threading;
using Daramee.DaramCommonLib;

namespace Daramkun.DaramRenamer
{
	public partial class MainWindow : Window
	{
		#region Commands
		public static RoutedCommand CommandOpenFiles = new RoutedCommand ();
		public static RoutedCommand CommandClearList = new RoutedCommand ();
		public static RoutedCommand CommandApplyFile = new RoutedCommand ();
		public static RoutedCommand CommandUndoWorks = new RoutedCommand ();
		public static RoutedCommand CommandRedoWorks = new RoutedCommand ();
		public static RoutedCommand CommandUpperItem = new RoutedCommand ();
		public static RoutedCommand CommandLowerItem = new RoutedCommand ();
		public static RoutedCommand CommandItemsSort = new RoutedCommand ();

		private void commandOpenFiles_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_Open ( sender, e ); }
		private void commandClearList_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_Clear ( sender, e ); }
		private void commandApplyFile_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_Apply ( sender, e ); }

		private void commandUndoWorks_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_Undo ( sender, e ); }
		private void commandRedoWorks_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_Redo ( sender, e ); }

		private void commandApplyCanc_Executed ( object sender, ExecutedRoutedEventArgs e )
		{ while ( !undoManager.IsUndoStackEmpty ) Menu_System_Undo ( sender, e ); }

		private void commandUpperItem_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_ItemUp ( sender, e ); }
		private void commandLowerItem_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_ItemDown ( sender, e ); }
		private void commandItemsSort_Executed ( object sender, ExecutedRoutedEventArgs e ) { Menu_System_ItemSort ( sender, e ); }
		#endregion

		public static MainWindow SharedWindow { get; private set; }

		UndoManager<ObservableCollection<FileInfo>> undoManager = new UndoManager<ObservableCollection<FileInfo>> ();
		Optionizer<SaveData> option;
		UpdateChecker updateChecker;
		
		public UndoManager<ObservableCollection<FileInfo>> UndoManager => undoManager;

		public RenameMode RenameMode { get { return option.Options.RenameMode; } set { option.Options.RenameMode = value; } }
		public bool HardwareAccelerationMode { get { return option.Options.HardwareAccelerationMode; } set { option.Options.HardwareAccelerationMode = value; } }
		public bool AutomaticFilenameFix { get { return option.Options.AutomaticFilenameFix; } set { option.Options.AutomaticFilenameFix = value; } }
		public bool AutomaticListCleaning { get { return option.Options.AutomaticListCleaning; } set { option.Options.AutomaticListCleaning = value; } }
		public bool Overwrite { get { return option.Options.Overwrite; } set { option.Options.Overwrite = value; } }

		public MainWindow ()
		{
			SharedWindow = this;

			updateChecker = new UpdateChecker ( "{0}.{1}{2}{3}" );
			
			option = new Optionizer<SaveData> ( "DARAM WORLD", "DaramRenamer" );

			InitializeComponent ();

			optionRenameMode.SelectedIndex = option.Options.RenameModeInteger;
			
			Version currentVersion = Assembly.GetEntryAssembly ().GetName ().Version;
			Title = $"{Localizer.SharedStrings [ "daram_renamer" ]} - v{currentVersion.Major}.{currentVersion.Minor}{currentVersion.Build}0";
			
			translationAuthor.Text = Localizer.SharedLocalizer.Culture.Contact != null ?
				$"{Localizer.SharedLocalizer.Culture.Author}<{Localizer.SharedLocalizer.Culture.Contact}> - {Localizer.SharedLocalizer.Culture.Culture}" :
				$"{Localizer.SharedLocalizer.Culture.Author} - {Localizer.SharedLocalizer.Culture.Culture}";

			UndoManager<ObservableCollection<FileInfo>> restored = UndoManager<ObservableCollection<FileInfo>>.Restore ();
			if ( restored != null )
			{
				undoManager = restored;
				Menu_System_Undo ( this, null );
			}

			listViewFiles.ItemsSource = FileInfo.Files;
		}

		private async void Window_Loaded ( object sender, RoutedEventArgs e )
		{
			if ( await updateChecker.CheckUpdate () == true )
			{
				Title = $"{Title} - [{Localizer.SharedStrings [ "available_update" ]}]";
			}
		}

		public static TaskDialogResult MessageBox ( string message, string content, VistaTaskDialogIcon icon, params string [] buttons )
		{
			TaskDialogOptions config = new TaskDialogOptions
			{
				Owner = null,
				Title = Localizer.SharedStrings [ "daram_renamer" ],
				MainInstruction = message,
				Content = content,
				MainIcon = icon,
				CustomButtons = buttons
			};
			return TaskDialog.Show ( config );
		}

		public void AddItem ( string s )
		{
			if ( System.IO.File.Exists ( s ) )
				FileInfo.Files.Add ( new FileInfo ( s ) );
			else
				foreach ( string ss in System.IO.Directory.GetFiles ( s, "*.*", SearchOption.AllDirectories ) )
					AddItem ( ss );
		}

		public void ShowPopup<T> ( params object [] args ) where T : IProcessor
		{
			T processor = Activator.CreateInstance<T> ();
			if ( processor is ManualEditProcessor )
			{
				( processor as ManualEditProcessor ).ChangeName = ( args [ 0 ] as FileInfo ).ChangedFilename;
				( processor as ManualEditProcessor ).ChangePath = ( args [ 0 ] as FileInfo ).ChangedPath;
				( processor as ManualEditProcessor ).ProcessingFileInfo = args [ 0 ] as FileInfo;
			}
			ISubWindow window = ( processor is BatchProcessor )
				? new SubWindow_Batch () as ISubWindow
				: new SubWindow ( processor );
			UserControl windowControl = window as UserControl;
			window.OKButtonClicked += SubWindow_OKButtonClicked;
			window.CancelButtonClicked += SubWindow_CancelButtonClicked;
			windowControl.VerticalAlignment = VerticalAlignment.Center;
			windowControl.HorizontalAlignment = HorizontalAlignment.Center;
			overlayWindowContainer.Children.Add ( windowControl );
			overlayWindowGrid.Visibility = Visibility.Visible;
		}

		public void ClosePopup ( bool apply = false )
		{
			overlayWindowGrid.Visibility = Visibility.Hidden;
			if ( apply )
			{
				undoManager.SaveToUndoStack ( FileInfo.Files );
				var processor = ( overlayWindowContainer.Children [ 0 ] as ISubWindow ).Processor;
				if ( processor is ManualEditProcessor )
				{
					processor.Process ( ( processor as ManualEditProcessor ).ProcessingFileInfo );
				}
				else
				{
					if ( !processor.CannotMultithreadProcess )
						Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => processor.Process ( fileInfo ) );
					else foreach ( var fileInfo in FileInfo.Files ) processor.Process ( fileInfo );
				}
			}
			overlayWindowContainer.Children.Clear ();
		}

		private void Item_DoubleClick ( object sender, RoutedEventArgs e )
		{
			if ( ( sender as ListViewItem ).Content == null ) return;
			FileInfo info = ( sender as ListViewItem ).Content as FileInfo;
			ShowPopup<ManualEditProcessor> ( info );
		}

		private void listViewFiles_DragEnter ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) ) e.Effects = DragDropEffects.None;
		}

		private void listViewFiles_Drop ( object sender, DragEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				undoManager.SaveToUndoStack ( FileInfo.Files );

				var temp = e.Data.GetData ( DataFormats.FileDrop ) as string [];
				foreach ( string s in from b in temp orderby b select b ) AddItem ( s );
			}
		}

		private void listViewFiles_KeyUp ( object sender, KeyEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			if ( e.Key == Key.Delete )
			{
				List<FileInfo> tempFileInfos = new List<FileInfo> ();
				foreach ( FileInfo fileInfo in listViewFiles.SelectedItems ) tempFileInfos.Add ( fileInfo );
				foreach ( FileInfo fileInfo in tempFileInfos ) FileInfo.Files.Remove ( fileInfo );
				if ( FileInfo.Files.Count == 0 ) { undoManager.ClearAll (); }
			}
		}

		private void Menu_System_Open ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
			{
				Title = Localizer.SharedStrings [ "open_files" ],
				Filter = Localizer.SharedStrings [ "all_files" ],
				Multiselect = true
			};
			if ( openFileDialog.ShowDialog () == false ) return;

			undoManager.SaveToUndoStack ( FileInfo.Files );

			foreach ( string s in from s in openFileDialog.FileNames orderby s select s )
				AddItem ( s );
		}

		private void Menu_System_Clear ( object sender, RoutedEventArgs e )
		{
			undoManager.ClearAll ();
			FileInfo.Files.Clear ();
		}

		private void Menu_System_Apply ( object sender, RoutedEventArgs e )
		{
			undoManager.ClearUndoStack ();

			progressBar.Foreground = Brushes.Green;
			progressBar.Maximum = FileInfo.Files.Count;
			progressBar.Value = 0;
			int failed = 0;
			Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) =>
			{
				if ( option.Options.AutomaticFilenameFix )
				{
					fileInfo.ReplaceInvalidPathCharacters ();
					fileInfo.ReplaceInvalidFilenameCharacters ();
				}
				ErrorCode errorMessage = ErrorCode.NoError;
				if ( option.Options.RenameMode == RenameMode.Move ) FileInfo.Move ( fileInfo, option.Options.Overwrite, out errorMessage );
				else if ( option.Options.RenameMode == RenameMode.Copy ) FileInfo.Copy ( fileInfo, option.Options.Overwrite, out errorMessage );
				Dispatcher.BeginInvoke ( ( Action ) ( () => { ++progressBar.Value; } ) );
				if ( errorMessage != ErrorCode.NoError )
					Interlocked.Increment ( ref failed );
			} );
			if ( failed != 0 )
				progressBar.Foreground = Brushes.Red;
			Application.Current.Dispatcher.Invoke ( DispatcherPriority.Background, new ThreadStart ( delegate { } ) );
			MessageBox ( Localizer.SharedStrings [ "applied" ], string.Format ( Localizer.SharedStrings [ "applied_message" ],
				progressBar.Value, progressBar.Maximum ),
				VistaTaskDialogIcon.Information, Localizer.SharedStrings [ "ok_button" ] );

			if ( failed == 0 && option.Options.AutomaticListCleaning )
			{
				undoManager.SaveToUndoStack ( FileInfo.Files );
				FileInfo.Files.Clear ();
			}
		}

		private void Menu_System_Undo ( object sender, RoutedEventArgs e )
		{
			if ( undoManager.IsUndoStackEmpty )
				return;

			undoManager.SaveToRedoStack ( FileInfo.Files );
			listViewFiles.ItemsSource = FileInfo.Files = undoManager.LoadFromUndoStack ();
		}

		private void Menu_System_Redo ( object sender, RoutedEventArgs e )
		{
			if ( undoManager.IsRedoStackEmpty )
				return;

			undoManager.SaveToUndoStack ( FileInfo.Files );
			listViewFiles.ItemsSource = FileInfo.Files = undoManager.LoadFromRedoStack ();
		}

		private void Menu_System_ItemUp ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			undoManager.SaveToUndoStack ( FileInfo.Files );
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = FileInfo.Files.IndexOf ( fileInfo );
				if ( lastIndex == 0 ) continue;
				FileInfo.Files.Move ( lastIndex, lastIndex - 1 );
			}
		}

		private void Menu_System_ItemDown ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			undoManager.SaveToUndoStack ( FileInfo.Files );
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = FileInfo.Files.IndexOf ( fileInfo );
				if ( lastIndex == FileInfo.Files.Count - 1 ) continue;
				FileInfo.Files.Move ( lastIndex, lastIndex + 1 );
			}
		}

		private void Menu_System_ItemSort ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			FileInfo.Sort ( FileInfo.Files );
		}

		private async void Menu_System_CheckUpdate ( object sender, RoutedEventArgs e )
		{
			if ( await updateChecker.CheckUpdate () == true )
			{
				if ( MessageBox ( Localizer.SharedStrings [ "update_exist" ], Localizer.SharedStrings [ "current_old" ],
								VistaTaskDialogIcon.Information, Localizer.SharedStrings [ "ok_button" ], Localizer.SharedStrings [ "download_button" ] ).
								CustomButtonResult == 1 )
					updateChecker.ShowDownloadPage ();
			}
			else
			{
				MessageBox ( Localizer.SharedStrings [ "no_update" ], Localizer.SharedStrings [ "current_stable" ],
								VistaTaskDialogIcon.Information, Localizer.SharedStrings [ "ok_button" ] );
			}
		}

		private void Menu_System_Feedback ( object sender, RoutedEventArgs e )
		{
			Process.Start ( "https://github.com/Daramkun/DaramRenamer/issues" );
		}

		private void ComboBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			option.Options.RenameModeInteger = ( sender as ComboBox ).SelectedIndex;
		}

		private void SubWindow_OKButtonClicked ( object sender, RoutedEventArgs e) { ClosePopup ( true ); }
		private void SubWindow_CancelButtonClicked ( object sender, RoutedEventArgs e ) { ClosePopup (); }

		private void ReplacePlainText_Click ( object sender, RoutedEventArgs e ) { ShowPopup<ReplacePlainProcessor> (); }
		private void ReplaceRegex_Click ( object sender, RoutedEventArgs e ) { ShowPopup<ReplaceRegexpProcessor> (); }
		private void ConcatText_Click ( object sender, RoutedEventArgs e ) { ShowPopup<ConcatenateProcessor> (); }
		private void Trimming_Click ( object sender, RoutedEventArgs e ) { ShowPopup<TrimmingProcessor> (); }
		private void DeleteBlock_Click ( object sender, RoutedEventArgs e ) { ShowPopup<DeleteBlockProcessor> (); }
		private void DeleteText_Click ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new DeleteFilenameProcessor ().Process ( fileInfo ) );
		}
		private void Substring_Click ( object sender, RoutedEventArgs e ) { ShowPopup<SubstringProcessor> (); }
		private void Castcast_Click ( object sender, RoutedEventArgs e ) { ShowPopup<CasecastProcessor> (); }

		private void AddExtension_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddExtensionProcessor> (); }
		private void AddExtensionAutomatically_Click ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new AddExtensionAutomatedProcessor ().Process ( fileInfo ) );
		}
		private void RemoveExtension_Click ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new DeleteExtensionProcessor ().Process ( fileInfo ) );
		}
		private void ChangeExtension_Click ( object sender, RoutedEventArgs e ) { ShowPopup<ReplaceExtensionProcessor> (); }
		private void CastcastExtension_Click ( object sender, RoutedEventArgs e ) { ShowPopup<CasecastExtensionProcessor> (); }

		private void DeleteWithoutNumbers_Click ( object sender, RoutedEventArgs e ) { ShowPopup<DeleteWithoutNumbersProcessor> (); }
		private void MatchingNumberCount_Click ( object sender, RoutedEventArgs e ) { ShowPopup<NumberCountMatchProcessor> (); }
		private void AddIndexNumbers_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddIndexNumberProcessor> (); }
		private void IncreaseDecreaseNumbers_Click ( object sender, RoutedEventArgs e ) { ShowPopup<IncreaseDecreaseNumbersProcessor> (); }

		private void AddDate_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddDateProcessor> (); }
		private void DeleteDate_Click ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( FileInfo.Files );
			Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new DeleteDateProcessor ().Process ( fileInfo ) );
		}
		private void IncreaseDecreaseDate_Click ( object sender, RoutedEventArgs e ) { /* TODO */ }

		private void ChangePath_Click ( object sender, RoutedEventArgs e ) { ShowPopup<ChangePathProcessor> (); }
		private void MovePathRelative_Click ( object sender, RoutedEventArgs e ) { /* TODO */ }

		private void AddMediaTag_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddMediaTagProcessor> (); }
		private void AddDocumentTag_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddDocumentTagProcessor> (); }
		private void AddFileHash_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddHashProcessor> (); }

		private void BatchProcess_Click ( object sender, RoutedEventArgs e )
		{
			ShowPopup<BatchProcessor> ();
		}

		private async void licenseTextBox_Loaded ( object sender, RoutedEventArgs e )
		{
			string downloaded = null;
			if ( File.Exists ( "DaramRenamer.License.txt" ) && ( DateTime.Today - File.GetLastWriteTime ( "DaramRenamer.License.txt" ) ).Days < 7 )
				downloaded = File.ReadAllText ( "DaramRenamer.License.txt" );
			else
			{
				WebClient client = new WebClient ();
				downloaded = await client.DownloadStringTaskAsync ( "https://raw.githubusercontent.com/daramkun/DaramRenamer/master/LICENSE" );
				File.WriteAllText ( "DaramRenamer.License.txt", downloaded );
			}
			licenseTextBox.Text = downloaded;
		}
	}
}
