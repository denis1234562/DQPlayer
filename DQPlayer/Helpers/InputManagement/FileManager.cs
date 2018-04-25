using System;
using System.Linq;
using System.Collections.Generic;
using DQPlayer.Annotations;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.InputManagement
{
    public sealed class FileManager<TFileInformation>
        : IManager<FileManagerEventArgs<TFileInformation>>
        where TFileInformation : IFileInformation
    {
        public delegate void FileManagerCallback(TFileInformation fileInformation, bool repeatState);

        private static readonly object _padlock = new object();

        private static readonly Lazy<FileManager<TFileInformation>> _instance =
            new Lazy<FileManager<TFileInformation>>(() => new FileManager<TFileInformation>());

        public static FileManager<TFileInformation> Instance
        {
            get
            {
                lock (_padlock)
                {
                    return _instance.Value;
                }
            }
        }

        public event EventHandler<FileManagerEventArgs<TFileInformation>> Notify;

        private FileManager()
        {
        }

        public void Request(object sender, [NotNull] IEnumerable<TFileInformation> selectedFiles)
        {
            if (selectedFiles == null) throw new ArgumentNullException(nameof(selectedFiles));

            Request(sender, new FileManagerEventArgs<TFileInformation>(selectedFiles));
        }

        public void Request(object sender, [NotNull] FileManagerEventArgs<TFileInformation> args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            OnNewRequest(sender, args);
        }

        /// <summary>
        /// Called via reflection by <see cref="FileManagerHelper"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="selectedFiles"></param>
        [UsedImplicitly]
        private void Request(object sender, IEnumerable<object> selectedFiles)
        {
            Request(sender, selectedFiles.Cast<TFileInformation>());
        }

        private void OnNewRequest(object sender, FileManagerEventArgs<TFileInformation> args)
        {
            Notify?.Invoke(sender, args);
        }
    }
}