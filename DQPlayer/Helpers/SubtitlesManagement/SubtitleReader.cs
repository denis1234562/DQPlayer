using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.Extensions.CollectionsExtensions;
using DQPlayer.Properties;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    public sealed class SubtitleReader
    {
        public Encoding Encoding { get; }

        public SubtitleReader([NotNull] Encoding encoding)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
        }

        public CircularList<SubtitleSegment> ExtractSubtitles([NotNull] string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

            var subtitles = new CircularList<SubtitleSegment>();
            using (var sr = new StreamReader(path, Encoding))
            {
                var text = sr.ReadToEnd();
                var lines = text.Split(new[] {"\r\n"}, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (TryParseSubtitleInterval(lines[i], out var interval))
                    {
                        var content = ExtractCurrentSubtitleContent(i, lines);
                        subtitles.Add(new SubtitleSegment(interval, content));
                    }
                }
            }
            return subtitles.OrderBy(s => s).ToCircularList();
        }

        private string ExtractCurrentSubtitleContent(int startIndex, string[] lines)
        {
            var subtitleContent = new StringBuilder();
            int endIndex = Array.IndexOf(lines, string.Empty, startIndex);
            for (int i = startIndex + 1; i < endIndex; i++)
            {
                subtitleContent.AppendLine(lines[i].Trim(' '));
            }
            return subtitleContent.ToString();
        }

        private bool TryParseSubtitleInterval(string input, out SubtitleInterval interval)
        {
            interval = null;
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            var segments = input.Split(new[] { Settings.SubtitleSeparationString }, StringSplitOptions.None);
            if (segments.Length != 2)
            {
                return false;
            }
            segments = segments.Select(s => s.Trim(' ').Replace(',', '.').Replace('.', ':')).ToArray();
            if (TimeSpan.TryParseExact(segments[0], Settings.GetTimeSpanStringFormats(), DateTimeFormatInfo.InvariantInfo,
                    out var start) &&
                TimeSpan.TryParseExact(segments[1], Settings.GetTimeSpanStringFormats(), DateTimeFormatInfo.InvariantInfo,
                    out var end) &&
                start < end)
            {
                interval = new SubtitleInterval(start, end);
                return true;
            }
            return false;
        }
    }
}