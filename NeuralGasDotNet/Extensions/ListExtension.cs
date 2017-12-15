using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LiveCharts;
using LiveCharts.Defaults;

namespace NeuralGasDotNet.Extensions
{
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random _local;

        public static Random ThisThreadsRandom => _local ?? (_local =
                                                      new Random(unchecked(Environment.TickCount * 31 +
                                                                           Thread.CurrentThread.ManagedThreadId)));
    }

    internal static class ListExtension
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static ChartValues<ObservablePoint> ToChartValues(this List<(double, double)> weights)
        {
            return new ChartValues<ObservablePoint>(weights.Select(w => new ObservablePoint(w.Item1, w.Item2)));
        }
    }
}