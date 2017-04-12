using System;

namespace SampleDataGenerators
{
    public interface IGenerator<out T>
    {
        T Gen(Random random);
    }
}