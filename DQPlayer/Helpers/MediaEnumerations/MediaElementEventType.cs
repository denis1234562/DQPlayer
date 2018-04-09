namespace DQPlayer.Helpers.MediaEnumerations
{
    public sealed class MediaElementEventType : Enumeration<MediaElementEventType>
    {
        public static readonly MediaElementEventType Started = new MediaElementEventType(nameof(Started), 0);
        public static readonly MediaElementEventType Ended = new MediaElementEventType(nameof(Ended), 1);

        private MediaElementEventType(string name, int value)
            : base(name, value)
        {
        }
    }
}