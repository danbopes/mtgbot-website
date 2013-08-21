using System;
using System.Collections.Generic;
using System.Linq;

namespace MTGO.Common.Helpers
{
    public static class EnumerableHelper
    {
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }

        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            return source.Skip(Math.Max(0, source.Count() - count)).Take(count);
        }

        public static IEnumerable<T> MoveUp<T>(this IEnumerable<T> enumerable, int itemIndex)
        {
            int i = 0;

            IEnumerator<T> enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                i++;

                if (itemIndex.Equals(i))
                {
                    T previous = enumerator.Current;

                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }

                    yield return previous;

                    break;
                }

                yield return enumerator.Current;
            }

            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source.Where((x, i) => i % chunkSize == 0).Select((x, i) => source.Skip(i * chunkSize).Take(chunkSize));
        }
    }
}
