using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DQPlayer.ControlTemplates;
using DQPlayer.CustomControls;
using static System.Windows.Interop.Imaging;
using Size = System.Windows.Size;

namespace DQPlayer
{
    public static class Settings
    {
        public static HashSet<string> AllowedExtensions { get; }

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
            AllowedExtensions = new HashSet<string>
            {
                ".mp3",
                ".mkv",
                ".mp4"
            };
            MinimumWindowSize = new Size(600, 410);

            var mediaTemplate = new ControlTemplate<MediaElement>()
                .WithArgument(m => m.ScrubbingEnabled = true)
                .WithArgument(m => m.LoadedBehavior = MediaState.Manual)
                .WithArgument(m => m.UnloadedBehavior = MediaState.Manual)
                .WithArgument(m => m.Volume = 0);
            MediaPlayerTemplate = new ControlTemplateCreator<MediaElement>(mediaTemplate);

            SpashScreenImage = ExtractImageSourceFromBitmap(ResourceFiles.Strings.SplashScreenImage);
            PlayImage = ExtractImageSourceFromBitmap(ResourceFiles.Strings.PlayImage);
            PauseImage = ExtractImageSourceFromBitmap(ResourceFiles.Strings.PauseImage);
            SkipBack = ExtractImageSourceFromBitmap(ResourceFiles.Strings.SkipBackImage);
            SkipForward = ExtractImageSourceFromBitmap(ResourceFiles.Strings.SkipForwardImage);
            Stop = ExtractImageSourceFromBitmap(ResourceFiles.Strings.StopImage);
        }

        private static ImageSource ExtractImageSourceFromBitmap(Bitmap bitmap)
        {
            return CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
