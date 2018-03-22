using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DQPlayer.Helpers.FileManagement.FileInformation;
using ExtendedDotNet.ReflectionHelpers;

namespace DQPlayer.Helpers.InputManagement
{
    public static class ManagerHelper
    {
        private static readonly Dictionary<Type, Action<object,IEnumerable<object>>> _newRequests;

        static ManagerHelper()
        {
            var fileInformations = typeof(IFileInformation).GetDerivedTypesFor(Assembly.GetExecutingAssembly());

            _newRequests = new Dictionary<Type, Action<object,IEnumerable<object>>>();
            foreach (var information in fileInformations)
            {
                var instance = typeof(Manager<>).MakeGenericType(information);
                var methodInfo = instance.GetMethod("Request", BindingFlags.NonPublic | BindingFlags.Static);
                _newRequests.Add(information,(sender ,enumerable) => methodInfo.Invoke(instance, new object[] { sender,enumerable }));
            }
        }

        public static void Request(object sender, IEnumerable<IFileInformation> selectedFiles)
        {
            Request(sender,new ManagerEventArgs<IFileInformation>(selectedFiles));
        }

        public static void Request(object sender, ManagerEventArgs<IFileInformation> args)
        {
            var typeGroups = args.SelectedFiles.GroupBy(information => information.GetType());
            foreach (var typeGroup in typeGroups)
            {
                _newRequests[typeGroup.Key].Invoke(sender,typeGroup);
            }
        }
    }
}