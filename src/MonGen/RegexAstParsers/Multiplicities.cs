namespace MonoGen.Regex
{
    public static class Multiplicities
    {
        public static readonly Multiplicity One = new Multiplicity(1);
        public static readonly Multiplicity ZeroToMany = new Multiplicity(0, null);
        public static readonly Multiplicity OneToMany = new Multiplicity(1, null);
    }
}