using System.Text.RegularExpressions;
using System.Windows.Input;
using NeuralGasDotNet.ViewModels;
using Unity.Attributes;

namespace NeuralGasDotNet.Views
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [Dependency]
        internal MainViewModel ViewModel
        {
            set => DataContext = value;
        }

        private void TextInputPreviewer(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(text);
        }
    }
}