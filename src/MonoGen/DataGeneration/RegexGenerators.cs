using System;
using System.Linq;
using MonoGen.Regex;

namespace MonoGen.DataGeneration
{
    public class RegexGenerators
    {
        public static IGenerator<string> Make(Alternatives alternatives)
        {
            var n = alternatives.Count;
            return from i in Generators.Range(0, n)
                   from g in Make(alternatives[i])
                   select g;
        }

        public static IGenerator<string> Make(Sequence sequence)
        {
            return sequence.Select(atom => Make(atom)).Pivot().Select(lst => string.Concat(lst));
        }

        public static IGenerator<string> Make(Atom atom)
        {
            var m = atom.Multiplicity;
            var expr = Make(atom.Expression);
            return from lst in expr.Sequence(m.MinOccurs, m.MaxOccurs ?? 20)
                   select string.Concat(lst);
        }

        public static IGenerator<string> Make(ISimpleExpression expression)
        {
            var charset = expression as Charset;
            if (charset != null)
            {
                var cnt = charset.Count;
                return from i in Generators.Range(0, cnt-1)
                       select charset[i].ToString();
            }

            var literal = expression as Literal;
            if (literal != null)
            {
                return Generators.Return(literal.Value);
            }

            var group = expression as Group;
            if (group != null)
            {
                return Make(group.Root);
            }

            //var x = expression as AnyChar;
            return Generators.Return("<unimplemented>");
        }
    }
}