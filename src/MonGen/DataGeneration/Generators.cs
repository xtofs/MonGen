using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MonGen.ParserCombinators;
using MonGen.DataGeneration;
using MonGen.RegexParsers;

namespace MonGen.DataGeneration
{
    public static class Generators
    {     
        /// <summary>
        ///     construct a Generator that returns the given value always
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IGenerator<T> Constant<T>(T value)
        {
            return Create(_ => value);
        }


        public static IGenerator<T> Sized<T>(this Func<int, IGenerator<T>> f)
        {
            throw new NotImplementedException();
        }
        // sized:: (Int -> Gen a) -> Gen a

        [SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames", MessageId = "0#")]
        public static IGenerator<string> Regex(string regex)
        {
            var ast = RegexAstParsers.Alternatives.Parse(regex);

            return ast.Generator();
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

        public static IGenerator<TTarget> Select<TSource, TTarget>(this IGenerator<TSource> gen,
            Func<TSource, TTarget> f)
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

        public static IGenerator<IReadOnlyList<T>> Pivot<T>(this IEnumerable<IGenerator<T>> seq)
        {
            return Create(rng => seq.Select(i => i.Gen(rng)).ToList());
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

        /// <summary>
        ///     Generator that generates a character from the character set.
        ///     The character set is represented similar to regular expression character classes: a sequence of single characters
        ///     or character ranges (e.g. a-z). 
        /// </summary>
        /// <param name="charsetString">a sequence of single characters or ranges of characters. E.g.  "a-z", "a-z0-9", "a-z$%*-"</param>
        /// <returns>Generator that generates singel characters from teh character set</returns>
        public static IGenerator<char> Character(string charsetString)

        {
            var charset = RegexAstParsers.RawCharterSets.Parse(charsetString);
            return
                from i in Range(0, charset.Count)
                select charset[i];
        }

        /// <summary>
        /// generator that generates a permutation of the given items
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static IGenerator<IReadOnlyList<T>> Shuffle<T>(IReadOnlyList<T> items)
        {
            return Create(rng => rng.Shuffle(items));
        }

        /// <summary>
        ///     Generate a string based on character classes        
        /// </summary>        
        /// <returns></returns>
        public static IGenerator<string> Password(int length, params string[] classes)
        {
            if (classes.Length > length)
                throw new ArgumentOutOfRangeException(
                    $"{nameof(length)} needs to be larger then number of character classes");

            var n = classes.Length;

            // list of generators starting with one for each class and then one for all classes combined.
            var generators = ImmutableList
                .CreateRange(from c in classes select Character(c))                
                .Add(Character(string.Concat(classes)));

            // create a list of indices into the generator list of length {length}
            // starting with the index of the {classes} and followed by the index of the "all" class:  0, 1, 2, ... n-1, n, n, n ..
            var indices = ImmutableList.CreateRange(Enumerable.Range(0, n)).AddRange(Enumerable.Repeat(n, length - n));

            // create the final generator by combining a generator that creates a permutation of the indices 
            // with the generators in the `generators` list
            var gen = 
                from perm in Shuffle(indices)
                from chars in perm.Select(ix => generators[ix]).Pivot()
                select new string(chars.ToArray());
                
            return gen;
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