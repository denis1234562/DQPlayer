using System;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.FileManagement
{
    public class FileExtension : IEquatable<FileExtension>
    {
        public string Extension { get; }
        public string Name { get; }

        public FileExtension([NotNull] string extension, string name)
        {
            if (string.IsNullOrEmpty(extension)) throw new ArgumentException(nameof(extension));

            Extension = extension;
            Name = name;
        }

        public FileExtension(string extension) 
            : this(extension, string.Empty)
        {         
        }

        public override string ToString() => $"{Name} ({Extension})";

        #region Equality members

        public bool Equals(FileExtension other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Extension, other.Extension);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileExtension) obj);
        }

        public override int GetHashCode()
        {
            return Extension != null ? Extension.GetHashCode() : 0;
        }

        #endregion
    }
}