using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GroupRenamer
{
	public partial class MainWindow : Window
	{
		#region Member variable
		BinaryFormatter bf;
		ObservableCollection<FileInfo> fileInfoCollection;
		Stack<byte []> undoStack;
		Stack<byte []> redoStack;
		#endregion

		#region Constructors
		public MainWindow ()
		{
			InitializeComponent ();

			bf = new BinaryFormatter ();

			fileInfoCollection = new ObservableCollection<FileInfo> ();
			listViewFiles.ItemsSource = fileInfoCollection;

			undoStack = new Stack<byte []> ();
			redoStack = new Stack<byte []> ();
		}
		#endregion

		#region Utilities
		private void AddItem ( string s )
		{
			string filename = System.IO.Path.GetFileName ( s );
			string path = System.IO.Path.GetDirectoryName ( s );
			fileInfoCollection.Add ( new FileInfo ()
			{
				ON = filename.Clone () as string,
				CN = filename.Clone () as string,
				OP = path.Clone () as string,
				CP = path.Clone () as string,
			} );
		}
		#endregion

		#region Undo and Redo
		private void SaveCurrentStateToUndoStack ()
		{
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

		#region ListView Event Handlers
		private void ListView_DragEnter ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				e.Effects = DragDropEffects.None;
			}
		}

		private void ListView_Drop ( object sender, DragEventArgs e )
		{
			if ( e.Data.GetDataPresent ( DataFormats.FileDrop ) )
			{
				string [] temp = e.Data.GetData ( DataFormats.FileDrop ) as string [];
				foreach ( string s in from b in temp orderby b select b )
					AddItem ( s );
			}
		}

		private void listViewFiles_KeyUp ( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Delete )
			{
				List<FileInfo> tempFileInfos = new List<FileInfo> ();
				foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
					tempFileInfos.Add ( fileInfo );
				foreach ( FileInfo fileInfo in tempFileInfos )
					fileInfoCollection.Remove ( fileInfo );
			}
		}
		#endregion

		#region File Menu
		private void menuItemOpenFiles_Click ( object sender, RoutedEventArgs e )
		{
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog ();
			openFileDialog.Title = "열기";
			openFileDialog.Multiselect = true;
			openFileDialog.Filter = "모든 파일(*.*)|*.*";
			if ( openFileDialog.ShowDialog () == false ) return;

			foreach ( string s in from s in openFileDialog.FileNames orderby s select s )
				AddItem ( s );
		}

		private void menuItemCloseFiles_Click ( object sender, RoutedEventArgs e )
		{
			fileInfoCollection.Clear ();
		}

		private void menuItemCommitChanges_Click ( object sender, RoutedEventArgs e )
		{
			if ( System.Windows.Forms.MessageBox.Show ( "적용하시겠습니까? 적용 시 되돌릴 수 없습니다.",
				"안내", System.Windows.Forms.MessageBoxButtons.YesNo ) == System.Windows.Forms.DialogResult.No )
				return;

			foreach ( FileInfo fileInfo in fileInfoCollection )
			{
				try
				{
					File.Move ( System.IO.Path.Combine ( fileInfo.OP, fileInfo.ON ),
						System.IO.Path.Combine ( fileInfo.CP, fileInfo.CN ) );
					fileInfo.Changed ();
				}
				catch ( UnauthorizedAccessException ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"\"{0}\" 파일의 경로를 변경할 권한이 없습니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( PathTooLongException ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"\"{0}\"파일의 경로가 너무 깁니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( DirectoryNotFoundException ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"\"{0}\"의 디렉토리가 존재하지 않습니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( IOException ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"\"{0}\" 경로에 파일이 이미 있거나 원본 파일을 찾을 수 없습니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( Exception ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"알 수 없는 이유로 파일의 경로를 변경할 수 없었습니다: {0}", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
			}

			undoStack.Clear ();
			redoStack.Clear ();
		}

		private void menuItemCheckForUpdates_Click ( object sender, RoutedEventArgs e )
		{
			HttpWebRequest req = HttpWebRequest.CreateHttp ( "https://daram.pe.kr/2014/07/다람-리네이머/" );
			HttpWebResponse  res = req.GetResponse () as HttpWebResponse;
			using ( Stream stream = res.GetResponseStream () )
			{
				using ( StreamReader reader = new StreamReader ( stream ) )
				{
					string text = reader.ReadToEnd ();
					int begin = text.IndexOf ( "<p>다운로드: <a href=\"https://daram.pe.kr/wp-content/uploads" );
					if ( begin == -1 ) { MessageBox.Show ( "업데이트를 확인할 수 없습니다." ); return; };
					int end = text.IndexOf ( "</a></p>", begin );
					if ( end == -1 ) { MessageBox.Show ( "업데이트를 확인할 수 없습니다." ); return; };
					text = text.Substring ( end - 5, 5 );
					if ( text != "1.100" )
						if ( MessageBox.Show ( string.Format ( "업데이트가 있습니다 (버전: {0}). 홈페이지에서 다운로드해주세요.\r\n홈페이지로 바로 가려면 \"예\"를 눌러주세요.", text ),
							"업데이트 확인", MessageBoxButton.YesNo ) == MessageBoxResult.Yes )
							Process.Start ( "https://daram.pe.kr/2014/07/다람-리네이머/" );
						else MessageBox.Show ( "업데이트가 없습니다.", "업데이트 확인" );
				}
			}
		}
		
		private void menuItemExit_Click ( object sender, RoutedEventArgs e )
		{
			this.Close ();
		}
		#endregion

		#region Edit Menu - Undo/Redo
		private void menuItemUndo_Click ( object sender, RoutedEventArgs e )
		{
			RepairFromUndoStack ();
		}

		private void menuItemRedo_Click ( object sender, RoutedEventArgs e )
		{
			RepairFromRedoStack ();
		}
		#endregion

		#region Edit Menu - Cancel
		private void menuItemCancel_Click ( object sender, RoutedEventArgs e )
		{
			while ( RepairFromUndoStack () ) ;
		}
		#endregion

		#region Edit Menu - String Add/Replace
		private void menuItemReplace_Click ( object sender, RoutedEventArgs e )
		{
			ReplaceWindow window = new ReplaceWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string original = window.Original;
			string replace = window.Replace;
			bool isExcludeExt = window.IsExcludeExtension;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.Replace ( fileInfo.CN, original, replace, isExcludeExt ) );
		}

		private void menuItemPrestring_Click ( object sender, RoutedEventArgs e )
		{
			AddStringWindow window = new AddStringWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string str = window.String;
			bool isPrestring = window.IsPrestring;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = isPrestring ? FilenameProcessor.Prestring ( fileInfo.CN, str ) : FilenameProcessor.Poststring ( fileInfo.CN, str ) );
		}
		#endregion

		#region Edit Menu - String Delete
		private void menuItemDeleteName_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.DeleteName ( fileInfo.CN ) );
		}

		private void menuItemDeleteEnclosed_Click ( object sender, RoutedEventArgs e )
		{
			DeleteEnclosedWindow window = new DeleteEnclosedWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string pre = window.Prestring;
			string post = window.Poststring;
			bool isAllDelete = window.IsDeleteAllEnclosed;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.DeleteEnclosed ( fileInfo.CN, pre, post, isAllDelete ) );
		}
		#endregion

		#region Edit Menu - Number Process
		private void menuItemDeleteWithoutNumber_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.DeleteWithoutNumber ( fileInfo.CN ) );
		}

		private void menuItemSameNumberOfDigits_Click ( object sender, RoutedEventArgs e )
		{
			DigitWindow window = new DigitWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			int digitCount = window.Value;
			bool isOffsetFromBack = window.ProcessOffset == 0 ? true : false;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.SameNumberOfDigit ( fileInfo.CN, digitCount, isOffsetFromBack ) );
		}

		private void menuItemAddNumbers_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.For ( 1, fileInfoCollection.Count + 1, (int i) =>
			{
				FileInfo fileInfo = fileInfoCollection [ i - 1 ];
				fileInfo.CN = FilenameProcessor.AddNumber ( fileInfo.CN, i );
			} );
		}

		private void menuItemNumberIncrease_Click ( object sender, RoutedEventArgs e )
		{
			IncreaseWindow window = new IncreaseWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			int increaseValue = window.IncreaseValue;
			bool isOffsetFromBack = window.ProcessOffset == 0 ? true : false;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.NumberIncrese ( fileInfo.CN, increaseValue, isOffsetFromBack ) );
		}
		#endregion

		#region Edit Menu - Extension
		private void menuItemDeleteExtension_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.RemoveExtension ( fileInfo.CN ) );
		}

		private void menuItemAddExtension_Click ( object sender, RoutedEventArgs e )
		{
			AddExtensionWindow window = new AddExtensionWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string ext = window.Extension;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.AddExtension ( fileInfo.CN, ext ) );
		}

		private void menuItemChangeExtension_Click ( object sender, RoutedEventArgs e )
		{
			ChangeExtensionWindow window = new ChangeExtensionWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string ext = window.Extension;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.ChangeExtension ( fileInfo.CN, ext ) );
		}
		#endregion

		#region Edit Menu - Path
		private void menuItemChangePath_Click ( object sender, RoutedEventArgs e )
		{
			System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog ();
			if ( dialog.ShowDialog () == System.Windows.Forms.DialogResult.Cancel ) return;
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CP = dialog.SelectedPath );
		}
		#endregion

		#region Edit Menu - Extensions
		private void menuItemExtensionsToLower_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.ExtensionToLower ( fileInfo.CN ) );
		}

		private void menuItemExtensionsToUpper_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.ExtensionToUpper ( fileInfo.CN ) );
		}
		#endregion

		#region Edit Menu - Regular Expression
		private void menuItemRegularExpression_Click ( object sender, RoutedEventArgs e )
		{
			RegularExpressionWindow window = new RegularExpressionWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			Regex exp = new Regex ( window.RegularExpression, RegexOptions.IgnoreCase );
			string formstr = window.FormatString;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = FilenameProcessor.RegularExpression ( fileInfo.CN, exp, formstr ) );
		}
		#endregion

		#region Item Menu
		private void menuItemGotoUp_Click ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = fileInfoCollection.IndexOf ( fileInfo );
				if ( lastIndex == 0 ) continue;
				fileInfoCollection.Move ( lastIndex, lastIndex - 1 );
			}
		}

		private void menuItemGotoDown_Click ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count == 0 ) return;
			foreach ( FileInfo fileInfo in listViewFiles.SelectedItems )
			{
				int lastIndex = fileInfoCollection.IndexOf ( fileInfo );
				if ( lastIndex == fileInfoCollection.Count - 1 ) continue;
				fileInfoCollection.Move ( lastIndex, lastIndex + 1 );
			}
		}

		private void menuItemSort_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();
			listViewFiles.ItemsSource = fileInfoCollection = FileInfo.Sort ( fileInfoCollection );
		}

		private void menuItemEditItem_Click ( object sender, RoutedEventArgs e )
		{
			if ( listViewFiles.SelectedItems.Count != 1 ) return;

			FileInfo fileInfo = listViewFiles.SelectedItem as FileInfo;
			EditWindow window = new EditWindow ( fileInfo.CN, fileInfo.CP );
			
			if ( window.ShowDialog () == false ) return;

			( listViewFiles.SelectedItem as FileInfo ).CN = window.Filename;
			( listViewFiles.SelectedItem as FileInfo ).CP = window.Path;
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
		public static RoutedCommand CommandEditItems = new RoutedCommand ();
		#endregion
		#region Executes
		private void commandOpenFiles_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemOpenFiles_Click ( sender, e );
		}

		private void commandClearList_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemCloseFiles_Click ( sender, e );
		}

		private void commandApplyFile_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemChangePath_Click ( sender, e );
		}

		private void commandUndoWorks_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemUndo_Click ( sender, e );
		}

		private void commandRedoWorks_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemRedo_Click ( sender, e );
		}

		private void commandApplyCanc_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemCancel_Click ( sender, e );
		}

		private void commandUpperItem_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemGotoUp_Click ( sender, e );
		}

		private void commandLowerItem_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemGotoDown_Click ( sender, e );
		}

		private void commandItemsSort_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemSort_Click ( sender, e );
		}

		private void commandEditItems_Executed ( object sender, ExecutedRoutedEventArgs e )
		{
			menuItemEditItem_Click ( sender, e );
		}
		#endregion
		#endregion
	}
}