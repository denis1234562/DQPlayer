using System;
using System.IO;

namespace DQPlayer.Helpers.FileManagement.FileInformation
{
    public class FileInformation : IFileInformation
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
    }
}
