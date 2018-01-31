using DQPlayer.Helpers.Extensions;
using System;
using System.IO;
using System.Windows;

namespace DQPlayer.MVVMFiles.Models.PlayList
{
    public class FileInformation : IEquatable<FileInformation>
    {
        public string Title { get; }
        public TimeSpan Time { get; }
        public Uri FilePath { get; }
        public bool IsPlaying { get; set; } = false;

        public FileInformation(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentException(nameof(uri));
            }
            Time = uri.AbsolutePath.GetFileDuration();
            FilePath = uri;
            Title = Path.GetFileNameWithoutExtension(uri.AbsolutePath);
        }

        public bool Equals(FileInformation other)
        {
            if (other == null)
            {
                return false;
            }
            return ReferenceEquals(this,other);
        }
    }
}
