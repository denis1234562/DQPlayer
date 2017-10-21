using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        static Settings()
        {
            SkipSeconds = TimeSpan.FromSeconds(10);
            AllowedExtensions = new HashSet<string>
            {
                ".mp3",
                ".mkv",
                ".mp4"
            };
            MinimumWindowSize = new Size(600, 410);
            SpashScreenImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.SpashScreenImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            PlayImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.PlayImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            PauseImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.PauseImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            SkipBack = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.SkipBackImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            SkipForward = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.SkipForwardImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Stop = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.StopImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
