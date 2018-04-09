using System;
using System.Collections;
using System.Collections.Generic;
using DQPlayer.Annotations;
using DQPlayer.Helpers.CustomCollections;

namespace DQPlayer.Helpers
{
    internal class MultipleProvidersCache
    {
        protected readonly ProviderActionCache<EventHandler<object>> _notificationsCache =
            new ProviderActionCache<EventHandler<object>>();

        protected readonly ProviderActionCache<Action> _providerUnsubscriberCache = new ProviderActionCache<Action>();

        internal virtual void AddProvider<T, TArgs>(
            [NotNull] ICustomObservable<TArgs> provider,
            [NotNull] ObservableMap<T, TArgs> map)
            where TArgs : EventArgs
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }
            var providerType = provider.GetType();
            _notificationsCache.Add<TArgs>(providerType, (sender, args) =>
            {
                var unboxedArgs = (TArgs) args;
                map.GetHandler(unboxedArgs)?.Invoke(sender, unboxedArgs);
            });
            void NotifierDelegate(object sender, TArgs args) => Provider_OnNotify(sender, args, providerType);
            provider.Notify += NotifierDelegate;
            _providerUnsubscriberCache.Add<TArgs>(providerType, () => provider.Notify -= NotifierDelegate);
        }

        internal virtual bool RemoveProvider<TProvider, TArgs>()
            where TArgs : EventArgs
            where TProvider : ICustomObservable<TArgs>
        {
            return RemoveProviderImpl<TArgs>(typeof(TProvider));
        }

        protected virtual bool RemoveProviderImpl<TArgs>([NotNull] Type providerType)
            where TArgs : EventArgs
        {
            if (providerType == null)
            {
                throw new ArgumentNullException(nameof(providerType));
            }
            if (_notificationsCache.RemoveAction<TArgs>(providerType))
            {
                _providerUnsubscriberCache.GetAction<TArgs>(providerType).Invoke();
                _providerUnsubscriberCache.RemoveAction<TArgs>(providerType);
                return true;
            }
            return false;
        }

        protected virtual void Provider_OnNotify<TArgs>(object sender, TArgs e, Type providerType)
            where TArgs : EventArgs
        {
            if (_notificationsCache.TryGetAction<TArgs>(providerType, out var action))
            {
                action.Invoke(sender, e);
            }
        }

        protected sealed class ProviderActionCache<TAction> : IEnumerable<KeyValuePair<Type, Dictionary<Type, TAction>>>
        {
            private readonly IDictionary<Type, Dictionary<Type, TAction>> _dictionary;

            internal ProviderActionCache()
            {
                _dictionary = new Dictionary<Type, Dictionary<Type, TAction>>();
            }

            internal void Add<TArgs>([NotNull] Type key, [NotNull] TAction value)
                where TArgs : EventArgs
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (_dictionary.TryGetValue(key, out var values))
                {
                    values.Add(typeof(TArgs), value);
                }
                else
                {
                    _dictionary.Add(key, new Dictionary<Type, TAction> { [typeof(TArgs)] = value });
                }
            }

            internal bool RemoveProvider([NotNull] Type providerType)
            {
                if (providerType == null)
                {
                    throw new ArgumentNullException(nameof(providerType));
                }
                return _dictionary.Remove(providerType);
            }

            internal bool RemoveAction<TArgs>([NotNull] Type providerType)
                where TArgs : EventArgs
            {
                if (providerType == null)
                {
                    throw new ArgumentNullException(nameof(providerType));
                }
                return _dictionary.TryGetValue(providerType, out var values) && values.Remove(typeof(TArgs));
            }

            internal bool TryGetProvider([NotNull] Type providerType, out Dictionary<Type, TAction> values)
            {
                if (providerType == null)
                {
                    throw new ArgumentNullException(nameof(providerType));
                }
                return _dictionary.TryGetValue(providerType, out values);
            }

            internal bool TryGetAction<TArgs>([NotNull] Type providerType, out TAction action)
                where TArgs : EventArgs
            {
                if (providerType == null)
                {
                    throw new ArgumentNullException(nameof(providerType));
                }
                if (TryGetProvider(providerType, out var value))
                {
                    action = value[typeof(TArgs)];
                    return true;
                }
                action = default(TAction);
                return false;
            }

            internal TAction GetAction<TArgs>([NotNull] Type providerType)
                where TArgs : EventArgs
            {
                if (providerType == null)
                {
                    throw new ArgumentNullException(nameof(providerType));
                }
                return _dictionary[providerType][typeof(TArgs)];
            }

            internal Dictionary<Type, TAction> GetProvider([NotNull] Type providerType)
            {
                if (providerType == null)
                {
                    throw new ArgumentNullException(nameof(providerType));
                }
                return _dictionary[providerType];
            }

            #region Implementation of IEnumerable

            public IEnumerator<KeyValuePair<Type, Dictionary<Type, TAction>>> GetEnumerator()
            {
                return _dictionary.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }
    }
}
