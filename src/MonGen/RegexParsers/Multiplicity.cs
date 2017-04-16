namespace MonoGen.RegexParsers
{
    public class Multiplicity
    {
        public Multiplicity(int exact)
        {
            this.MinOccurs = exact;
            this.MaxOccurs = exact;
        }

        public Multiplicity(int min, int? max)
        {
            this.MinOccurs = min;
            this.MaxOccurs = max;
        }

        public int MinOccurs { get; }

        public int? MaxOccurs { get; }

        public bool IsUnbounded => !MaxOccurs.HasValue;

        public bool IsRepeating => !MaxOccurs.HasValue || MaxOccurs.Value > 1;

        public bool Equals(Multiplicity other)
        {
            return this.MinOccurs == other.MinOccurs && this.MaxOccurs == other.MaxOccurs;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Multiplicity;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCodes.CombineHashCodes(MinOccurs.GetHashCode(), MaxOccurs.GetHashCode());
        }

        public override string ToString() => $"Multiplicity({MinOccurs}{((MaxOccurs == MinOccurs) ? "" : $" {MaxOccurs}")})";

    }
}