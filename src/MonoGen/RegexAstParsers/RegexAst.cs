using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MonoGen.Regex
{
    public class Charset : ISimpleExpression, IEquatable<Charset>
    {
        public bool Negated { get; set; }

        public ISet<IElement> Elements { get; set; }

        public Charset(params IElement[] elements) { Elements = new HashSet<IElement>(elements); }

        public Charset(IEnumerable<IElement> elements) { Elements = new HashSet<IElement>(elements); }

        public int Count => Elements.Sum(e => e.Count);

        public char this[int i] => Get(i, 0);

        private char Get(int n, int segmentNo)
        {
            if (segmentNo >= Elements.Count)
                throw new ArgumentOutOfRangeException();
            var segment = Elements.ElementAt(segmentNo);
            if (n >= segment.Count)
                return Get(n - segment.Count, segmentNo + 1);
            else
                return segment[n];
        }

        #region Element ADT
        public interface IElement { int Count { get; } bool Contains(char chr); char this[int i] { get; } }

        private static char ArgumentOutOfRange()
        {
            throw new ArgumentOutOfRangeException();
        }

        public class Single : IElement, IEquatable<Single>
        {
            public char Char { get; }

            public Single(char c) { Char = c; }

            public bool Contains(char chr) => chr == Char;

            public int Count => 1;

            public char this[int i] => i == 0 ? Char : ArgumentOutOfRange();

            public bool Equals(Single other)
            {
                return this.Char == other.Char;
            }
            public override bool Equals(object obj)
            {
                var other = obj as Single;
                return other != null && Equals(other);
            }

            public override int GetHashCode()
            {
                return Char.GetHashCode();
            }

            public override string ToString() => $"{Char}";
        }

        public class Range : IElement, IEquatable<Range>
        {
            public char First { get; }

            public char Last { get; }

            public Range(char first, char rest) { this.First = first; this.Last = rest; }

            public bool Contains(char chr) => First <= chr && chr <= Last;

            public int Count => (Last - First) + 1;

            public char this[int i] => (i + First <= Last + 1) ? (char)(First + i) : ArgumentOutOfRange();

            public bool Equals(Range other)
            {
                return this.First == other.First && this.Last == other.Last;
            }

            public override bool Equals(object obj)
            {
                var other = obj as Range;
                return other != null && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCodes.CombineHashCodes(
                    First.GetHashCode(),
                    Last.GetHashCode());
            }

            public override string ToString() => $"{First}-{Last}";
        }

        #endregion

        public bool Contains(char chr) { return Elements.Any(e => e.Contains(chr)); }

        public bool Equals(Charset other)
        {
            return Elements.Count == other.Elements.Count && Elements.Zip(other.Elements, (e, o) => e.Equals(o)).All(i => i);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Charset;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Elements.GetHashCode();
        }

        public override string ToString() => $"Charset({string.Concat(Elements)})";

    }

    public interface ISimpleExpression // sealed: Group,  Literal, Charset (maybe StartOfMatch, EndOfMatch, AnyChar,)
    { }

    public class Literal : ISimpleExpression
    {
        public string Value { get; }

        public Literal(string v)
        {
            this.Value = v;
        }

        public bool Equals(Literal other)
        {
            return this.Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Literal;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString() => $"Literal({Value})";

    }

    public class Atom : IEquatable<Atom>
    {
        public Atom(ISimpleExpression e, Multiplicity m = null)
        {
            this.Expression = e;
            this.Multiplicity = m ?? Multiplicities.One;
        }

        public ISimpleExpression Expression { get; }

        public Multiplicity Multiplicity { get; }

        public bool Equals(Atom other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Multiplicity, other.Multiplicity) && Equals(Expression, other.Expression);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Atom)) return false;
            return Equals((Atom)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Multiplicity != null ? Multiplicity.GetHashCode() : 0) * 397) ^ (Expression != null ? Expression.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Atom left, Atom right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Atom left, Atom right)
        {
            return !Equals(left, right);
        }

        public override string ToString() => $"Atom({Expression}{IfNot(Multiplicity, Multiplicities.One)})";

        private static string IfNot<T>(T a, T b) => $"{(a.Equals(b) ? "" : $" {a}")}";
    }

    public class Sequence : Collection<Atom>, IEquatable<Sequence>
    {
        public Sequence(params Atom[] atoms) : base(atoms)
        {
        }

        public Sequence(IList<Atom> atoms) : base(atoms)
        {
        }

        public bool Equals(Sequence other)
        {
            return other != null && this.SequenceEqual(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Sequence)obj);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(0, (h, a) => (h * 397) ^ a.GetHashCode());
        }

        public static bool operator ==(Sequence left, Sequence right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Sequence left, Sequence right)
        {
            return !Equals(left, right);
        }

        public override string ToString() => $"Sequence({string.Join(" ", this.Items)})";

    }

    public class Alternatives : Collection<Sequence>, IEquatable<Alternatives>
    {
        public Alternatives(params Sequence[] sequences) : base(sequences)
        {
        }

        public Alternatives(IList<Sequence> sequences) : base(sequences)
        {
        }

        public bool Equals(Alternatives other)
        {
            return other != null && this.SequenceEqual(other);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Alternatives;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCodes.CombineHashCodes(this.Items);
        }

        public override string ToString() => $"Alternatives({string.Join(" ", this.Items)})";

    }

    public class Group : ISimpleExpression
    {
        public Group(Alternatives root)
        {
            Root = root;
        }

        public Alternatives Root { get; }


        public bool Equals(Group other)
        {
            return other != null && this.Root.Equals(other.Root);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Group;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return Root.GetHashCode();
        }

        public override string ToString() => $"Group({Root})";

    }
}
