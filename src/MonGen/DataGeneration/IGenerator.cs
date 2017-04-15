using System;

namespace MonoGen.DataGeneration
{
    public interface IGenerator<out T>
    {
        T Gen(Random random);
    }
}