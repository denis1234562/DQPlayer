namespace DQPlayer.Helpers.MediaEnumerations
{
    public sealed class SubtitlesEventType : Enumeration<SubtitlesEventType>
    {
        public static SubtitlesEventType Display = new SubtitlesEventType(nameof(Display), 0);
        public static SubtitlesEventType Hide = new SubtitlesEventType(nameof(Hide), 1);

        private SubtitlesEventType(string name, int value)
            : base(name, value)
        {
        }
    }
}