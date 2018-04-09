namespace DQPlayer.Helpers.MediaEnumerations
{
    public sealed class SubscriptionEventType : Enumeration<SubscriptionEventType>
    {
        public static SubscriptionEventType MediaAttached = new SubscriptionEventType(nameof(MediaAttached), 0);
        public static SubscriptionEventType MediaDetached = new SubscriptionEventType(nameof(MediaDetached), 1);

        private SubscriptionEventType(string name, int value)
            : base(name, value)
        {
        }
    }
}