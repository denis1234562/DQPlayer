using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using DQPlayer.Annotations;
using DQPlayer.Helpers.Extensions.CollectionsExtensions;

namespace DQPlayer.Helpers.LocalizationManagement
{
    public class TranslationSource : INotifyPropertyChanged
    {
        private readonly Dictionary<string, ResourceManager> _rmCache = new Dictionary<string, ResourceManager>();

        public static TranslationSource Instance { get; } = new TranslationSource();

        public event PropertyChangedEventHandler PropertyChanged;

        public string this[string key, string resourceManager]
        {
            get
            {
                var rm = _rmCache.GetOrAdd(resourceManager,
                    () => new ResourceManager(resourceManager, Assembly.GetExecutingAssembly()));
                return rm.GetString(key, currentCulture);
            }
        }

        private CultureInfo currentCulture;
        public CultureInfo CurrentCulture
        {
            get => currentCulture;
            set
            {
                if (!Equals(currentCulture, value))
                {
                    currentCulture = value;
                    OnPropertyChanged(string.Empty);
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
