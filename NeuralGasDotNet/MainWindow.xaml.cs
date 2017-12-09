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

            DataContext = this;
        }

        public ChartValues<ObservablePoint> ValuesA { get; set; }
        public ChartValues<ObservablePoint> ValuesB { get; set; }
        public NetworkInititalizer NetworkInititalizer { get; set; }

        public SeriesCollection SeriesCollection { get; set; }


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            NetworkInititalizer = new NetworkInititalizer();
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
        }
    }
}
