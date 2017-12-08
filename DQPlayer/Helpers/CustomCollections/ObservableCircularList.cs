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

        private int lastUsedElementIndex;

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

            lastUsedElementIndex = 0;
        }

        public bool Contains(T item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _elements.CopyTo(array,arrayIndex);
        }

        public bool Remove(T item)
        {
            var result = _elements.Remove(item);

            if (result)
            {
                OnPropertyChanged(nameof(_elements.Count));
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, _elements.Count);
            }

            return result;
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

            OnPropertyChanged(nameof(_elements.Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
        }

        public void RemoveAt(int index)
        {
            var item = _elements[index];
            _elements.RemoveAt(index);

            OnPropertyChanged(nameof(_elements.Count));
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
        }

        public T this[int index]
        {
            get => _elements[index];
            set
            {
                var item = _elements[index];
                _elements[index] = value;

                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, index);
            }
        }

        #endregion

        #region Implementation of ICircularList<T>

        public T MoveNext()
        {
            int temp = lastUsedElementIndex;
            lastUsedElementIndex++;
            if (lastUsedElementIndex >= _elements.Count)
            {
                lastUsedElementIndex = 0;
            }
            return _elements[temp];
        }

        public T MovePrevious()
        {
            int temp = lastUsedElementIndex;
            lastUsedElementIndex--;
            if (lastUsedElementIndex < 0)
            {
                lastUsedElementIndex = _elements.Count - 1;
            }
            return _elements[temp];
        }

        public T Current => _elements.Count == 0
            ? default(T)
            : _elements[lastUsedElementIndex];

        public void SetCurrent(int currentIndex)
        {
            lastUsedElementIndex = currentIndex;
        }

        public void Reset()
        {
            lastUsedElementIndex = 0;
        }

        #endregion
    }
}
