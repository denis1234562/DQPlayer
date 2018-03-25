using System;
using System.Linq;
using System.Collections.Generic;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.InputManagement
{
    public interface IManager<TArgs>
        where TArgs : EventArgs
    {
        event EventHandler<TArgs> NewRequest;
        void Request(object sender, TArgs e);
    }

    public sealed class FileManager<TFileInformation>
        : IManager<FileManagerEventArgs<TFileInformation>>
        where TFileInformation : IFileInformation
    {
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

        public event EventHandler<FileManagerEventArgs<TFileInformation>> NewRequest;

        private FileManager()
        {

        }

        public void Request(object sender, IEnumerable<TFileInformation> selectedFiles)
        {
            Request(sender, new FileManagerEventArgs<TFileInformation>(selectedFiles));
        }

        public void Request(object sender, FileManagerEventArgs<TFileInformation> args)
        {
            OnNewRequest(sender, args);
        }

        //required for ManagerHelper's reflection
        private void Request(object sender, IEnumerable<object> selectedFiles)
        {
            Request(sender, selectedFiles.Cast<TFileInformation>());
        }

        private void OnNewRequest(object sender, FileManagerEventArgs<TFileInformation> args)
        {
            NewRequest?.Invoke(sender, args);
        }
    }
}