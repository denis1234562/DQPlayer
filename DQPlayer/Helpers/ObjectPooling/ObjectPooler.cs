using System;
using System.Collections.Generic;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.Helpers.ObjectPooling
{ 
    public enum PoolRefillMethod
    {
        None,

        /// <summary>
        /// Initializes a single new object everytime the pool goes dry.
        /// </summary>
        SingleObject,

        /// <summary>
        /// Reinitializes the whole pool everytime it goes dry.
        /// </summary>
        WholePool
    }

    public sealed class ObjectPooler<T>
    {
        public int Amount { get; }
        public PoolRefillMethod RefillMethod { get; set; }

        private readonly PooledObject<T> _pooledObject;
        private readonly Queue<T> _pooledObjects;

        private readonly Dictionary<PoolRefillMethod, Action> _poolRefillMethods;

        public ObjectPooler(PooledObject<T> pooledObject, int amount, PoolRefillMethod refillMethod)
        {
            _pooledObject = pooledObject ?? throw new ArgumentNullException(nameof(pooledObject));
            Amount = amount;
            RefillMethod = refillMethod;
            _poolRefillMethods = new Dictionary<PoolRefillMethod, Action>
            {
                [PoolRefillMethod.None] = () => throw new InvalidOperationException("Pool is empty"),
                [PoolRefillMethod.SingleObject] = AddNewObject,
                [PoolRefillMethod.WholePool] = InitializePool,
            };

            _pooledObjects = new Queue<T>();
            if (_pooledObject.CreateOnStartup)
            {
                InitializePool();
            }
        }

        public void InitializePool()
        {
            _pooledObjects.Clear();
            LoopUtilities.Repeat(Amount, AddNewObject);
        }

        private void AddNewObject() => _pooledObjects.Enqueue(_pooledObject.ObjectInitializer.Invoke());

        public T GetObject()
        {
            if (_pooledObjects.Count == 0)
            {
                _poolRefillMethods[RefillMethod].Invoke();
            }
            return _pooledObjects.Dequeue();
        }
    }
}