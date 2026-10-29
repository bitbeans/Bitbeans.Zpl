using Bitbeans.Zpl.ImageConverters;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Tests
{
    /// <summary>
    /// Deterministic <see cref="IImageConverter"/> that records its calls
    /// </summary>
    internal sealed class FakeImageConverter : IImageConverter
    {
        public byte[] RawData { get; set; } = [0xFF, 0x00, 0x00, 0x00, 0xFF, 0x00];

        public int BytesPerRow { get; set; } = 2;

        public byte[] Png { get; set; } = [1, 2, 3];

        public List<byte[]> ConvertedImages { get; } = [];

        public List<double> ScaleFactors { get; } = [];

        public ImageResult ConvertImage(byte[] imageData)
        {
            ConvertedImages.Add(imageData);
            return new ImageResult
            {
                RawData = RawData,
                BinaryByteCount = RawData.Length,
                BytesPerRow = BytesPerRow
            };
        }

        public byte[] ConvertToPng(byte[] imageData, double scaleFactor)
        {
            ScaleFactors.Add(scaleFactor);
            return Png;
        }
    }
}
