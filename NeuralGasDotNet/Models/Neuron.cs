namespace NeuralGasDotNet.Models
{
    internal class Neuron
    {
        public int Id { get; set; }
        public (double, double) Value { get; set; }
        public double Error { get; set; }
    }
}
