using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DQPlayer.Helpers.FileManagement.FileInformation;
using DQPlayer.Properties;
using ExtendedDotNet.ReflectionHelpers;

namespace DQPlayer.Helpers.InputManagement
{
    public static class FileManagerHelper
    {
        private static readonly Dictionary<Type, Action<object, IEnumerable<object>>> _newRequests;

        static FileManagerHelper()
        {
            var fileInformations = typeof(IFileInformation).GetDerivedTypesFor(Assembly.GetExecutingAssembly());

            _newRequests = new Dictionary<Type, Action<object, IEnumerable<object>>>();
            foreach (var information in fileInformations)
            {
                var type = typeof(FileManager<>).MakeGenericType(information);
                var instance = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
                var methodInfo = type.GetMethod("Request", BindingFlags.NonPublic | BindingFlags.Instance);
                _newRequests.Add(information,
                    (sender, enumerable) => methodInfo.Invoke(instance, new[] {sender, enumerable}));
            }
        }

        public static void Request(object sender, [NotNull] IEnumerable<IFileInformation> selectedFiles)
        {
            if (selectedFiles == null)
            {
                throw new ArgumentNullException(nameof(selectedFiles));
            }
            Request(sender, new FileManagerEventArgs<IFileInformation>(selectedFiles));
        }

        public static void Request(object sender, [NotNull] FileManagerEventArgs<IFileInformation> args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            var typeGroups = args.SelectedFiles.GroupBy(information => information.GetType());
            foreach (var typeGroup in typeGroups)
            {
                _newRequests[typeGroup.Key].Invoke(sender, typeGroup);
            }
        }
    }
}