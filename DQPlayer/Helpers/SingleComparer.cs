using System;
using System.Collections.Generic;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers
{
    public class SingleComparer<T> : ISingleComparer<T>
    {
        public T TargetValue { get; set; }
        protected IComparer<T> _comparer;
        protected Func<T, T, bool> _predicate;

        /// <summary>
        /// Initializes new instance of <see cref="SingleComparer{T}"/> with specified predicate and a concrete comparer.
        /// </summary>
        /// <param name="predicate">Compare will return 0 if the predicate evaluates to true, else the comparer is used.</param>
        /// <param name="comparer">Comparer used in <see cref="Compare"/> method.</param>
        public SingleComparer(Func<T, T, bool> predicate, [NotNull] IComparer<T> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _predicate = predicate;
        }

        /// <summary>
        /// Initializes new instance of <see cref="SingleComparer{T}"/> with no predicate and a concrete comparer.
        /// </summary>
        /// <param name="comparer"></param>
        public SingleComparer(IComparer<T> comparer)
            : this(null, comparer)
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="SingleComparer{T}"/> with specified predicate and default comparer.
        /// </summary>
        public SingleComparer(Func<T, T, bool> predicate)
            : this(predicate, Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="SingleComparer{T}"/> using no predicate and default comparer.
        /// </summary>
        public SingleComparer()
            : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Compares value against <see cref="TargetValue"/>
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public virtual int Compare(T x)
        {
            return _predicate?.Invoke(x, TargetValue) == true ? 0 : _comparer.Compare(x, TargetValue);
        }
    }
}