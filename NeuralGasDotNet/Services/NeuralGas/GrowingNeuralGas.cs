using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra;
using NeuralGasDotNet.Extensions;

namespace NeuralGasDotNet.Services.NeuralGas
{
    public enum HistoryLevel
    {
        None = 0,
        Epochs = 1,
        Steps = 2
    }

    internal class GrowingNeuralGas
    {
        private readonly Dictionary<HashSet<int>, int> _ageOfConnections;
        private readonly List<Hashtable> _history;
        private readonly Hashtable _neurons;
        private readonly double _tolerance = 0.00001;

        public GrowingNeuralGas(
            ICollection<List<double>> weights,
            Hashtable kwargs,
            double winnerLearningRate,
            double neighboursLearningRate,
            double learningRateDecay = 1.0,
            double edgeMaxAge = 100,
            double populateIterationsDivisor = 25,
            int maxNeurons = 10,
            double insertionErrorDecay = 0.8F,
            double iterationErrorDecay = 0.99,
            HistoryLevel withHistory = HistoryLevel.None,
            bool forceDying = false
        )
        {
            // Not really necessary, but for the sake of simplicity (no need to generate initial connections)
            // let's assume there are only 2 neurons
            if (weights.Count != 2)
                throw new CustomAttributeFormatException("GNG should initially have 2 neurons");
            NeuronIdx = -1;
            var neurons = new List<(int, List<double>, int)>();
            foreach (var w in weights)
            {
                NeuronIdx += 1;
                neurons.Add((NeuronIdx, w, 0));
            }
            _neurons = new Hashtable
            {
                {"id", new List<int>()},
                {"value", new List<List<double>>()},
                {"error", new List<double>()}
            };
            foreach (var neuron in neurons)
            {
                //string buffer = neuron.Item1 + Char.MinValue;
                ((List<int>) _neurons["id"]).Add(neuron.Item1); //////
                ((List<List<double>>) _neurons["value"]).Add(neuron.Item2);
                ((List<double>) _neurons["error"]).Add(neuron.Item3);
            }

            _ageOfConnections = new Dictionary<HashSet<int>, int>(HashSet<int>.CreateSetComparer())
            {
                {new HashSet<int> {0, 1}, 0}
            };

            WinnerLearningRate = winnerLearningRate;
            NeighboursLearningRate = neighboursLearningRate;
            LearningRateDecay = learningRateDecay;
            EdgeMaxAge = edgeMaxAge;
            PopulateIterationsDivisor = populateIterationsDivisor;
            MaxNeurons = maxNeurons;
            InsertionErrorDecay = insertionErrorDecay;
            IterationErrorDecay = iterationErrorDecay;
            WithHistory = withHistory;
            TotalEpoch = 0;
            TotalSampled = 0;
            Order = new List<int>();
            IterationNum = 0;
            _history = new List<Hashtable>();
            ForceDying = forceDying;
        }

        public int NeuronIdx { get; set; }

        public bool ForceDying { get; set; }

        public int IterationNum { get; set; }

        public List<int> Order { get; set; }

        public int TotalSampled { get; set; }

        public int TotalEpoch { get; set; }

        public HistoryLevel WithHistory { get; set; }

        public double IterationErrorDecay { get; set; }

        public double InsertionErrorDecay { get; set; }

        public int MaxNeurons { get; set; }

        public double PopulateIterationsDivisor { get; set; }

        public double EdgeMaxAge { get; set; }

        public double LearningRateDecay { get; set; }

        public double NeighboursLearningRate { get; set; }

        public double WinnerLearningRate { get; set; }

