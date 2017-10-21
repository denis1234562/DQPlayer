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
        public static ImageSource PlayImage { get; }
        public static ImageSource PauseImage { get; }

        static Settings()
        {
            AllowedExtensions = new HashSet<string>
            {
                ".mp3",
                ".mkv",
                ".mp4"
            };
            MinimumWindowSize = new Size(600, 410);
            PlayImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.PlayImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            PauseImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ResourceFiles.Strings.PauseImage.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
