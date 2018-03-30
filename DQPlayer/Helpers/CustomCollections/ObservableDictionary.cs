using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.CustomCollections
{
    [Serializable]
    public sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ISerializable,
        IDeserializationCallback, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string IndexerName = "Item[]";
        private const string ValuesName = "Values";
        private const string KeysName = "Keys";

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private event PropertyChangedEventHandler PropertyChanged;

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }

        private readonly SerializationInfo _serializationInfo;

        private bool _requiresSyncronization;

        private KeyedEntryCollection<TKey> _keyedEntryCollection;

        private Dictionary<TKey, TValue> _dictionary;

        private Dictionary<TKey, TValue> Dictionary
        {
            get
            {
                if (_requiresSyncronization)
                {
                    _dictionary = _keyedEntryCollection.ToDictionary(entry => (TKey)entry.Key,
                        entry => (TValue)entry.Value);
                    _requiresSyncronization = false;
                }
                return _dictionary;
            }
        }

        #region Constructors

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer = null)
            : this(comparer)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            foreach (var kvp in dictionary)
            {
                AddEntry(kvp);
            }
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer = null)
        {
            _keyedEntryCollection = new KeyedEntryCollection<TKey>(comparer);
        }

        /// <summary>
        /// Used for deserialization.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        private ObservableDictionary(SerializationInfo info, StreamingContext context)
        {
            _serializationInfo = info;
        }

        #endregion

        #region KeyedEntryCollection Modifiers

        private void AddEntry(KeyValuePair<TKey, TValue> keyValuePair)
        {
            var entry = new DictionaryEntry(keyValuePair.Key, keyValuePair.Value);
            _keyedEntryCollection.Add(entry);
            _requiresSyncronization = true;

            OnCommonPropertiesChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Add, keyValuePair, _keyedEntryCollection.Count - 1);
        }

        private void AddEntry(TKey key, TValue value)
        {
            AddEntry(new KeyValuePair<TKey, TValue>(key, value));
        }

        private bool RemoveEntry(TKey key)
        {
            var itemIndex = _keyedEntryCollection.IndexOf(key);
            if (itemIndex != -1)
            {
                var value = _keyedEntryCollection[key];
                _keyedEntryCollection.RemoveAt(itemIndex);

                _requiresSyncronization = true;

                OnCommonPropertiesChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, itemIndex);

            }
            return false;
        }

        private void SetEntry(TKey key, TValue value)
        {
            if (_keyedEntryCollection.Contains(key))
            {
                var entry = _keyedEntryCollection[key];
                var entryIndex = _keyedEntryCollection.IndexOf(entry);
                if (!entry.Value.Equals(value))
                {
                    _keyedEntryCollection.Remove(key);
                    _keyedEntryCollection.Insert(entryIndex, new DictionaryEntry(key, value));
                }

                _requiresSyncronization = true;

                OnCommonPropertiesChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, entry, new DictionaryEntry(key, value), entryIndex);
            }
        }

        #endregion

        #region Implementation of IEnumerable

        public void Clear()
        {
            _keyedEntryCollection.Clear();

            OnCommonPropertiesChanged();
            OnCollectionReset();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDictionary.Remove(object key)
        {
            RemoveEntry((TKey)key);
        }

        object IDictionary.this[object key]
        {
            get => TryGetValue((TKey)key, out var value) ? (object)value : null;
            set => this[(TKey)key] = (TValue)value;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<KeyValuePair<TKey,TValue>>

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            AddEntry(item);
        }

        bool IDictionary.Contains(object key)
        {
            return _keyedEntryCollection.Contains((TKey)key);
        }

        void IDictionary.Add(object key, object value)
        {
            AddEntry(new KeyValuePair<TKey, TValue>((TKey)key, (TValue)value));
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _keyedEntryCollection.Contains(new DictionaryEntry(item.Key, item.Value));
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            for (int i = 0; i < _keyedEntryCollection.Count; i++)
            {
                var entry = _keyedEntryCollection[i];
                array[i] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return RemoveEntry(item.Key);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            ((ICollection)_keyedEntryCollection).CopyTo(array, arrayIndex);
        }

        int ICollection.Count => _keyedEntryCollection.Count;
        object ICollection.SyncRoot => ((ICollection)_keyedEntryCollection).SyncRoot;
        bool ICollection.IsSynchronized => ((ICollection)_keyedEntryCollection).IsSynchronized;
        int ICollection<KeyValuePair<TKey, TValue>>.Count => _keyedEntryCollection.Count;
        ICollection IDictionary.Values => Dictionary.Values;
        bool IDictionary.IsReadOnly => false;
        public bool IsFixedSize => false;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        #endregion

        #region Implementation of IDictionary<TKey,TValue>

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        public bool ContainsKey(TKey key)
        {
            return _keyedEntryCollection.Contains(key);
        }

        public void Add(TKey key, TValue value)
        {
            AddEntry(key, value);
        }

        public bool Remove(TKey key)
        {
            return RemoveEntry(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_keyedEntryCollection.Contains(key))
            {
                value = (TValue)_keyedEntryCollection[key].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        public TValue this[TKey key]
        {
            get => (TValue)_keyedEntryCollection[key].Value;
            set => SetEntry(key, value);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Dictionary.Keys;
        ICollection IDictionary.Keys => Dictionary.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => Dictionary.Values;

        #endregion

        #region Implementation of ISerializable

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var entries = new DictionaryEntry[_keyedEntryCollection.Count];
            for (int i = 0; i < entries.Length; i++)
            {
                entries[i] = _keyedEntryCollection[i];
            }
            info.AddValue("entries", entries);
            info.AddValue("comparer", _keyedEntryCollection.Comparer);
        }

        #endregion

        #region Implementation of IDeserializationCallback

        public void OnDeserialization(object sender)
        {
            var entries = (DictionaryEntry[])_serializationInfo.GetValue("entries", typeof(DictionaryEntry[]));
            var comparer =
                (EqualityComparer<TKey>)_serializationInfo.GetValue("comparer", typeof(EqualityComparer<TKey>));

            _keyedEntryCollection = new KeyedEntryCollection<TKey>(comparer);
            foreach (var entry in entries)
            {
                AddEntry((TKey)entry.Key, (TValue)entry.Value);
            }
        }

        #endregion

        #region INotifyCollectionChanged implementation

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem,
            int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        #endregion

        #region INotifyPropertyChanged implementation

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnCommonPropertiesChanged()
        {
            OnPropertyChanged(IndexerName);
            OnPropertyChanged(ValuesName);
            OnPropertyChanged(KeysName);
            OnPropertyChanged(nameof(_keyedEntryCollection.Count));
        }

        #endregion
    }
}
