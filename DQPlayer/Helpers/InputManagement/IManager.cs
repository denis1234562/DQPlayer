using System;

namespace DQPlayer.Helpers.InputManagement
{
    public interface IManager<TArgs>
        where TArgs : EventArgs
    {
        event EventHandler<TArgs> NewRequest;
        void Request(object sender, TArgs e);
    }
}