using System;
using System.Collections.Generic;
using System.IO;
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
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException(@"The type must be serializable.", nameof(source));
            }

            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
