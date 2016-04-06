using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

using JsonSerializer = System.Runtime.Serialization.Json.DataContractJsonSerializer;
using JsonSerializerSettings = System.Runtime.Serialization.Json.DataContractJsonSerializerSettings;

namespace Daramkun.DaramRenamer
{
	[DataContract]
	public class GlobalCulture
	{
		[DataMember ( Name = "version" )]
		public string Version;
		[DataMember ( Name = "target" )]
		public string Target;
		[DataMember ( Name = "culture" )]
		public string Culture;
		[DataMember ( Name = "author" )]
		public string Author;
		[DataMember ( Name = "contact", IsRequired = false )]
		public string Contact;
		[DataMember ( Name = "contents" )]
		public Dictionary<string, string> Contents;
	}

	[DataContract]
	class GlobalizationContainer
	{
		[DataMember ( Name = "lang" )]
		public List<GlobalCulture> Languages = new List<GlobalCulture> ();
	}

	[AttributeUsage ( AttributeTargets.Property, AllowMultiple = false, Inherited = true )]
	public class GlobalizedAttribute : Attribute
	{
		public string Field { get; set; }
		public uint Order { get; set; }
		public GlobalizedAttribute ( string field, uint order = 0 ) { Field = field; Order = order; }
	}

	public static class Globalizer
	{
		const string Namespace = "Daramkun.DaramRenamer";
		const string Target = "daram_renamer";

		static Dictionary<CultureInfo, GlobalCulture> Cultures = new Dictionary<CultureInfo, GlobalCulture> ();

		public static GlobalCulture Culture
		{
			get
			{
				if ( Cultures.ContainsKey ( CultureInfo.CurrentUICulture ) )
					return Cultures [ CultureInfo.CurrentUICulture ];
				else return Cultures [ CultureInfo.GetCultureInfo ( "en-US" ) ];
			}
		}
		public static Dictionary<string, string> Strings { get { return Culture.Contents; } }

		static Globalizer ()
		{
			var json = new JsonSerializer ( typeof ( GlobalCulture ), new JsonSerializerSettings () { UseSimpleDictionaryFormat = true } );
			var json2 = new JsonSerializer ( typeof ( GlobalizationContainer ), new JsonSerializerSettings () { UseSimpleDictionaryFormat = true } );

			var queue = new Queue<GlobalCulture> ();
			
			foreach ( var ci in CultureInfo.GetCultures ( CultureTypes.AllCultures ) )
			{
				var globalizationFiles = new string [] {
					$".\\Globalizations\\Globalization.{ci}.json",
					$".\\Globalizations\\Globalization.{Target}.{ci}.json",
					$"Globalization.{ci}.json",
					$"Globalization.{Target}.{ci}.json",
					$".\\Localization\\Localization.{ci}.json",
					$".\\Localization\\Localization.{Target}.{ci}.json",
					$"Localization.{ci}.json",
					$"Localization.{Target}.{ci}.json"
				};
				foreach ( var globalizationFile in globalizationFiles )
				{
					Stream gs = null;
					if ( File.Exists ( globalizationFile ) ) gs = new FileStream ( globalizationFile, FileMode.Open );
					else continue;

					gs.Position = 3;
					var igc = json.ReadObject ( gs ) as GlobalCulture;
					if ( igc.Target != Target ) continue;
					queue.Enqueue ( igc );

					gs.Dispose ();
				}
            }

			var globalizationContainerFiles = new string [] {
				".\\Globalizations\\Globalization.json",
				$".\\Globalizations\\Globalization.{Target}.json",
				"Globalization.json",
				$"Globalization.{Target}.json",
				".\\Localization\\Localization.json",
				$".\\Localization\\Localization.{Target}.json",
				"Localization.json",
				$"Localization.{Target}.json"
			};
			foreach ( var globalizationContainerFile in globalizationContainerFiles )
			{
				Stream gs2 = null;
				if ( File.Exists ( globalizationContainerFile ) )
					gs2 = new FileStream ( globalizationContainerFile, FileMode.Open );

				if ( gs2 != null )
				{
					gs2.Position = 3;
					var iggc = json2.ReadObject ( gs2 ) as GlobalizationContainer;
					foreach ( var l in iggc.Languages )
						if ( !Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( l.Culture ) ) )
							if ( l.Target != Target ) continue;
							else queue.Enqueue ( l );
				}
			}

			using ( var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ( $"{Namespace}.Globalization.json" ) )
			{
				stream.Position = 3;
				var ggc = json2.ReadObject ( stream ) as GlobalizationContainer;
				foreach ( var l in ggc.Languages )
					if ( !Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( l.Culture ) ) )
						if ( l.Target != Target ) continue;
						else queue.Enqueue ( l );
			}

			while ( queue.Count > 0 )
			{
				var g = queue.Dequeue ();
				var cultureInfo = CultureInfo.GetCultureInfo ( g.Culture );
				if ( Cultures.ContainsKey ( cultureInfo ) )
				{
					if ( new Version ( Cultures [ cultureInfo ].Version ) < new Version ( g.Version ) )
						Cultures [ cultureInfo ] = g;
				}
				else Cultures.Add ( cultureInfo, g );
			}
		}
	}
}
