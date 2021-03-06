﻿namespace NeuralGasDotNet.Extensions
{
    public static class TupleExtensions
    {
        public static (double, double) TupleSubtraction(this (double, double) tuple1, (double, double) tuple2)
        {
            return (tuple1.Item1 - tuple2.Item1, tuple1.Item2 - tuple2.Item2);
        }

        public static bool Contains(this (int, int) tuple, int element)
        {
            return tuple.Item1 == element || tuple.Item2 == element;
        }
    }
}