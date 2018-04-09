using System;
using System.Text;
using System.Windows;
using System.Drawing;
using System.Windows.Media;
using DQPlayer.ResourceFiles;
using System.Windows.Controls;
using DQPlayer.Helpers.Templates;
using System.Collections.Generic;
using DQPlayer.Helpers.Animations;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using DQPlayer.Helpers.FileManagement;
using static System.Windows.Interop.Imaging;
using Size = System.Windows.Size;

namespace DQPlayer
{
    public static class Settings
    {
        public static TimeSpan TimerTickUpdate { get; private set; }
        public static TimeSpan FastForwardSeconds { get; private set; }
        public static TimeSpan RewindSeconds { get; private set; }
        public static Size MinimumMainWindowSize { get; private set; }
        public static Size MinimumSettingsWindowSize { get; private set; }
        public static Encoding Cyrillic { get; private set; }

        public static TemplateCreator<MediaElement, UIElement> MediaPlayerTemplate { get; private set; }

        public static FilePickerFilter NonMediaExtensionPackageFilter { get; private set; }
        public static FilePickerFilter MediaExtensionPackageFilter { get; private set; }
        public static FilePickerFilter MediaPlayerExtensionPackageFilter { get; private set; }

        public static FileExtensionPackage NonMediaExtensionsPackage { get; private set; }
        public static FileExtensionPackage MediaExtensionsPackage { get; private set; }
        public static FileExtensionPackage MediaPlayerExtensionsPackage { get; private set; }

        public static AnimationManager AnimationManager { get; private set; }

        public const string SubtitleSeparationString = @"-->";
        public const string SubtitleExtensionString = @".srt";

        //Will be replaced later.
        public static bool DetectSubtitlesAutomatically { get; } = true;
        public const string PreferedSubtitleLanguage = ".bg";

        private static readonly string[] _timeSpanStringFormats =
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

        static Settings()
        {
            ConfigureGeneralSettings();
            ConfigureTemplates();
            ConfigureExtensionPackages();
            ConfigureAnimations();
        }

        private static void ConfigureGeneralSettings()
        {
            TimerTickUpdate = TimeSpan.FromMilliseconds(250);
            FastForwardSeconds = TimeSpan.FromSeconds(15);
            RewindSeconds = -TimeSpan.FromSeconds(10);
            MinimumMainWindowSize = new Size(width: 720, height: 410);
            MinimumSettingsWindowSize = new Size(550, 610);
            Cyrillic = Encoding.GetEncoding("Windows-1251");
        }

        private static void ConfigureTemplates()
        {
            var mediaTemplate = new Template<MediaElement, UIElement>()
                .WithArgument(m => m.ScrubbingEnabled = true)
                .WithArgument(m => m.LoadedBehavior = MediaState.Manual)
                .WithArgument(m => m.UnloadedBehavior = MediaState.Manual)
                .WithArgument(m => m.Volume = 0);
            MediaPlayerTemplate = new TemplateCreator<MediaElement, UIElement>(mediaTemplate);
        }

        private static void ConfigureExtensionPackages()
        {
            NonMediaExtensionsPackage = new FileExtensionPackage(Strings.NonMediaFiles, new HashSet<FileExtension>
            {
                new FileExtension(SubtitleExtensionString, Strings.MovieFile),
            });

            MediaExtensionsPackage = new FileExtensionPackage(Strings.MediaFiles, new HashSet<FileExtension>
            {
                new FileExtension(".mkv", Strings.MatrioshkaFile),
                new FileExtension(".mp3", Strings.MusicFile),
                new FileExtension(".mp4", Strings.MovieFile),
                new FileExtension(".avi", Strings.AviFile)
            });

            MediaPlayerExtensionsPackage = new FileExtensionPackage(Strings.AllFiles, NonMediaExtensionsPackage);
            MediaPlayerExtensionsPackage.UnionWith(MediaExtensionsPackage);

            MediaPlayerExtensionPackageFilter = new FilePickerFilter(MediaPlayerExtensionsPackage);
            NonMediaExtensionPackageFilter = new FilePickerFilter(NonMediaExtensionsPackage);
            MediaExtensionPackageFilter = new FilePickerFilter(MediaExtensionsPackage);
        }

        private static void ConfigureAnimations()
        {
            AnimationManager = new AnimationManager();

            AnimationManager.AddAnimation(new AnimationWrapper("FadeOut", () => new DoubleAnimation
            {
                To = 0,
                BeginTime = TimeSpan.FromMilliseconds(2500),
                Duration = TimeSpan.FromMilliseconds(1000),
                EasingFunction = new QuadraticEase()
            }, UIElement.OpacityProperty, (sender, args, element) => element.Opacity = 0));
        }

        public static string[] GetTimeSpanStringFormats() => _timeSpanStringFormats;

        private static ImageSource ExtractImageSourceFromBitmap(Bitmap bitmap)
        {
            return CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
