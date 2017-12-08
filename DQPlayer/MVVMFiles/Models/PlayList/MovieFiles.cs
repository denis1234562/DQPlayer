using System;
using Shell32;
using System.IO;

namespace DQPlayer.MVVMFiles.Models.PlayList
{
    public class MovieFiles
    {
        public string Title { get; }
        public TimeSpan Time { get; }
        public Uri MoviePath { get; }

        public MovieFiles(Uri path)
        {
            Time = GetDuration(path);
            MoviePath = path;
            Title = Path.GetFileName(path.LocalPath);
        }

        private TimeSpan GetDuration(Uri uri)
        {
            Shell shell = new Shell();
            Folder folder = shell.NameSpace(Path.GetDirectoryName(uri.LocalPath));
            FolderItem folderItem = folder.ParseName(Path.GetFileName(uri.LocalPath));
            return TimeSpan.Parse(folder.GetDetailsOf(folderItem, 27));
        }
    }
}
