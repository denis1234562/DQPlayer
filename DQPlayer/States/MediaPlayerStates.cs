using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            ConstructorInfo ctor = typeof(MediaPlayerState)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType)
                    .SequenceEqual(new[] {typeof(int), typeof(bool), typeof(Action<IRegulatableMediaService>)}));
            None = (MediaPlayerState) ctor.Invoke(new object[] {0, false, null});
            Pause = (MediaPlayerState) ctor.Invoke(new object[]
                {1, false, (Action<IRegulatableMediaService>) (m => m.Pause())});
            Play = (MediaPlayerState) ctor.Invoke(new object[]
                {2, true, (Action<IRegulatableMediaService>) (m => m.Play())});
            Stop = (MediaPlayerState) ctor.Invoke(new object[]
                {3, false, (Action<IRegulatableMediaService>) (m => m.Stop())});
            FastForward = (MediaPlayerState) ctor.Invoke(new object[]
                {4, true, (Action<IRegulatableMediaService>) (m => m.FastForward())});
            Rewind = (MediaPlayerState) ctor.Invoke(new object[]
                {5, true, (Action<IRegulatableMediaService>) (m => m.Rewind())});
        }

        private readonly List<MediaPlayerState> _playerStates = new List<MediaPlayerState>
        {
            Pause,
            Play,
            Stop
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