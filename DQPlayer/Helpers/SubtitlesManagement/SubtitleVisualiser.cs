using System;
using System.Collections.Generic;
using System.Linq;
using DQPlayer.Annotations;
using DQPlayer.Helpers.CustomControls;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    public static class SubtitleVisualiser
    {
        private static readonly Dictionary<SubtitleSegment, IEnumerable<OutlinedLabel>> _currentlyShownSubtitles =
            new Dictionary<SubtitleSegment, IEnumerable<OutlinedLabel>>();

        public static void ShowSubtitle(
            [NotNull] this IEnumerable<OutlinedLabel> labels,
            [NotNull] SubtitleSegment segment)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            if (_currentlyShownSubtitles.ContainsKey(segment)) return;

            var lines = GetAvailableLines(segment, labels).ToArray();
            foreach (var line in lines)
            {
                line.Label.Text = line.Value;
            }
            _currentlyShownSubtitles.Add(segment, lines.Select(l => l.Label));
        }

        private static IEnumerable<SubtitleLabelWrapper> GetAvailableLines(
            SubtitleSegment segment,
            IEnumerable<OutlinedLabel> labels)
        {
            var splitedLines = segment.Content.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var availableLines = labels.Where(l => string.IsNullOrEmpty(l.Text));

            return splitedLines.Reverse().Zip(availableLines, (s, label) => new SubtitleLabelWrapper(label, s));
        }

        public static void HideSubtitle(
            [NotNull] this IEnumerable<OutlinedLabel> labels,
            [NotNull] SubtitleSegment segment)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            if (segment == null) throw new ArgumentNullException(nameof(segment));

            if (_currentlyShownSubtitles.TryGetValue(segment, out var shownLabels))
            {
                foreach (var label in shownLabels)
                {
                    label.Text = string.Empty;
                }
                _currentlyShownSubtitles.Remove(segment);
            }
        }

        private class SubtitleLabelWrapper
        {
            internal OutlinedLabel Label { get; }
            internal string Value { get; }

            public SubtitleLabelWrapper(OutlinedLabel label, string value)
            {
                Label = label;
                Value = value;
            }
        }
    }
}
