using System.Resources;
using System.Windows.Data;

namespace DQPlayer.Helpers.LocalizationManagement
{
    public class LocExtension : Binding
    {
        public LocExtension(string name, ResourceManager resManager)
            : base($"[{name},{resManager.BaseName}]")
        {
            Mode = BindingMode.OneWay;
            Source = TranslationSource.Instance;
        }
    }
}
