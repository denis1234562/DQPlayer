using System;
using System.Collections.Generic;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    [Serializable]
    public class SubtitleSegment : IComparable<SubtitleSegment>, IEquatable<SubtitleSegment>
    {
        public SubtitleInterval SubtitleInterval { get; }

        public string Content { get; }

        public SubtitleSegment(SubtitleInterval subtitleInterval, string content)
        {
            SubtitleInterval = subtitleInterval;
            Content = content;
        }

        public override string ToString()
        {
            return $"{SubtitleInterval} {Environment.NewLine}{Content}";
        }

        #region IEquatable implementation

        public bool Equals(SubtitleSegment other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(SubtitleInterval, other.SubtitleInterval) && string.Equals(Content, other.Content);
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
                return ((SubtitleInterval != null ? SubtitleInterval.GetHashCode() : 0) * 397) ^
                       (Content != null ? Content.GetHashCode() : 0);
            }
        }

        #endregion

        #region IComparable implementation

        public int CompareTo(SubtitleSegment other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Comparer<SubtitleInterval>.Default.Compare(SubtitleInterval, other.SubtitleInterval);
        }
        #endregion
    }
}
