using System.Collections.Generic;

namespace NeuralGasDotNet.Services.NeuralGas.Models
{
    internal class History
    {
        public List<(double, double)> Weights { get; set; }
    }
}
