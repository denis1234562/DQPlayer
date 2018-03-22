using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DQPlayer.Helpers.CustomControls;
using DQPlayer.Helpers.FileManagement.FileInformation;
using DQPlayer.MVVMFiles.Commands;
using ExtendedDotNet.ReflectionHelpers;

namespace DQPlayer.Helpers.InputManagement
{
    public static class ManagerHelper
    {
        private static readonly Dictionary<Type, Action<IEnumerable<object>>> _newRequests;

        static ManagerHelper()
        {
            var fileInformations = typeof(IFileInformation).GetDerivedTypesFor(Assembly.GetExecutingAssembly());

            _newRequests = new Dictionary<Type, Action<IEnumerable<object>>>();
            foreach (var information in fileInformations)
            {
                var instance = typeof(Manager<>).MakeGenericType(information);
                var methodInfo = instance.GetMethod("Request", BindingFlags.NonPublic | BindingFlags.Static);
                _newRequests.Add(information, enumerable => methodInfo.Invoke(instance, new object[] { enumerable }));
            }
        }

        public static void Request(IEnumerable<IFileInformation> selectedFiles)
        {
            Request(new ManagerEventArgs<IFileInformation>(selectedFiles));
        }

        public static void Request(ManagerEventArgs<IFileInformation> args)
        {
            var typeGroups = args.SelectedFiles.GroupBy(information => information.GetType());
            foreach (var typeGroup in typeGroups)
            {
                _newRequests[typeGroup.Key].Invoke(typeGroup);
            }
        }
    }
}