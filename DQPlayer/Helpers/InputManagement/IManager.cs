using System;

namespace DQPlayer.Helpers.InputManagement
{
    public interface IManager<TArgs> : ICustomObservable<TArgs>
        where TArgs : EventArgs
    {
        void Request(object sender, TArgs e);
    }
}