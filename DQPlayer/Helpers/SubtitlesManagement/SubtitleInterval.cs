using System;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    [Serializable]
    public class SubtitleInterval : IInterval<SubtitleInterval>
    {
        public TimeSpan Start { get; }
        public TimeSpan End { get; }

        public TimeSpan Duration => End.Subtract(Start);

        public SubtitleInterval(TimeSpan start, TimeSpan end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"{Start} --> {End}";
        }

        #region Implementation of IEquatable<SubtitleInterval>

        public bool Equals(SubtitleInterval other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubtitleInterval)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        #endregion

        #region Implementation of IComparable<SubtitleInterval>

        public int CompareTo(SubtitleInterval other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var startComparison = Start.CompareTo(other.Start);
            if (startComparison != 0) return startComparison;
            return End.CompareTo(other.End);
        }

        #endregion
    }
}
