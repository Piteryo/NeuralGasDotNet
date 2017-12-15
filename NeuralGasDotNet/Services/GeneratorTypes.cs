using System.ComponentModel;

namespace NeuralGasDotNet.Services
{
    public enum GeneratorTypes
    {
        [Description("Круг с линией")] CircleWithLine,
        [Description("5 холмов")] FiveHills,
        [Description("2 окружности")] TwoBlobs,
        [Description("Окружность внутри окружности")] BlobInsideBlob,
        [Description("Несколько окружностей")] Donut
    }
}