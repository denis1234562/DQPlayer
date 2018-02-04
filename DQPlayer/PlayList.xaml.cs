using DQPlayer.MVVMFiles.ViewModels;

namespace DQPlayer
{
    public partial class PlayList
    {
        public PlayListViewModel ViewModel => DataContext as PlayListViewModel;

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
