using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.CustomCollections
{
    public class ObservableCircularList<T> : INotifyCollectionChanged, INotifyPropertyChanged, ICircularList<T>
    {
        private const string IndexerName = "Item[]";

        private readonly List<T> _elements;

        private int _lastUsedElementIndex;

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCircularList(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            _elements = new List<T>(items);
        }

        public ObservableCircularList() : this(Enumerable.Empty<T>())
        {
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            foreach (var item in items)
            {
                Add(item);
            }
        }

        #region INotifyCollectionChanged implementation
        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Raise CollectionChanged event to any listeners.
        /// Properties/methods modifying this ObservableCircularList will raise
        /// a collection changed event through this virtual method.
        /// </summary>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
        #endregion

        #region INotifyPropertyChanged implementation

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Implementation of IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Implementation of ICollection<T>

        public void Add(T item)
        {
            _elements.Add(item);

            OnPropertyChanged(nameof(_elements.Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, _elements.Count);
        }

        public void Clear()
        {
            _elements.Clear();

            OnPropertyChanged(nameof(_elements.Count));
            OnPropertyChanged(IndexerName);
            OnCollectionReset();

            _lastUsedElementIndex = 0;
        }

        public bool Contains(T item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _elements.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            int index = _elements.IndexOf(item);
            if (index != -1)
            {
                RemoveAt(index);
            }

            return index != -1;
        }

        public int Count => _elements.Count;
        public bool IsReadOnly => false;

        #endregion

        #region Implementation of IList<T>

        public int IndexOf(T item)
        {
            return _elements.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _elements.Insert(index, item);
            if (index <= _lastUsedElementIndex)
            {
                OnPropertyChanged(nameof(Current));
            }

            OnPropertyChanged(nameof(_elements.Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public void RemoveAt(int index)
        {
            var item = _elements[index];
            _elements.RemoveAt(index);
            var previousCurrentIndex = _lastUsedElementIndex;
            if (_elements.Count > 0 && _lastUsedElementIndex >= _elements.Count)
            {
                _lastUsedElementIndex--;
            }
            if (index <= previousCurrentIndex)
            {
                OnPropertyChanged(nameof(Current));
            }

            OnPropertyChanged(nameof(_elements.Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        public T this[int index]
        {
            get => _elements[index];
            set
            {
                if (index == _lastUsedElementIndex)
                {
                    OnPropertyChanged(nameof(Current));
                }

                var item = _elements[index];
                _elements[index] = value;

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, index);
            }
        }

        #endregion

        #region Implementation of ICircularList<T>

        public T Next => _lastUsedElementIndex + 1 >= _elements.Count
            ? _elements[0]
            : _elements[_lastUsedElementIndex + 1];

        public T Previous => _lastUsedElementIndex - 1 < 0
            ? _elements[_elements.Count -1]
            : _elements[_lastUsedElementIndex - 1];


        public T MoveNext()
        {
            int temp = _lastUsedElementIndex;
            _lastUsedElementIndex++;
            if (_lastUsedElementIndex >= _elements.Count)
            {
                _lastUsedElementIndex = 0;
            }
            OnPropertyChanged(nameof(Current));
            return _elements[temp];
        }

        public T MovePrevious()
        {
            int temp = _lastUsedElementIndex;
            _lastUsedElementIndex--;
            if (_lastUsedElementIndex < 0)
            {
                _lastUsedElementIndex = _elements.Count - 1;
            }
            OnPropertyChanged(nameof(Current));
            return _elements[temp];
        }

        public T Current => _elements.Count == 0
            ? default(T)
            : _elements[_lastUsedElementIndex];

        public void SetCurrent(int currentIndex)
        {
            _lastUsedElementIndex = currentIndex;
            OnPropertyChanged(nameof(Current));
        }

        public void Reset()
        {
            _lastUsedElementIndex = 0;
            OnPropertyChanged(nameof(Current));
        }

        #endregion
    }
}
