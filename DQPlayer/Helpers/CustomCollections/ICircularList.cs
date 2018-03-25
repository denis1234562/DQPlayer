using System.Collections.Generic;

namespace DQPlayer.Helpers.CustomCollections
{
    public interface ICircularList<T> : IList<T>
    {
        T Next { get; }
        T Previous { get; }
        T MoveNext();
        T MovePrevious();
        T Current { get; }
        void SetCurrent(int currentIndex);
        void Reset();
    }
}
