using System;
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
                var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (TryParseSubtitleInterval(lines[i], out var interval))
                    {
                        var content = ExtractCurrentSubtitleContent(ref i, lines);
                        subtitles.Add(new SubtitleSegment(interval, content));
                    }
                }
            }
            subtitles.Sort();
            return subtitles;
        }

        private static string ExtractCurrentSubtitleContent(ref int currentIndex, string[] lines)
        {
            var subtitleContent = new StringBuilder();
            int endIndex = Array.IndexOf(lines, string.Empty, currentIndex);
            for (currentIndex++; currentIndex < endIndex; currentIndex++)
            {
                subtitleContent.AppendLine(lines[currentIndex].Trim(' '));
            }
            return subtitleContent.ToString();
        }

        private static bool TryParseSubtitleInterval(string input, out SubtitleInterval interval)
        {
            if (string.IsNullOrEmpty(input))
            {
                interval = null;
                return false;
            }

            var segments = input.Split(new[] { Settings.SubtitleSeparationString }, StringSplitOptions.None)
                .Select(s => s.Trim(' ').Replace(',', '.').Replace('.', ':')).ToArray();
            return SubtitleInterval.TryParse(segments, Settings.GetTimeSpanStringFormats(), out interval);
        }
    }
}