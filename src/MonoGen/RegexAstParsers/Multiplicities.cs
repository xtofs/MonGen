namespace MonoGen.Regex
{
    public class Multiplicities
    {
        public static Multiplicity One = new Multiplicity(1);
        public static Multiplicity ZeroToMany = new Multiplicity(0, null);
        public static Multiplicity OneToMany = new Multiplicity(1, null);
    }
}