using System;
using System.Collections.Generic;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.InputManagement
{
    public class FileManagerEventArgs<TFileInformation> : EventArgs
        where TFileInformation : IFileInformation
    {
        public IEnumerable<TFileInformation> SelectedFiles { get; }

        public FileManagerEventArgs(IEnumerable<TFileInformation> selectedFiles)
        {
            SelectedFiles = selectedFiles ?? throw new ArgumentNullException(nameof(selectedFiles));
        }
    }
}
