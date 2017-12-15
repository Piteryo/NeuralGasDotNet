using System.ComponentModel;

namespace NeuralGasDotNet.Services
{
    public enum GeneratorTypes
    {
        [Description("Circle with line")] CircleWithLine,
        [Description("5 hills")] FiveHills,
        [Description("2 blobs")] TwoBlobs,
        [Description("Blob inside blob")] BlobInsideBlob,
        [Description("Several blobs")] Donut
    }
}