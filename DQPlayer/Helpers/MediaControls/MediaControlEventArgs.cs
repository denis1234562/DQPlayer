using System;

namespace DQPlayer.Helpers.MediaControls
{
    public enum MediaControlEventType
    {
        PlaylistClick,
        SettingsClick,
        RepeatCheck,

        BrowseClick,
        RewindClick,
        MovePreviousClick,
        PlayClick,
        PauseClick,
        MoveNextClick,
        FastForwardClick,
        StopClick,

        PositionSliderDragStarted,
        PositionSliderDragCompleted,

        VolumeSliderValueChanged,
    }

    public class MediaControlEventArgs : EventArgs
    {
        public MediaControlEventType EventType { get; }
        public object AdditionalInfo { get; }

        public MediaControlEventArgs(MediaControlEventType eventType, object additionalInfo = null)
        {
            EventType = eventType;
            AdditionalInfo = additionalInfo;
        }
    }
}
