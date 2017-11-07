using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DQPlayer.Helpers
{
    public class FileExtensionPackage : ISet<FileExtension>
    {
        private readonly ISet<FileExtension> _extensions;

        public string PackageName { get; }

        public FileExtensionPackage(string packageName, ISet<FileExtension> extensions)
        {
            _extensions = extensions ?? throw new ArgumentNullException(nameof(extensions));
            PackageName = packageName;
        }

        public override string ToString()
        {
            return string.Join(", ", _extensions.Select(fe => fe.ToString()));
        }

        #region ISet Implementation

        public int Count => _extensions.Count;
        public bool IsReadOnly => _extensions.IsReadOnly;
        public IEnumerator<FileExtension> GetEnumerator() => _extensions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<FileExtension>.Add(FileExtension item) => _extensions.Add(item);
        public bool Add(FileExtension item) => _extensions.Add(item);
        public void UnionWith(IEnumerable<FileExtension> other) => _extensions.UnionWith(other);
        public void IntersectWith(IEnumerable<FileExtension> other) => _extensions.IntersectWith(other);
        public void ExceptWith(IEnumerable<FileExtension> other) => _extensions.ExceptWith(other);
        public void SymmetricExceptWith(IEnumerable<FileExtension> other) => _extensions.SymmetricExceptWith(other);
        public bool IsSubsetOf(IEnumerable<FileExtension> other) => _extensions.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<FileExtension> other) => _extensions.IsSupersetOf(other);
        public bool IsProperSupersetOf(IEnumerable<FileExtension> other) => _extensions.IsProperSupersetOf(other);
        public bool IsProperSubsetOf(IEnumerable<FileExtension> other) => _extensions.IsProperSubsetOf(other);
        public bool Overlaps(IEnumerable<FileExtension> other) => _extensions.Overlaps(other);
        public bool SetEquals(IEnumerable<FileExtension> other) => _extensions.SetEquals(other);
        public void Clear() => _extensions.Clear();
        public bool Contains(FileExtension item) => _extensions.Contains(item);
        public void CopyTo(FileExtension[] array, int arrayIndex) => _extensions.CopyTo(array, arrayIndex);
        public bool Remove(FileExtension item) => _extensions.Remove(item);

        #endregion
    }
}