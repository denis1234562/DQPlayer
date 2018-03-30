using System;

namespace DQPlayer.Helpers.ObjectPooling
{
    public class PooledObject<T>
    {
        public Func<T> ObjectInitializer { get; }
        public bool CreateOnStartup { get; }

        public PooledObject(Func<T> objectInitializer, bool createOnStartup = true)
        {
            ObjectInitializer = objectInitializer ?? throw new ArgumentNullException(nameof(objectInitializer)); ;
            CreateOnStartup = createOnStartup;
        }
    }
}
