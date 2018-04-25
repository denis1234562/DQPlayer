using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DQPlayer.Annotations;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.States;

namespace DQPlayer.Helpers.Extensions
{
    public static class IRegulatableMediaPlayerExtensions
    {
        public static void SetPlayerPositionToCursor([NotNull] this IRegulatableMediaPlayer player, [NotNull] Track relativeTo)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (relativeTo == null)throw new ArgumentNullException(nameof(relativeTo));

            Point mousePosition = new Point(Mouse.GetPosition(relativeTo).X, 0);
            double simulatedValue = relativeTo.SimulateTrackPosition(mousePosition);
            player.MediaController.SetNewPlayerPosition(TimeSpan.FromSeconds(simulatedValue));
        }

        public static void PlayNewPlayerSource([NotNull] this IRegulatableMediaPlayer player, [NotNull] Uri source)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (source == null)throw new ArgumentNullException(nameof(source));

            player.MediaController.SetNewPlayerSource(source);
            player.SetMediaState(MediaPlayerStates.Play);
        }
    }
}