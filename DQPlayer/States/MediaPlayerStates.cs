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
        public static MediaPlayerState FastForward { get; }
        public static MediaPlayerState Rewind { get; }

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

            FastForward = (MediaPlayerState) ctor.Invoke(new object[]
                {4, true, new Action<IRegulatableMediaService>(m => m.FastForward())});

            Rewind = (MediaPlayerState) ctor.Invoke(new object[]
                {5, true, new Action<IRegulatableMediaService>(m => m.Rewind())});
        }

        private readonly IReadOnlyList<MediaPlayerState> _playerStates = new List<MediaPlayerState>(5)
        {
            None,
            Pause,
            Play,
            Stop,
            FastForward,
            Rewind
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