using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DQPlayer.MVVMFiles.Models.MediaPlayer;

namespace DQPlayer.States
{
    public class MediaPlayerStates : IEnumerable<MediaPlayerState>
    {
        public static MediaPlayerState None { get; }
        public static MediaPlayerState Pause { get; }
        public static MediaPlayerState Play { get; }
        public static MediaPlayerState Stop { get; }

        static MediaPlayerStates()
        {
            var ctor = typeof(MediaPlayerState).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                new[] {typeof(int), typeof(bool), typeof(Action<IRegulatableMediaService>)}, null);

            if (ctor == null)
            {
                throw new MissingMethodException(nameof(ctor));
            }

            None = (MediaPlayerState) ctor.Invoke(new object[] {0, false, null});

            Pause = (MediaPlayerState) ctor.Invoke(new object[]
                {1, false, new Action<IRegulatableMediaService>(m => m.Pause())});

            Play = (MediaPlayerState) ctor.Invoke(new object[]
                {2, true, new Action<IRegulatableMediaService>(m => m.Play())});

            Stop = (MediaPlayerState) ctor.Invoke(new object[]
                {3, false, new Action<IRegulatableMediaService>(m => m.Stop())});
        }

        private readonly IReadOnlyList<MediaPlayerState> _playerStates = new List<MediaPlayerState>(5)
        {
            None,
            Pause,
            Play,
            Stop,
        };

        public IEnumerator<MediaPlayerState> GetEnumerator()
        {
            return _playerStates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}