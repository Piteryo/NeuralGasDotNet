using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Troschuetz.Random;

namespace NeuralGasDotNet.Services.NeuralGas.DataGeneration
{
    public static class DataGenerator
    {
        public static async Task<List<(double, double)>> GenerateLineInsideCircle(int sizeOfCluster = 100,
            double circleRadius = 1.0)
        {
            var returnValue = GenerateRandomArray(sizeOfCluster, circleRadius);
            var randomAngles = new List<double>();
            var distributor = new ContinuousUniformDistribution();
            await Task.Run(() =>
            {
                for (var i = 0; i < sizeOfCluster; ++i)
                {
                    randomAngles.Add(distributor.NextDouble() * 1337);
                    returnValue[i] =
                        (returnValue[i].Item1 + Math.Sin(randomAngles[i]) * circleRadius, returnValue[i].Item2 +
                                                                                          Math.Cos(randomAngles[i]) *
                                                                                          circleRadius);
                }
            });
            returnValue.AddRange(GenerateRandomArray(sizeOfCluster, circleRadius, false));
            return returnValue;
        }

        private static List<(double, double)> GenerateRandomArray(int size, double circleRadius, bool isX1 = true)
        {
            var rnd = new Random(1337);
            var returnValue = new List<(double, double)>();
            for (var i = 0; i < size; ++i)
                returnValue.Add(isX1
                    ? (rnd.NextDouble() * circleRadius / 5.0, rnd.NextDouble() * circleRadius / 5.0)
                    : (
                    (rnd.NextDouble() - 0.5) * circleRadius * 2.0 / 20,
                    (rnd.NextDouble() - 0.5) * circleRadius * 2.0
                    ));
            return returnValue;
        }
    }
}