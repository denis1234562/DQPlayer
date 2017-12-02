using System.Windows;

namespace DQPlayer.MVVMFiles.View
{
    /// <summary>
    /// Interaction logic for PlayList.xaml
    /// </summary>
    public partial class PlayList : Window
    {
        public PlayList()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
