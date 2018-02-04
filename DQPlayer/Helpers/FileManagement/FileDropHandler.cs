using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DQPlayer.Helpers.Extensions;
using DQPlayer.MVVMFiles.Models.PlayList;

namespace DQPlayer.Helpers.FileManagement
{
    public static class FileDropHandler
    {
        public static bool ExtractDroppedItemsUri(DragEventArgs e, IEnumerable<FileExtension> extensions, out IEnumerable<FileInformation> fileUris)
        {                     
            IEnumerable<string> filePaths = ((DataObject)e.Data).GetFileDropList().Cast<string>();
            IEnumerable<string> validFiles = filePaths.Where(f => extensions.Select(fe => fe.Extension).Contains(f.GetFileExtension()));
            fileUris = validFiles.Select(f => new FileInformation(f));
            return validFiles.Any();
        }

        public static bool ExtractDroppedItemsUri(DragEventArgs e, out IEnumerable<FileInformation> fileUris)
        {
            return ExtractDroppedItemsUri(e, null, out fileUris);
        }
    }
}
