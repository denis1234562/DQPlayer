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
    }
}
