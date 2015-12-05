﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Daramkun.DaramRenamer
{
	[DataContract]
	public class GlobalCulture
	{
		[DataMember ( Name = "culture" )]
		public string Culture;
		[DataMember ( Name = "author" )]
		public string Author;
		[DataMember ( Name = "contents" )]
		public Dictionary<string, string> Contents;
	}

	[DataContract]
	class GlobalizationContainer
	{
		[DataMember ( Name = "lang" )]
		public List<GlobalCulture> Languages = new List<GlobalCulture> ();
	}

	public static class Globalizer
	{
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
			var json = new DataContractJsonSerializer ( typeof ( GlobalCulture ), new DataContractJsonSerializerSettings ()
			{
				UseSimpleDictionaryFormat = true
			} );
			var json2 = new DataContractJsonSerializer ( typeof ( GlobalizationContainer ), new DataContractJsonSerializerSettings ()
			{
				UseSimpleDictionaryFormat = true
			} );

			foreach ( var ci in CultureInfo.GetCultures ( CultureTypes.AllCultures ) )
			{
				Stream gs = null;
				if ( File.Exists ( $".\\Globalizations\\Globalization.{ci.DisplayName}.json" ) )
					gs = new FileStream ( $".\\Globalizations\\Globalization.{ci.DisplayName}.json", FileMode.Open );
				else if ( File.Exists ( $"Globalization.{ci.DisplayName}.json" ) )
					gs = new FileStream ( $"Globalization.{ci.DisplayName}.json", FileMode.Open );
				else
				{
					if ( File.Exists ( $".\\Globalizations\\Globalization.{ci.DisplayName}.json" ) )
						gs = new FileStream ( $".\\Globalizations\\Globalization.{ci.DisplayName}.json", FileMode.Open );
					else if ( File.Exists ( $"Globalization.{ci.DisplayName}.json" ) )
						gs = new FileStream ( $"Globalization.{ci.DisplayName}.json", FileMode.Open );
					else continue;

					gs.Position = 3;
					var iggc = json2.ReadObject ( gs ) as GlobalizationContainer;
					foreach ( var l in iggc.Languages )
						if ( !Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( l.Culture ) ) )
							Cultures.Add ( CultureInfo.GetCultureInfo ( l.Culture ), l );

					continue;
				}

				gs.Position = 3;
				var igc =  json.ReadObject ( gs ) as GlobalCulture;
				Cultures.Add ( ci, igc );
            }

			using ( var stream = Assembly.GetExecutingAssembly ().GetManifestResourceStream ( "Daramkun.DaramRenamer.Globalization.json" ) )
			{
				stream.Position = 3;
				var ggc = json2.ReadObject ( stream ) as GlobalizationContainer;
				foreach ( var l in ggc.Languages )
					if ( !Cultures.ContainsKey ( CultureInfo.GetCultureInfo ( l.Culture ) ) )
						Cultures.Add ( CultureInfo.GetCultureInfo ( l.Culture ), l );
			}
		}
	}
}