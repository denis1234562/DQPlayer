using System;

namespace DQPlayer.Helpers.InputManagement
{
    public enum PlaylistAction
    {
        PlayNext,
        PlayPrevious
    }

    public class PlaylistManagerEventArgs : EventArgs
    {
        public PlaylistAction PlaylistAction { get; }

        public PlaylistManagerEventArgs(PlaylistAction playlistAction)
        {
            PlaylistAction = playlistAction;
        }
    }
}