using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DQPlayer
{
    public static class Settings
    {
        public static HashSet<string> AllowedExtensions { get; }

        static Settings()
        {
            AllowedExtensions = new HashSet<string>
            {
                ".mp3",
                ".mkv",
            };
        }
    }
}
