using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Troschuetz.Random;

namespace NeuralGasDotNet.Services.NeuralGas.DataGeneration
{
    public static class DataGenerator
    {
        public static List<List<double>> GenerateLineInsideCircle(int sizeOfCluster = 100, double circleRadius = 1.0)
        {
            List<List<double>> x1 = GenerateRandomArray(sizeOfCluster, circleRadius);
            var randomAngles = new List<double>();
            var distributor = new ContinuousUniformDistribution();
            for (var i = 0; i < sizeOfCluster; ++i)
            {
                randomAngles.Add(distributor.NextDouble()*1337);
                x1[i][0] += Math.Sin(randomAngles[i]) * circleRadius;
                x1[i][1] += Math.Cos(randomAngles[i]) * circleRadius;
            }
            var x2 = GenerateRandomArray(sizeOfCluster, circleRadius, false);
            x1.AddRange(x2);
            return x1;
        }

        private static List<List<double>> GenerateRandomArray(int size, double circleRadius, bool isX1 = true)
        {
            var rnd = new Random(1337);
            var returnValue = new List<List<double>>();
            for (var i = 0; i < size; ++i)
            {
                returnValue.Add(isX1
                    ? new List<double> {rnd.NextDouble() * circleRadius / 5.0, rnd.NextDouble() * circleRadius / 5.0}
                    : new List<double>
                    {
                        (rnd.NextDouble() - 0.5) * circleRadius * 2.0 / 20,
                        (rnd.NextDouble() - 0.5) * circleRadius * 2.0
                    });
            }
            return returnValue;
        }
    }
}
