using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DQPlayer.ControlTemplates;
using DQPlayer.CustomControls;
using DQPlayer.Helpers;
using DQPlayer.ResourceFiles;
using static System.Windows.Interop.Imaging;
using Size = System.Windows.Size;

namespace DQPlayer
{
    public static class Settings
    {
        public static FileExtensionPackage MediaPlayerExtensionPackage { get; }
        public static FilePickerFilter MediaPlayerExtensionPackageFilter { get; }

        public static Size MinimumWindowSize { get; }

        public static ImageSource SpashScreenImage { get; }
        public static ImageSource PlayImage { get; }
        public static ImageSource PauseImage { get; }
        public static ImageSource SkipBack { get; }
        public static ImageSource SkipForward { get; }
        public static ImageSource Stop { get; }

        public static TimeSpan SkipSeconds { get; }

        public static TimeSpan TimerTickUpdate { get; }

        public static ControlTemplateCreator<MediaElement> MediaPlayerTemplate { get; }

        static Settings()
        {
            TimerTickUpdate = TimeSpan.FromSeconds(0.25);
            SkipSeconds = TimeSpan.FromSeconds(10);
            MinimumWindowSize = new Size(600, 410);

            MediaPlayerExtensionPackage = new FileExtensionPackage(Strings.MediaFiles, new HashSet<FileExtension>
            {
                new FileExtension(".mkv", Strings.MatrioshkaFile),
                new FileExtension(".mp3", Strings.MusicFile),
                new FileExtension(".mp4", Strings.MovieFile),
            });
            MediaPlayerExtensionPackageFilter = new FilePickerFilter(MediaPlayerExtensionPackage);

            var mediaTemplate = new ControlTemplate<MediaElement>()
                .WithArgument(m => m.ScrubbingEnabled = true)
                .WithArgument(m => m.LoadedBehavior = MediaState.Manual)
                .WithArgument(m => m.UnloadedBehavior = MediaState.Manual)
                .WithArgument(m => m.Volume = 0);
            MediaPlayerTemplate = new ControlTemplateCreator<MediaElement>(mediaTemplate);

            SpashScreenImage = ExtractImageSourceFromBitmap(Strings.SplashScreenImage);
            PlayImage = ExtractImageSourceFromBitmap(Strings.PlayImage);
            PauseImage = ExtractImageSourceFromBitmap(Strings.PauseImage);
            SkipBack = ExtractImageSourceFromBitmap(Strings.SkipBackImage);
            SkipForward = ExtractImageSourceFromBitmap(Strings.SkipForwardImage);
            Stop = ExtractImageSourceFromBitmap(Strings.StopImage);
        }

        private static ImageSource ExtractImageSourceFromBitmap(Bitmap bitmap)
        {
            return CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
