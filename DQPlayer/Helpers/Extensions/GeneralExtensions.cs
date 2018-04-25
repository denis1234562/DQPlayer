using System;
using System.IO;
using System.Collections.Generic;
using DQPlayer.MVVMFiles.Commands;
using System.Runtime.Serialization.Formatters.Binary;

namespace DQPlayer.Helpers.Extensions
{
    public static class GeneralExtensions
    {
        public static T Min<T>(params T[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var comparer = Comparer<T>.Default;
            switch (values.Length)
            {
                case 0: throw new ArgumentException();
                case 1: return values[0];
                case 2:
                    return comparer.Compare(values[0], values[1]) < 0
                        ? values[0]
                        : values[1];
                default:
                    T best = values[0];
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (comparer.Compare(values[i], best) < 0)
                        {
                            best = values[i];
                        }
                    }
                    return best;
            }
        }

        public static T Max<T>(params T[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var comparer = Comparer<T>.Default;
            switch (values.Length)
            {
                case 0: throw new ArgumentException();
                case 1: return values[0];
                case 2:
                    return comparer.Compare(values[0], values[1]) > 0
                        ? values[0]
                        : values[1];
                default:
                    T best = values[0];
                    for (int i = 1; i < values.Length; i++)
                    {
                        if (comparer.Compare(values[i], best) > 0)
                        {
                            best = values[i];
                        }
                    }
                    return best;
            }
        }

        public static T SerializedClone<T>(this T source)
        {
            if (!typeof(T).IsSerializable) throw new ArgumentException(@"The type must be serializable.", nameof(source));

            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }

        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }

        public static Lazy<RelayCommand> CreateLazyRelayCommand(Action execute, Func<bool> canExecute = null)
            => new Lazy<RelayCommand>(() => new RelayCommand(execute, canExecute));

        public static Lazy<RelayCommand<T>> CreateLazyRelayCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
            => new Lazy<RelayCommand<T>>(() => new RelayCommand<T>(execute, canExecute));
    }
}
