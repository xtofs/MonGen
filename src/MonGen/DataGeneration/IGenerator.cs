using System;

namespace MonGen.DataGeneration
{
    public interface IGenerator<out T>
    {
        T Gen(Random random);
    }
}