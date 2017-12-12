using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using NeuralGasDotNet.Services.NeuralGas;
using NeuralGasDotNet.ViewModels;
using Unity.Attributes;

namespace NeuralGasDotNet.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [Dependency]
        internal MainViewModel ViewModel
        {
            set => DataContext = value;
        }
        public MainWindow()
        {
            InitializeComponent();
            var r = new Random();
            ValuesA = new ChartValues<ObservablePoint>();
            ValuesB = new ChartValues<ObservablePoint>();
            
            //for (var i = 0; i < 20; i++)
            //{
            //    ValuesA.Add(new ObservablePoint(r.NextDouble() * 10, r.NextDouble() * 10));
            //}
            SeriesCollection = new SeriesCollection
            {
                new ScatterSeries
                {
                    Values = ValuesA,
                },
                new ScatterSeries
                {
                    Values = ValuesB,
                    PointGeometry = DefaultGeometries.Diamond
                }
            };

            
            DataContext = this;
        }

        public ChartValues<ObservablePoint> ValuesA { get; set; }
        public ChartValues<ObservablePoint> ValuesB { get; set; }

        public Visibility GraphVisibility { get; set; } = Visibility.Hidden;

        public Visibility TextBlockVisibility { get; set; } = Visibility.Visible;

        public string TextBlockText { get; set; } = "Здесь будет выводиться информация об этапах обучения";

        public SeriesCollection SeriesCollection { get; set; }

        public List<string> Data { get; set; } = new List<string> {"Круг с линией внутри"};


        //private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        //{
        //    NeuralGasService = new NeuralGasService(this);
        //    int i = 0;
        //    foreach (var point in NeuralGasService.X)
        //    {
        //        if (ValuesA.Count > i)
        //        {
        //            ValuesA[i].X = point.Item1;
        //            ValuesA[i].Y = point.Item2;
        //            ++i;
        //        }
        //        else
        //        {
        //            ValuesA.Add(new ObservablePoint(point.Item1, point.Item2));
        //        }
        //    }
        //}

        //private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        //{
        //    foreach (var point in NeuralGasService.W)
        //    {
        //        ValuesB.Add(new ObservablePoint(point.Item1, point.Item2));
        //    }
        //    foreach (var pair in NeuralGasService.C)
        //    {
        //        var Lines = new ChartValues<ObservablePoint>();
        //        Lines.Add(new ObservablePoint(NeuralGasService.W[pair.Item1].Item1, NeuralGasService.W[pair.Item1].Item2));
        //        Lines.Add(new ObservablePoint(NeuralGasService.W[pair.Item2].Item1, NeuralGasService.W[pair.Item2].Item2));
        //        SeriesCollection.Add(new LineSeries
        //        {
        //            Values = Lines,
        //            StrokeThickness = 4,
        //            Stroke = Brushes.Bisque,
        //            Fill = Brushes.Transparent,
        //            PointGeometrySize = 0
        //        });
        //    }
        //}
    }
}
