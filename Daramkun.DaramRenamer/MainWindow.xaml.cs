using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Daramkun.DaramRenamer.Properties;
using TaskDialogInterop;

namespace Daramkun.DaramRenamer
{
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window
	{
		BinaryFormatter bf;
		//XmlSerializer xs;
		ObservableCollection<FileInfo> fileInfoCollection;
		Stack<byte []> undoStack;
		Stack<byte []> redoStack;

		#region Constructor
		public MainWindow ()
		{
			InitializeComponent ();

			Version currentVersion = Assembly.GetEntryAssembly ().GetName ().Version;
			Title = string.Format ( "{0} - v{1}.{2}{3}0", Daramkun.DaramRenamer.Properties.Resources.DaramRenamer, 
				currentVersion.Major, currentVersion.Minor, currentVersion.Build );
			
			bf = new BinaryFormatter ();
			//xs = new XmlSerializer ( typeof ( ObservableCollection<FileInfo> ) );
			fileInfoCollection = new ObservableCollection<FileInfo> ();
			listViewFiles.ItemsSource = fileInfoCollection;

			undoStack = new Stack<byte []> ();
			redoStack = new Stack<byte []> ();
		}
		#endregion

		#region Window Subclasses
		protected override void OnClosed ( EventArgs e )
		{
			Settings.Default.Save ();
			base.OnClosed ( e );
		}

		protected override void OnActivated ( EventArgs e )
		{
			if ( Settings.Default.AutoRemoveTurnOn )
			{
				SaveCurrentStateToUndoStack ();
				Parallel.ForEach ( fileInfoCollection.ToArray (), ( fileInfo ) =>
				{
					try
					{
						if ( !File.Exists ( System.IO.Path.Combine ( fileInfo.OriginalPath, fileInfo.OriginalName ) ) )
							fileInfoCollection.Remove ( fileInfo );
					}
					catch { }
				} );
			}
			base.OnActivated ( e );
		}
		#endregion

		#region Shortcut Keys
		#region Commands
		public static RoutedCommand CommandOpenFiles = new RoutedCommand ();
		public static RoutedCommand CommandClearList = new RoutedCommand ();
		public static RoutedCommand CommandApplyFile = new RoutedCommand ();
		public static RoutedCommand CommandUndoWorks = new RoutedCommand ();
		public static RoutedCommand CommandRedoWorks = new RoutedCommand ();
		public static RoutedCommand CommandApplyCanc = new RoutedCommand ();
		public static RoutedCommand CommandUpperItem = new RoutedCommand ();
		public static RoutedCommand CommandLowerItem = new RoutedCommand ();
		public static RoutedCommand CommandItemsSort = new RoutedCommand ();
		#endregion
		#region Executes
		private void commandOpenFiles_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_Open_Click ( sender, e );
		}

