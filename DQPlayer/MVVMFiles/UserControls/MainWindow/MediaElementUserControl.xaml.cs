using DQPlayer.MVVMFiles.ViewModels;

namespace DQPlayer.MVVMFiles.UserControls.MainWindow
{
    public partial class MediaElementUserControl
    {
        public MediaElementUserControl()
        {
            InitializeComponent();
            MediaPlayer = Settings.MediaPlayerTemplate.CloneAndOverride(MediaPlayer);
            DataContext = new MediaElementViewModel(MediaPlayer);
        }
    }
}
