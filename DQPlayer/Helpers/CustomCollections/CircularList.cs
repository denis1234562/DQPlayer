using System.Collections;
using System.Collections.Generic;

namespace DQPlayer.Helpers.CustomCollections
{
    public class CircularList<T> : ICircularList<T>
    {
        private readonly IList<T> _elements = new List<T>();

        private int lastUsedElementIndex;

        public CircularList(IEnumerable<T> collection, int startingIterableIndex = 0)
        {
            foreach (T item in collection)
            {
                _elements.Add(item);
            }
            lastUsedElementIndex = startingIterableIndex;
        }

        public CircularList()
        {
        }

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
        }

        public void Clear()
        {
            _elements.Clear();
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
            return _elements.Remove(item);
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
        }

        public void RemoveAt(int index)
        {
            _elements.RemoveAt(index);
        }

        public T this[int index]
        {
            get => _elements[index];
            set => _elements[index] = value;
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

        public T Current
        {
            get => _elements.Count == 0
             ? default(T)
             : _elements[lastUsedElementIndex];
        }

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
