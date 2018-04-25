using System;
using System.IO;
using System.Linq;
using DQPlayer.Annotations;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.SubtitlesManagement
{
    public static class SubtitleDetector
    {
        public static FileInformation DetectSubtitles(
            [NotNull] MediaFileInformation file,
            string preferedSubtitleLanguage)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var availableSubtitles =
                file.FileInfo.Directory.GetFiles($"*{Settings.SubtitleExtensionString}", SearchOption.AllDirectories);

            if (!string.IsNullOrEmpty(preferedSubtitleLanguage))
            {
                if (preferedSubtitleLanguage[0] != '.')
                {
                    preferedSubtitleLanguage = preferedSubtitleLanguage.Insert(0, ".");
                }

                var preferedLanguageSubtitle = availableSubtitles
                    .Where(s => s.Name.Contains(
                        $"{preferedSubtitleLanguage}{Settings.SubtitleExtensionString}"))
                    .FirstOrDefault(info => Path.GetFileNameWithoutExtension(info.Name) ==
                                            $"{file.FileName}{preferedSubtitleLanguage}");

                if (preferedLanguageSubtitle != null)
                {
                    return new FileInformation(preferedLanguageSubtitle.FullName);
                }
            }

            return availableSubtitles.Where(subs => Path.GetFileNameWithoutExtension(subs.Name) == file.FileName)
                .Select(subs => new FileInformation(subs.FullName)).FirstOrDefault();
        }
    }
}