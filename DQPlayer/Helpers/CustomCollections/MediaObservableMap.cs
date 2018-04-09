using System;
using System.Collections.Generic;
using DQPlayer.Helpers.MediaEnumerations;

namespace DQPlayer.Helpers.CustomCollections
{
    internal class MediaObservableMap<TEnumeration> : ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>
        where TEnumeration : Enumeration<TEnumeration>
    {
        internal MediaObservableMap(
            Func<ObservableMap<TEnumeration, MediaEventArgs<TEnumeration>>,
                MediaEventArgs<TEnumeration>, TEnumeration> invokator)
            : base(invokator)
        {
        }

        internal MediaObservableMap(IDictionary<TEnumeration, EventHandler<MediaEventArgs<TEnumeration>>> values)
            : base(values)
        {
        }

        internal EventHandler<MediaEventArgs<TEnumeration>> GetHandler(TEnumeration enumerationValue)
        {
            return TryGetValue(enumerationValue, out var handler) ? handler : null;
        }
    }
}