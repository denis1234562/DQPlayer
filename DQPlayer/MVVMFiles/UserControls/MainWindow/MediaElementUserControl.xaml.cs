using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DQPlayer.MVVMFiles.Models.MediaPlayer;
using DQPlayer.MVVMFiles.ViewModels;
using DQPlayer.States;

namespace DQPlayer.MVVMFiles.UserControls.MainWindow
{
    public partial class MediaElementUserControl : IMediaElementUserControl
    {
        public MediaElementViewModel ViewModel => DataContext as MediaElementViewModel;

        public MediaElement MediaElement => MediaPlayer;

        public MediaPlayerModel MediaPlayerModel
        {
            get => (MediaPlayerModel)GetValue(MediaPlayerModelProperty);
            set => SetValue(MediaPlayerModelProperty, value);
        }

        public static readonly DependencyProperty MediaPlayerModelProperty =
            DependencyProperty.Register(nameof(MediaPlayerModel), typeof(MediaPlayerModel), typeof(MediaElementUserControl),
                new PropertyMetadata(null));

        public MediaElementUserControl()
        {
            InitializeComponent();
            ViewModel.ControlsAttached += ViewModel_ControlsAttached;
            MediaPlayer = Settings.MediaPlayerTemplate.CloneAndOverride(MediaPlayer);
        }

        private void ViewModel_ControlsAttached(IMediaControlsViewModel obj)
        {
            //path binds to viewmodel
            SetBinding(MediaPlayerModelProperty,
                new Binding { Path = new PropertyPath("MediaPlayerModel"), Mode = BindingMode.OneWayToSource });
            MediaPlayerModel = new MediaPlayerModel(MediaPlayerStates.None);
            MediaPlayerModel.MediaController = new RegulatableMediaPlayerService(MediaPlayer, MediaPlayerModel);
        }
    }
}
