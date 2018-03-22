using System;
using System.IO;

namespace DQPlayer.Helpers.FileManagement.FileInformation
{
    public interface IFileInformation
    {
        string FileName { get; }
        FileInfo FileInfo { get; }
        Uri Uri { get; }
    }
}