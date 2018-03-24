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

        public static void Request(object sender, IEnumerable<TFileInformation> selectedFiles)
        {
            Request(sender, new ManagerEventArgs<TFileInformation>(selectedFiles));
        }

        public static void Request(object sender, ManagerEventArgs<TFileInformation> args)
        {
            OnNewRequest(sender, args);
        }

        //required for ManagerHelper's reflection
        private static void Request(object sender, IEnumerable<object> selectedFiles)
        {
            Request(sender, selectedFiles.Cast<TFileInformation>());
        }

        private static void OnNewRequest(object sender, ManagerEventArgs<TFileInformation> args)
        {
            NewRequest?.Invoke(sender, args);
        }
    }
}