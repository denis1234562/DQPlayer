using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DQPlayer
{
    public class PlayerStates : IEnumerable<PlayerState>
    {
        public static PlayerState Pause;
        public static PlayerState Play;
        public static PlayerState Stop;

        static PlayerStates()
        {
            ConstructorInfo ctor = typeof(PlayerState).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Select(p => p.ParameterType)
                    .SequenceEqual(new[] { typeof(int), typeof(bool) }));

            Pause =(PlayerState)ctor.Invoke(new object[] { 1, false });
            Play = (PlayerState)ctor.Invoke(new object[] { 2, true });
            Stop = (PlayerState)ctor.Invoke(new object[] { 3, false });
        }

        private readonly List<PlayerState> _playerStates = new List<PlayerState>
        {
            Pause, Play, Stop
        };

        public IEnumerator<PlayerState> GetEnumerator()
        {
            return _playerStates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}