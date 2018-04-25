using System;
using System.Collections.Generic;
using DQPlayer.Helpers.MediaEnumerations;

namespace DQPlayer.Helpers.CustomCollections
{
    internal class MediaObservableMap<TEnumeration> : ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>
        where TEnumeration : Enumeration<TEnumeration>
    {
        internal MediaObservableMap(
            IDictionary<TEnumeration, EventHandler<MediaEventArgs<TEnumeration>>> values,
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>, MediaEventArgs<TEnumeration>, TEnumeration> selector,
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>, TEnumeration, bool> predicate)
            : base(values, selector, predicate)
        {
        }

        internal MediaObservableMap(
            IDictionary<TEnumeration, EventHandler<MediaEventArgs<TEnumeration>>> values,
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>, MediaEventArgs<TEnumeration>, TEnumeration> selector)
            : base(values, selector)
        {
        }

        internal MediaObservableMap(
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>, MediaEventArgs<TEnumeration>, TEnumeration> selector, 
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>, TEnumeration, bool> predicate)
            : base(selector, predicate)
        {
        }

        internal MediaObservableMap(
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>, MediaEventArgs<TEnumeration>, TEnumeration> selector) 
            : base(selector)
        {
        }
    }
}