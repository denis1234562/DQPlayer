namespace DQPlayer.Helpers.MediaEnumerations
{
    public sealed class MediaControlEventType : Enumeration<MediaControlEventType>
    {
        public static MediaControlEventType BrowseClick = new MediaControlEventType(nameof(BrowseClick), 0);
        public static MediaControlEventType RewindClick = new MediaControlEventType(nameof(RewindClick), 1);
        public static MediaControlEventType PlayClick = new MediaControlEventType(nameof(PlayClick), 2);
        public static MediaControlEventType PauseClick = new MediaControlEventType(nameof(PauseClick), 3);
        public static MediaControlEventType FastForwardClick = new MediaControlEventType(nameof(FastForwardClick), 4);
        public static MediaControlEventType StopClick = new MediaControlEventType(nameof(StopClick), 5);
        public static MediaControlEventType PositionSliderDragStarted = new MediaControlEventType(nameof(PositionSliderDragStarted), 6);
        public static MediaControlEventType PositionSliderDragCompleted = new MediaControlEventType(nameof(PositionSliderDragCompleted), 7);
        public static MediaControlEventType VolumeSliderValueChanged = new MediaControlEventType(nameof(VolumeSliderValueChanged), 8);
        public static MediaControlEventType MovePreviousClick = new MediaControlEventType(nameof(MovePreviousClick), 9);
        public static MediaControlEventType MoveNextClick = new MediaControlEventType(nameof(MoveNextClick), 10);
        public static MediaControlEventType RepeatCheck = new MediaControlEventType(nameof(RepeatCheck), 11);

        private MediaControlEventType(string name, int value)
            : base(name, value)
        {
        }
    }
}