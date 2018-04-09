using System;

namespace DQPlayer.Helpers
{
    public interface ICustomObservable<TArgs>
        where TArgs : EventArgs
    {
        event EventHandler<TArgs> Notify;
    }
}