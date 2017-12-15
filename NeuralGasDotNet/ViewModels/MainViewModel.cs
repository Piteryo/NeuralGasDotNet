using System;
using System.Collections.Generic;
using System.Windows;
using LiveCharts;
using NeuralGasDotNet.Helpers;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Models;
using NeuralGasDotNet.Services;
using Prism.Commands;
using Prism.Mvvm;

// ReSharper disable ExplicitCallerInfoArgument

namespace NeuralGasDotNet.ViewModels
{
    internal class MainViewModel : BindableBase
    {
        private readonly NeuralGasModel _neuralGasModel;
        private GeneratorTypes _currentEffectStyle;
        private double _learningRateDecay;
        private bool _buttonsVisibility;

        public MainViewModel(IGrowingNeuralGasService neuralGasService)
        {
            _neuralGasModel = new NeuralGasModel(neuralGasService);
            _neuralGasModel.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            EdgeMaxAge = 5;
            NumberOfEpochs = 20;
            LearningRateDecay = 1.0;
            MaxNumberOfNeurons = 50;
            ButtonsVisibility = true;
            SelectedItem = new KeyValuePair<string, string>(GeneratorTypes.CircleWithLine.ToString(), GeneratorTypes.CircleWithLine.Description());
            ShowDataCommand = new DelegateCommand(async () =>
            {
                GeneratorTypes selectedItem;
                if (Enum.TryParse(SelectedItem.Key, out selectedItem))
                {
                    ButtonsVisibility = false;
                    await _neuralGasModel.GenerateData(selectedItem);
                    ButtonsVisibility = true;
                }
            });
            StartTrainingCommand = new DelegateCommand(async () =>
            {
                GeneratorTypes selectedItem;
                if (Enum.TryParse(SelectedItem.Key, out selectedItem))
                {
                    ButtonsVisibility = false;
                    await _neuralGasModel.Init(NumberOfEpochs, LearningRateDecay, EdgeMaxAge, MaxNumberOfNeurons,
                        IsForceDying, selectedItem);
                    ButtonsVisibility = true;
                }
            });
        }

        public GeneratorTypes CurrentEffectStyle
        {
            get => _currentEffectStyle;
            set
            {
                _currentEffectStyle = value;
                RaisePropertyChanged(nameof(CurrentEffectStyle));
            }
        }

        public KeyValuePair<string, string> SelectedItem { get; set; }


        public IEnumerable<KeyValuePair<string, string>> GeneratorTypeList => EnumHelper
            .GetAllValuesAndDescriptions<GeneratorTypes>();

        public DelegateCommand ShowDataCommand { get; }

        public bool ButtonsVisibility
        {
            get => _buttonsVisibility;
            set
            {
                _buttonsVisibility = value;
                RaisePropertyChanged(nameof(ButtonsVisibility));
            }
        }

        public DelegateCommand StartTrainingCommand { get; }
        public SeriesCollection SeriesChartsCollection => _neuralGasModel.SeriesChartsCollection;
        public string NeuralNetworkLog => _neuralGasModel.NeuralNetworkLog;
        public int NumberOfEpochs { get; set; }

        public double LearningRateDecay
        {
            get => _learningRateDecay;
            set
            {
                if (value > 1.3)
                    MessageBox.Show("Спад скорости обучения не должен превышать 1.3", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                else
                    _learningRateDecay = value;
            }
        }

        public int EdgeMaxAge { get; set; }

        public int MaxNumberOfNeurons { get; set; }

        public bool IsForceDying { get; set; }
    }
}