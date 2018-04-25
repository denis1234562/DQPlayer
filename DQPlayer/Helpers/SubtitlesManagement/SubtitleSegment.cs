using System;
using System.Collections.Generic;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    [Serializable]
    public class SubtitleSegment : IEquatable<SubtitleSegment>, IComparable<SubtitleSegment>
    {
        public SubtitleInterval Interval { get; }

        public string Content { get; }

        public SubtitleSegment([NotNull] SubtitleInterval subtitleInterval, string content)
        {
            Interval = subtitleInterval ?? throw new ArgumentNullException(nameof(subtitleInterval));
            Content = content;
        }

        public override string ToString()
        {
            return $"{Interval} {Environment.NewLine} {Content}";
        }

        #region IEquatable implementation

        public bool Equals(SubtitleSegment other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Interval, other.Interval) && string.Equals(Content, other.Content);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubtitleSegment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Interval != null ? Interval.GetHashCode() : 0) * 397) ^
                       (Content != null ? Content.GetHashCode() : 0);
            }
        }

        #endregion

        #region IComparable implementation

        public int CompareTo(SubtitleSegment other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Interval.CompareTo(other.Interval);
        }

        #endregion
    }
}
