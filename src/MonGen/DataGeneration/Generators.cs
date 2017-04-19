using System;
using System.Collections.Generic;
using System.Linq;
using MonGen;
using MonGen.RegexParsers;
using MonGen.ParserCombinators;

namespace MonGen.DataGeneration
{
    public static class Generators
    {
     
        /// <summary>
        /// construct a Generator that returns the given value always
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IGenerator<T> Constant<T>(T value) => Create(_ => value);


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        public static IGenerator<string> Regex(string regex)
        {
            var ast = RegexAstParsers.Alternatives.Parse(regex);

            return RegexGenerators.Generator(ast);
        }

        public static IGenerator<char> CharacterSet(string characterSet)
        {
            var parser = RegexAstParsers.Range.OrElse(RegexAstParsers.Single).Many()
                .Select(elements => 
                new Charset(elements));
                
            var charset = parser.Parse(characterSet);

            return from i in Generators.Range(0, charset.Count)
                   select charset[i];
        }



        public static IGenerator<int> Range(int minValue, int maxValue)
        {
            return Create(rng => rng.Next(minValue, maxValue));
        }

        public static IGenerator<long> Range(long minValue, long maxValue)
        {
            return Create(rng => rng.GetNextInt64(minValue, maxValue));
        }

        public static IGenerator<double> Double()
        {
            return Create(rng => rng.NextDouble());
        }

        public static IGenerator<DateTime> Range(DateTime min, DateTime max)
        {
            var ticks = (max - min).Ticks;
            return Range(0, ticks).Select(i => min + TimeSpan.FromTicks(i));
        }

        public static IGenerator<TTarget> Select<TSource, TTarget>(this IGenerator<TSource> gen, Func<TSource, TTarget> f)
        {
            return Create(rng => f(gen.Gen(rng)));
        }

        public static IGenerator<U> SelectMany<S, T, U>(this IGenerator<S> rand, Func<S, IGenerator<T>> r2,
            Func<S, T, U> f)
        {
            return Create(rng =>
            {
                var a = rand.Gen(rng);
                var b = r2(a);
                var c = b.Gen(rng);
                return f(a, c);
            });
        }

        /// <summary>
        /// if T is an enum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IGenerator<T> OneOf<T>()
        {
            return OneOf(Enum.GetValues(typeof(T)).Cast<T>().ToList());
        }

        public static IGenerator<T> OneOf<T>(IList<T> items)
        {
            return Create(rng =>
                items[rng.Next(0, items.Count)]);
        }

        public static IGenerator<char> Char(string items)
        {
            return Create(rng => items[rng.Next(0, items.Length)]);
        }

        /// <summary>
        /// transforms a list of generators into a generator of lists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static IGenerator<IReadOnlyList<T>> Pivot<T>(this IEnumerable<IGenerator<T>> seq)
        {
            return Create(rng => seq.Select(i => i.Gen(rng)).ToList().AsReadOnly());
        }

        public static IGenerator<IList<T>> Sequence<T>(this IGenerator<T> gen, int length)
        {
            return Create(rng => Enumerable.Range(0, length).Select(i => gen.Gen(rng)).ToList());
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

        public static IGenerator<IReadOnlyList<T>> Shuffle<T>(IReadOnlyList<T> items)
        {
            return Generators.Create(rng => rng.Shuffle(items));
        }

        public static IGenerator<string> Password(int len, params string[] categories)
        {
            var n = categories.Length;

            // array of generators for the given character sets and one "all" category
            var gens = categories
                .Select(c => Generators.CharacterSet(c))
                .Append(Generators.CharacterSet(string.Concat(categories)))
                .ToArray();

            // array of indices (into categories) starting with the given categories
            // and then repeatedly the new "all" category:  0, 1, 2, ..., n-1, n, n, n, ...
            var indices = Enumerable.Range(0, n).Concat(Enumerable.Repeat(n, len - n)).ToArray();

            var g = from perm in Shuffle(indices)
                    from x in perm.Select(i => gens[i]).Pivot()
                    select new string(x.ToArray());
            return g;
        }


        public static IGenerator<T> Create<T>(Func<Random, T> func)
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