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
    /// Interaction logic for VideoTab.xaml
    /// </summary>
    public partial class VideoTab : UserControl
    {
        public VideoTab()
        {
            InitializeComponent();
        }

        private void TbShowVideoTitle_OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string copyPaste = (string)e.DataObject.GetData(typeof(string));
                if (!TextBoxTextAllowed(copyPaste))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void TbShowVideoTitle_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!TextBoxTextAllowed(e.Key))
            {
                e.Handled = true;
            }
        }

        private bool TextBoxTextAllowed(Key key) => key >= Key.D0 && key <= Key.D9 ||
                                                    key >= Key.NumPad0 && key <= Key.NumPad9;

        private bool TextBoxTextAllowed(string input) => double.TryParse(input, out var value) && value >= 0;
    }
}
