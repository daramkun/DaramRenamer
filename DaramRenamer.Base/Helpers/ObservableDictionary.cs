using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DaramRenamer.Helpers;

[DebuggerDisplay("Count={Count}")]
public class ObservableDictionary<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    where TKey : notnull
{
    public int Count => dictionary.Count;

    public ObservableDictionary()
        : this(new Dictionary<TKey, TValue>())
    {
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged = (_, _) => { };
    public event PropertyChangedEventHandler? PropertyChanged = (_, _) => { };

    private void AddWithNotification(KeyValuePair<TKey, TValue> item)
    {
        AddWithNotification(item.Key, item.Value);
    }

    private void AddWithNotification(TKey key, TValue value)
    {
        dictionary.Add(key, value);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
            new KeyValuePair<TKey, TValue>(key, value)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
    }

    private bool RemoveWithNotification(TKey key)
    {
        if (!dictionary.TryGetValue(key, out var value) || !dictionary.Remove(key))
            return false;

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
            new KeyValuePair<TKey, TValue>(key, value)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));

        return true;
    }

    private void UpdateWithNotification(TKey key, TValue value)
    {
        if (dictionary.TryGetValue(key, out var existing))
        {
            dictionary[key] = value;

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                new KeyValuePair<TKey, TValue>(key, value),
                new KeyValuePair<TKey, TValue>(key, existing)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
        }
        else
        {
            AddWithNotification(key, value);
        }
    }

    #region IDictionary<TKey,TValue> Members

    public void Add(TKey key, TValue value)
    {
        AddWithNotification(key, value);
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys => dictionary.Keys;

    public bool Remove(TKey key)
    {
        return RemoveWithNotification(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return dictionary.TryGetValue(key, out value);
    }

    public ICollection<TValue> Values => dictionary.Values;

    public TValue this[TKey key]
    {
        get => dictionary[key];
        set => UpdateWithNotification(key, value);
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        AddWithNotification(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Clear()
    {
        dictionary.Clear();

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Keys"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Values"));
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return dictionary.Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        dictionary.CopyTo(array, arrayIndex);
    }

    int ICollection<KeyValuePair<TKey, TValue>>.Count => dictionary.Count;

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => dictionary.IsReadOnly;

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return RemoveWithNotification(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    #endregion
}