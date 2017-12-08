using System;

namespace DQPlayer.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string GetFileExtension(this string fileName)
        {
            return fileName?.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal)) ??
                   throw new ArgumentNullException(nameof(fileName));
        }
    }
}