        private List<int> GetNeighboursIdxsOfNeuron(int neuronIdx)
        {
            var neuronId = _neurons.Get<List<int>>("id")[neuronIdx];
            var neighboursIds = new HashSet<int>();
            foreach (var keyset in _ageOfConnections.Keys)
                if (keyset.Contains(neuronId))
                {
                    var keysetUpd = new HashSet<int>();
                    foreach (var key in keyset)
                        keysetUpd.Add(key);
                    neighboursIds.UnionWith(keysetUpd);
                    // for the sake of algorithm functionality node is NOT a neighbour to itself
                }
            neighboursIds.Remove(neuronId);
            return neighboursIds.Select(id => _neurons.Get<List<int>>("id").IndexOf(id)).ToList();
        }

        public List<List<int>> get_connections_idx_pairs()
        {
            var idxPairs = new List<List<int>>();
            foreach (var keyset in _ageOfConnections.Keys)
            {
                var pair = new List<int>();
                foreach (var x in keyset)
                    pair.Add(_neurons.Get<List<int>>("id").IndexOf(x));
                idxPairs.Add(pair);
            }
            return idxPairs;
        }

        public void _pre_fit_one(Hashtable kwargs)
        {
            IterationNum += 1;
            if (WithHistory == HistoryLevel.Steps)
                _history.Add(new Hashtable
                {
                    {'W', _neurons.Get<List<List<double>>>("value")},
                    {'x', kwargs["x"]}
                });
        }

        public (int, int) _update_weights_and_connections(List<double> x)
        {
            var m = Matrix<double>.Build;
            var rawVectorDifference = new double[_neurons.Get<List<List<double>>>("value").Count, 2];

            for (var i = 0; i < _neurons.Get<List<List<double>>>("value").Count; ++i)
            for (var j = 0; j < _neurons.Get<List<List<double>>>("value")[i].Count; ++j)
                rawVectorDifference[i, j] = _neurons.Get<List<List<double>>>("value")[i][j] - x[j];
            var matrix = m.DenseOfArray(rawVectorDifference);


            var distancesFromX = new List<double>
            {
                matrix.SubMatrix(0, 1, 0, 2).FrobeniusNorm(),
                matrix.SubMatrix(1, 1, 0, 2).FrobeniusNorm()
            };

            var sorted = distancesFromX
                .Select((val, i) => new KeyValuePair<double, int>(val, i))
                .OrderBy(val => val.Key)
                .ToList();

            var indexes = sorted.Select(val => val.Value).ToList();

            var closestNeuronIdx = indexes[0];
            var secondClosestNeuronIdx = indexes[1];
            ((List<double>) _neurons["error"])[closestNeuronIdx] += distancesFromX[closestNeuronIdx];
            var vectorDifferenceClosestNeuron = rawVectorDifference.GetRow(closestNeuronIdx);
            for (var i = 0; i < vectorDifferenceClosestNeuron.Length; ++i)
                vectorDifferenceClosestNeuron[i] *= WinnerLearningRate;
            for (var i = 0; i < ((List<List<double>>) _neurons["value"])[closestNeuronIdx].Count; ++i)
                ((List<List<double>>) _neurons["value"])[closestNeuronIdx][i] -= vectorDifferenceClosestNeuron[i];

            ;

            var idxs = GetNeighboursIdxsOfNeuron(closestNeuronIdx);
            foreach (var idx in idxs)
            {
                var vectorDifferenceNeuron = rawVectorDifference.GetRow(idx);
                for (var i = 0; i < vectorDifferenceNeuron.Length; ++i)
                    vectorDifferenceNeuron[i] *= NeighboursLearningRate;

                for (var i = 0; i < ((List<List<double>>) _neurons["value"])[closestNeuronIdx].Count; ++i)
                    ((List<List<double>>) _neurons["value"])[idx][i] -= vectorDifferenceNeuron[i];
            }
            var allNeuronIds = _neurons.Get<List<int>>("id");
            var closestNeuronName = allNeuronIds[closestNeuronIdx];
            var secondClosestNeuronName = allNeuronIds[secondClosestNeuronIdx];
            _ageOfConnections[new HashSet<int> {closestNeuronName, secondClosestNeuronName}] =
                0; /////////!!!!!!!!!!!!!!!///
            return (closestNeuronIdx, secondClosestNeuronIdx);
        }

