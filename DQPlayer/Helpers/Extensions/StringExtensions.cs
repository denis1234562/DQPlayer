using Shell32;
using System;
using System.IO;
using DQPlayer.Annotations;

namespace DQPlayer.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileExtension([NotNull] this string fileName)
        {
            return fileName?.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal)) ??
                   throw new ArgumentNullException(nameof(fileName));
        }

        public static TimeSpan GetFileDuration([NotNull] this FileInfo fileInfo)
        {
            if(fileInfo == null) throw new ArgumentNullException(nameof(fileInfo));
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