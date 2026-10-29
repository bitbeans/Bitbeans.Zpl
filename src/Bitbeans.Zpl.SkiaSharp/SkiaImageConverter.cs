using Bitbeans.Zpl.ImageConverters;

using SkiaSharp;

using System;
using System.IO;

namespace Bitbeans.Zpl.SkiaSharp
{
    /// <summary>
    /// <see cref="IImageConverter"/> implementation based on SkiaSharp
    /// </summary>
    public class SkiaImageConverter : IImageConverter
    {
        /// <summary>
        /// Default luminance threshold
        /// </summary>
        public const int DefaultThreshold = 128;

        private readonly int threshold;

        /// <summary>
        /// Create a converter
        /// </summary>
        /// <param name="threshold">Pixels with an average luminance below this value (0-256) become black</param>
        public SkiaImageConverter(int threshold = DefaultThreshold)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(threshold);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(threshold, 256);
            this.threshold = threshold;
        }

        /// <summary>
        /// Convert an encoded image to a bitonal image (GRF).
        /// Transparent pixels are composed onto a white background.
        /// </summary>
        /// <param name="imageData">Encoded image data</param>
        /// <returns>The GRF data</returns>
        public ImageResult ConvertImage(byte[] imageData)
        {
            using SKBitmap image = Decode(imageData);

            int bytesPerRow = (image.Width + 7) / 8;
            byte[] rawData = new byte[bytesPerRow * image.Height];

            ReadOnlySpan<byte> pixels = image.GetPixelSpan();
            int rowBytes = image.RowBytes;

            for (int y = 0; y < image.Height; y++)
            {
                ReadOnlySpan<byte> row = pixels.Slice(y * rowBytes, image.Width * 4);
                int rowOffset = y * bytesPerRow;

                for (int x = 0; x < image.Width; x++)
                {
                    int pixelOffset = x * 4;
                    int alpha = row[pixelOffset + 3];
                    int red = Blend(row[pixelOffset], alpha);
                    int green = Blend(row[pixelOffset + 1], alpha);
                    int blue = Blend(row[pixelOffset + 2], alpha);

                    if ((red + green + blue) / 3 < this.threshold)
                    {
                        rawData[rowOffset + (x >> 3)] |= (byte)(0x80 >> (x & 7));
                    }
                }
            }

            return new ImageResult
            {
                RawData = rawData,
                BinaryByteCount = rawData.Length,
                BytesPerRow = bytesPerRow
            };
        }

        /// <summary>
        /// Decode an image, scale it and encode it as PNG
        /// </summary>
        /// <param name="imageData">Encoded image data</param>
        /// <param name="scaleFactor">Scale factor, 1 keeps the original size</param>
        /// <returns>PNG data</returns>
        public byte[] ConvertToPng(byte[] imageData, double scaleFactor)
        {
            if (double.IsNaN(scaleFactor) || double.IsInfinity(scaleFactor) || scaleFactor <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scaleFactor), scaleFactor, "Scale factor must be a positive number.");
            }

            using SKBitmap image = Decode(imageData);

            if (scaleFactor == 1)
            {
                return EncodePng(image);
            }

            int width = Math.Max(1, (int)Math.Round(image.Width * scaleFactor));
            int height = Math.Max(1, (int)Math.Round(image.Height * scaleFactor));

            using SKBitmap resized = new(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            image.ScalePixels(resized, new SKSamplingOptions(SKCubicResampler.Mitchell));

            return EncodePng(resized);
        }

        /// <summary>
        /// Convert a bitonal image (GRF) to a PNG image with a transparent background
        /// </summary>
        /// <param name="grfData">Packed pixel data, most significant bit first, 1 = black</param>
        /// <param name="bytesPerRow">Number of bytes per row</param>
        /// <returns>PNG data</returns>
        public byte[] ConvertGrfToPng(byte[] grfData, int bytesPerRow)
        {
            ArgumentNullException.ThrowIfNull(grfData);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bytesPerRow);

            int height = grfData.Length / bytesPerRow;
            int width = bytesPerRow * 8;
            if (height == 0)
            {
                throw new InvalidDataException("Graphic data is smaller than one row.");
            }

            using SKBitmap image = new(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
            image.Erase(SKColors.Transparent);

            Span<byte> pixels = image.GetPixelSpan();
            int rowBytes = image.RowBytes;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if ((grfData[(y * bytesPerRow) + (x >> 3)] & (0x80 >> (x & 7))) != 0)
                    {
                        // opaque black: B, G, R stay 0, only alpha is set
                        pixels[(y * rowBytes) + (x * 4) + 3] = 0xFF;
                    }
                }
            }

            return EncodePng(image);
        }

        private static SKBitmap Decode(byte[] imageData)
        {
            ArgumentNullException.ThrowIfNull(imageData);

            // SKBitmap.Decode(byte[]) throws an ArgumentNullException for undecodable data instead of returning null
            using SKData data = SKData.CreateCopy(imageData);
            using SKCodec codec = SKCodec.Create(data)
                ?? throw new InvalidDataException("Unable to decode image data.");
            using SKBitmap decoded = SKBitmap.Decode(codec)
                ?? throw new InvalidDataException("Unable to decode image data.");

            // Normalize to a known pixel layout so that the conversion can read raw bytes
            SKBitmap normalized = new(new SKImageInfo(decoded.Width, decoded.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            decoded.ScalePixels(normalized, new SKSamplingOptions(SKFilterMode.Nearest));
            return normalized;
        }

        private static int Blend(int channel, int alpha)
        {
            // compose onto white: c * a + 255 * (1 - a)
            return ((channel * alpha) + (255 * (255 - alpha)) + 127) / 255;
        }

        private static byte[] EncodePng(SKBitmap image)
        {
            using SKImage skImage = SKImage.FromBitmap(image);
            using SKData data = skImage.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
