using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daramkun.DaramRenamer.ShellExtension.Installer
{
	public class ShellExtensionInstaller
	{
		public static void Main ( string [] args )
		{
			if ( ( args.Length == 1 && args [ 0 ] == "--uninstall" )
				|| ( args.Length == 2 && ( args [ 0 ] == "--uninstall" || args [ 1 ] == "--uninstall" ) ) )
			{
				RenamerContextMenu.Uninstall ();
				Console.WriteLine ( "Uninstall complete." );
				RestartQuestion ( args, "--uninstall" );
			}
			else if ( args.Length == 0 || ( args.Length == 1 && args [ 0 ] == "--install" ) ||
				( args.Length == 2 && ( args [ 0 ] == "--install" || args [ 1 ] == "--install" ) ) )
			{
				RenamerContextMenu.Install ();
				Console.WriteLine ( "Install complete." );

				RestartQuestion ( args, "--install" );
			}
			else if ( args.Length == 1 && args [ 0 ] == "--isinstalled" )
			{
				Console.WriteLine ( RenamerContextMenu.IsInstalled () ? "Installed" : "Not Installed." );
			}
			else throw new ArgumentException ();
		}

		private static void RestartQuestion ( string [] args, string v )
		{
			if ( args.Length <= 1 )
			{
				bool reask = true;
				do
				{
					Console.Write ( "Are you want to restart Explorer?(y/N)>" );
					char pressed = Console.ReadKey ().KeyChar;
					switch ( pressed )
					{
						case 'y':
						case 'Y':
							{
								RestartExplorer ();
								reask = false;
							}
							break;

						case 'n':
						case 'N':
							reask = false;
							break;
					}
				}
				while ( reask );
			}
			else
			{
				if ( ( args [ 0 ] == v && ( args [ 1 ] == "y" || args [ 1 ] == "Y" ) )
					|| ( args [ 1 ] == v && ( args [ 0 ] == "y" || args [ 0 ] == "Y" ) ) )
					RestartExplorer ();
			}
		}

		private static void RestartExplorer ()
		{
			foreach ( Process p in Process.GetProcesses () )
			{
				try
				{
					if ( p.MainModule.FileName.ToLower ().EndsWith ( ":\\windows\\explorer.exe" ) )
					{
						p.Kill ();
						break;
					}
				}
				catch ( Exception ex )
				{
					Debug.WriteLine ( ex );
					return;
				}
			}

			if ( Environment.Version < new Version ( 6, 2 ) )
				Process.Start ( "explorer.exe" );
		}
	}
}
