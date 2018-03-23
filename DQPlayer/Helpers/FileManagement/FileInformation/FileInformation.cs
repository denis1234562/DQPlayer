using System;
using System.IO;

namespace DQPlayer.Helpers.FileManagement.FileInformation
{
    public class FileInformation : IFileInformation , IEquatable<FileInformation>
    {
        public string FileName { get; }
        public FileInfo FileInfo { get; }
        public Uri Uri { get; }

        public FileInformation(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            FileInfo = new FileInfo(filePath);
            FileName = Path.GetFileNameWithoutExtension(FileInfo.Name);
            Uri = new Uri(FileInfo.FullName);
        }

        public FileInformation(Uri fileUri)
            : this(fileUri.OriginalString)
        {
        }

        #region Equality members

        public bool Equals(FileInformation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(FileName, other.FileName) && Equals(FileInfo, other.FileInfo) && Equals(Uri, other.Uri);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FileInformation)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FileName != null ? FileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FileInfo != null ? FileInfo.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Uri != null ? Uri.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion
    }
}
