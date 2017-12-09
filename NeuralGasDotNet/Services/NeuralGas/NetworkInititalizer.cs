using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralGasDotNet.Services.NeuralGas.DataGeneration;

namespace NeuralGasDotNet.Services.NeuralGas
{
    public class NetworkInititalizer
    {
        public List<(double, double)> X = DataGenerator.GenerateLineInsideCircle(150);
        public List<(double, double)> W;
        public List<List<double>> C;


        public NetworkInititalizer()
        {
            X = new List<(double, double)>();
            using (StreamReader sr = new StreamReader("test.txt"))
            {
                String line;
                // Read the stream to a string, and write the string to the console.
                while ((line = sr.ReadLine()) != null)
                {
                    string[] tokens = line.Split(' ');
                    X.Add(( Double.Parse(tokens[0], CultureInfo.InvariantCulture), Double.Parse(tokens[1], CultureInfo.InvariantCulture)));
                }

            }
            var gng = new GrowingNeuralGas(new List<(double, double)>
                {
                    (0.0, 0.0),
                    (1.0, 1.0),
                }, winnerLearningRate: 0.2,
                neighboursLearningRate: 0.01,
                learningRateDecay: 1.0,
                edgeMaxAge: 5,  
                populateIterationsDivisor: 100,
                maxNeurons: 50,
                insertionErrorDecay: 0.8,
                iterationErrorDecay: 0.99,
                forceDying: false
            );
            var epochStride = 20;
            gng.Fit(X, epochStride);
            W = gng.GetWeights();
            var C = gng.GetConnectionsIdxPairs();
        }
    }
}