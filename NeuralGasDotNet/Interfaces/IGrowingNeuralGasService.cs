using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NeuralGasDotNet.Interfaces
{
    internal interface IGrowingNeuralGasService : INotifyPropertyChanged
    {
        string NeuralNetworkLog { get; set; }

        void Init(ICollection<(double, double)> weights,
            double winnerLearningRate,
            double neighboursLearningRate,
            double learningRateDecay = 1.0,
            double edgeMaxAge = 100,
            double populateIterationsDivisor = 25,
            int maxNeurons = 10,
            double insertionErrorDecay = 0.8F,
            double iterationErrorDecay = 0.99,
            bool forceDying = false);

        List<(int, int)> GetConnectionsIdxPairs();
        Task Fit(List<(double, double)> x, int numberOfEpochs);
        List<(double, double)> GetWeights();
    }
}