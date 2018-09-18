using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Daramkun.DaramRenamer.Extension
{
	public class ToolBarIconExtension
	{
		static ToolBarIconExtension sharedExtension;
		public static ToolBarIconExtension SharedExtension
		{
			get => sharedExtension ?? new ToolBarIconExtension ();
			set => sharedExtension = value;
		}

		public object OpenIcon { get; private set; }
		public object DeleteIcon { get; private set; }
		public object ApplyIcon { get; private set; }

		public object UndoIcon { get; private set; }
		public object RedoIcon { get; private set; }

		public object ItemUpIcon { get; private set; }
		public object ItemDownIcon { get; private set; }
		public object ItemSortIcon { get; private set; }

		public object ReplaceTextIcon { get; private set; }
		public object ConcatTextIcon { get; private set; }
		public object TrimTextIcon { get; private set; }
		public object DeleteBlockIcon { get; private set; }
		public object DeleteTextIcon { get; private set; }
		public object SubstringIcon { get; private set; }
		public object CasecastTextIcon { get; private set; }

		public object AddExtensionIcon { get; private set; }
		public object DeleteExtensionIcon { get; private set; }
		public object ReplaceExtensionIcon { get; private set; }
		public object CasecastExtensionIcon { get; private set; }

		public object DeleteWithoutNumbersIcon { get; private set; }
		public object MatchNumberCountIcon { get; private set; }
		public object AddIndexIcon { get; private set; }
		public object IncrementDecrementNumberIcon { get; private set; }

		public object AddDateIcon { get; private set; }

		public ToolBarIconExtension ( string name = null )
		{
			if ( name != null && File.Exists ( $"{name}.zip" ) )
			{
				using ( Stream fs = new FileStream ( $"{name}.zip", FileMode.Open ) )
				{
					using ( ZipArchive archive = new ZipArchive ( fs, ZipArchiveMode.Read, true ) )
					{
						foreach ( var entry in archive.Entries )
						{
							string entryName = Path.GetFileNameWithoutExtension ( entry.Name );

							if ( entryName == "README" || entryName == "VERSION" || entryName == "LICENSE" )
								continue;

							object icon = DecodeEntry ( entry );
							switch ( entryName )
							{
								case "open": OpenIcon = icon; break;
								case "clear": DeleteIcon = icon; break;
								case "apply": ApplyIcon = icon; break;

								case "undo": UndoIcon = icon; break;
								case "redo": RedoIcon = icon; break;

								case "item_up": ItemUpIcon = icon; break;
								case "item_down": ItemDownIcon = icon; break;
								case "item_sort": ItemSortIcon = icon; break;

								case "replace_text": ReplaceTextIcon = icon; break;
								case "concat_text": ConcatTextIcon = icon; break;
								case "trim_text": TrimTextIcon = icon; break;
								case "delete_block": DeleteBlockIcon = icon; break;
								case "delete_text": DeleteTextIcon = icon; break;
								case "substring": SubstringIcon = icon; break;
								case "casecast_text": CasecastTextIcon = icon; break;

								case "add_ext": AddExtensionIcon = icon; break;
								case "delete_ext": DeleteExtensionIcon = icon; break;
								case "replace_ext": ReplaceExtensionIcon = icon; break;
								case "casecast_ext": CasecastExtensionIcon = icon; break;

								case "del_without_num": DeleteWithoutNumbersIcon = icon; break;
								case "match_num_count": MatchNumberCountIcon = icon; break;
								case "add_index": AddIndexIcon = icon; break;
								case "inc_dec_num": IncrementDecrementNumberIcon = icon; break;

								case "add_date": AddDateIcon = icon; break;
							}
						}
					}
				}
			}
			else
			{
				OpenIcon = App.Current.Resources [ "iconOpenButton" ];
				DeleteIcon = App.Current.Resources [ "iconDeleteButton" ];
				ApplyIcon = App.Current.Resources [ "iconApplyButton" ];

				UndoIcon = App.Current.Resources [ "iconUndoButton" ];
				RedoIcon = App.Current.Resources [ "iconRedoButton" ];

				ItemUpIcon = App.Current.Resources [ "iconItemUp" ];
				ItemDownIcon = App.Current.Resources [ "iconItemDown" ];
				ItemSortIcon = App.Current.Resources [ "iconItemSort" ];

				ReplaceTextIcon = App.Current.Resources [ "iconReplaceText" ];
				ConcatTextIcon = App.Current.Resources [ "iconConcatText" ];
				TrimTextIcon = App.Current.Resources [ "iconTrimText" ];
				DeleteBlockIcon = App.Current.Resources [ "iconDeleteBlock" ];
				DeleteTextIcon = App.Current.Resources [ "iconDeleteText" ];
				SubstringIcon = App.Current.Resources [ "iconSubstringText" ];
				CasecastTextIcon = App.Current.Resources [ "iconCasecastText" ];

				AddExtensionIcon = App.Current.Resources [ "iconAddExtension" ];
				DeleteExtensionIcon = App.Current.Resources [ "iconDeleteExtension" ];
				ReplaceExtensionIcon = App.Current.Resources [ "iconReplaceExtension" ];
				CasecastExtensionIcon = App.Current.Resources [ "iconCasecastExtension" ];

				DeleteWithoutNumbersIcon = App.Current.Resources [ "iconDeleteWithoutNumber" ];
				MatchNumberCountIcon = App.Current.Resources [ "iconMatchNumberCount" ];
				AddIndexIcon = App.Current.Resources [ "iconAddIndex" ];
				IncrementDecrementNumberIcon = App.Current.Resources [ "iconIncreaseDecreaseNumber" ];

				AddDateIcon = App.Current.Resources [ "iconAddDate" ];
			}

			SharedExtension = this;
		}

		private object DecodeEntry ( ZipArchiveEntry entry )
		{
			var ext = Path.GetExtension ( entry.FullName );
			if ( ext == ".png" )
			{
				BitmapImage image = new BitmapImage ();
				image.BeginInit ();
				image.StreamSource = entry.Open ();
				image.EndInit ();
				//image.Freeze ();

				var element = new Image
				{
					Source = image,
					VerticalAlignment = System.Windows.VerticalAlignment.Center,
					HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
					Stretch = System.Windows.Media.Stretch.None,
				};

				return element;
			}
			else if ( ext == ".txt" )
			{
				using ( Stream stream = entry.Open () )
				{
					using ( StreamReader reader = new StreamReader ( stream, Encoding.ASCII, false, 256, true ) )
					{
						return System.Windows.Media.Geometry.Parse ( reader.ReadToEnd () );
					}
				}
			}
			return null;
		}
	}
}
