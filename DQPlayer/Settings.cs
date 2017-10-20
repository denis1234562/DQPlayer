using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DQPlayer
{
    public static class Settings
    {
        public static HashSet<string> AllowedExtensions { get; }
        public static Size MinimumWindowSize { get; }

        static Settings()
        {
            AllowedExtensions = new HashSet<string>
            {
                ".mp3",
                ".mkv",
            };
            MinimumWindowSize = new Size(600, 410);
        }
    }
}
