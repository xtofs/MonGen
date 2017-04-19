using System;
using System.Linq;
using System.Collections.Generic;

namespace MonGen
{
    public static class RandomExtensions
    {
        public static long GetNextInt64(this Random rng, long min, long max)
        {
            var buf = new byte[8];
            rng.NextBytes(buf);
            var longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (max - min)) + min;
        }


        /// <summary>
        ///     inside out Yates Fisher shufffle
        ///     https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle#The_.22inside-out.22_algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rand"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IReadOnlyList<T> Shuffle<T>(this Random rand, IReadOnlyList<T> source)

        {
            var result = new T[source.Count];
            for (var i = 0; i < source.Count; i++)
            {
                var j = rand.Next(0, i);
                if (j != i)
                    result[i] = result[j];
                result[j] = source[i];
            }
            return result;
        }

        public static T Choose<T>(this Random rand, IReadOnlyList<T> source)
        {
            return source[rand.Next(0, source.Count)];
        }

        public static T Choose<T>(this Random rand, IReadOnlyCollection<T> source)
        {
            var array = source.ToArray();
            return array[rand.Next(0, array.Length)];
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> items, T last)
        {
            foreach (var item in items) yield return item;
            yield return last;
        }
    }
}