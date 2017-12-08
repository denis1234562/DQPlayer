using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DQPlayer.Helpers.Extensions;

namespace DQPlayer.Helpers.FileManagement
{
    public static class FileDropHandler
    {
        public static bool TryExtractDroppedItemsUri(DragEventArgs e, IEnumerable<FileExtension> extensions, out IEnumerable<Uri> fileUris)
        {                     
            IEnumerable<string> filePaths = ((DataObject)e.Data).GetFileDropList().Cast<string>();
            IEnumerable<string> validFiles = filePaths.Where(f => extensions.Select(fe => fe.Extension).Contains(f.GetFileExtension()));
            fileUris = validFiles.Select(f => new Uri(f));
            return validFiles.Any();
        }

        public static bool TryExtractDroppedItemsUri(DragEventArgs e, out IEnumerable<Uri> fileUris)
        {
            return TryExtractDroppedItemsUri(e, null, out fileUris);
        }
    }
}
