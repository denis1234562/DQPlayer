using System;
using System.Collections.Generic;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.InputManagement
{
    public class ManagerEventArgs<TFileInformation>
        where TFileInformation : IFileInformation
    {
        public IEnumerable<TFileInformation> SelectedFiles { get; }

        public ManagerEventArgs(IEnumerable<TFileInformation> selectedFiles)
        {
            SelectedFiles = selectedFiles ?? throw new ArgumentNullException(nameof(selectedFiles));
        }
    }
}
