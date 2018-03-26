using Shell32;
using System;
using System.IO;

namespace DQPlayer.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileExtension(this string fileName)
        {
            return fileName?.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal)) ??
                   throw new ArgumentNullException(nameof(fileName));
        }

        public static TimeSpan GetFileDuration(this FileInfo fileInfo)
        {
            if (fileInfo.Extension != Settings.SubtitleExtensionString)
            {
                Shell shell = new Shell();
                Folder folder = shell.NameSpace(fileInfo.DirectoryName);
                FolderItem folderItem = folder.ParseName(fileInfo.Name);
                return TimeSpan.Parse(folder.GetDetailsOf(folderItem, 27));
            }
            return TimeSpan.Zero;
        }
    }
}