using Daramee.DaramCommonLib;
using Daramkun.DaramRenamer.Processors;
using Daramkun.DaramRenamer.Processors.Date;
using Daramkun.DaramRenamer.Processors.Extension;
using Daramkun.DaramRenamer.Processors.Filename;
using Daramkun.DaramRenamer.Processors.FilePath;
using Daramkun.DaramRenamer.Processors.Number;
using Daramkun.DaramRenamer.Processors.Tag;
using SharpShell.Attributes;
using SharpShell.ServerRegistration;
using SharpShell.SharpContextMenu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace Daramkun.DaramRenamer
{
	[ComVisible ( true )]
	[COMServerAssociation ( AssociationType.AllFiles )]
	[COMServerAssociation ( AssociationType.Directory )]
	public class RenamerContextMenu : SharpContextMenu
	{
		protected override bool CanShowMenu () { return true; }

		public new string DisplayName => "DaramRenamer Shell Extension Context Menu";

		protected override ContextMenuStrip CreateMenu ()
		{
			ProgramHelper.Initialize ( Assembly.GetAssembly ( typeof ( MainWindow ) ),
				"daramkun", "DaramRenamer" );
			var localizer = new StringTable ();

			var menu = new ContextMenuStrip ();
			var renamerMenuItem = menu.Items.Add ( localizer.Strings [ "daram_renamer" ] ) as ToolStripMenuItem;

			var filenameProcessors = renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "filename_processes" ] ) as ToolStripMenuItem;
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_replace_plain_text" ], null, ( sender, e ) => { ShowPopup<ReplacePlainProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_replace_regex_text" ], null, ( sender, e ) => { ShowPopup<ReplaceRegexpProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_rearrange_regex_text" ], null, ( sender, e ) => { ShowPopup<RearrangeRegexpProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_concatenate_text" ], null, ( sender, e ) => { ShowPopup<ConcatenateProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_trimming_text" ], null, ( sender, e ) => { ShowPopup<TrimmingProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_delete_block" ], null, ( sender, e ) => { ShowPopup<DeleteBlockProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_delete_text" ], null, ( sender, e ) =>
			{
				InitializeFileList ();
				Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new DeleteFilenameProcessor ().Process ( fileInfo ) );
			} );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_substring_text" ], null, ( sender, e ) => { ShowPopup<SubstringProcessor> (); } );
			filenameProcessors.DropDownItems.Add ( localizer.Strings [ "process_casecast_text" ], null, ( sender, e ) => { ShowPopup<CasecastProcessor> (); } );

			var extensionProcessors = renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "extension_processes" ] ) as ToolStripMenuItem;
			extensionProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_extension" ], null, ( sender, e ) => { ShowPopup<AddExtensionProcessor> (); } );
			extensionProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_extension_automatically" ], null, ( sender, e ) =>
			{
				InitializeFileList ();
				Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new AddExtensionAutomatedProcessor ().Process ( fileInfo ) );
			} );
			extensionProcessors.DropDownItems.Add ( localizer.Strings [ "process_delete_extension" ], null, ( sender, e ) =>
			{
				InitializeFileList ();
				Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new DeleteExtensionProcessor ().Process ( fileInfo ) );
			} );
			extensionProcessors.DropDownItems.Add ( localizer.Strings [ "process_change_extension" ], null, ( sender, e ) => { ShowPopup<ReplaceExtensionProcessor> (); } );
			extensionProcessors.DropDownItems.Add ( localizer.Strings [ "process_casecast_extension" ], null, ( sender, e ) => { ShowPopup<CasecastExtensionProcessor> (); } );

			var numberProcessors = renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "number_processes" ] ) as ToolStripMenuItem;
			numberProcessors.DropDownItems.Add ( localizer.Strings [ "process_delete_without_numbers" ], null, ( sender, e ) => { ShowPopup<DeleteWithoutNumbersProcessor> (); } );
			numberProcessors.DropDownItems.Add ( localizer.Strings [ "process_matching_number_count" ], null, ( sender, e ) => { ShowPopup<NumberCountMatchProcessor> (); } );
			numberProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_index_numbers" ], null, ( sender, e ) => { ShowPopup<AddIndexNumberProcessor> (); } );
			numberProcessors.DropDownItems.Add ( localizer.Strings [ "process_increase_decrease_numbers" ], null, ( sender, e ) => { ShowPopup<IncreaseDecreaseNumbersProcessor> (); } );

			var dateProcessors = renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "date_processes" ] ) as ToolStripMenuItem;
			dateProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_date" ], null, ( sender, e ) => { ShowPopup<AddDateProcessor> (); } );
			dateProcessors.DropDownItems.Add ( localizer.Strings [ "process_delete_date" ], null, ( sender, e ) =>
			{
				InitializeFileList ();
				Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => new DeleteDateProcessor ().Process ( fileInfo ) );
			} );
			//dateProcessors.DropDownItems.Add ( localizer.Strings [ "process_increase_decrease_date" ], null, ( sender, e ) => { } );

			var pathProcessors = renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "path_processes" ] ) as ToolStripMenuItem;
			pathProcessors.DropDownItems.Add ( localizer.Strings [ "process_change_path" ], null, ( sender, e ) => { ShowPopup<ChangePathProcessor> (); } );
			//pathProcessors.DropDownItems.Add ( localizer.Strings [ "process_move_path_relative" ], null, ( sender, e ) => { } );

			var propertyProcessors = renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "properties_processes" ] ) as ToolStripMenuItem;
			propertyProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_media_tag" ], null, ( sender, e ) => { ShowPopup<AddMediaTagProcessor> (); } );
			propertyProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_document_tag" ], null, ( sender, e ) => { ShowPopup<AddDocumentTagProcessor> (); } );
			propertyProcessors.DropDownItems.Add ( localizer.Strings [ "process_add_file_hash" ], null, ( sender, e ) => { ShowPopup<AddHashProcessor> (); } );

			renamerMenuItem.DropDownItems.Add ( localizer.Strings [ "process_batch_process" ], null, ( sender, e ) => { ShowPopup<BatchProcessor> (); } );

			return menu;
		}

		private void InitializeFileList ()
		{
			FileInfo.Files = new ObservableCollection<FileInfo> ();
			Parallel.ForEach ( SelectedItemPaths, ( path ) =>
			{
				AddItem ( path );
			} );
		}

		Form window = null;
		ISubWindow subWindow = null;

		public void ShowPopup<T> ( params object [] args ) where T : IProcessor
		{
			try
			{
				T processor = Activator.CreateInstance<T> ();
				subWindow = ( processor is BatchProcessor )
					? new SubWindow_Batch ( false ) as ISubWindow
					: new SubWindow ( processor, false );
				var windowControl = subWindow as System.Windows.Controls.UserControl;
				windowControl.Measure ( new Size ( double.PositiveInfinity, double.PositiveInfinity ) );
				subWindow.OKButtonClicked += SubWindow_OKButtonClicked;
				subWindow.CancelButtonClicked += SubWindow_CancelButtonClicked;

				window = new Form ()
				{
					ClientSize = new System.Drawing.Size ( ( int ) windowControl.DesiredSize.Width, ( int ) windowControl.DesiredSize.Height ),
					StartPosition = FormStartPosition.CenterParent,
					SizeGripStyle = SizeGripStyle.Hide,
					FormBorderStyle = FormBorderStyle.FixedSingle,
					Font = System.Drawing.SystemFonts.DefaultFont,
					MaximizeBox = false,
					MinimizeBox = false,
					ShowIcon = false,
				};
				ElementHost host = new ElementHost () { Child = subWindow as UIElement };
				host.Dock = DockStyle.Fill;
				window.Dock = DockStyle.Fill;
				window.Controls.Add ( host );

				window.Show ();
			}
			catch ( Exception ex )
			{
				System.Windows.MessageBox.Show ( $"알 수 없는 오류가 발생했습니다.\n{ex}",
					"오류", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		public void ClosePopup ( bool apply = false )
		{
			try
			{
				if ( apply )
				{
					var processor = subWindow.Processor;
					InitializeFileList ();
					if ( !processor.CannotMultithreadProcess )
						Parallel.ForEach<FileInfo> ( FileInfo.Files, ( fileInfo ) => processor.Process ( fileInfo ) );
					else foreach ( var fileInfo in FileInfo.Files ) processor.Process ( fileInfo );

					Daramee.Winston.File.Operation.Begin ( true );
					Parallel.ForEach ( FileInfo.Files, ( fileInfo ) => {
						if ( !fileInfo.Move ( false, out ErrorCode errorCode ) )
						{

						}
					} );
					Daramee.Winston.File.Operation.End ();
				}
				window.Close ();
				window.Dispose ();
				window = null;
				subWindow = null;
			}
			catch ( Exception ex )
			{
				System.Windows.MessageBox.Show ( $"알 수 없는 오류가 발생했습니다.\n{ex}",
					"오류", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		private void SubWindow_OKButtonClicked ( object sender, RoutedEventArgs e ) { ClosePopup ( true ); }
		private void SubWindow_CancelButtonClicked ( object sender, RoutedEventArgs e ) { ClosePopup (); }
		
		public void AddItem ( string s )
		{
			if ( System.IO.File.Exists ( s ) )
				FileInfo.Files.Add ( new FileInfo ( s ) );
			else
				foreach ( string ss in System.IO.Directory.GetFiles ( s, "*.*", SearchOption.AllDirectories ) )
					AddItem ( ss );
		}

		public static void Install ()
		{
			var server = new RenamerContextMenu ();
			var registrationType = Environment.Is64BitOperatingSystem ? RegistrationType.OS64Bit : RegistrationType.OS32Bit;
			ServerRegistrationManager.InstallServer ( server, registrationType, true );
			ServerRegistrationManager.RegisterServer ( server, registrationType );
		}

		public static void Uninstall ()
		{
			var server = new RenamerContextMenu ();
			var registrationType = Environment.Is64BitOperatingSystem ? RegistrationType.OS64Bit : RegistrationType.OS32Bit;
			ServerRegistrationManager.UnregisterServer ( server, registrationType );
			ServerRegistrationManager.UninstallServer ( server, registrationType );
		}

		public static bool IsInstalled ()
		{
			try
			{
				var server = new RenamerContextMenu ();
				var registrationType = Environment.Is64BitOperatingSystem ? RegistrationType.OS64Bit : RegistrationType.OS32Bit;
				var info = ServerRegistrationManager.GetServerRegistrationInfo ( server, registrationType );
				if ( info == null ) return false;
			}
			catch { return false; }
			return true;
		}
	}
}
