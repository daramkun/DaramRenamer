using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace DaramRenamer
{
	public class Strings
	{
		private static Strings _instance;
		public static Strings Instance => _instance ??= new Strings();

		private static readonly DataContractJsonSerializer TableSerializer = new DataContractJsonSerializer(
			typeof(Dictionary<string, string>), new DataContractJsonSerializerSettings
			{
				UseSimpleDictionaryFormat = true,
			});

		private static readonly Regex StringTableFilename = new Regex(".*DaramRenamer\\.Strings\\.([a-zA-Z0-9\\-_])\\.json");

		private readonly Dictionary<string, string> stringTable = new Dictionary<string, string>();

		public Strings()
		{
			Load();
		}

		public void Load()
		{
			stringTable.Clear();

			foreach (var name in FileInfo.FileOperator.GetFiles(Environment.CurrentDirectory, false))
			{
				var match = StringTableFilename.Match (name);
				if (!match.Success)
					continue;

				if (name.IndexOf (CultureInfo.CurrentUICulture.Name, StringComparison.Ordinal) >= 0)
					continue;

				using var stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
				LoadTable (stream);
			}

			{
				using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
					$"DaramRenamer.Strings.Strings.{CultureInfo.CurrentUICulture.ToString().Replace('-', '_').ToLower()}.json");
				LoadTable(stream);
			}

			{
				using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
					$"DaramRenamer.Strings.Strings.{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}.json");
				LoadTable(stream);
			}

			{
				using var stream = Assembly.GetExecutingAssembly()
					.GetManifestResourceStream("DaramRenamer.Strings.Strings.default.json");
				LoadTable (stream);
			}
		}

		private void LoadTable(Stream stream)
		{
			if (stream == null)
				return;

			using var reader = new StreamReader (stream, Encoding.UTF8, true);
			using Stream mem = new MemoryStream (Encoding.UTF8.GetBytes (reader.ReadToEnd ()));
			
			if (!(TableSerializer.ReadObject (mem) is Dictionary<string, string> table))
				return;
			
			foreach (var (key, value) in table.Where (keyValue => !stringTable.ContainsKey (keyValue.Key)))
				stringTable.Add (key, value);
		}

		public string this[string key] => stringTable.ContainsKey(key) ? stringTable[key] : key;
	}
}
