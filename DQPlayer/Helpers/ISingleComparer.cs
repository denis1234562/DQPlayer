namespace DQPlayer.Helpers
{
    public interface ISingleComparer<in T>
    {
        int Compare(T x);
    }
}