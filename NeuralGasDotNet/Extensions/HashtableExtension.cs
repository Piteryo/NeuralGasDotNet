using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralGasDotNet.Extensions
{
    public static class HashtableExtension
    {
        public static T Get<T>(this Hashtable table, object key)
        {
            return (T)table[key];
        }
    }
}
