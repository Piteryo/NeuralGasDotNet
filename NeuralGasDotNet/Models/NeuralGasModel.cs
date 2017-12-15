using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using NeuralGasDotNet.Extensions;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Services;
using NeuralGasDotNet.Services.NeuralGas.DataGeneration;
using Prism.Mvvm;
// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable ExplicitCallerInfoArgument

namespace NeuralGasDotNet.Models
{
    /// <summary>
    /// This class calls NeuralNetworkService to train network and helps to generate data
    /// </summary>
    internal class NeuralGasModel : BindableBase
    {
        private readonly IGrowingNeuralGasService _growingNeuralGasService;
        private GeneratorTypes? _currentGeneratorType;
        private SeriesCollection _seriesChartsCollection;
        public List<(int, int)> C;
        public List<(double, double)> W;
        public List<(double, double)> X;

        public NeuralGasModel(IGrowingNeuralGasService growingNeuralGasService)
        {
            _growingNeuralGasService = growingNeuralGasService;
            _growingNeuralGasService.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
        }

        public string NeuralNetworkLog => _growingNeuralGasService.NeuralNetworkLog;


        public SeriesCollection SeriesChartsCollection
        {
            get => _seriesChartsCollection;
            set
            {
                _seriesChartsCollection = value;
                RaisePropertyChanged(nameof(SeriesChartsCollection));
            }
        }


        public ChartValues<ObservablePoint> InputDataChartValues { get; set; }

        public async Task Init(int numberOfEpochs, double learningRateDecay, int edgeMaxAge, int maxNumberOfNeurons,
            bool isForceDying, GeneratorTypes generatorType)
        {
            if (X == null || !X.Any() || generatorType != _currentGeneratorType)
                await GenerateData(generatorType);

            _growingNeuralGasService.Init(new List<(double, double)>
                {
                    (0.0, 0.0),
                    (1.0, 1.0)
                }, 0.2,
                0.01,
                learningRateDecay,
                edgeMaxAge,
                100,
                maxNumberOfNeurons,
                0.8,
                0.99,
                isForceDying);

            await _growingNeuralGasService.Fit(X, numberOfEpochs);
            W = _growingNeuralGasService.GetWeights();
            C = _growingNeuralGasService.GetConnectionsIdxPairs();
            ShowWeightsAndConnections();
        }

        private void ShowWeightsAndConnections()
        {
            if (SeriesChartsCollection.Count > 1)
            {
                var i = SeriesChartsCollection.Count - 1;
                while (SeriesChartsCollection.Count != 1)
                {
                    if (i == 0)
                        i = SeriesChartsCollection.Count - 1;
                    SeriesChartsCollection.RemoveAt(i--);
                }

                RaisePropertyChanged(nameof(SeriesChartsCollection));
            }
            var weightsChartValues = W.ToChartValues();
            SeriesChartsCollection.Add(new ScatterSeries
            {
                Values = weightsChartValues,
                PointGeometry = DefaultGeometries.Diamond,
                Stroke = Brushes.Tomato
            });
            foreach (var pair in C)
                SeriesChartsCollection.Add(new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>
                    {
                        new ObservablePoint(W[pair.Item1].Item1, W[pair.Item1].Item2),
                        new ObservablePoint(W[pair.Item2].Item1, W[pair.Item2].Item2)
                    },
                    StrokeThickness = 4,
                    Stroke = Brushes.Bisque,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 0
                });
            RaisePropertyChanged(nameof(SeriesChartsCollection));
        }

        public async Task GenerateData(GeneratorTypes? generatorType)
        {
            switch (generatorType)
            {
                case GeneratorTypes.CircleWithLine:
                    if (X == null || generatorType != _currentGeneratorType)
                    {
                        _currentGeneratorType = generatorType;
                        X = await DataGenerator.GenerateLineInsideCircle(150);
                        InputDataChartValues = X.ToChartValues();
                        SeriesChartsCollection = new SeriesCollection
                        {
                            new ScatterSeries
                            {
                                Values = InputDataChartValues
                            }
                        };
                        RaisePropertyChanged(nameof(SeriesChartsCollection));
                    }
                    break;
                case GeneratorTypes.FiveHills:
                    if (X == null || generatorType != _currentGeneratorType)
                    {
                        _currentGeneratorType = generatorType;
                        X = new List<(double, double)>();
                        using (var sr = new StreamReader(Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("NeuralGasDotNet.Resources.five_hills.txt")))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                var tokens = line.Split(' ');
                                X.Add((double.Parse(tokens[0], CultureInfo.InvariantCulture), double.Parse(tokens[1],
                                    CultureInfo.InvariantCulture)));
                            }
                        }
                        InputDataChartValues = X.ToChartValues();
                        SeriesChartsCollection = new SeriesCollection
                        {
                            new ScatterSeries
                            {
                                Values = InputDataChartValues
                            }
                        };
                        RaisePropertyChanged(nameof(SeriesChartsCollection));
                    }
                    break;
                case GeneratorTypes.TwoBlobs:
                    if (X == null || generatorType != _currentGeneratorType)
                    {
                        _currentGeneratorType = generatorType;
                        X = new List<(double, double)>();
                        using (var sr = new StreamReader(Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("NeuralGasDotNet.Resources.two_blobs.txt")))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                var tokens = line.Split(' ');
                                X.Add((double.Parse(tokens[0], CultureInfo.InvariantCulture), double.Parse(tokens[1],
                                    CultureInfo.InvariantCulture)));
                            }
                        }
                        InputDataChartValues = X.ToChartValues();
                        SeriesChartsCollection = new SeriesCollection
                        {
                            new ScatterSeries
                            {
                                Values = InputDataChartValues
                            }
                        };
                        RaisePropertyChanged(nameof(SeriesChartsCollection));
                    }
                    break;
                case GeneratorTypes.BlobInsideBlob:
                    if (X == null || generatorType != _currentGeneratorType)
                    {
                        _currentGeneratorType = generatorType;
                        X = new List<(double, double)>();
                        using (var sr = new StreamReader(Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("NeuralGasDotNet.Resources.blob_inside_blob.txt")))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                var tokens = line.Split(' ');
                                X.Add((double.Parse(tokens[0], CultureInfo.InvariantCulture), double.Parse(tokens[1],
                                    CultureInfo.InvariantCulture)));
                            }
                        }
                        InputDataChartValues = X.ToChartValues();
                        SeriesChartsCollection = new SeriesCollection
                        {
                            new ScatterSeries
                            {
                                Values = InputDataChartValues
                            }
                        };
                        RaisePropertyChanged(nameof(SeriesChartsCollection));
                    }
                    break;
                case GeneratorTypes.Donut:
                    if (X == null || generatorType != _currentGeneratorType)
                    {
                        _currentGeneratorType = generatorType;
                        X = new List<(double, double)>();
                        using (var sr = new StreamReader(Assembly.GetExecutingAssembly()
                            .GetManifestResourceStream("NeuralGasDotNet.Resources.donut.txt")))
                        {
                            string line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                var tokens = line.Split(' ');
                                X.Add((double.Parse(tokens[0], CultureInfo.InvariantCulture), double.Parse(tokens[1],
                                    CultureInfo.InvariantCulture)));
                            }
                        }
                        InputDataChartValues = X.ToChartValues();
                        SeriesChartsCollection = new SeriesCollection
                        {
                            new ScatterSeries
                            {
                                Values = InputDataChartValues
                            }
                        };
                        RaisePropertyChanged(nameof(SeriesChartsCollection));
                    }
                    break;
            }
        }
    }
}