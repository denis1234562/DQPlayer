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

        public static TimeSpan GetFileDuration(this string FilePath)
        {
            if (FilePath.GetFileExtension() != Settings.SubtitleExtensionString)
            {
                Shell shell = new Shell();
                Folder folder = shell.NameSpace(Path.GetDirectoryName(FilePath));
                FolderItem folderItem = folder.ParseName(Path.GetFileName(FilePath));
                return TimeSpan.Parse(folder.GetDetailsOf(folderItem, 27));
            }
            return new TimeSpan(0);
        }
    }
}