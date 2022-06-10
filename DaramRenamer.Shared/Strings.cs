using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;

namespace DaramRenamer;

public class Strings : IReadOnlyDictionary<string, string>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private static Strings _instance;

    private static readonly DataContractJsonSerializer TableSerializer = new(
        typeof(Dictionary<string, string>), new DataContractJsonSerializerSettings
        {
            UseSimpleDictionaryFormat = true
        });

    private static readonly Regex StringTableFilename =
        new(".*DaramRenamer\\.Strings\\.([a-zA-Z0-9\\-_]*)\\.json");

    private readonly List<CultureInfo> _availableCustomLanguages = new();

    private readonly ObservableDictionary<string, string> _stringTable = new();

    public Strings()
    {
        GetAdditionalAvailableLanguages();
        Load();
    }

    public static Strings Instance => _instance ??= new Strings();

    public IEnumerable<CultureInfo> AvailableLanguages
    {
        get
        {
            yield return CultureInfo.GetCultureInfo("en");
            yield return CultureInfo.GetCultureInfo("ko-kr");
            yield return CultureInfo.GetCultureInfo("nl-nl");
            foreach (var lang in _availableCustomLanguages)
                yield return lang;
        }
    }

    /// <summary>Event raised when the collection changes.</summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, _) => { };

    /// <summary>Event raised when a property on the collection changes.</summary>
    public event PropertyChangedEventHandler PropertyChanged = (_, _) => { };

    public string this[string key] => _stringTable.ContainsKey(key) ? _stringTable[key] : key;

    public bool ContainsKey(string key)
    {
        return _stringTable.ContainsKey(key);
    }

    public bool TryGetValue(string key, out string value)
    {
        return _stringTable.TryGetValue(key, out value);
    }

    public IEnumerable<string> Keys => _stringTable.Keys;
    public IEnumerable<string> Values => _stringTable.Values;
    public int Count => _stringTable.Count();

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
    {
        return _stringTable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public CultureInfo GetDefaultLanguage()
    {
        var currentUiCulture = CultureInfo.CurrentUICulture;
        return AvailableLanguages.Any(lang =>
            lang.Name.Equals(currentUiCulture.Name, StringComparison.OrdinalIgnoreCase))
            ? currentUiCulture
            : CultureInfo.GetCultureInfo("en");
    }

    private void GetAdditionalAvailableLanguages()
    {
        foreach (var name in FileInfo.FileOperator.GetFiles(Environment.CurrentDirectory, false))
        {
            var match = StringTableFilename.Match(name);
            if (!match.Success)
                continue;

            var code = match.Groups[1].Value;
            _availableCustomLanguages.Add(CultureInfo.GetCultureInfo(code.Replace('_', '-')));
        }
    }

    public void Load()
    {
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DaramRenamer.Strings.Strings.default.json");
            LoadTable(stream);
        }

        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                $"DaramRenamer.Strings.Strings.{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}.json");
            LoadTable(stream);
        }

        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                $"DaramRenamer.Strings.Strings.{CultureInfo.CurrentUICulture.ToString().Replace('-', '_').ToLower()}.json");
            LoadTable(stream);
        }

        foreach (var name in FileInfo.FileOperator.GetFiles(Environment.CurrentDirectory, false))
        {
            var match = StringTableFilename.Match(name);
            if (!match.Success)
                continue;

            var filename = Path.GetFileName(name).Replace('_', '-');
            if (filename.IndexOf($".{CultureInfo.CurrentUICulture.Name}.", StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            using var stream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            LoadTable(stream);
        }

        PropertyChanged(this, new PropertyChangedEventArgs(Binding.IndexerName));
        PropertyChanged(this, new PropertyChangedEventArgs("Values"));

        ((MainWindow) Application.Current.MainWindow)?.RefreshTitle();
        PluginToMenu.RefreshBinding();
    }

    private void LoadTable(Stream stream)
    {
        if (stream == null)
            return;

        using var reader = new StreamReader(stream, Encoding.UTF8, true);
        using Stream mem = new MemoryStream(Encoding.UTF8.GetBytes(reader.ReadToEnd()));

        if (TableSerializer.ReadObject(mem) is not Dictionary<string, string> table)
            return;

        foreach (var (key, value) in table)
            if (_stringTable.ContainsKey(key))
            {
                var existing = _stringTable[key];
                _stringTable[key] = value;
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<string, string>(key, value),
                    new KeyValuePair<string, string>(key, existing)));
            }
            else
            {
                _stringTable[key] = value;
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    new KeyValuePair<string, string>(key, value)));
            }
    }
}