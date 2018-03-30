using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DQPlayer.Helpers.CustomCollections
{
    public class KeyedEntryCollection<TKey> : KeyedCollection<TKey, DictionaryEntry>
    {
        public KeyedEntryCollection()
        {            
        }

        public KeyedEntryCollection(IEqualityComparer<TKey> comparer) 
            : base(comparer)
        {           
        }

        public int IndexOf(TKey key)
            => this.Contains(key) ? base.IndexOf(new DictionaryEntry(key, this[key].Value)) : -1;

        protected override TKey GetKeyForItem(DictionaryEntry entry) => (TKey)entry.Key;
    }
}