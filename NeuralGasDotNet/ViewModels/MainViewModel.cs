using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Models;
using NeuralGasDotNet.Services;
using NeuralGasDotNet.Services.NeuralGas;
using Prism.Commands;
using Prism.Mvvm;
using Unity.Attributes;

namespace NeuralGasDotNet.ViewModels
{
    internal class MainViewModel
    {
        private NeuralGasModel _neuralGasModel;
        private readonly IGrowingNeuralGasService _neuralGasService;
        public MainViewModel(IGrowingNeuralGasService neuralGasService)
        {
            _neuralGasService = neuralGasService;
            _neuralGasModel = new NeuralGasModel(_neuralGasService);
            ShowDataCommand = new DelegateCommand<string>(generatorType =>
            {
                DataGeneratorType type;
                if (Enum.TryParse(generatorType, out type))
                    _neuralGasModel.GenerateData(type);
            });
        }

        public DelegateCommand<string> ShowDataCommand { get; }
    }
}
