using System.Windows;
using NeuralGasDotNet.Interfaces;
using NeuralGasDotNet.Services.NeuralGas;
using NeuralGasDotNet.Views;
using Unity;
using Unity.Lifetime;

namespace NeuralGasDotNet
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            IUnityContainer container = new UnityContainer();
            container.RegisterType<IGrowingNeuralGasService, GrowingNeuralGasService>(
                new ContainerControlledLifetimeManager());
            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
        }
    }
}