        public HashSet<int> _remove_invalid_neurons(int sourceNeuronIdx)
        {
            var allNeuronIds = _neurons.Get<List<int>>("id");
            var sourceNeuronId = allNeuronIds[sourceNeuronIdx];
            var validNeurons = new HashSet<int>();


            foreach (var keyset in _ageOfConnections.Keys)
                if (ForceDying || keyset.Contains(sourceNeuronId))
                {
                    _ageOfConnections[keyset] += 1;
                    if (_ageOfConnections[keyset] >= EdgeMaxAge)
                        _ageOfConnections.Remove(keyset);
                    else
                        validNeurons.UnionWith(keyset);
                }
                else
                {
                    validNeurons.UnionWith(keyset);
                }
            if (validNeurons.Count != allNeuronIds.Count)
            {
                var invalidNeuronsIds = new HashSet<int>(allNeuronIds);
                invalidNeuronsIds.ExceptWith(validNeurons);

                var invalidNeuronsIdxs = invalidNeuronsIds.Select(id => allNeuronIds.IndexOf(id));
                var validIndexes = new List<int>();
                for (var i = 0; i < allNeuronIds.Count; ++i)
                    if (!invalidNeuronsIdxs.Contains(i))
                        validIndexes.Add(i);
                for (var i = 0; i < _neurons.Get<List<int>>("id").Count; ++i)
                    if (!validIndexes.Contains(_neurons.Get<List<int>>("id")[i]))
                    {
                        ((List<int>) _neurons["id"]).RemoveAt(i);
                        ((List<List<double>>) _neurons["value"]).RemoveAt(i);
                        ((List<double>) _neurons["error"]).RemoveAt(i);
                    }
                return invalidNeuronsIds;
            }
            return new HashSet<int>();
        }

        public bool _should_insert_neuron_predicate()
        {
            return _neurons.Get<List<int>>("id").Count < MaxNeurons &&
                   Math.Abs(IterationNum % PopulateIterationsDivisor) > _tolerance;
        }

        public void _insert_neuron()
        {
            ++NeuronIdx;
            var largestErrorNeuronIdx = !_neurons.Get<List<double>>("error").Any()
                ? -1
                : _neurons.Get<List<double>>("error")
                    .Select((value, index) => new {Value = value, Index = index})
                    .Aggregate((a, b) => a.Value > b.Value ? a : b)
                    .Index;
            var idxs = GetNeighboursIdxsOfNeuron(largestErrorNeuronIdx);
            var maxError = 0.0;
            var largestErrorNeighbourNeuronIdx = 0;
            foreach (var idx in idxs)
                if (_neurons.Get<List<double>>("id")[idx] > maxError)
                {
                    largestErrorNeighbourNeuronIdx = idx;
                    maxError = _neurons.Get<List<double>>("id")[idx];
                }
            var newNeuronWeights = new List<double>();
            for (var i = 0; i < _neurons.Get<List<List<double>>>("value")[largestErrorNeighbourNeuronIdx].Count; ++i)
                newNeuronWeights.Add(
                    (_neurons.Get<List<List<double>>>("value")[largestErrorNeighbourNeuronIdx][i] + 2 *
                     _neurons.Get<List<List<double>>>("value")[largestErrorNeuronIdx][i]) / 3);
            var newId = NeuronIdx;
            ((List<double>) _neurons["error"])[largestErrorNeuronIdx] *= InsertionErrorDecay;
            ((List<double>) _neurons["error"])[largestErrorNeighbourNeuronIdx] *= InsertionErrorDecay;

            //New neuron insert
            ((List<int>) _neurons["id"]).Add(newId);
            ((List<List<double>>) _neurons["values"]).Add(newNeuronWeights);
            ((List<double>) _neurons["error"]).Add(
                _neurons.Get<List<double>>("error")[largestErrorNeighbourNeuronIdx] +
                _neurons.Get<List<double>>("error")[largestErrorNeuronIdx] / 2.0);
            //

            _ageOfConnections[new HashSet<int> {_neurons.Get<List<int>>("id")[largestErrorNeuronIdx], newId}] = 0;
            _ageOfConnections[
                new HashSet<int> {_neurons.Get<List<int>>("id")[largestErrorNeighbourNeuronIdx], newId}] = 0;
            var removedArc = new HashSet<int> {largestErrorNeuronIdx, largestErrorNeighbourNeuronIdx};
            if (_ageOfConnections.Keys.Contains(removedArc))
                _ageOfConnections.Remove(removedArc);
        }

