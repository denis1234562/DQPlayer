using System;
using System.Collections.Generic;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.Extensions.CollectionsExtensions
{
    public static class IEnumerableExtensions
    {
        public static int BinarySearch<T>(
            [NotNull] this IList<T> values,
            [NotNull] T value,
            [NotNull] Comparison<T> comparer)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));

            if (values.Count == 0)
            {
                return -1;
            }
            if (value == null)
            {
                return values[0] == null ? 0 : -1;
            }
            int start = 0;
            int end = values.Count - 1;
            int midIndex = end / 2;
            T midItem = values[midIndex];
            int compareResult = comparer.Invoke(value, midItem);
            while (compareResult != 0)
            {
                if (compareResult > 0)
                {
                    start = midIndex + 1;
                }
                else if (compareResult < 0)
                {
                    end = midIndex - 1;
                }
                if (start > end)
                {
                    return -1;
                }
                midIndex = (start + end) / 2;
                midItem = values[midIndex];
                compareResult = comparer.Invoke(value, midItem);
            }
            return midIndex;
        }

        public static int BinarySearch<T>(
            [NotNull] this IList<T> values,
            [NotNull] T value,
            [NotNull] IComparer<T> comparer)
        {
            return BinarySearch(values, value, comparer.Compare);
        }

        public static int DuplicateBinarySearch<T>(
            [NotNull] this IList<T> values,
            [NotNull] T value,
            [NotNull] Comparison<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (values.Count == 0)
            {
                return -1;
            }
            if (value == null)
            {
                return values[0] == null ? 0 : -1;
            }
            int start = 0;
            int end = values.Count - 1;
            int midIndex = end / 2;
            T midItem = values[midIndex];
            int compareResult = comparer.Invoke(value, midItem);
            int firstOccuranceIndex = -1;
            while (start <= end)
            {
                if (compareResult > 0)
                {
                    start = midIndex + 1;
                }
                else
                {
                    end = midIndex - 1;
                    if (compareResult == 0)
                    {
                        firstOccuranceIndex = midIndex;
                    }
                }
                midIndex = (start + end) / 2;
                midItem = values[midIndex];
                compareResult = comparer.Invoke(value, midItem);
            }
            return firstOccuranceIndex;
        }

        public static int DuplicateBinarySearch<T>(
            [NotNull] this IList<T> values, 
            [NotNull] T value,
            [NotNull] IComparer<T> comparer)
        {
            return DuplicateBinarySearch(values, value, comparer.Compare);
        }

        public static int DuplicateBinarySearch<T, TTarget>(
            [NotNull] this IList<T> values,
            [NotNull] SingleComparer<TTarget> comparer, 
            [NotNull] Func<T, TTarget> converter)
        {
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (values.Count == 0)
            {
                return -1;
            }
            int start = 0;
            int end = values.Count - 1;
            int midIndex = end / 2;
            int firstOccuranceIndex = -1;
            TTarget midItem = converter.Invoke(values[midIndex]);
            int compareResult = comparer.Compare(midItem);
            while (start <= end)
            {
                if (compareResult < 0)
                {
                    start = midIndex + 1;
                }
                else
                {
                    end = midIndex - 1;
                    if (compareResult == 0)
                    {
                        firstOccuranceIndex = midIndex;
                    }
                }
                midIndex = (start + end) / 2;
                midItem = converter.Invoke(values[midIndex]);
                compareResult = comparer.Compare(midItem);
            }
            return firstOccuranceIndex;
        }
    }
}
