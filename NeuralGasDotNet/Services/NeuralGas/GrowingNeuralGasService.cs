using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using NeuralGasDotNet.Extensions;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Services.NeuralGas.Models;

namespace NeuralGasDotNet.Services.NeuralGas
{
    public enum HistoryLevel
    {
        None = 0,
        Epochs = 1,
        Steps = 2
    }

    internal class GrowingNeuralGasService : IGrowingNeuralGasService
    {
        private Dictionary<(int, int), int> _ageOfConnections;
        private readonly List<Neuron> _neurons = new List<Neuron>();
        private readonly double _tolerance = 0.00001;

        public void Init(ICollection<(double, double)> weights,
            double winnerLearningRate,
            double neighboursLearningRate,
            double learningRateDecay = 1.0,
            double edgeMaxAge = 100,
            double populateIterationsDivisor = 25,
            int maxNeurons = 10,
            double insertionErrorDecay = 0.8F,
            double iterationErrorDecay = 0.99,
            bool forceDying = false)
        {
            // Not really necessary, but for the sake of simplicity (no need to generate initial connections)
            // let's assume there are only 2 neurons
            if (weights.Count != 2)
                throw new CustomAttributeFormatException("GNG should initially have 2 neurons");
            NeuronIdx = 0;
            _neurons.AddRange((from weight in weights
                select new Neuron
                {
                    Id = NeuronIdx++,
                    Value = weight,
                    Error = 0
                }).ToList());

            _ageOfConnections = new Dictionary<(int, int), int>
            {
                {(0, 1), 0}
            };

            WinnerLearningRate = winnerLearningRate;
            NeighboursLearningRate = neighboursLearningRate;
            LearningRateDecay = learningRateDecay;
            EdgeMaxAge = edgeMaxAge;
            PopulateIterationsDivisor = populateIterationsDivisor;
            MaxNeurons = maxNeurons;
            InsertionErrorDecay = insertionErrorDecay;
            IterationErrorDecay = iterationErrorDecay;
            TotalEpoch = 0;
            Order = new List<int>();
            IterationNum = 0;
            ForceDying = forceDying;
        }

        private int NeuronIdx { get; set; }

        private bool ForceDying { get; set; }

        private int IterationNum { get; set; }

        private List<int> Order { get; set; }

        private int TotalEpoch { get; set; }

        private double IterationErrorDecay { get; set; }

        private double InsertionErrorDecay { get; set; }

        private int MaxNeurons { get; set; }

        private double PopulateIterationsDivisor { get; set; }

        private double EdgeMaxAge { get; set; }

        private double LearningRateDecay { get; set; }

        private double NeighboursLearningRate { get; set; }

        private double WinnerLearningRate { get; set; }

        private IEnumerable<int> GetNeighboursIdxsOfNeuron(int neuronIdx)
        {
            var neuronId = _neurons[neuronIdx].Id;
            var neighboursIds = new HashSet<int>();
            foreach (var keyset in _ageOfConnections.Keys)
                if (keyset.Contains(neuronId))
                    neighboursIds.UnionWith(new[] {keyset.Item1, keyset.Item2});
            // for the sake of algorithm functionality node is NOT a neighbour to itself
            neighboursIds.Remove(neuronId);
            // ReSharper disable once PossibleNullReferenceException
            return neighboursIds.Select(id => _neurons.FindIndex(neuron => neuron.Id == id)).ToList();
        }


        public List<(int, int)> GetConnectionsIdxPairs()
        {
            return _ageOfConnections.Keys
                .Select(keyset =>
                    (_neurons.FindIndex(neuron => neuron.Id == keyset.Item1),
                    _neurons.FindIndex(neuron => neuron.Id == keyset.Item2)))
                .ToList();
        }

        private (int, int) UpdateWeightsAndConnections((double, double) x)
        {
            var m = Matrix<double>.Build;
            var rawVectorDifference = new double[_neurons.Count, 2];
            for (var i = 0; i < rawVectorDifference.GetLength(0); ++i)
            {
                rawVectorDifference[i, 0] = _neurons[i].Value.Item1 - x.Item1;
                rawVectorDifference[i, 1] = _neurons[i].Value.Item2 - x.Item2;
            }
            var matrix = m.DenseOfArray(rawVectorDifference);

            var distancesFromX = Enumerable.Range(0, rawVectorDifference.GetLength(0))
                .Select(idx => matrix.SubMatrix(idx, 1, 0, 2).FrobeniusNorm()).ToList();
            var indexes = distancesFromX
                .Select((val, i) => new KeyValuePair<double, int>(val, i))
                .OrderBy(val => val.Key)
                .Select(val => val.Value).ToList();

            var closestNeuronIdx = indexes[0];
            var secondClosestNeuronIdx = indexes[1];
            _neurons[closestNeuronIdx].Error += distancesFromX[closestNeuronIdx];
            var vectorDifferenceClosestNeuron = rawVectorDifference.GetRow(closestNeuronIdx);
            vectorDifferenceClosestNeuron = (from difference in vectorDifferenceClosestNeuron
                select difference * WinnerLearningRate).ToArray();

            _neurons[closestNeuronIdx].Value = _neurons[closestNeuronIdx].Value
                .TupleSubtraction((vectorDifferenceClosestNeuron[0], vectorDifferenceClosestNeuron[1]));

            var idxs = GetNeighboursIdxsOfNeuron(closestNeuronIdx);

            foreach (var idx in idxs)
            {
                var vectorDifferenceNeuron = (from difference in rawVectorDifference.GetRow(idx)
                    select difference * NeighboursLearningRate).ToArray();
                _neurons[idx].Value = _neurons[idx].Value
                    .TupleSubtraction((vectorDifferenceNeuron[0], vectorDifferenceNeuron[1]));
            }

            var closestNeuronName = _neurons[closestNeuronIdx].Id;
            var secondClosestNeuronName = _neurons[secondClosestNeuronIdx].Id;
            _ageOfConnections[(closestNeuronName, secondClosestNeuronName)] =
                0;
            return (closestNeuronIdx, secondClosestNeuronIdx);
        }

