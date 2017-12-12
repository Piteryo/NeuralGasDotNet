using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Services;
using NeuralGasDotNet.Services.NeuralGas;
using NeuralGasDotNet.Services.NeuralGas.DataGeneration;
using Prism.Mvvm;

namespace NeuralGasDotNet.Models
{
    class NeuralGasModel : BindableBase
    {
        public List<(double, double)> X;
        public List<(double, double)> W;
        public List<(int, int)> C;
        private readonly IGrowingNeuralGasService _growingNeuralGasService;


        public NeuralGasModel(IGrowingNeuralGasService growingNeuralGasService)
        {
            _growingNeuralGasService = growingNeuralGasService;

        }

        public void Init()
        {
            X = new List<(double, double)>();
            //using (StreamReader sr = new StreamReader("test.txt"))
            //{
            //    String line;
            //    // Read the stream to a string, and write the string to the console.
            //    while ((line = sr.ReadLine()) != null)
            //    {
            //        string[] tokens = line.Split(' ');
            //        X.Add((Double.Parse(tokens[0], CultureInfo.InvariantCulture), Double.Parse(tokens[1], CultureInfo.InvariantCulture)));
            //    }

            //}
            _growingNeuralGasService.Init(new List<(double, double)>
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
                forceDying: false);
            var epochStride = 20;
            _growingNeuralGasService.Fit(X, epochStride);
            W = _growingNeuralGasService.GetWeights();
            C = _growingNeuralGasService.GetConnectionsIdxPairs();
        }
        public List<(double, double)> GenerateData(DataGenerators dataGenerator)
        {
            switch (dataGenerator)
            {
                case DataGenerators.CircleWithLine:
                    if (X != null)
                        X = DataGenerator.GenerateLineInsideCircle(150);
                    break;
            }
            return X;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
