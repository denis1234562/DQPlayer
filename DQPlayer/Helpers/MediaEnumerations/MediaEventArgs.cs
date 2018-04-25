using System;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.MediaEnumerations
{
    public class MediaEventArgs<TEnumeration> : EventArgs
        where TEnumeration : Enumeration<TEnumeration>
    {
        public TEnumeration EventType { get; }
        public object AdditionalInfo { get; }
        public object OriginalSource { get; }

        public MediaEventArgs([NotNull] TEnumeration eventType, object additionalInfo = null)
        {
            EventType = eventType ?? throw new ArgumentNullException(nameof(eventType), @"Enumeration cannot be null.");
            AdditionalInfo = additionalInfo;
        }

        public MediaEventArgs([NotNull] TEnumeration eventType, object originalSource, object additionalInfo = null)
            :this(eventType, additionalInfo)
        {
            OriginalSource = originalSource;
        }
    }
}
