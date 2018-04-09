using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DQPlayer.Annotations;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.FileManagement
{
    public static class FileDropHandler
    {
        public static bool ExtractDroppedFiles([NotNull] DragEventArgs e, IEnumerable<FileExtension> extensions, out IEnumerable<IFileInformation> fileUris)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }
            IEnumerable<string> filePaths = ((DataObject)e.Data).GetFileDropList().Cast<string>();
            IEnumerable<string> validFiles = filePaths.Where(f => extensions.Select(fe => fe.Extension).Contains(f.GetFileExtension()));           
            fileUris = validFiles.Select(FileProcesser.Selector);
            return validFiles.Any();
        }

        public static bool ExtractDroppedFiles([NotNull] DragEventArgs e, out IEnumerable<IFileInformation> fileUris)
        {
            return ExtractDroppedFiles(e, null, out fileUris);
        }
    }
}
