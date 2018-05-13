using System;
using System.Globalization;
using DQPlayer.Annotations;

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

        public static bool TryParse(
            [NotNull] string[] segments,
            [NotNull] string[] formats,
            out SubtitleInterval interval)
        {
            return TryParse(segments, formats, DateTimeFormatInfo.InvariantInfo, out interval);
        }

        public static bool TryParse(
            [NotNull] string[] segments,
            [NotNull] string[] formats,
            DateTimeFormatInfo formatInfo,
            out SubtitleInterval interval)
        {
            if (segments == null) throw new ArgumentNullException(nameof(segments));
            if (formats == null) throw new ArgumentNullException(nameof(formats));

            if (segments.Length != 2)
            {
                interval = null;
                return false;
            }

            if (TimeSpan.TryParseExact(segments[0], formats, formatInfo, out var start) &&
                TimeSpan.TryParseExact(segments[1], formats, formatInfo, out var end))
            {
                interval = new SubtitleInterval(start, end);
                return true;
            }
            interval = null;
            return false;
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
            return obj is SubtitleInterval si && Equals(si);
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
