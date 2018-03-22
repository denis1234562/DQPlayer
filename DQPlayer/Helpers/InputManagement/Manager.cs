using System;
using System.Linq;
using System.Collections.Generic;
using DQPlayer.Helpers.FileManagement.FileInformation;

namespace DQPlayer.Helpers.InputManagement
{
    public static class Manager<TFileInformation>
        where TFileInformation : IFileInformation
    {
        public static event EventHandler<ManagerEventArgs<TFileInformation>> NewRequest;

        public static void Request(IEnumerable<TFileInformation> selectedFiles)
        {
            Request(new ManagerEventArgs<TFileInformation>(selectedFiles));
        }

        public static void Request(ManagerEventArgs<TFileInformation> args)
        {
            OnNewRequest(args);
        }

        private static void Request(IEnumerable<object> selectedFiles)
        {
            Request(selectedFiles.Cast<TFileInformation>());
        }

        private static void OnNewRequest(ManagerEventArgs<TFileInformation> args)
        {
            NewRequest?.Invoke(typeof(Manager<TFileInformation>), args);
        }
    }
}