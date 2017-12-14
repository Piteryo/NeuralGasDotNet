using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Defaults;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Models;
using NeuralGasDotNet.Services;
using NeuralGasDotNet.Services.NeuralGas;
using Prism.Commands;
using Prism.Mvvm;
using Unity.Attributes;
// ReSharper disable ExplicitCallerInfoArgument

namespace NeuralGasDotNet.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        private readonly NeuralGasModel _neuralGasModel;
        private readonly IGrowingNeuralGasService _neuralGasService;
        public MainViewModel(IGrowingNeuralGasService neuralGasService)
        {
            _neuralGasService = neuralGasService;
            _neuralGasModel = new NeuralGasModel(_neuralGasService);
            _neuralGasModel.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            ShowDataCommand = new DelegateCommand<DataGenerators?>(generatorType => _neuralGasModel.GenerateData(generatorType));
            StartTrainingCommand = new DelegateCommand(() => _neuralGasModel.Init());
        }

        public DataGenerators CurrentEffectStyle { get; set; }
        public DelegateCommand<DataGenerators?> ShowDataCommand { get; }

        public DelegateCommand StartTrainingCommand { get; }
        //public ChartValues<ObservablePoint> InputDataChartValues => _neuralGasModel.InputDataChartValues;
        public SeriesCollection SeriesChartsCollection => _neuralGasModel.SeriesChartsCollection;

        public string NeuralNetworkLog => _neuralGasModel.NeuralNetworkLog;
    }
}
