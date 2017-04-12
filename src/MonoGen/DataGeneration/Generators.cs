using System;
using System.Collections.Generic;
using System.Linq;
using MonoGen;
using MonoGen.Regex;
using MonoGen.ParserCombinators;

namespace MonoGen.DataGeneration
{
    public static class Generators
    {
     
        public static IGenerator<T> Return<T>(T t) => FromFunc(_ => t);


        public static IGenerator<string> Regex(string regex)
        {
            var ast = RegexAstParsers.Alternatives.Parse(regex);

            return RegexGenerators.Make(ast);
        }


        public static IGenerator<int> Range(int minValue, int maxValue)
        {
            return FromFunc(rng => rng.Next(minValue, maxValue));
        }

        public static IGenerator<long> Range(long minValue, long maxValue)
        {
            return FromFunc(rng => rng.GetNextInt64(minValue, maxValue));
        }

        public static IGenerator<double> Double()
        {
            return FromFunc(rng => rng.NextDouble());
        }

        public static IGenerator<DateTime> Range(DateTime min, DateTime max)
        {
            var ticks = (max - min).Ticks;
            return Range(0, ticks).Select(i => min + TimeSpan.FromTicks(i));
        }

        public static IGenerator<T> Select<S, T>(this IGenerator<S> gen, Func<S, T> f)
        {
            return FromFunc(rng => f(gen.Gen(rng)));
        }

        public static IGenerator<U> SelectMany<S, T, U>(this IGenerator<S> rand, Func<S, IGenerator<T>> r2,
            Func<S, T, U> f)
        {
            return FromFunc(rng =>
            {
                var a = rand.Gen(rng);
                var b = r2(a);
                var c = b.Gen(rng);
                return f(a, c);
            });
        }

        public static IGenerator<T> OneOf<T>()
        {
            return OneOf(Enum.GetValues(typeof(T)).Cast<T>().ToList());
        }

        public static IGenerator<T> OneOf<T>(IList<T> items)
        {
            return FromFunc(rng =>
                items[rng.Next(0, items.Count)]);
        }

        public static IGenerator<char> Char(string items)
        {
            return FromFunc(rng => items[rng.Next(0, items.Length)]);
        }

        public static IGenerator<IList<T>> Pivot<T>(this IEnumerable<IGenerator<T>> seq)
        {
            return FromFunc(rng => seq.Select(i => i.Gen(rng)).ToList());
        }

        public static IGenerator<IList<T>> Sequence<T>(this IGenerator<T> gen, int length)
        {
            return FromFunc(rng => Enumerable.Range(0, length).Select(i => gen.Gen(rng)).ToList());
        }

        public static IGenerator<ICollection<T>> Sequence<T>(this IGenerator<T> gen, int minLength, int maxLength)
        {
            return
                from n in Range(minLength, maxLength)
                from s in Sequence(gen, n)
                select s;
        }

        public static IGenerator<string> Sequence(this IGenerator<char> gen, int minLength, int maxLength)
        {
            return
                from n in Range(minLength, maxLength)
                from s in Sequence(gen, n)
                select new string(s.ToArray());
        }



        public static IGenerator<T> FromFunc<T>(Func<Random, T> func)
        {
            return new FuncGenerator<T>(func);
        }

        #region private IGenerator implementation

        private class FuncGenerator<T> : IGenerator<T>
        {
            private readonly Func<Random, T> _func;

            public FuncGenerator(Func<Random, T> func)
            {
                _func = func;
            }

            public T Gen(Random random)
            {
                return _func(random);
            }
        }

        #endregion
    }
}