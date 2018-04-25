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

        private Dictionary<TKey, TValue> _dictionary;

        #region Constructors

        public ObservableDictionary([NotNull] IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            foreach (var entry in dictionary)
            {
                AddEntry(entry);
            }
        }

        public ObservableDictionary(IDictionary<TKey, TValue> dictionary)
            : this(dictionary, null)
        {
        }

        public ObservableDictionary(IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary(int capacity = 0)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity);
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
            _dictionary.Add(keyValuePair.Key, keyValuePair.Value);

            OnCommonPropertiesChanged();
            OnCollectionChanged(NotifyCollectionChangedAction.Add, keyValuePair, -1);
        }

        private void AddEntry(TKey key, TValue value)
        {
            AddEntry(new KeyValuePair<TKey, TValue>(key, value));
        }

        private bool RemoveEntry(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var value))
            {
                OnCommonPropertiesChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, -1);
                return true;
            }
            return false;
        }

        private void SetEntry(TKey key, TValue value)
        {
            if (_dictionary.TryGetValue(key, out var currentValue))
            {
                _dictionary[key] = value;
                OnCommonPropertiesChanged();
                OnCollectionChanged(NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, currentValue),
                    new KeyValuePair<TKey, TValue>(key, value), -1);
            }
            AddEntry(key, value);
        }

        #endregion

        #region Implementation of IEnumerable

        public void Clear()
        {
            _dictionary.Clear();

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
            return _dictionary.ContainsKey((TKey) key);
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
            return _dictionary.TryGetValue(item.Key, out var value) && value.Equals(item.Value);
        }

        public void CopyTo([NotNull] KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            ((IDictionary<TKey, TValue>) _dictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return RemoveEntry(item.Key);
        }

        void ICollection.CopyTo([NotNull] Array array, [NotNull] int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            ((ICollection) _dictionary).CopyTo(array, arrayIndex);
        }

        int ICollection.Count => _dictionary.Count;
        object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;
        bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;
        int ICollection<KeyValuePair<TKey, TValue>>.Count => _dictionary.Count;
        bool IDictionary.IsReadOnly => false;
        public bool IsFixedSize => false;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        #endregion

        #region Implementation of IDictionary<TKey,TValue>

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        [System.Diagnostics.Contracts.Pure]
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
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
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => SetEntry(key, value);
        }

        ICollection IDictionary.Keys => _dictionary.Keys;
        ICollection IDictionary.Values => _dictionary.Values;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dictionary.Keys;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => _dictionary.Values;

        #endregion

        #region Implementation of ISerializable

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var entries = _dictionary.ToDictionary(entry => entry.Key, entry => entry.Value);
            info.AddValue("entries", entries);
            info.AddValue("comparer", _dictionary.Comparer);
        }

        #endregion

        #region Implementation of IDeserializationCallback

        public void OnDeserialization(object sender)
        {
            var entries =
                (Dictionary<TKey, TValue>) _serializationInfo.GetValue("entries", typeof(Dictionary<TKey, TValue>));
            var comparer =
                (EqualityComparer<TKey>)_serializationInfo.GetValue("comparer", typeof(EqualityComparer<TKey>));

            _dictionary = new Dictionary<TKey, TValue>(entries, comparer);
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
            OnPropertyChanged(nameof(_dictionary.Count));
        }

        #endregion
    }
}
