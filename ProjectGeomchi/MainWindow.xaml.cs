using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
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

		private string GetFilenameWithoutExtension ( string filename )
		{
			int lastDot = filename.LastIndexOf ( '.' );
			return filename.Substring ( 0, lastDot );
		}

		private string GetExtensionWithoutFilename ( string filename )
		{
			int lastDot = filename.LastIndexOf ( '.' );
			return filename.Substring ( lastDot, filename.Length - lastDot );
		}

		private string GetReverseString ( string str )
		{
			StringBuilder sb = new StringBuilder();
			foreach ( char ch in str.Reverse () )
				sb.Append ( ch );
			return sb.ToString ();
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
						"\"{0}\"파일의 경로를 변경할 권한이 없습니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( FileNotFoundException ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"\"{0}\"원본 파일이 존재하지 않습니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
				catch ( Exception ex )
				{
					System.Windows.Forms.MessageBox.Show ( String.Format (
						"\"{0}\"파일의 경로를 변경할 수 없었습니다.", fileInfo.ON ) );
					Debug.WriteLine ( ex.Message );
				}
			}

			undoStack.Clear ();
			redoStack.Clear ();
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
			{
				fileInfo.CN = ( isExcludeExt ) ? (
					GetFilenameWithoutExtension ( fileInfo.CN ).Replace ( original, replace )
						+ GetExtensionWithoutFilename ( fileInfo.CN ) )
						: fileInfo.CN.Replace ( original, replace );
			} );
		}

		private void menuItemPrestring_Click ( object sender, RoutedEventArgs e )
		{
			AddStringWindow window = new AddStringWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string str = window.String;
			string form = ( !window.IsPrestring ) ? "{1}{0}{2}" : "{0}{1}{2}";
			
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				fileInfo.CN = string.Format ( form, str, GetFilenameWithoutExtension ( fileInfo.CN ),
					GetExtensionWithoutFilename ( fileInfo.CN ) );
			} );
		}
		#endregion

		#region Edit Menu - String Delete
		private void menuItemDeleteName_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = GetExtensionWithoutFilename ( fileInfo.CN ) );
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
			{
				int first, last;
				while ( ( first = fileInfo.CN.IndexOf ( pre ) ) != -1 )
				{
					last = fileInfo.CN.IndexOf ( post, first + 1 );
					if ( last == -1 ) break;
					fileInfo.CN = fileInfo.CN.Remove ( first, last - first + post.Length );
					if ( !isAllDelete ) break;
				}
			} );
		}
		#endregion

		#region Edit Menu - Number Process
		private void menuItemDeleteWithoutNumber_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();
			
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				StringBuilder sb = new StringBuilder ();

				foreach ( char ch in GetFilenameWithoutExtension ( fileInfo.CN ) )
					if ( ch >= '0' && ch <= '9' )
						sb.Append ( ch );
				fileInfo.CN = sb.ToString () + GetExtensionWithoutFilename ( fileInfo.CN );
			} );
		}

		private void menuItemSameNumberOfDigits_Click ( object sender, RoutedEventArgs e )
		{
			DigitWindow window = new DigitWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			int digitCount = window.DigitCount;
			bool isOffsetFromBack = window.ProcessOffset == 0 ? true : false;
			
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				string filename = GetFilenameWithoutExtension ( fileInfo.CN );
				if ( isOffsetFromBack ) filename = GetReverseString ( filename );

				bool meetTheNumber = false;
				int offset = 0, count = 0;
				StringBuilder sb = new StringBuilder ();
				foreach ( char ch in filename )
				{
					if ( ( ch >= '0' && ch <= '9' ) )
					{
						if ( !meetTheNumber && !isOffsetFromBack ) offset = count;
						sb.Append ( ch );
						meetTheNumber = true;
					}
					else
					{
						if ( meetTheNumber )
						{
							if ( isOffsetFromBack ) offset = filename.Length - count;
							break;
						}
					}
					count++;
				}

				if ( !meetTheNumber ) return;

				string origin, temp;
				origin = temp = sb.ToString ();
				if ( isOffsetFromBack )
				{
					filename = GetReverseString ( filename );
					temp = GetReverseString ( temp );
				}

				if ( digitCount > temp.Length )
					for ( int i = 0; i < digitCount - temp.Length + 1; i++ )
						temp = '0' + temp;

				string filenameTemp = filename.Remove ( offset, origin.Length );
				fileInfo.CN = filenameTemp.Insert ( offset, temp ) + GetExtensionWithoutFilename ( fileInfo.CN );
			} );

			GC.Collect ();
		}

		private void menuItemAddNumbers_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			int fileCount = 1;
			foreach ( FileInfo fileInfo in fileInfoCollection )
				fileInfo.CN = GetFilenameWithoutExtension ( fileInfo.CN ) + fileCount++ +
					GetExtensionWithoutFilename ( fileInfo.CN );
		}

		private void menuItemNumberIncrease_Click ( object sender, RoutedEventArgs e )
		{
			IncreaseWindow window = new IncreaseWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			bool isOffsetFromBack = window.ProcessOffset == 0 ? true : false;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				string filename = GetFilenameWithoutExtension ( fileInfo.CN );
				if ( isOffsetFromBack ) filename = GetReverseString ( filename );

				bool meetTheNumber = false;
				int offset = 0, count = 0;
				StringBuilder sb = new StringBuilder ();
				foreach ( char ch in filename )
				{
					if ( ( ch >= '0' && ch <= '9' ) )
					{
						if ( !meetTheNumber && !isOffsetFromBack ) offset = count;
						sb.Append ( ch );
						meetTheNumber = true;
					}
					else
					{
						if ( meetTheNumber )
						{
							if ( isOffsetFromBack ) offset = filename.Length - count;
							break;
						}
					}
					count++;
				}

				if ( !meetTheNumber ) return;

				string origin, temp;
				origin = sb.ToString ();
				if ( isOffsetFromBack )
				{
					filename = GetReverseString ( filename );
					origin = GetReverseString ( origin );
				}

				int digitCount = origin.Length;
				int digit = int.Parse ( origin ) + window.IncreaseValue;
				temp = String.Format ( "{0}", digit );
				for ( ; temp.Length < digitCount; )
					temp = "0" + temp;

				string filenameTemp = filename.Remove ( offset, origin.Length );
				fileInfo.CN = filenameTemp.Insert ( offset, temp ) + GetExtensionWithoutFilename ( fileInfo.CN );
			} );

			GC.Collect ();
		}
		#endregion

		#region Edit Menu - Extension
		private void menuItemDeleteExtension_Click ( object sender, RoutedEventArgs e )
		{
			SaveCurrentStateToUndoStack ();

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = GetFilenameWithoutExtension ( fileInfo.CN ) );
		}

		private void menuItemAddExtension_Click ( object sender, RoutedEventArgs e )
		{
			AddExtensionWindow window = new AddExtensionWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string extension = String.Format ( ".{0}", window.Extension );

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN += extension );
		}

		private void menuItemChangeExtension_Click ( object sender, RoutedEventArgs e )
		{
			ChangeExtensionWindow window = new ChangeExtensionWindow ();
			if ( window.ShowDialog () == false ) return;

			SaveCurrentStateToUndoStack ();

			string extension = String.Format ( ".{0}", window.Extension );

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
				fileInfo.CN = fileInfo.CN.Replace ( GetExtensionWithoutFilename ( fileInfo.CN ),
					extension ) );
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
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				fileInfo.CN = string.Format ( "{0}{1}", GetFilenameWithoutExtension ( fileInfo.CN ),
					GetExtensionWithoutFilename ( fileInfo.CN ).ToLower () );
			} );
		}

		private void menuItemExtensionsToUpper_Click ( object sender, RoutedEventArgs e )
		{
			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				fileInfo.CN = string.Format ( "{0}{1}", GetFilenameWithoutExtension ( fileInfo.CN ),
					GetExtensionWithoutFilename ( fileInfo.CN ).ToUpper () );
			} );
		}
		#endregion

		#region Edit Menu - Regular Expression
		private void menuItemRegularExpression_Click ( object sender, RoutedEventArgs e )
		{
			RegularExpressionWindow window = new RegularExpressionWindow ();
			if ( window.ShowDialog () == false ) return;

			Regex exp = new Regex ( window.RegularExpression, RegexOptions.IgnoreCase );
			string formstr = window.FormatString;

			Parallel.ForEach ( fileInfoCollection, ( FileInfo fileInfo ) =>
			{
				try
				{
					Match match = exp.Match ( fileInfo.CN );
					GroupCollection group = match.Groups;
					object [] groupArr = new object [ group.Count ];
					for ( int i = 0; i < groupArr.Length; i++ )
						groupArr [ i ] = group [ i ].Value.Trim ();
					fileInfo.CN = string.Format ( formstr, groupArr );
				}
				catch { }
			} );

			SaveCurrentStateToUndoStack ();
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