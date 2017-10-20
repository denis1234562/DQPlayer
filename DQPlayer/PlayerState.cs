using System;

namespace DQPlayer
{
    public class PlayerState : IEquatable<PlayerState>
    {
        public bool IsRunning { get; }

        private readonly int _id;

        private PlayerState(int id, bool isRunning)
        {
            _id = id;
            IsRunning = isRunning;
        }

        public bool Equals(PlayerState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id == other._id && IsRunning == other.IsRunning;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PlayerState)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_id * 397) ^ IsRunning.GetHashCode();
            }
        }
    }
}