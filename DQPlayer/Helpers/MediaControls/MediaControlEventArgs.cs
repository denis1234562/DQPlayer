using System;

namespace DQPlayer.Helpers.MediaControls
{
    public enum MediaControlEventType
    {
        BrowseClick,
        RewindClick,
        PlayClick,
        PauseClick,
        FastForwardClick,
        StopClick,

        PositionSliderDragStarted,
        PositionSliderDragCompleted,
        VolumeSliderValueChanged,

        MovePreviousClick,
        MoveNextClick,
        RepeatCheck,
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