		private void commandClearList_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_DeleteAll_Click ( sender, e );
		}

		private void commandApplyFile_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_Apply_Click ( sender, e );
		}

		private void commandUndoWorks_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_Undo_Click ( sender, e );
		}

		private void commandRedoWorks_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_Redo_Click ( sender, e );
		}

		private void commandApplyCanc_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_Restore_Click ( sender, e );
		}

		private void commandUpperItem_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_UpTo_Click ( sender, e );
		}

		private void commandLowerItem_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_DownTo_Click ( sender, e );
		}

		private void commandItemsSort_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			ToolBarButton_Sort_Click ( sender, e );
		}
		#endregion
		#endregion

		#region Undo and Redo
		private void SaveCurrentStateToUndoStack ()
		{
			if ( undoStack.Count == 0 && fileInfoCollection.Count == 0 ) return;
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection );
				undoStack.Push ( memStream.ToArray () );
			}
		}

		private void SaveCurrentStateToRedoStack ()
		{
			using ( MemoryStream memStream = new MemoryStream () )
			{
				bf.Serialize ( memStream, fileInfoCollection );
				redoStack.Push ( memStream.ToArray () );
			}
		}

		private bool RepairFromUndoStack ()
		{
			if ( undoStack.Count == 0 ) return false;
			SaveCurrentStateToRedoStack ();
			using ( MemoryStream memStream = new MemoryStream ( undoStack.Pop () ) )
			{
				fileInfoCollection = bf.Deserialize ( memStream ) as ObservableCollection<FileInfo>;
				listViewFiles.ItemsSource = fileInfoCollection;
			}
			return true;
		}

		private bool RepairFromRedoStack ()
		{
			if ( redoStack.Count == 0 ) return false;
			SaveCurrentStateToUndoStack ();
			using ( MemoryStream memStream = new MemoryStream ( redoStack.Pop () ) )
			{
				fileInfoCollection = bf.Deserialize ( memStream ) as ObservableCollection<FileInfo>;
				listViewFiles.ItemsSource = fileInfoCollection;
			}
			return true;
		}
		#endregion

		#region Utilities
		public void AddItem ( string s )
		{
			if ( System.IO.File.Exists ( s ) )
			{
				string filename = System.IO.Path.GetFileName ( s );
				string path = System.IO.Path.GetDirectoryName ( s );
				fileInfoCollection.Add ( new FileInfo ()
				{
					OriginalName = filename.Clone () as string,
					ChangedName = filename.Clone () as string,
					OriginalPath = path.Clone () as string,
					ChangedPath = path.Clone () as string,
				} );
			}
			else
			{
				foreach ( string ss in System.IO.Directory.GetFiles ( s ) )
					AddItem ( ss );
			}
		}

		public static void SimpleErrorMessage ( string message )
		{
			TaskDialogOptions config = new TaskDialogOptions ();
			config.Owner = null;
			config.Title = Daramkun.DaramRenamer.Properties.Resources.DaramRenamer;
			config.MainInstruction =  Daramkun.DaramRenamer.Properties.Resources.ErrorRaised;
			config.Content = message;
			config.MainIcon = VistaTaskDialogIcon.Error;
			config.CustomButtons = new [] { Daramkun.DaramRenamer.Properties.Resources.OK };
			TaskDialog.Show ( config );
		}
		#endregion

		#region ToolBar Button Events
		private void ToolBarButton_Open_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog ();
			openFileDialog.Title = Daramkun.DaramRenamer.Properties.Resources.Open;
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = Daramkun.DaramRenamer.Properties.Resources.AllFiles;
			if ( openFileDialog.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			foreach ( string s in from s in openFileDialog.FileNames orderby s select s )
				AddItem ( s );
		}

		private void ToolBarButton_DeleteAll_Click ( object sender, RoutedEventArgs e )
		{
			undoStack.Clear ();
			redoStack.Clear ();
			fileInfoCollection.Clear ();
		}

		private void ToolBarButton_Apply_Click ( object sender, RoutedEventArgs e )
		{
			TaskDialogOptions config = new TaskDialogOptions ();
			config.Owner = this;
			config.Title = Daramkun.DaramRenamer.Properties.Resources.DaramRenamer;
			config.MainInstruction = Daramkun.DaramRenamer.Properties.Resources.AskApplyWorked;
			config.Content = Daramkun.DaramRenamer.Properties.Resources.ApplyWorked;
			config.MainIcon = VistaTaskDialogIcon.Warning;
			config.CustomButtons = new [] { Daramkun.DaramRenamer.Properties.Resources.Yes, Daramkun.DaramRenamer.Properties.Resources.No };
			if ( TaskDialog.Show ( config ).CustomButtonResult != 0 )
				return;

			SaveCurrentStateToUndoStack ();

			uint total = 0, succeed = 0;
			foreach ( var fileInfo in fileInfoCollection )
			{
				try
				{
					++total;
					if ( !Settings.Default.FileCopyWhenApply ) fileInfo.ToMove ();
					else fileInfo.ToCopy ();
					++succeed;
				}
				catch ( UnauthorizedAccessException ex )
				{
					SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_NoAuthentication, fileInfo.OriginalName ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( PathTooLongException ex )
				{
					SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_PathIsTooLong, fileInfo.OriginalName ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( DirectoryNotFoundException ex )
				{
					SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_NoPath, fileInfo.OriginalName ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( IOException ex )
				{
					SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_IOException, fileInfo.OriginalName ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( Exception ex )
				{
					SimpleErrorMessage ( string.Format ( Daramkun.DaramRenamer.Properties.Resources.PathError_Unknown, fileInfo.OriginalName ) );
					Debug.WriteLine ( ex.Message );
				}
			}

			config = new TaskDialogOptions ();
			config.Owner = this;
			config.Title = Daramkun.DaramRenamer.Properties.Resources.DaramRenamer;
			config.MainInstruction = Daramkun.DaramRenamer.Properties.Resources.ApplyComplete;
			config.Content = string.Format ( Daramkun.DaramRenamer.Properties.Resources.ThatFilesNameChanged, succeed, total );
			config.MainIcon = VistaTaskDialogIcon.Information;
			config.CustomButtons = new [] { Daramkun.DaramRenamer.Properties.Resources.OK };
			TaskDialog.Show ( config );
		}

		private void ToolBarButton_Undo_Click ( object sender, RoutedEventArgs e ) { RepairFromUndoStack (); }
		private void ToolBarButton_Redo_Click ( object sender, RoutedEventArgs e ) { RepairFromRedoStack (); }
		private void ToolBarButton_Restore_Click ( object sender, RoutedEventArgs e ) { while ( RepairFromUndoStack () ) ; }

		private void ToolBarButton_UpTo_Click ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			SaveCurrentStateToUndoStack ();
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = fileInfoCollection.IndexOf ( fileInfo );
				if ( lastIndex == 0 ) continue;
				fileInfoCollection.Move ( lastIndex, lastIndex - 1 );
			}
		}

		private void ToolBarButton_DownTo_Click ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			SaveCurrentStateToUndoStack ();
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = fileInfoCollection.IndexOf ( fileInfo );
				if ( lastIndex == fileInfoCollection.Count - 1 ) continue;
				fileInfoCollection.Move ( lastIndex, lastIndex + 1 );
			}
		}

		private void ToolBarButton_Sort_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();
			FileInfo.Sort ( fileInfoCollection );
		}

		private void ToolBarButton_CheckUpdate_Click ( object sender, RoutedEventArgs e )
		{
			HttpWebRequest req = HttpWebRequest.CreateHttp ( "http://daram.pe.kr/2014/07/다람-리네이머/" );
			HttpWebResponse res = req.GetResponse () as HttpWebResponse;
			Stream stream = null;
			try
			{
				stream = res.GetResponseStream ();
				using ( StreamReader reader = new StreamReader ( stream ) )
				{
					stream = null;
					string text = reader.ReadToEnd ();
					int begin = text.IndexOf ( "<p>다운로드: <a href=\"" );
					if ( begin == -1 ) { SimpleErrorMessage ( Daramkun.DaramRenamer.Properties.Resources.CannotCheckUpdate ); return; };
					int end = text.IndexOf ( "</a></p>", begin );
					if ( end == -1 ) { SimpleErrorMessage ( Daramkun.DaramRenamer.Properties.Resources.CannotCheckUpdate ); return; };
					text = text.Substring ( end - 5, 5 );
					Version currentVersion = Assembly.GetEntryAssembly ().GetName ().Version;
					if ( text != string.Format ( "{0}.{1}{2}0", currentVersion.Major, currentVersion.Minor, currentVersion.Build ) )
					{
						TaskDialogOptions config = new TaskDialogOptions ();
						config.Title = Daramkun.DaramRenamer.Properties.Resources.DaramRenamer;
						config.MainInstruction = Daramkun.DaramRenamer.Properties.Resources.ThereIsUpdate;
						config.Content = string.Format ( Daramkun.DaramRenamer.Properties.Resources.NoticeUpdate, text );
						config.MainIcon = VistaTaskDialogIcon.Information;
						config.CustomButtons = new [] { Daramkun.DaramRenamer.Properties.Resources.OK, Daramkun.DaramRenamer.Properties.Resources.ToHomepage };
						if ( TaskDialog.Show ( config ).CustomButtonResult == 1 )
							Process.Start ( "http://daram.pe.kr/2014/07/다람-리네이머/" );
					}
					else
					{
						TaskDialogOptions config = new TaskDialogOptions ();
						config.Title = Daramkun.DaramRenamer.Properties.Resources.DaramRenamer;
						config.MainInstruction = Daramkun.DaramRenamer.Properties.Resources.NoUpdate;
						config.Content = Daramkun.DaramRenamer.Properties.Resources.ThisIsStable;
						config.MainIcon = VistaTaskDialogIcon.Information;
						config.CustomButtons = new [] { Daramkun.DaramRenamer.Properties.Resources.OK };
						TaskDialog.Show ( config );
					}
				}
			}
			finally { if ( stream != null ) stream.Dispose (); }
		}

		private void ToolBarButton_IssueTracker_Click ( object sender, RoutedEventArgs e )
		{
			Process.Start ( "https://github.com/Daramkun/DaramRenamer/issues" );
		}

		private void ToolBarButton_Donation_Click ( object sender, RoutedEventArgs e )
		{
			Process.Start ( "https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=K96K9B2GBKJVA&lc=KR&item_name=DARAM%20WORLD&currency_code=USD&bn=PP%2dDonationsBF%3ax%2dclick%2dbut21%2egif%3aNonHosted" );
		}
		#endregion

		#region ListView Subclasses
		private void ListView_DragEnter ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) ) e.Effects = DragDropEffects.None;
		}

		private void ListView_Drop ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				SaveCurrentStateToUndoStack ();
				
				var temp = e.Data.GetData ( DataFormats.FileDrop ) as string [];
				foreach ( string s in from b in temp orderby b select b ) AddItem ( s );
			}
		}

		private void ListView_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Delete )
			{
				List<FileInfo> tempFileInfos = new List<FileInfo> ();
				foreach ( FileInfo fileInfo in listViewFiles.SelectedItems ) tempFileInfos.Add ( fileInfo );
				foreach ( FileInfo fileInfo in tempFileInfos ) fileInfoCollection.Remove ( fileInfo );
				if ( fileInfoCollection.Count == 0 ) { undoStack.Clear (); redoStack.Clear (); }
			}
		}

		private void TextBox_MouseDoubleClick ( object sender, MouseButtonEventArgs e )
		{
			( sender as TextBox ).IsReadOnly = false;
			( sender as TextBox ).IsReadOnlyCaretVisible = true;
			( sender as TextBox ).BorderThickness = new Thickness ( 1 );
			( sender as TextBox ).Focus ();
		}

		private void TextBox_LostFocus ( object sender, RoutedEventArgs e )
		{
			( sender as TextBox ).IsReadOnly = true;
			( sender as TextBox ).IsReadOnlyCaretVisible = false;
			( sender as TextBox ).BorderThickness = new Thickness ( 0 );
		}

		private void TextBox_PreviewKeyDown ( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Enter )
			{
				TextBox_LostFocus ( sender, e );
				e.Handled = true;
			}
		}
		#endregion

		#region String Process
		private void StringProcess_Replace_Click ( object sender, RoutedEventArgs e )
		{
			if ( stringReplaceOriginalText.Text.Trim ().Length == 0 || stringReplaceNewText.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string originT = stringReplaceOriginalText.Text;
			string newT = stringReplaceNewText.Text;
			bool check = stringReplaceIncludeExt.IsChecked.Value;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.Replace ( fileInfo.ChangedName, originT, newT, !check )
			);
		}

		private void StringProcess_Concat_Click ( object sender, RoutedEventArgs e )
		{
			if ( stringConcatText.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			bool where = stringConcatPreRadio.IsChecked.Value;
			string concat = stringConcatText.Text;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = ( where == true ? 
					FilenameProcessor.Prestring ( fileInfo.ChangedName, concat ) :
					FilenameProcessor.Poststring ( fileInfo.ChangedName, concat ) )
			);
		}

		private void StringProcess_Trim_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			bool? when = null;
			if ( stringTrimPreRadio.IsChecked == true ) when = false;
			if ( stringTrimPostRadio.IsChecked == true ) when = true;
			bool check = stringTrimIncludeExt.IsChecked.Value;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.Trimming ( fileInfo.ChangedName, check, when )
			);
		}

		private void StringProcess_DelEnclosed_Click ( object sender, RoutedEventArgs e )
		{
			if ( stringEnclosedPreText.Text.Trim ().Length == 0 || stringEnclosedPostText.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string pre = stringEnclosedPreText.Text;
			string post = stringEnclosedPostText.Text;
			bool all = stringEnclosedAll.IsChecked.Value;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.DeleteEnclosed ( fileInfo.ChangedName,
				pre, post, all )
			);
		}

		private void StringProcess_DelName_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.DeleteName ( fileInfo.ChangedName )
			);
		}

		private void StringProcess_LowUp_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			bool firstletter = stringUpLowOneLetterUpper.IsChecked.Value;
			bool upper = stringUpLowUpper.IsChecked.Value;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = firstletter ?
					FilenameProcessor.NameToUpperFirstLetterOnly(fileInfo.ChangedName ) :
					( upper ? FilenameProcessor.NameToUpper ( fileInfo.ChangedName ) : FilenameProcessor.NameToLower(fileInfo.ChangedName) )
			);
		}
		#endregion

		#region Extension Process
		private void ExtensionProcess_AddExt_Click ( object sender, RoutedEventArgs e )
		{
			if ( extAddExtensionText.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string ext = extAddExtensionText.Text;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.AddExtension ( fileInfo.ChangedName, ext )
			);
		}

		private void ExtensionProcess_RemoveExt_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.RemoveExtension ( fileInfo.ChangedName )
			);
		}

		private void ExtensionProcess_ChangeExt_Click ( object sender, RoutedEventArgs e )
		{
			if ( extChangeExtensionText.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string ext = extChangeExtensionText.Text;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.ChangeExtension ( fileInfo.ChangedName, ext )
			);
		}

		private void ExtensionProcess_LowUp_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			bool upper = extUpLowUpper.IsChecked.Value;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = upper ? FilenameProcessor.ExtensionToUpper ( fileInfo.ChangedName ) :
					FilenameProcessor.ExtensionToLower ( fileInfo.ChangedName )
			);
		}
		#endregion

		#region Number Process
		private void Number_DeleteWithoutNumber_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			bool wordly = numDelWoNoNumberWordOnly.IsChecked.Value;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = wordly ? FilenameProcessor.DeleteWithoutNumberWordly ( fileInfo.ChangedName ) :
					FilenameProcessor.DeleteWithoutNumber ( fileInfo.ChangedName )
			);
		}

		private void Digit_PreviewTextInput ( object sender, TextCompositionEventArgs e )
		{
			Regex regex = new Regex ( "[^0-9]+" );
			e.Handled = regex.IsMatch ( e.Text );
		}

		private void Number_SameDigits_Click ( object sender, RoutedEventArgs e )
		{
			int digits;
			if ( !int.TryParse ( numSameDigitCount.Text, out digits ) ) return;

			SaveCurrentStateToUndoStack ();

			bool pre = numSameDigitPre.IsChecked.Value;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.SameNumberOfDigit ( fileInfo.ChangedName, digits, !pre )
			);
		}

		private void Number_AddNum_Click ( object sender, RoutedEventArgs e )
		{
			int term;
			if ( !int.TryParse ( numAddNumTerm.Text, out term ) ) return;

			SaveCurrentStateToUndoStack ();

			bool pre = numAddNumPre.IsChecked.Value;

			Parallel.For ( 1, fileInfoCollection.Count, ( index ) =>
			{
				FileInfo fileInfo = fileInfoCollection [ index ];
				fileInfo.ChangedName = FilenameProcessor.AddNumber ( fileInfo.ChangedName, index * term, !pre );
			} );
		}

		private void Number_Increase_Click ( object sender, RoutedEventArgs e )
		{
			int value;
			if ( !int.TryParse ( numAddSubNumTerm.Text, out value ) ) return;

			SaveCurrentStateToUndoStack ();

			bool pre = numAddSubNumPre.IsChecked.Value;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.NumberIncrese ( fileInfo.ChangedName, value, !pre )
			);
		}
		#endregion

		#region Date Process
		private void Date_AddCreation_Click ( object sender, RoutedEventArgs e )
		{
			if ( dateCreatedFormat.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string format = dateCreatedFormat.Text;
			bool pre = dateCreatedPre.IsChecked.Value;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.AddCreationDate ( fileInfo.OriginalFullName, fileInfo.ChangedName, !pre, format )
			);
		}

		private void Date_AddLastAccess_Click ( object sender, RoutedEventArgs e )
		{
			if ( dateAccessFormat.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string format = dateAccessFormat.Text;
			bool pre = dateAccessPre.IsChecked.Value;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.AddLastAccessDate ( fileInfo.OriginalFullName, fileInfo.ChangedName, !pre, format )
			);
		}

		private void Date_AddLastWrite_Click ( object sender, RoutedEventArgs e )
		{
			if ( dateWriteFormat.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			string format = dateWriteFormat.Text;
			bool pre = dateWritePre.IsChecked.Value;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.AddLastWriteDate ( fileInfo.OriginalFullName, fileInfo.ChangedName, !pre, format )
			);
		}
		#endregion

		#region Path Process
		private void Path_Change_Browse_Click ( object sender, RoutedEventArgs e )
		{
			WPFFolderBrowser.WPFFolderBrowserDialog fbd = new WPFFolderBrowser.WPFFolderBrowserDialog ();
			fbd.InitialDirectory = pathToText.Text;

			if ( fbd.ShowDialog () == true )
			{
				pathToText.Text = fbd.FileName;
			}
		}

		private void Path_Change_Click ( object sender, RoutedEventArgs e )
		{
			if ( pathToText.Text.Trim ().Length == 0 ) return;
			if ( !System.IO.Directory.Exists ( pathToText.Text ) )
			{
				TaskDialogOptions config = new TaskDialogOptions ();
				config.Owner = this;
				config.Title = Daramkun.DaramRenamer.Properties.Resources.DaramRenamer;
				config.MainInstruction = Daramkun.DaramRenamer.Properties.Resources.NoExistDirectory;
				config.Content = string.Format ( Daramkun.DaramRenamer.Properties.Resources.JustContinueWhenNoExistPath, pathToText.Text );
				config.MainIcon = VistaTaskDialogIcon.Warning;
				config.CustomButtons = new [] { Daramkun.DaramRenamer.Properties.Resources.OK, Daramkun.DaramRenamer.Properties.Resources.No };
				if ( TaskDialog.Show ( config ).CustomButtonResult == 1 ) return;
			}

			SaveCurrentStateToUndoStack ();

			string path = pathToText.Text;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) => fileInfo.ChangedPath = path );
		}
		#endregion

		#region Regular Expression Process
		private void RegularExpression_Process_Click ( object sender, RoutedEventArgs e )
		{
			if ( regexpOriginal.Text.Trim ().Length == 0 || regexpReplace.Text.Trim ().Length == 0 ) return;

			SaveCurrentStateToUndoStack ();

			Regex regex = new Regex ( regexpOriginal.Text );
			string format = regexpReplace.Text;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.ChangedName = FilenameProcessor.RegularExpression ( fileInfo.ChangedName, regex, format )
			);

			if ( ( regexpOriginal.ItemsSource as ObservableCollection<string> ).IndexOf ( regexpOriginal.Text ) < 0 )
				( regexpOriginal.ItemsSource as ObservableCollection<string> ).Insert ( 0, regexpOriginal.Text );
			if ( ( regexpReplace.ItemsSource as ObservableCollection<string> ).IndexOf ( regexpReplace.Text ) < 0 )
				( regexpReplace.ItemsSource as ObservableCollection<string> ).Insert ( 0, regexpReplace.Text );
		}
		#endregion

		#region Settings
		private void checkHwAccel_Checked ( object sender, RoutedEventArgs e )
		{
			if ( Settings.Default.HardwareTurnOn )
				RenderOptions.ProcessRenderMode = RenderMode.Default;
			else
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
		}
		#endregion
	}
}