        private void post_fit_one() // TODO: google for better codestyle
        {
            for (var i = 0; i < _neurons.Get<List<double>>("error").Count; ++i)
                ((List<double>) _neurons["error"])[i] *= IterationErrorDecay;
        }

        private void _post_epoch(int epoch)
        {
            WinnerLearningRate *= LearningRateDecay;
            NeighboursLearningRate *= LearningRateDecay;
            if (WithHistory == HistoryLevel.Epochs)
                _history.Add(new Hashtable
                {
                    {'W', _neurons.Get<List<List<double>>>("value")},
                    {"connections", get_connections_idx_pairs()}
                });
            Debug.WriteLine($"Neural gas: Ending epoch {epoch + 1} ({TotalEpoch} total)");
        }

        private void pre_epoch(int epoch)
        {
            ++TotalEpoch;
            Debug.WriteLine($"Neural gas: Beginning epoch {epoch + 1} ({TotalEpoch} total)");
            Order.Shuffle();
        }

        public void fit_one(List<double> x)
        {
            _pre_fit_one(new Hashtable {{"x", x}});
            var tup1 = _update_weights_and_connections(x);
            var closestNeuronIdx = tup1.Item1;
            var secondClosestNeuronIdx = tup1.Item2;
            _remove_invalid_neurons(closestNeuronIdx);
            if (_should_insert_neuron_predicate())
                _insert_neuron();
            post_fit_one();
        }

        public void Fit(List<List<double>> x, int numberOfEpochs)
        {
            Order = Enumerable.Range(0, x.Count).ToList();
            foreach (var epoch in Enumerable.Range(0, numberOfEpochs))
            {
                pre_epoch(epoch);
                foreach (var i in Order)
                    fit_one(x[i]);
                _post_epoch(epoch);
            }
        }


        public void fit_p(Func<int, int, List<List<double>>> fn, int timesToSample = 1500,
            int numberOfEpochsPerBatch = 1, int sizeOfBatch = 8)
        {
            foreach (var t in Enumerable.Range(0, timesToSample))
            {
                TotalSampled += 1;
                Debug.WriteLine($"Sampling {t + 1} ({TotalSampled})");
                var x = fn(t, sizeOfBatch);
                Fit(x, numberOfEpochsPerBatch);
                if (WithHistory == HistoryLevel.Epochs)
                    _history[_history.Count - 1]["x"] = x;
            }
        }

        public List<List<double>> GetWeights()
        {
            return _neurons.Get<List<List<double>>>("value");
        }

        public List<List<List<double>>> GetWeightsHistory()
        {
            return _history.Select(hashtable => (List<List<double>>) hashtable["W"]).ToList();
        }

        public List<List<List<int>>> GetConnectionsHistory()
        {
            return _history.Select(hashtable => (List<List<int>>) hashtable["connections"]).ToList();
        }

        public List<List<List<int>>> GetSamplesHistory()
        {
            if (_history.Count > 0 && _history[0].Contains("x"))
                return _history.Select(hashtable => (List<List<int>>) hashtable["x"]).ToList();
            return new List<List<List<int>>>();
        }
    }
}