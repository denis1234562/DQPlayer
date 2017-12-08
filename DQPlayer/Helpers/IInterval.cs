using System;

namespace DQPlayer.Helpers
{
    public interface IInterval<T> : IEquatable<T>, IComparable<T>
        where T : IInterval<T>
    {
        TimeSpan Start { get; }
        TimeSpan End { get; }
    }
}
