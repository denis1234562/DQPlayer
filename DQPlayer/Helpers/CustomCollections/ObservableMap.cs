using System;
using System.Collections.Generic;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.CustomCollections
{
    /// <summary>
    /// Helper to avoid long dictionary declarations when used for <see cref="ICustomObservable{TArgs}"/>> mapping.
    /// </summary>
    internal class ObservableMap<T, TArgs> : Dictionary<T, EventHandler<TArgs>>
    {
        protected Func<ObservableMap<T, TArgs>, T, bool> _predicate;
        protected Func<ObservableMap<T, TArgs>, TArgs, T> _selector;

        /// <param name="values">Values to populate into the <see cref="ObservableMap{T,TArgs}"/>.</param>
        /// <param name="selector">Used for complex index selection.</param>
        /// <param name="predicate">If the condition is false <see cref="TryGetHandler(T, out EventHandler{TArgs})"/> 
        /// will not look for entry and instead will return false.</param>
        internal ObservableMap(
            [NotNull] IDictionary<T, EventHandler<TArgs>> values,
            [NotNull] Func<ObservableMap<T, TArgs>, TArgs, T> selector,
            [NotNull]  Func<ObservableMap<T, TArgs>, T, bool> predicate)
            : this(values, selector)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <param name="values">Values to populate into the <see cref="ObservableMap{T,TArgs}"/>.</param>
        /// <param name="selector">Used for complex index selection.</param>
        internal ObservableMap(
            [NotNull]   IDictionary<T, EventHandler<TArgs>> values,
            [NotNull]  Func<ObservableMap<T, TArgs>, TArgs, T> selector)
            : base(values)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        /// <param name="selector">Used for complex index selection.</param>
        /// <param name="predicate">If the condition is false <see cref="TryGetHandler(T, out EventHandler{TArgs})"/> 
        /// will not look for entry and instead will return false.</param>
        internal ObservableMap(
            [NotNull]   Func<ObservableMap<T, TArgs>, TArgs, T> selector,
            [NotNull]   Func<ObservableMap<T, TArgs>, T, bool> predicate)
            : this(selector)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <param name="selector">Used for complex index selection.</param>
        internal ObservableMap(
            [NotNull]  Func<ObservableMap<T, TArgs>, TArgs, T> selector)
        {
            _selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        internal virtual bool TryGetHandler([NotNull] TArgs args, out EventHandler<TArgs> handler)
        {
            if(args == null) throw new ArgumentNullException(nameof(args));
            return TryGetHandler(_selector.Invoke(this, args), out handler);
        }

        internal virtual bool TryGetHandler([NotNull] T key, out EventHandler<TArgs> handler)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (_predicate?.Invoke(this, key) == false)
            {
                handler = null;
                return false;
            }
            return TryGetValue(key, out handler);
        }

        internal new EventHandler<TArgs> this[T key] => base[key];
    }
}
