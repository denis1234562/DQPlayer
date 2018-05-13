using System;
using System.Collections.Generic;
using DQPlayer.Helpers.CustomCollections;

namespace DQPlayer.Helpers.Extensions.CollectionsExtensions
{
    public static class CircularListExtensions
    {
        public static CircularList<T> ToCircularList<T>(this IEnumerable<T> collection)
        {
            return new CircularList<T>(collection);
        }

        public static ObservableCircularList<T> ToObservableCircularList<T>(this IEnumerable<T> collection)
        {
            return new ObservableCircularList<T>(collection);
        }

        public static void Sort<T>(this ICircularList<T> source)
            where T : IComparable<T>
        {
            Sort(source, 0, source.Count);
        }

        private static void Sort<T>(IList<T> source, int low, int high)
            where T : IComparable<T>
        {
            var N = high - low;
            if (N <= 1) return;

            int mid = low + N / 2;

            Sort(source, low, mid);
            Sort(source, mid, high);

            var aux = new T[N];
            int i = low, j = mid;
            for (int k = 0; k < N; k++)
            {
                if (i == mid)
                {
                    aux[k] = source[j++];
                }
                else if (j == high)
                {
                    aux[k] = source[i++];
                }
                else if (source[j].CompareTo(source[i]) < 0)
                {
                    aux[k] = source[j++];
                }
                else
                {
                    aux[k] = source[i++];
                }
            }

            for (int k = 0; k < N; k++)
            {
                source[low + k] = aux[k];
            }
        }
    }
}
