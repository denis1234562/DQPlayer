using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DQPlayer.Helpers
{
    public class FileDropHandler
    {
        public bool TryExtractDroppedItemUri(DragEventArgs e, IEnumerable<FileExtension> extensions, out Uri fileUri)
        {
            string filePath = ((DataObject) e.Data).GetFileDropList()[0];
            var fileExtension = filePath.Substring(filePath.LastIndexOf(".", StringComparison.Ordinal));
            fileUri = extensions != null && !extensions.Select(fe => fe.Extension).Contains(fileExtension) ? null : new Uri(filePath);
            return fileUri != null;
        }

        public bool TryExtractDroppedItemUri(DragEventArgs e, out Uri fileUri)
        {
            return TryExtractDroppedItemUri(e, null, out fileUri);
        }
    }
}
