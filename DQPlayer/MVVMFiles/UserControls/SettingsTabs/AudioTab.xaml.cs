using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DQPlayer.MVVMFiles.UserControls.SettingsTabs
{
    /// <summary>
    /// Interaction logic for AudioTab.xaml
    /// </summary>
    public partial class AudioTab : UserControl
    {
        public AudioTab()
        {
            InitializeComponent();
        }

        private void SBalanceSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            imgRightHeadphone.Opacity = Math.Max(0.01, 0.5 + e.NewValue / 2);
            imgLeftHeadphone.Opacity = Math.Max(0.01, 0.5 - e.NewValue / 2);           
        }
    }
}
