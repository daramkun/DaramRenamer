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

		ObservableCollection<FileInfo> current = new ObservableCollection<FileInfo> ();
		UndoManager<ObservableCollection<FileInfo>> undoManager = new UndoManager<ObservableCollection<FileInfo>> ();

		public ObservableCollection<FileInfo> Files { get { return current; } }

		public MainWindow ()
		{
			InitializeComponent ();

			optionRenameMode.SelectedIndex = Optionizer.SharedOptionizer.RenameModeInteger;
			
			Version currentVersion = Assembly.GetEntryAssembly ().GetName ().Version;
			Title = $"{Globalizer.Strings [ "daram_renamer" ]} - v{currentVersion.Major}.{currentVersion.Minor}{currentVersion.Build}0";
			translationAuthor.Text = Globalizer.Culture.Contact != null ? $"{Globalizer.Culture.Author}<{Globalizer.Culture.Contact}> - {Globalizer.Culture.Culture}" :
				$"{Globalizer.Culture.Author} - {Globalizer.Culture.Culture}";

			listViewFiles.ItemsSource = current;
		}

		public static TaskDialogResult MessageBox ( string message, string content, VistaTaskDialogIcon icon, params string [] buttons )
		{
			TaskDialogOptions config = new TaskDialogOptions ();
			config.Owner = null;
			config.Title = Globalizer.Strings [ "daram_renamer" ];
			config.MainInstruction = message;
			config.Content = content;
			config.MainIcon = icon;
			config.CustomButtons = buttons;
			return TaskDialog.Show ( config );
		}

		public void AddItem ( string s )
		{
			if ( System.IO.File.Exists ( s ) )
				current.Add ( new FileInfo ( s ) );
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
			var window = new SubWindow ( processor );
			window.OKButtonClicked += SubWindow_OKButtonClicked;
			window.CancelButtonClicked += SubWindow_CancelButtonClicked;
			window.VerticalAlignment = VerticalAlignment.Center;
			window.HorizontalAlignment = HorizontalAlignment.Center;
			overlayWindowContainer.Children.Add ( window );
			overlayWindowGrid.Visibility = Visibility.Visible;
		}

		public void ClosePopup ( bool apply = false )
		{
			overlayWindowGrid.Visibility = Visibility.Hidden;
			if ( apply )
			{
				undoManager.SaveToUndoStack ( current );
				var processor = ( overlayWindowContainer.Children [ 0 ] as SubWindow ).Processor;
				if ( processor is ManualEditProcessor )
				{
					processor.Process ( ( processor as ManualEditProcessor ).ProcessingFileInfo );
				}
				else
				{
					if ( !processor.CannotMultithreadProcess )
						Parallel.ForEach<FileInfo> ( current, ( fileInfo ) => processor.Process ( fileInfo ) );
					else foreach ( var fileInfo in current ) processor.Process ( fileInfo );
				}
			}
			overlayWindowContainer.Children.Clear ();
		}

		public async Task<bool?> CheckUpdate ( bool messageShow = false )
		{
			HttpWebRequest req = WebRequest.CreateHttp ( "https://github.com/Daramkun/DaramRenamer/releases" );
			HttpWebResponse res = await req.GetResponseAsync () as HttpWebResponse;
			Stream stream = null;
			string version = null;
			bool checkUpdate = false;
			try
			{
				stream = res.GetResponseStream ();
				using ( StreamReader reader = new StreamReader ( stream ) )
				{
					stream = null;
					string text = reader.ReadToEnd ();
					int begin = text.IndexOf ( "<span class=\"css-truncate-target\">" );
					if ( begin == -1 ) {  version = null; return false; };
					int end = text.IndexOf ( "</span>", begin );
					if ( end == -1 ) {  version = null; return false; };
					version = text.Substring ( end - 5, 5 );
					Version currentVersion = Assembly.GetEntryAssembly ().GetName ().Version;
					checkUpdate = version != string.Format ( "{0}.{1}{2}0", currentVersion.Major, currentVersion.Minor, currentVersion.Build );

					if ( messageShow )
					{
						if ( checkUpdate == true )
						{
							if ( MessageBox ( Globalizer.Strings [ "update_exist" ], Globalizer.Strings [ "current_old" ],
								VistaTaskDialogIcon.Information, Globalizer.Strings [ "ok_button" ], Globalizer.Strings [ "download_button" ] ).
								CustomButtonResult == 1 )
								Process.Start ( "https://github.com/Daramkun/DaramRenamer/releases" );
						}
						else
						{
							MessageBox ( Globalizer.Strings [ "no_update" ], Globalizer.Strings [ "current_stable" ],
								VistaTaskDialogIcon.Information, Globalizer.Strings [ "ok_button" ] );
						}
					}
				}
			}
			catch { version = null; return null; }
			finally { if ( stream != null ) stream.Dispose (); }

			return checkUpdate;
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
			undoManager.SaveToUndoStack ( current );
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				undoManager.SaveToUndoStack ( current );

				var temp = e.Data.GetData ( DataFormats.FileDrop ) as string [];
				foreach ( string s in from b in temp orderby b select b ) AddItem ( s );
			}
		}

		private void listViewFiles_KeyUp ( object sender, KeyEventArgs e )
		{
			undoManager.SaveToUndoStack ( current );
			if ( e.Key == Key.Delete )
			{
				List<FileInfo> tempFileInfos = new List<FileInfo> ();
				foreach ( FileInfo fileInfo in listViewFiles.SelectedItems ) tempFileInfos.Add ( fileInfo );
				foreach ( FileInfo fileInfo in tempFileInfos ) current.Remove ( fileInfo );
				if ( current.Count == 0 ) { undoManager.ClearAll (); }
			}
		}

		private void Menu_System_Open ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog ();
			openFileDialog.Title = Globalizer.Strings [ "open_files" ];
			openFileDialog.Filter = Globalizer.Strings [ "all_files" ];
			openFileDialog.Multiselect = true;
			if ( openFileDialog.ShowDialog () == false ) return;

			undoManager.SaveToUndoStack ( current );

			foreach ( string s in from s in openFileDialog.FileNames orderby s select s )
				AddItem ( s );
		}

		private void Menu_System_Clear ( object sender, RoutedEventArgs e )
		{
			undoManager.ClearAll ();
			current.Clear ();
		}

		private void Menu_System_Apply ( object sender, RoutedEventArgs e )
		{
			undoManager.ClearUndoStack ();

			progressBar.Foreground = Brushes.Green;
			progressBar.Maximum = current.Count;
			progressBar.Value = 0;
			int failed = 0;
			Parallel.ForEach<FileInfo> ( current, ( fileInfo ) =>
			{
				ErrorCode errorMessage = ErrorCode.NoError;
				if ( Optionizer.SharedOptionizer.RenameMode == RenameMode.Move ) fileInfo.Move ( Optionizer.SharedOptionizer.Overwrite, out errorMessage );
				else if ( Optionizer.SharedOptionizer.RenameMode == RenameMode.Copy ) fileInfo.Copy ( Optionizer.SharedOptionizer.Overwrite, out errorMessage );
				Dispatcher.BeginInvoke ( ( Action ) ( () => { ++progressBar.Value; } ) );
				if ( errorMessage != ErrorCode.NoError )
					Interlocked.Increment ( ref failed );
			} );
			if ( failed != 0 )
				progressBar.Foreground = Brushes.Red;
			Application.Current.Dispatcher.Invoke ( DispatcherPriority.Background, new ThreadStart ( delegate { } ) );
			MessageBox ( Globalizer.Strings [ "applied" ], string.Format ( Globalizer.Strings [ "applied_message" ],
				progressBar.Value, progressBar.Maximum ),
				VistaTaskDialogIcon.Information, Globalizer.Strings [ "ok_button" ] );
		}

		private void Menu_System_Undo ( object sender, RoutedEventArgs e )
		{
			if ( undoManager.IsUndoStackEmpty )
				return;

			undoManager.SaveToRedoStack ( current );
			listViewFiles.ItemsSource = current = undoManager.LoadFromUndoStack ();
		}

		private void Menu_System_Redo ( object sender, RoutedEventArgs e )
		{
			if ( undoManager.IsRedoStackEmpty )
				return;

			undoManager.SaveToUndoStack ( current );
			listViewFiles.ItemsSource = current = undoManager.LoadFromRedoStack ();
		}

		private void Menu_System_ItemUp ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			undoManager.SaveToUndoStack ( current );
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = current.IndexOf ( fileInfo );
				if ( lastIndex == 0 ) continue;
				current.Move ( lastIndex, lastIndex - 1 );
			}
		}

		private void Menu_System_ItemDown ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			undoManager.SaveToUndoStack ( current );
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = current.IndexOf ( fileInfo );
				if ( lastIndex == current.Count - 1 ) continue;
				current.Move ( lastIndex, lastIndex + 1 );
			}
		}

		private void Menu_System_ItemSort ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( current );
			FileInfo.Sort ( current );
		}

		private async void Menu_System_CheckUpdate ( object sender, RoutedEventArgs e ) { await CheckUpdate ( true ); }

		private void Menu_System_Feedback ( object sender, RoutedEventArgs e )
		{
			Process.Start ( "https://github.com/Daramkun/DaramRenamer/issues" );
		}

		private void ComboBox_SelectionChanged ( object sender, SelectionChangedEventArgs e )
		{
			Optionizer.SharedOptionizer.RenameModeInteger = ( sender as ComboBox ).SelectedIndex;
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
			undoManager.SaveToUndoStack ( current );
			Parallel.ForEach<FileInfo> ( current, ( fileInfo ) => new DeleteFilenameProcessor ().Process ( fileInfo ) );
		}
		private void Substring_Click ( object sender, RoutedEventArgs e ) { ShowPopup<SubstringProcessor> (); }
		private void Castcast_Click ( object sender, RoutedEventArgs e ) { ShowPopup<CasecastProcessor> (); }

		private void AddExtension_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddExtensionProcessor> (); }
		private void AddExtensionAutomatically_Click ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( current );
			Parallel.ForEach<FileInfo> ( current, ( fileInfo ) => new AddExtensionAutomatedProcessor ().Process ( fileInfo ) );
		}
		private void RemoveExtension_Click ( object sender, RoutedEventArgs e )
		{
			undoManager.SaveToUndoStack ( current );
			Parallel.ForEach<FileInfo> ( current, ( fileInfo ) => new DeleteExtensionProcessor ().Process ( fileInfo ) );
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
			undoManager.SaveToUndoStack ( current );
			Parallel.ForEach<FileInfo> ( current, ( fileInfo ) => new DeleteDateProcessor ().Process ( fileInfo ) );
		}
		private void IncreaseDecreaseDate_Click ( object sender, RoutedEventArgs e ) { /* TODO */ }

		private void ChangePath_Click ( object sender, RoutedEventArgs e ) { ShowPopup<ChangePathProcessor> (); }
		private void MovePathRelative_Click ( object sender, RoutedEventArgs e ) { /* TODO */ }

		private void AddMediaTag_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddMediaTagProcessor> (); }
		private void AddDocumentTag_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddDocumentTagProcessor> (); }
		private void AddFileHash_Click ( object sender, RoutedEventArgs e ) { ShowPopup<AddHashProcessor> (); }
	}
}
