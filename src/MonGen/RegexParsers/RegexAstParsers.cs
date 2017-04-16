using System;
using MonoGen.ParserCombinators;

namespace MonoGen.RegexParsers
{
    public static class RegexAstParsers
    {
        public static StringParser<Charset.IElement> Single =
            Parsers.Any.Except(']').Select(c => new Charset.Single(c));

        public static StringParser<Charset.IElement> Range =
            from m in Parsers.Regex("(.)-(.)")
            select new Charset.Range(m.Groups[1].Value[0], m.Groups[2].Value[0]);

        public static StringParser<Charset> Charset =
            from o in Parsers.Char('[')
            from n in Parsers.Char('^').Optional()
            from e in Range.OrElse(Single).Many()
            from c in Parsers.Char(']')
            select new Charset(e) { Negated = n.Count > 0 };


        // TODO: the list of chars is incomplete and also doesn't allow for excaped control chars.
        public static StringParser<Literal> Literal =
            Parsers.Regex("[a-zA-Z0-9$@#%&_-]+").Select(s => new Literal(s.Value));

        public static StringParser<ISimpleExpression> Expression =
            Parsers.OneOf<ISimpleExpression>(Charset, Literal, Group());
        
        public static StringParser<Multiplicity> RangeMultiplicity =
            from m in Parsers.Regex("\\{([0-9]+)(,([0-9]+)?)?\\}")
            let g = m.Groups
            let min = int.Parse(g[1].Value)
            let max = g[2].Value.StartsWith(",") ? (string.IsNullOrEmpty(g[3].Value) ? (int?)null : int.Parse(g[3].Value)): min
            select new Multiplicity(min, max);

        public static StringParser<Multiplicity> Asterix =
            Parsers.Char('*').Select(_ => Multiplicities.ZeroToMany);

        public static StringParser<Multiplicity> Plus =
            Parsers.Char('+').Select(_ => Multiplicities.OneToMany);

        public static StringParser<Multiplicity> Multiplicity =
            Parsers.OneOf(
                RangeMultiplicity, Asterix, Plus, Parsers.Return(Multiplicities.One));

        public static StringParser<Atom> Atom =
            from e in Expression
            from m in Multiplicity
            select new Atom(e, m);

        public static StringParser<Sequence> Sequence =
            from atoms in Atom.AtLeastOne()
            select new Sequence(atoms);

        public static StringParser<Regex> Alternatives =
            from sequences in Sequence.SeparatedBy(Parsers.Char('|'))
            select new Regex(sequences);


        public static StringParser<Group> Group() =>
            from o in Parsers.Char('(')
            from a in Alternatives
            from c in Parsers.Char(')')
            select new Group(a);

    }
}