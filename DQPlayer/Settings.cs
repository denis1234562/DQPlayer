using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DQPlayer.Helpers.FileManagement;
using DQPlayer.Helpers.Template;
using DQPlayer.ResourceFiles;
using static System.Windows.Interop.Imaging;

namespace DQPlayer
{
    public static class Settings
    {
        public static FileExtensionPackage MediaPlayerExtensionPackage { get; }
        public static FilePickerFilter MediaPlayerExtensionPackageFilter { get; }

        public static System.Windows.Size MinimumMainWindowSize { get; }
        public static System.Windows.Size MinimumSettingsWindowSize { get; }

        public static TimeSpan SkipSeconds { get; }

        public static TimeSpan TimerTickUpdate { get; }

        public static TemplateCreator<MediaElement, UIElement> MediaPlayerTemplate { get; }

        public const string SubtitleSeparationString = @"-->";
        public const string SubtitleExtensionString = @".srt";

        private static readonly string[] _timeSpanStringFormats;

        public static Encoding Cyrillic { get; }

        static Settings()
        {
            TimerTickUpdate = TimeSpan.FromSeconds(0.25);
            SkipSeconds = TimeSpan.FromSeconds(10);
            MinimumMainWindowSize = new System.Windows.Size(600, 410);
            MinimumSettingsWindowSize = new System.Windows.Size(550, 610);
            _timeSpanStringFormats = new[]
            {
                @"h\:m\:s",
                @"h\:m\:s\:f",
                @"h\:m\:s\:ff",
                @"h\:m\:s\:fff",
                @"h\:m\:ss",
                @"h\:m\:ss\:f",
                @"h\:m\:ss\:ff",
                @"h\:m\:ss\:fff",
                @"h\:mm\:s",
                @"h\:mm\:s\:f",
                @"h\:mm\:s\:ff",
                @"h\:mm\:s\:fff",
                @"h\:mm\:ss",
                @"h\:mm\:ss\:f",
                @"h\:mm\:ss\:ff",
                @"h\:mm\:ss\:fff",
                @"hh\:m\:s",
                @"hh\:m\:s\:f",
                @"hh\:m\:s\:ff",
                @"hh\:m\:s\:fff",
                @"hh\:m\:ss",
                @"hh\:m\:ss\:f",
                @"hh\:m\:ss\:ff",
                @"hh\:m\:ss\:fff",
                @"hh\:mm\:s",
                @"hh\:mm\:s\:f",
                @"hh\:mm\:s\:ff",
                @"hh\:mm\:s\:fff",
                @"hh\:mm\:ss",
                @"hh\:mm\:ss\:f",
                @"hh\:mm\:ss\:ff",
                @"hh\:mm\:ss\:fff",
            };

            MediaPlayerExtensionPackage = new FileExtensionPackage(Strings.MediaFiles, new HashSet<FileExtension>
            {
                new FileExtension(".mkv", Strings.MatrioshkaFile),
                new FileExtension(".mp3", Strings.MusicFile),
                new FileExtension(".mp4", Strings.MovieFile),
                new FileExtension(SubtitleExtensionString, Strings.MovieFile),
            });
            MediaPlayerExtensionPackageFilter = new FilePickerFilter(MediaPlayerExtensionPackage);

            var mediaTemplate = new Template<MediaElement, UIElement>()
                .WithArgument(m => m.ScrubbingEnabled = true)
                .WithArgument(m => m.LoadedBehavior = MediaState.Manual)
                .WithArgument(m => m.UnloadedBehavior = MediaState.Manual)
                .WithArgument(m => m.Volume = 0);
            MediaPlayerTemplate = new TemplateCreator<MediaElement, UIElement>(mediaTemplate);
            Cyrillic = Encoding.GetEncoding("Windows-1251");
        }

        public static string[] GetTimeSpanStringFormats() => _timeSpanStringFormats;

        private static ImageSource ExtractImageSourceFromBitmap(Bitmap bitmap)
        {
            return CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
