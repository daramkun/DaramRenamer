using Daramee.DaramCommonLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer
{
	public enum ErrorCode
	{
		NoError,
		Unknown,
		UnauthorizedAccess,
		PathTooLong,
		DirectoryNotFound,
		IOError,
		FailedOverwrite,
	}

	[Serializable]
	public class FileInfo : IComparable<FileInfo>, INotifyPropertyChanged
	{
		public static ObservableCollection<FileInfo> Files { get; set; } = new ObservableCollection<FileInfo> ();

		string originalFullPath;
		string changedPath, changedFilename;

		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;

		public string OriginalFullPath
		{
			get { return originalFullPath; }
			private set
			{
				originalFullPath = value;
				PC ( nameof ( OriginalFullPath ) );
				PC ( nameof ( OriginalPath ) );
				PC ( nameof ( OriginalFilename ) );
			}
		}
		public string OriginalPath => Path.GetDirectoryName ( OriginalFullPath );
		public string OriginalFilename => Path.GetFileName ( OriginalFullPath );
		public string ChangedPath { get => changedPath; set { changedPath = value; PC ( nameof ( ChangedPath ) ); } }
		public string ChangedFilename { get => changedFilename; set { changedFilename = value; PC ( nameof ( ChangedFilename ) ); } }
		public string ChangedFullPath => Path.Combine ( ChangedPath, ChangedFilename );
		public bool IsDirectory { get; set; }

		public FileInfo ( string fullPath )
		{
			OriginalFullPath = fullPath; ChangedFilename = OriginalFilename; ChangedPath = OriginalPath;
			IsDirectory = File.GetAttributes ( fullPath ).HasFlag ( FileAttributes.Directory );
		}
		public FileInfo ( FileInfo file )
		{
			OriginalFullPath = file.OriginalFullPath;
			ChangedPath = file.ChangedPath;
			ChangedFilename = file.ChangedFilename;
			IsDirectory = file.IsDirectory;
		}

		IShellItem OriginalPathShellItem
		{
			get
			{
				SHCreateItemFromParsingName ( OriginalPath, IntPtr.Zero, new Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ), out IShellItem shellItem );
				return shellItem;
			}
		}
		IShellItem OriginalFullPathShellItem
		{
			get
			{
				SHCreateItemFromParsingName ( OriginalFullPath, IntPtr.Zero, new Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ), out IShellItem shellItem );
				return shellItem;
			}
		}
		IShellItem ChangedPathShellItem
		{
			get
			{
				SHCreateItemFromParsingName ( ChangedPath, IntPtr.Zero, new Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ), out IShellItem shellItem );
				return shellItem;
			}
		}
		IShellItem ChangedFullPathShellItem
		{
			get
			{
				SHCreateItemFromParsingName ( ChangedFullPath, IntPtr.Zero, new Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ), out IShellItem shellItem );
				return shellItem;
			}
		}

		static IFileOperation fileOperation;
		public static void BeginFileOperation ()
		{
			if ( fileOperation != null )
				throw new InvalidOperationException ( "File Operation is already beginned." );

			fileOperation = Activator.CreateInstance ( Type.GetTypeFromCLSID ( new Guid ( "3ad05575-8857-4850-9277-11b85bdb8e09" ) ) ) as IFileOperation;
			fileOperation.SetOperationFlags ( OperationFlag.FOF_ALLOWUNDO 
				| OperationFlag.FOF_NO_UI | OperationFlag.FOF_FILESONLY );
		}
		public static void EndFileOperation ()
		{
			if ( fileOperation != null )
			{
				fileOperation.PerformOperations ();
				Marshal.ReleaseComObject ( fileOperation );
				fileOperation = null;
			}
		}

		public static bool Move ( FileInfo fileInfo, bool overwrite, out ErrorCode errorMessage )
		{
			if ( fileOperation == null )
				throw new InvalidOperationException ( "File Operation is not ready." );

			try
			{
				if ( overwrite && File.Exists ( fileInfo.ChangedFullPath ) )
					File.Delete ( fileInfo.ChangedFullPath );
				//File.Move ( fileInfo.OriginalFullPath, fileInfo.ChangedFullPath );
				long result = 0;
				if ( fileInfo.OriginalPath != fileInfo.ChangedPath )
				{
					IShellItem originalFullPath = fileInfo.OriginalFullPathShellItem;
					IShellItem changedPath = fileInfo.ChangedPathShellItem;
					result = fileOperation.MoveItem ( originalFullPath, changedPath,
						fileInfo.ChangedFilename, IntPtr.Zero );
					Marshal.ReleaseComObject ( changedPath );
					Marshal.ReleaseComObject ( originalFullPath );
				}
				else
				{
					IShellItem originalFullPath = fileInfo.OriginalFullPathShellItem;
					result = fileOperation.RenameItem ( originalFullPath,
						fileInfo.ChangedFilename, IntPtr.Zero );
					Marshal.ReleaseComObject ( originalFullPath );
				}
				fileInfo.Changed ();
				errorMessage = ErrorCode.NoError;
				return true;
			}
			catch ( UnauthorizedAccessException ) { errorMessage = ErrorCode.UnauthorizedAccess; }
			catch ( PathTooLongException ) { errorMessage = ErrorCode.PathTooLong; }
			catch ( DirectoryNotFoundException ) { errorMessage = ErrorCode.DirectoryNotFound; }
			catch ( IOException ) { errorMessage = ErrorCode.IOError; }
			catch ( Exception ) { errorMessage = ErrorCode.Unknown; }
			return false;
		}

		public static bool Copy ( FileInfo fileInfo, bool overwrite, out ErrorCode errorMessage )
		{
			if ( fileOperation == null )
				throw new InvalidOperationException ( "File Operation is not ready." );

			try
			{
				if ( overwrite && File.Exists ( fileInfo.ChangedFullPath ) )
					File.Delete ( fileInfo.ChangedFullPath );

				//File.Copy ( fileInfo.OriginalFullPath, fileInfo.ChangedFullPath, overwrite );
				IShellItem originalFullPath = fileInfo.OriginalFullPathShellItem;
				IShellItem changedPath = fileInfo.ChangedPathShellItem;
				long result = fileOperation.CopyItem ( originalFullPath, changedPath, fileInfo.ChangedFilename, IntPtr.Zero );
				Marshal.ReleaseComObject ( changedPath );
				Marshal.ReleaseComObject ( originalFullPath );

				fileInfo.Changed ();
				errorMessage = ErrorCode.NoError;
				return true;
			}
			catch ( UnauthorizedAccessException ) { errorMessage = ErrorCode.UnauthorizedAccess; }
			catch ( PathTooLongException ) { errorMessage = ErrorCode.PathTooLong; }
			catch ( DirectoryNotFoundException ) { errorMessage = ErrorCode.DirectoryNotFound; }
			catch ( IOException ) { errorMessage = overwrite ? ErrorCode.FailedOverwrite : ErrorCode.IOError; }
			catch ( Exception ) { errorMessage = ErrorCode.Unknown; }
			return false;
		}

		private void Changed ()
		{
			OriginalFullPath = ChangedFullPath;
		}

		public static void Sort ( ObservableCollection<FileInfo> source )
		{
			if ( source == null ) return;
			Daramee.DaramCommonLib.Sort.Quicksort<FileInfo> ( source );
		}

		public int CompareTo ( FileInfo other ) => ChangedFilename.CompareTo ( other.ChangedFilename );

		private void PC ( string name ) { PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( name ) ); }

		#region Interop
		[Flags]
		enum OperationFlag : uint
		{
			FOF_MULTIDESTFILES = 0x0001,
			FOF_CONFIRMMOUSE = 0x0002,
			FOF_SILENT = 0x0004,
			FOF_RENAMEONCOLLISION = 0x0008,
			FOF_NOCONFIRMATION = 0x0010,
			FOF_WANTMAPPINGHANDLE = 0x0020,
			FOF_ALLOWUNDO = 0x0040,
			FOF_FILESONLY = 0x0080,
			FOF_SIMPLEPROGRESS = 0x0100,
			FOF_NOCONFIRMMKDIR = 0x0200,
			FOF_NOERRORUI = 0x0400,
			FOF_NOCOPYSECURITYATTRIBS = 0x0800,
			FOF_NORECURSION = 0x1000,
			FOF_NO_CONNECTED_ELEMENTS = 0x2000,
			FOF_WANTNUKEWARNING = 0x4000,
			FOF_NORECURSEREPARSE = 0x8000,
			FOF_NO_UI = ( FOF_SILENT | FOF_NOCONFIRMATION | FOF_NOERRORUI | FOF_NOCONFIRMMKDIR ),
		}

		enum SIGDN : uint
		{
			NormalDisplay = 0x00000000,
			ParentRelativeParsing = 0x80018001,
			DesktopAbsoluteParsing = 0x80028000,
			ParentRelativeEditing = 0x80031001,
			DesktopAbsoluteEditing = 0x8004c000,
			FileSysPath = 0x80058000,
			URL = 0x80068000,
			ParentRelativeForAddressBar = 0x8007c001,
			ParentRelative = 0x80080001
		}

		[ComImport,
			Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ),
			InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		interface IShellItem
		{
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void BindToHandler ( [In, MarshalAs ( UnmanagedType.Interface )] IntPtr pbc, [In] ref Guid bhid, [In] ref Guid riid, out IntPtr ppv );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetParent ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetDisplayName ( [In] SIGDN sigdnName, [MarshalAs ( UnmanagedType.LPWStr )] out string ppszName );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetAttributes ( [In] uint sfgaoMask, out uint psfgaoAttribs );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void Compare ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, [In] uint hint, out int piOrder );
		}

		[DllImport ( "shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false )]
		static extern void SHCreateItemFromParsingName (
		[In, MarshalAs ( UnmanagedType.LPWStr )] string pszPath,
		[In] IntPtr pbc,
		[In, MarshalAs ( UnmanagedType.LPStruct )] Guid iIdIShellItem,
		[Out, MarshalAs ( UnmanagedType.Interface, IidParameterIndex = 2 )] out IShellItem iShellItem );

		[ComImport,
			Guid ( "947aab5f-0a5c-4c13-b4d6-4bf7836fc9f8" ),
			InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		interface IFileOperation
		{
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void Advise ( IntPtr pfops, IntPtr pdwCookie );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void Unadvise ( uint dwCookie );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetOperationFlags ( OperationFlag dwOperationFlags );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetProgressMessage ( [MarshalAs ( UnmanagedType.LPWStr )]string pszMessage );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetProgressDialog ( IntPtr popd );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetProperties ( IntPtr pproparray );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetOwnerWindow ( IntPtr hwndOwner );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void ApplyPropertiesToItem ( IShellItem psiItem );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void ApplyPropertiesToItems ( IntPtr punkItems );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long RenameItem ( IShellItem psiItem, [MarshalAs ( UnmanagedType.LPWStr )] string pszNewName, IntPtr pfopsItem );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long RenameItems ( IntPtr pUnkItems, [MarshalAs ( UnmanagedType.LPWStr )] string pszNewName );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long MoveItem ( IShellItem psiItem, IShellItem psiDestinationFolder,
				 [MarshalAs ( UnmanagedType.LPWStr )]string pszNewName, IntPtr pfopsItem );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long MoveItems ( IntPtr punkItems, IShellItem psiDestinationFolder );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long CopyItem ( IShellItem psiItem, IShellItem psiDestinationFolder,
				 [MarshalAs ( UnmanagedType.LPWStr )]string pszCopyName, IntPtr pfopsItem );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long CopyItems ( IntPtr punkItems, IShellItem psiDestinationFolder );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			long DeleteItem ( IShellItem psiItem, IntPtr pfopsItem );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void DeleteItems ( IntPtr punkItems );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void NewItem ( IShellItem psiDestinationFolder, uint dwFileAttributes, [MarshalAs ( UnmanagedType.LPWStr )] string pszName,
				 [MarshalAs ( UnmanagedType.LPWStr )] string pszTemplateName, IntPtr pfopsItem );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void PerformOperations ();
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetAnyOperationsAborted ( [MarshalAs ( UnmanagedType.Bool )] ref bool pfAnyOperationsAborted );
		}
		#endregion
	}
}
