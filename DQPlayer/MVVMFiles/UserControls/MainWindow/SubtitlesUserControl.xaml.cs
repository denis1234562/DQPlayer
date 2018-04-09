using System.Windows.Controls;
using DQPlayer.Helpers;
using DQPlayer.Helpers.CustomCollections;
using DQPlayer.Helpers.MediaEnumerations;
using DQPlayer.Helpers.SubtitlesManagement;
using DQPlayer.MVVMFiles.ViewModels;

namespace DQPlayer.MVVMFiles.UserControls.MainWindow
{
    public partial class SubtitlesUserControl : UserControl
    {
        public SubtitlesViewModel ViewModel => DataContext as SubtitlesViewModel;

        private MediaObservableMap<SubtitlesEventType> _subtitlesMap;

        public SubtitlesUserControl()
        {
            InitializeComponent();

            InitializeMaps();
            ((ICustomObservable<MediaEventArgs<SubtitlesEventType>>) ViewModel).Notify += SubtitlesUserControl_OnNotify;
        }

        private void InitializeMaps()
        {
            _subtitlesMap = new MediaObservableMap<SubtitlesEventType>((map, args) => args.EventType)
            {
                [SubtitlesEventType.Display] = DisplaySubtitles,
                [SubtitlesEventType.Hide] = HideSubtitles
            };
        }

        private void HideSubtitles(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            gridSubtitles.HideSubtitle((SubtitleSegment) e.AdditionalInfo);
        }

        private void DisplaySubtitles(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            gridSubtitles.ShowSubtitle((SubtitleSegment) e.AdditionalInfo);
        }

        private void SubtitlesUserControl_OnNotify(object sender, MediaEventArgs<SubtitlesEventType> e)
        {
            _subtitlesMap[e.EventType].Invoke(sender, e);
        }
    }
}
