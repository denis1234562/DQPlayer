using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using DQPlayer.MVVMFiles.ViewModels;
using System.Windows.Controls.Primitives;

namespace DQPlayer.MVVMFiles.UserControls.MainWindow
{
    public partial class MediaPlayerControlsUserControl
    {
        public MediaPlayerControlsViewModel ViewModel => DataContext as MediaPlayerControlsViewModel;

        public MediaPlayerControlsUserControl()
        {
            InitializeComponent();
            ViewModel.MediaAttached += ViewModel_MediaElementAttached;
            ToolTipService.ShowDurationProperty.OverrideMetadata(
                typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
        }

        private void SetupTimers()
        {
            ViewModel.CurrentMediaPlayer.MediaPlayerModel.MediaPlayerTimer.Interval = Settings.TimerTickUpdate;
            ViewModel.CurrentMediaPlayer.MediaPlayerModel.MediaPlayerTimer.Tick += MediaPlayerTimer_Tick;
        }

        private void MediaPlayerTimer_Tick(object sender, EventArgs e)
        {
            sMovieSkipSlider.Value = ViewModel.CurrentMediaPlayer.MediaElement.Position;
        }

        private void ViewModel_MediaElementAttached(object sender, IMediaElementUserControl args)
        {
            SetupTimers();
            if (args.MediaElement.NaturalDuration.HasTimeSpan)
            {
                BindSliderMaxValueToMediaElement(args.MediaElement);
            }
            else
            {
                args.MediaElement.MediaOpened += MediaElement_MediaOpened;
            }
        }

        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            //TODO maybe detach?
            BindSliderMaxValueToMediaElement((MediaElement) sender);
        }

        private void BindSliderMaxValueToMediaElement(MediaElement args)
        {
            sMovieSkipSlider.SetBinding(RangeBase.MaximumProperty,
                new Binding("TotalSeconds") { Source = args.NaturalDuration.TimeSpan });
        }

        private void Slider_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            var slider = (Slider) e.Source;
            var tt = (ToolTip)slider.ToolTip;
            if (e.MouseDevice.DirectlyOver.GetType() == typeof(Thumb))
            {
                //in case it was covered by thumb
                //TODO doesn't really work
                tt.IsOpen = true;
            }
            tt.HorizontalOffset = Mouse.GetPosition(slider).X - tt.ActualWidth / 2;
            tt.VerticalOffset = 0;
        }

        private void Slider_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var tt = (ToolTip) ((Slider) e.Source).ToolTip;
            tt.IsOpen = false;
        }
    }
}