        private void RemoveInvalidNeurons(int sourceNeuronIdx)
        {
            var sourceNeuronId = _neurons[sourceNeuronIdx].Id;
            var validNeurons = new HashSet<int>();

            foreach (var keyset in _ageOfConnections.Keys.ToList())
                if (ForceDying || keyset.Contains(sourceNeuronId))
                {
                    _ageOfConnections[keyset] += 1;
                    if (_ageOfConnections[keyset] >= EdgeMaxAge)
                        _ageOfConnections.Remove(keyset);
                    else
                        validNeurons.UnionWith(new[] {keyset.Item1, keyset.Item2});
                }
                else
                {
                    validNeurons.UnionWith(new[] {keyset.Item1, keyset.Item2});
                }
            if (validNeurons.Count != _neurons.Count)
            {
                var invalidNeuronsIds = new HashSet<int>(_neurons.Select(neuron => neuron.Id).Except(validNeurons));
                var invalidNeuronsIdxs = invalidNeuronsIds.Select(id => _neurons.FindIndex(neuron => neuron.Id == id))
                    .ToList();
                var validIndexes = Enumerable.Range(0, _neurons.Count).Where(idx => !invalidNeuronsIdxs.Contains(idx))
                    .ToList();
                _neurons.RemoveAll(neuron => !validIndexes.Contains(_neurons.FindIndex(n => n == neuron)));
            }
        }

        private bool ShouldInsertNeuronPredicate()
        {
            return _neurons.Count < MaxNeurons && Math.Abs(IterationNum % PopulateIterationsDivisor) < _tolerance;
        }

        private void InsertNeuron()
        {
            ++NeuronIdx;
            var largestErrorNeuronIdx = !_neurons.Any()
                ? -1
                : _neurons
                    .Select((neuron, index) => new {Value = neuron, Index = index})
                    .Aggregate((a, b) => a.Value.Error > b.Value.Error ? a : b)
                    .Index;
            var idxs = GetNeighboursIdxsOfNeuron(largestErrorNeuronIdx);
            var maxError = 0.0;
            var largestErrorNeighbourNeuronIdx = 0;
            foreach (var idx in idxs)
                if (_neurons[idx].Error > maxError)
                {
                    largestErrorNeighbourNeuronIdx = idx;
                    maxError = _neurons[idx].Error;
                }
            var newNeuronWeights = ((_neurons[largestErrorNeighbourNeuronIdx].Value.Item1 +
                                     2 * _neurons[largestErrorNeuronIdx].Value.Item1) / 3,
            (_neurons[largestErrorNeighbourNeuronIdx].Value.Item2 +
             2 * _neurons[largestErrorNeuronIdx].Value.Item2) / 3);

            var newId = NeuronIdx;
            _neurons[largestErrorNeuronIdx].Error *= InsertionErrorDecay;
            _neurons[largestErrorNeighbourNeuronIdx].Error *= InsertionErrorDecay;

            //New neuron insert
            _neurons.Add(new Neuron
            {
                Id = newId,
                Value = newNeuronWeights,
                Error = (_neurons[largestErrorNeighbourNeuronIdx].Error + _neurons[largestErrorNeuronIdx].Error) / 2.0
            });

            _ageOfConnections[(_neurons[largestErrorNeuronIdx].Id, newId)] = 0;
            _ageOfConnections[
                (_neurons[largestErrorNeighbourNeuronIdx].Id, newId)] = 0;
            var removedArc = (largestErrorNeuronIdx, largestErrorNeighbourNeuronIdx);
            if (_ageOfConnections.Keys.Contains(removedArc))
                _ageOfConnections.Remove(removedArc);
        }

        private void PostFitOne()
        {
            _neurons.ForEach(neuron => neuron.Error *= IterationErrorDecay);
        }

        private void PostEpoch(int epoch)
        {
            WinnerLearningRate *= LearningRateDecay;
            NeighboursLearningRate *= LearningRateDecay;
            Debug.WriteLine($"Neural gas: Ending epoch {epoch + 1} ({TotalEpoch} total)");
        }

        private void PreEpoch(int epoch)
        {
            ++TotalEpoch;
            Debug.WriteLine($"Neural gas: Beginning epoch {epoch + 1} ({TotalEpoch} total)");
            Order.Shuffle();
        }

        private void FitOne((double, double) x)
        {
            ++IterationNum;
            var tup1 = UpdateWeightsAndConnections(x);
            var closestNeuronIdx = tup1.Item1;
            RemoveInvalidNeurons(closestNeuronIdx);
            if (ShouldInsertNeuronPredicate())
                InsertNeuron();
            PostFitOne();
        }

        public void Fit(List<(double, double)> x, int numberOfEpochs)
        {
            Order = Enumerable.Range(0, x.Count).ToList();
            for (var epoch = 0; epoch < numberOfEpochs; ++epoch)
            {
                PreEpoch(epoch);
                foreach (var i in Order)
                    FitOne(x[i]);
                PostEpoch(epoch);
            }
        }


        public List<(double, double)> GetWeights()
        {
            return _neurons.Select(neuron => neuron.Value).ToList();
        }
    }
}