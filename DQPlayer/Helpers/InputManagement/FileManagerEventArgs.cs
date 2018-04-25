using System;
using System.Collections.Generic;
using DQPlayer.Annotations;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.InputManagement
{
    public class FileManagerEventArgs<TFileInformation> : EventArgs
        where TFileInformation : IFileInformation
    {
        public IEnumerable<TFileInformation> SelectedFiles { get; }
        public FileManager<TFileInformation>.FileManagerCallback Callback { get; }
        public object OriginalSource { get; }

        public FileManagerEventArgs(
            [NotNull] IEnumerable<TFileInformation> selectedFiles,
            FileManager<TFileInformation>.FileManagerCallback callback = null)
        {
            SelectedFiles = selectedFiles ?? throw new ArgumentNullException(nameof(selectedFiles));
            Callback = callback;
        }

        public FileManagerEventArgs(
            [NotNull] IEnumerable<TFileInformation> selectedFiles,
            object originalSource,
            FileManager<TFileInformation>.FileManagerCallback callback = null)
            : this(selectedFiles, callback)
        {
            OriginalSource = originalSource;
        }
    }
}
