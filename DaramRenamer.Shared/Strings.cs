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

    private readonly ObservableDictionary<string, string> _stringTable = new();

    private Strings()
    {
        Load();
    }

    public static Strings Instance => _instance ??= new Strings();

    public IEnumerable<CultureInfo> AvailableLanguages { get; private set; }

    public event NotifyCollectionChangedEventHandler CollectionChanged = (_, _) => { };
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

    public void Load()
    {
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DaramRenamer.Strings.Strings.csv");
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

        using var reader = new CsvReader(stream);
        if (!reader.Read())
            return;

        AvailableLanguages = reader.Header
            .Skip(1)
            .Select(column => column == "invariant"
                ? CultureInfo.GetCultureInfo("en")
                : CultureInfo.GetCultureInfo(column))
            .ToArray();

        var readingColumnIndex = IndexOf(AvailableLanguages as CultureInfo[], CultureInfo.CurrentUICulture) + 1;

        while (reader.Read())
        {
            var columns = reader.CurrentRow;

            var key = columns[0];
            var value = columns[readingColumnIndex];
            if (string.IsNullOrEmpty(value))
                value = columns[1];
            
            if (!_stringTable.TryAdd(key, value))
            {
                var existing = _stringTable[key];
                _stringTable[key] = value;
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<string, string>(key, value),
                    new KeyValuePair<string, string>(key, existing)));
            }
            else
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                    new KeyValuePair<string, string>(key, value)));
            }
        }
    }

    private static int IndexOf(CultureInfo[] availableLanguages, CultureInfo current)
    {
        for (var i = 0; i < availableLanguages.Length; ++i)
            if (Equals(availableLanguages[i], current))
                return i;
        return 0;
    }
}