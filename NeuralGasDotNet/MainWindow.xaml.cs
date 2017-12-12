using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using NeuralGasDotNet.Services.NeuralGas;

namespace NeuralGasDotNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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


        public NetworkInititalizer NetworkInititalizer { get; set; }

        public SeriesCollection SeriesCollection { get; set; }

        public List<string> Data { get; set; } = new List<string> {"Круг с линией внутри"};


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            NetworkInititalizer = new NetworkInititalizer(this);
            int i = 0;
            foreach (var point in NetworkInititalizer.X)
            {
                if (ValuesA.Count > i)
                {
                    ValuesA[i].X = point.Item1;
                    ValuesA[i].Y = point.Item2;
                    ++i;
                }
                else
                {
                    ValuesA.Add(new ObservablePoint(point.Item1, point.Item2));
                }
            }
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            foreach (var point in NetworkInititalizer.W)
            {
                ValuesB.Add(new ObservablePoint(point.Item1, point.Item2));
            }
            foreach (var pair in NetworkInititalizer.C)
            {
                var Lines = new ChartValues<ObservablePoint>();
                Lines.Add(new ObservablePoint(NetworkInititalizer.W[pair.Item1].Item1, NetworkInititalizer.W[pair.Item1].Item2));
                Lines.Add(new ObservablePoint(NetworkInititalizer.W[pair.Item2].Item1, NetworkInititalizer.W[pair.Item2].Item2));
                SeriesCollection.Add(new LineSeries
                {
                    Values = Lines,
                    StrokeThickness = 4,
                    Stroke = Brushes.Bisque,
                    Fill = Brushes.Transparent,
                    PointGeometrySize = 0
                });
            }
        }
    }
}
