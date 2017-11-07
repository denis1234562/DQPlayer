using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DQPlayer.Helpers
{
    public class FilePickerFilter
    {
        public string Filter { get; }

        public FilePickerFilter(FileExtensionPackage extensionPackage)
        {
            if (extensionPackage == null)
            {
                throw new ArgumentNullException(nameof(extensionPackage));
            }
            Filter = ConstructFilter(extensionPackage);
        }

        private static string ConstructFilter(FileExtensionPackage extensionPackage)
        {
            IEnumerable<string> cache = extensionPackage.Select(ae => "*" + ae.Extension);
            StringBuilder filter =
                new StringBuilder(
                    $"{extensionPackage.PackageName} ({string.Join(",", cache)})|{string.Join(";", cache)}");
            foreach (var fileExtension in extensionPackage)
            {
                filter.Append("|");
                filter.Append($"{fileExtension.Name} (*{fileExtension.Extension})|*{fileExtension.Extension}");
            }
            return filter.ToString();
        }
    }
}