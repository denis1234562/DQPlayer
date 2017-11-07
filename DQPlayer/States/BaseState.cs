using System;

namespace DQPlayer.States
{
    [Serializable]
    public class BaseState : IEquatable<BaseState>
    {
        private readonly int _id;

        protected BaseState(int id)
        {
            _id = id;
        }

        public bool Equals(BaseState other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BaseState) obj);
        }

        public override int GetHashCode()
        {
            return _id;
        }
    }
}