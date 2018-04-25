using System;
using System.Collections.Generic;

namespace DQPlayer.Helpers.Extensions.CollectionsExtensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this Dictionary<TKey, TValue> source,
            TKey key,
            Func<TValue> valueFactory)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (valueFactory == null) throw new ArgumentNullException(nameof(valueFactory));

            if (source.TryGetValue(key, out TValue value))
            {
                return value;
            }
            value = valueFactory.Invoke();
            source.Add(key, value);
            return value;
        }
    }
}