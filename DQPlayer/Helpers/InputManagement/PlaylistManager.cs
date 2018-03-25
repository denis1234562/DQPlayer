using System;

namespace DQPlayer.Helpers.InputManagement
{
    public class PlaylistManager : IManager<PlaylistManagerEventArgs>
    {
        private static readonly object _padlock = new object();

        private static readonly Lazy<PlaylistManager> _instance =
            new Lazy<PlaylistManager>(() => new PlaylistManager());
        public static PlaylistManager Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance.Value;
                }
            }
        }

        public event EventHandler<PlaylistManagerEventArgs> NewRequest;

        private PlaylistManager()
        {

        }

        public void Request(object sender, PlaylistManagerEventArgs e)
            => OnNewRequest(sender, e);

        protected virtual void OnNewRequest(object sender, PlaylistManagerEventArgs e)
            => NewRequest?.Invoke(this, e);
    }
}