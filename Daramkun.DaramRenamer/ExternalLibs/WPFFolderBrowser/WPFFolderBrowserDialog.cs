using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WPFFolderBrowser
{
	public sealed class WPFFolderBrowserDialog
	{
		private readonly Collection<string> fileNames;
		internal NativeDialogShowState showState = NativeDialogShowState.PreShow;

		private IFileDialog nativeDialog;
		private bool? canceled;
		private Window parentWindow;

		private const string IllegalPropertyChangeString = " cannot be changed while dialog is showing";

		#region Constructors
		public WPFFolderBrowserDialog ()
		{
			fileNames = new Collection<string> ();
		}

		public WPFFolderBrowserDialog ( string title )
			: this ()
		{
			Title = title;
		}
		#endregion
		
		private IFileOpenDialog openDialogCoClass;
		
		#region Public API

		private string title;
		public string Title
		{
			get { return title; }
			set
			{
				title = value;
				if ( NativeDialogShowing )
					nativeDialog.SetTitle ( value );
			}
		}

		private bool showPlacesList = true;
		public bool ShowPlacesList
		{

			get { return showPlacesList; }
			set
			{
				ThrowIfDialogShowing ( "ShowPlacesList" + IllegalPropertyChangeString );
				showPlacesList = value;
			}
		}

		private bool addToMruList = true;
		public bool AddToMruList
		{
			get { return addToMruList; }
			set
			{
				ThrowIfDialogShowing ( "AddToMruList" + IllegalPropertyChangeString );
				addToMruList = value;
			}
		}

		private bool showHiddenItems;
		public bool ShowHiddenItems
		{
			get { return showHiddenItems; }
			set
			{
				ThrowIfDialogShowing ( "ShowHiddenItems" + IllegalPropertyChangeString );
				showHiddenItems = value;
			}
		}

		private bool dereferenceLinks;
		public bool DereferenceLinks
		{
			get { return dereferenceLinks; }
			set
			{
				ThrowIfDialogShowing ( "DereferenceLinks" + IllegalPropertyChangeString );
				dereferenceLinks = value;
			}
		}

		private string fileName;
		public string FileName
		{
			get
			{
				CheckFileNamesAvailable ();
				if ( fileNames.Count > 1 )
					throw new InvalidOperationException ( "Multiple files selected - the FileNames property should be used instead" );
				fileName = fileNames [ 0 ];
				return fileNames [ 0 ];
			}
			set
			{
				fileName = value;
			}
		}
		
		public string InitialDirectory { get; set; }

		public bool? ShowDialog ( Window owner )
		{
			parentWindow = owner;
			return ShowDialog ();
		}

		public bool? ShowDialog ()
		{
			bool? result = null;

			try
			{
				openDialogCoClass = Activator.CreateInstance ( Type.GetTypeFromCLSID ( new Guid ( "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7" ) ) ) as IFileOpenDialog;
				nativeDialog = openDialogCoClass ?? throw new Exception ( "Must call Initialize() before fetching dialog interface" );
				ApplyNativeSettings ( nativeDialog );
				
				showState = NativeDialogShowState.Showing;
				int hresult = nativeDialog.Show ( parentWindow != null ? ( new WindowInteropHelper ( parentWindow ) ).Handle : IntPtr.Zero );
				showState = NativeDialogShowState.Closed;
				
				if ( ErrorHelper.Matches ( hresult, 1223 ) )
				{
					canceled = true;
					fileNames.Clear ();
				}
				else
				{
					canceled = false;
					if ( fileNames != null )
					{
						openDialogCoClass.GetResults ( out IShellItemArray resultsArray );
						resultsArray.GetCount ( out uint count );

						fileNames.Clear ();
						for ( int i = 0; i < count; i++ )
						{
							resultsArray.GetItemAt ( ( uint ) i, out IShellItem r );
							fileNames.Add ( GetFileNameFromShellItem ( r ) );
						}

						if ( count > 0 )
							FileName = fileNames [ 0 ];
					}
				}
				result = !canceled.Value;
			}
			catch { }
			finally
			{
				if ( openDialogCoClass != null )
					Marshal.ReleaseComObject ( openDialogCoClass );
				showState = NativeDialogShowState.Closed;
			}
			return result;
		}
		#endregion

		#region Configuration
		private void ApplyNativeSettings ( IFileDialog dialog )
		{
			Debug.Assert ( dialog != null, "No dialog instance to configure" );

			if ( parentWindow == null )
				if ( Application.Current != null && Application.Current.MainWindow != null )
					parentWindow = Application.Current.MainWindow;

			dialog.SetOptions ( CalculateNativeDialogOptionFlags () );
			dialog.SetTitle ( title );
			
			string directory = ( String.IsNullOrEmpty ( fileName ) ) ? InitialDirectory : System.IO.Path.GetDirectoryName ( fileName );
			
			if ( directory != null )
			{
				try
				{
					SHCreateItemFromParsingName ( directory, IntPtr.Zero, new Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ), out IShellItem folder );

					if ( folder != null )
						dialog.SetFolder ( folder );
				}
				finally { }
			}


			if ( !String.IsNullOrEmpty ( fileName ) )
			{
				string name = System.IO.Path.GetFileName ( fileName );
				dialog.SetFileName ( name );
			}
		}

		private FOS CalculateNativeDialogOptionFlags ()
		{
			FOS flags = FOS.NoTestFileCreate | FOS.ForceFileSystem;
			
			flags |= FOS.PickFolders;
			
			if ( !showPlacesList )
				flags |= FOS.HidePinnedPlaces;
			if ( !addToMruList )
				flags |= FOS.DontAddToRecent;
			if ( showHiddenItems )
				flags |= FOS.ForceShowHidden;
			if ( !dereferenceLinks )
				flags |= FOS.NoDeReferenceLinks;
			return flags;
		}
		#endregion

		#region Helpers
		private void CheckFileNamesAvailable ()
		{
			if ( showState != NativeDialogShowState.Closed )
				throw new InvalidOperationException ( "Filename not available - dialog has not closed yet" );
			if ( canceled.GetValueOrDefault () )
				throw new InvalidOperationException ( "Filename not available - dialog was canceled" );
			Debug.Assert ( fileNames.Count != 0,
					"FileNames empty - shouldn't happen dialog unless dialog canceled or not yet shown" );
		}

		private bool IsOptionSet ( IFileDialog dialog, FOS flag )
		{
			dialog.GetOptions ( out FOS currentFlags );
			return ( currentFlags & flag ) == flag;
		}
		#endregion

		#region Helpers
		private bool NativeDialogShowing => ( nativeDialog != null )
					&& ( showState == NativeDialogShowState.Showing ||
					showState == NativeDialogShowState.Closing );

		internal string GetFileNameFromShellItem ( IShellItem item )
		{
			item.GetDisplayName ( SIGDN.DesktopAbsoluteParsing, out string filename );
			return filename;
		}

		private void ThrowIfDialogShowing ( string message )
		{
			if ( NativeDialogShowing )
				throw new NotSupportedException ( message );
		}
		#endregion

		#region Interop
		internal enum NativeDialogShowState
		{
			PreShow,
			Showing,
			Closing,
			Closed
		}

		[StructLayout ( LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4 )]
		internal struct COMDLG_FILTERSPEC
		{
			[MarshalAs ( UnmanagedType.LPWStr )]
			internal string pszName;
			[MarshalAs ( UnmanagedType.LPWStr )]
			internal string pszSpec;
		}

		internal enum FDAP
		{
			Bottom = 0x00000000,
			Top = 0x00000001,
		}
		
		internal enum ShareViolationResponse
		{
			Default = 0x00000000,
			Accept = 0x00000001,
			Refuse = 0x00000002
		}
		
		internal enum OverwriteResponse
		{
			Default = 0x00000000,
			Accept = 0x00000001,
			Refuse = 0x00000002
		}

		internal enum SIATTRIBFLAGS
		{
			And = 0x00000001,
			Or = 0x00000002,
			AppCompat = 0x00000003,
		}
		
		internal enum SIGDN : uint
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

		//wpffb used
		[Flags]
		internal enum FOS : uint
		{
			OverWritePrompt = 0x00000002,
			StrictFileTypes = 0x00000004,
			NoChangeDir = 0x00000008,
			PickFolders = 0x00000020,
			ForceFileSystem = 0x00000040,
			AllNonStorageItems = 0x00000080,
			NoValidate = 0x00000100,
			AllowMultiselect = 0x00000200,
			PathMustExist = 0x00000800,
			FileMustExist = 0x00001000,
			CreatePrompt = 0x00002000,
			ShareAware = 0x00004000,
			NoReadOnlyReturn = 0x00008000,
			NoTestFileCreate = 0x00010000,
			HideMRUPlaces = 0x00020000,
			HidePinnedPlaces = 0x00040000,
			NoDeReferenceLinks = 0x00100000,
			DontAddToRecent = 0x02000000,
			ForceShowHidden = 0x10000000,
			DefaultNoMiniMode = 0x20000000
		}

		internal enum CDCONTROLSTATE
		{
			Inactive = 0x00000000,
			Enabled = 0x00000001,
			Visible = 0x00000002
		}

		[StructLayout ( LayoutKind.Sequential, Pack = 4 )]
		internal struct PROPERTYKEY
		{
			internal Guid fmtid;
			internal uint pid;
		}

		[DllImport ( "shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false )]
		internal static extern void SHCreateItemFromParsingName (
		[In, MarshalAs ( UnmanagedType.LPWStr )] string pszPath,
		[In] IntPtr pbc,
		[In, MarshalAs ( UnmanagedType.LPStruct )] Guid iIdIShellItem,
		[Out, MarshalAs ( UnmanagedType.Interface, IidParameterIndex = 2 )] out IShellItem iShellItem );

		internal static class ErrorHelper
		{
			private const uint FACILITY_WIN32 = 7;

			internal const int IGNORED = ( int ) HRESULT.S_OK;

			internal static int HResultFromWin32 ( int win32ErrorCode )
			{
				if ( win32ErrorCode > 0 )
				{
					win32ErrorCode =
						( int ) ( ( win32ErrorCode & 0x0000FFFF ) | ( FACILITY_WIN32 << 16 ) | 0x80000000 );
				}
				return win32ErrorCode;
			}

			internal static bool Matches ( int hresult, int win32ErrorCode )
			{
				return ( hresult == HResultFromWin32 ( win32ErrorCode ) );
			}

			internal static bool Succeeded ( int hresult ) { return ( hresult >= 0 ); }
			internal static bool Failed ( HRESULT hresult ) { return ( ( int ) hresult < 0 ); }

			internal static COMException CreateException ( int hresult )
			{
				return new COMException ( "Unknown COM exception", hresult );
			}
		}

		internal enum HRESULT : long
		{
			S_FALSE = 0x0001,
			S_OK = 0x0000,
			E_INVALIDARG = 0x80070057,
			E_OUTOFMEMORY = 0x8007000E
		}

		[ComImport (),
		Guid ( "b4db1657-70d7-485e-8e3e-6fcb5a5c1802" ),
		InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		internal interface IModalWindow
		{
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ),
			PreserveSig]
			int Show ( [In] IntPtr parent );
		}

		[ComImport (),
		Guid ( "42f85136-db7e-439c-85f1-e4075d135fc8" ),
		InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		internal interface IFileDialog : IModalWindow
		{
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ),
			PreserveSig]
			new int Show ( [In] IntPtr parent );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetFileTypes ( [In] uint cFileTypes, [In] ref COMDLG_FILTERSPEC rgFilterSpec );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetFileTypeIndex ( [In] uint iFileType );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetFileTypeIndex ( out uint piFileType );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void Advise ( IntPtr pfde, out uint pdwCookie );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void Unadvise ( [In] uint dwCookie );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetOptions ( [In] FOS fos );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetOptions ( out FOS pfos );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetDefaultFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetFolder ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetCurrentSelection ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetFileName ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszName );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetFileName ( [MarshalAs ( UnmanagedType.LPWStr )] out string pszName );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetTitle ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszTitle );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetOkButtonLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetFileNameLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetResult ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void AddPlace ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, FDAP fdap );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetDefaultExtension ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszDefaultExtension );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void Close ( [MarshalAs ( UnmanagedType.Error )] int hr );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetClientGuid ( [In] ref Guid guid );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void ClearClientData ();
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void SetFilter ( [MarshalAs ( UnmanagedType.Interface )] IntPtr pFilter );
		}

		[ComImport (),
		Guid ( "d57c7288-d4ad-4768-be02-9d969532d960" ),
		InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		internal interface IFileOpenDialog : IFileDialog
		{
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime ),
			PreserveSig]
			new int Show ( [In] IntPtr parent );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetFileTypes ( [In] uint cFileTypes, [In] ref COMDLG_FILTERSPEC rgFilterSpec );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetFileTypeIndex ( [In] uint iFileType );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void GetFileTypeIndex ( out uint piFileType );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void Advise ( IntPtr pfde, out uint pdwCookie );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void Unadvise ( [In] uint dwCookie );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetOptions ( [In] FOS fos );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void GetOptions ( out FOS pfos );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetDefaultFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetFolder ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void GetFolder ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void GetCurrentSelection ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetFileName ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszName );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void GetFileName ( [MarshalAs ( UnmanagedType.LPWStr )] out string pszName );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetTitle ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszTitle );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetOkButtonLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszText );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetFileNameLabel ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszLabel );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void GetResult ( [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void AddPlace ( [In, MarshalAs ( UnmanagedType.Interface )] IShellItem psi, FDAP fdap );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetDefaultExtension ( [In, MarshalAs ( UnmanagedType.LPWStr )] string pszDefaultExtension );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void Close ( [MarshalAs ( UnmanagedType.Error )] int hr );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetClientGuid ( [In] ref Guid guid );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void ClearClientData ();
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			new void SetFilter ( [MarshalAs ( UnmanagedType.Interface )] IntPtr pFilter );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetResults ( [MarshalAs ( UnmanagedType.Interface )] out IShellItemArray ppenum );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetSelectedItems ( [MarshalAs ( UnmanagedType.Interface )] out IShellItemArray ppsai );
		}

		[ComImport,
		Guid ( "43826D1E-E718-42EE-BC55-A1E261C37BFE" ),
		InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		internal interface IShellItem
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

		[ComImport,
		Guid ( "B63EA76D-1F85-456F-A19C-48159EFA858B" ),
		InterfaceType ( ComInterfaceType.InterfaceIsIUnknown )]
		internal interface IShellItemArray
		{
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void BindToHandler ( [In, MarshalAs ( UnmanagedType.Interface )] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, out IntPtr ppvOut );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetPropertyStore ( [In] int Flags, [In] ref Guid riid, out IntPtr ppv );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetPropertyDescriptionList ( [In] ref PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetAttributes ( [In] SIATTRIBFLAGS dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetCount ( out uint pdwNumItems );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void GetItemAt ( [In] uint dwIndex, [MarshalAs ( UnmanagedType.Interface )] out IShellItem ppsi );
			[MethodImpl ( MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime )]
			void EnumItems ( [MarshalAs ( UnmanagedType.Interface )] out IntPtr ppenumShellItems );
		}
		#endregion
	}
}
