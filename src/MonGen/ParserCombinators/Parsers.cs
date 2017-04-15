using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RE = System.Text.RegularExpressions;

namespace MonoGen.ParserCombinators
{
    public static class Parsers
    {
        #region Output
        private class Triple<T> : IParserOutput<T>
        {
            public T Value { get; set; }
            public string Str { get; set; }
            public int Pos { get; set; }

            public Triple(string str, int pos, T expected)
            {
                this.Str = str;
                this.Pos = pos;
                this.Value = expected;
            }
        }
        private static IParserOutput<T> Failure<T>(string s, int i)
        {
            throw new ParserException(s, i);
        }

        private static IParserOutput<T> Success<T>(string s, int i, T t)
        {
            return new Triple<T>(s, i, t);
        }
        #endregion

        public static T Parse<T>(this StringParser<T> parser, string str) => parser(str, 0).Value;

        public static StringParser<object> Fail => (s, i) => { throw new ParserException(s, i); };

        public static StringParser<T> Return<T>(T t) => (s, p) => Success(s, p, t);


        public static StringParser<string> Expect(string expected)
        {
            return (string s, int i) =>
             (s.Substring(i, expected.Length) == expected) ?
                Success(s, i + expected.Length, expected) :
                Failure<string>(s, i);
        }


        public static StringParser<char> Any => (s, p) => (p < s.Length) ? Success(s, p + 1, s[p]) : Failure<char>(s, p);
        public static StringParser<char> Char(char chr) => (s, p) => (p < s.Length && s[p] == chr) ? Success(s, p + 1, s[p]) : Failure<char>(s, p);

        public static StringParser<Match> Regex(string regex)
        {
            var r = new RE.Regex("\\G" + regex, RE.RegexOptions.Singleline | RE.RegexOptions.Compiled);
            return (s, p) =>
             {
                 var m = r.Match(s, p);
                 if (m.Success)
                     return Success(s, p + m.Length, m);
                 else
                     return Failure<Match>(s, p);
             };
        }

        public static StringParser<T> Except<T>(this StringParser<T> parser, T item) => 
            parser.Where(t => !t.Equals(item));

        public static StringParser<T> Where<T>(this StringParser<T> parser, Func<T, bool> predicate) =>
            (s, p) => { var r = parser(s, p); return predicate(r.Value) ? r : Failure<T>(s, p); };

        public static StringParser<T> Select<S, T>(this StringParser<S> parser, Func<S, T> f) =>
            (s, p) => { var r = parser(s, p); return Success(r.Str, r.Pos, f(r.Value)); };

        public static StringParser<U> SelectMany<S, T, U>(this StringParser<S> parser, Func<S, StringParser<T>> f, Func<S, T, U> g) =>
            (s, p) => { var r = parser(s, p); var q = f(r.Value)(r.Str, r.Pos); return Success(q.Str, q.Pos, g(r.Value, q.Value)); };


        public static StringParser<T> OrElse<T>(this StringParser<T> parser, StringParser<T> other) =>
            (s, p) => { try { return parser(s, p); } catch(ParserException) { return other(s, p); } };

        public static StringParser<IList<T>> Many<T>(this StringParser<T> parser) =>
            (s, p) =>
            {
                var o = p;
                var r = new List<T>();
                try { while (true) { var q = parser(s, p); p = q.Pos; r.Add(q.Value); } }
                catch (ParserException) { }
                return Success(s, p, r);
            };

        public static StringParser<IList<T>> AtLeastOne<T>(this StringParser<T> parser) =>
            (str, pos) =>
            {
                var result = new List<T>();
                pos = AddSuccess(parser, str, pos, result);
                try
                {
                    while (true)
                    {
                        pos = AddSuccess(parser, str, pos, result);
                    }
                }
                catch (ParserException)
                {
                    // catch the exception that ended the while loop but swallow it.                    
                }
                return Success(str, pos, result);
            };

        public static StringParser<IList<T>> SeparatedBy<T, S>(this StringParser<T> parser, StringParser<S> separator) =>
           (str, pos) =>
           {
               var original = pos;
               var result = new List<T>();
               pos = AddSuccess(parser, str, pos, result);
               try
               {
                   while (true)
                   {
                       var sep = separator(str, pos);
                       var x = AddSuccess(parser, str, sep.Pos, result);
                       pos = x; // only avance if both separator and parser are successfull
                   }
               }
               catch (ParserException)
               {
                   // catch the exception that ended the while loop but swallow it.
               }
               return Success(str, pos, result);
           };

        private static int AddSuccess<T>(StringParser<T> parser, string s, int p, ICollection<T> r)
        {
            var output = parser(s, p);
            r.Add(output.Value);
            return output.Pos;
        }


        /// <summary>
        /// parse Zero or One items of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parser"></param>
        /// <returns>a collection with zero or ones items of T</returns>
        public static StringParser<ICollection<T>> Optional<T>(this StringParser<T> parser) =>
            (s, p) =>
            {
                var r = new List<T>();
                try { p = AddSuccess(parser, s, p, r); }
                catch (ParserException) { }
                return Success(s, p, r);
            };

        public static StringParser<Int32> Integer =>
            Regex("[0-9]+").Select(s => Int32.Parse(s.Value));


        internal static StringParser<T> OneOf<T>(params StringParser<T>[] parsers)
        {
            return (s, p) =>
            {
                foreach (var parser in parsers)
                {
                    try
                    {
                        return parser(s, p);
                    }
                    catch (ParserException)
                    {
                        // try next
                    }
                }
                return Failure<T>(s, p);
            };
        }
    }
}
