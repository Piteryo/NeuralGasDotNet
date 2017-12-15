using System.Collections;

namespace NeuralGasDotNet.Extensions
{
    public static class HashtableExtension
    {
        public static T Get<T>(this Hashtable table, object key)
        {
            return (T) table[key];
        }
    }
}