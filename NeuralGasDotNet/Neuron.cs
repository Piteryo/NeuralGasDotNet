using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralGasDotNet
{
    class Neuron
    {
        public int Id { get; set; }
        public (int, int) Value { get; set; }
        public double Error { get; set; }
    }
}
