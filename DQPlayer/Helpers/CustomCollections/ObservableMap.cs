using System;
using System.Collections.Generic;

namespace DQPlayer.Helpers.CustomCollections
{
    /// <summary>
    /// Helper to avoid long dictionary declarations when used for <see cref="ICustomObservable{TArgs}"/>> mapping.
    /// </summary>
    internal class ObservableMap<T, TArgs> : Dictionary<T, EventHandler<TArgs>>
        where TArgs : EventArgs
    {
        protected Func<ObservableMap<T, TArgs>, TArgs, T> _invokator;

        internal ObservableMap(Func<ObservableMap<T, TArgs>, TArgs, T> invokator)
            : base()
        {
            _invokator = invokator;
        }

        internal ObservableMap(IDictionary<T, EventHandler<TArgs>> values)
            : base(values)
        {
        }

        internal virtual EventHandler<TArgs> GetHandler(TArgs args)
        {
            return this.TryGetValue(_invokator.Invoke(this, args), out var handler) ? handler : null;
        }
    }
}
