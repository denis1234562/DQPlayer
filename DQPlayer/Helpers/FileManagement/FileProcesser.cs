using System;
using DQPlayer.Annotations;
using DQPlayer.Helpers.Extensions;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.FileManagement
{
    public static class FileProcesser
    {
        public static IFileInformation Selector([NotNull] string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            string extension = filePath.GetFileExtension();
            if (Settings.MediaExtensionsPackage.Contains(new FileExtension(extension)))
            {
                return new MediaFileInformation(filePath);
            }
            if (Settings.NonMediaExtensionsPackage.Contains(new FileExtension(extension)))
            {
                return new FileInformation.FileInformation(filePath);
            }
            throw new ArgumentException($"Unrecognized file - {filePath}");
        }
    }
}
