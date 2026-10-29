using Bitbeans.Zpl.ImageConverters;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;

namespace Bitbeans.Zpl.SkiaSharp.Tests
{
    [TestClass]
    public class SkiaImageConverterTests
    {
        private static readonly SKColor Black = new(0, 0, 0, 255);
        private static readonly SKColor White = new(255, 255, 255, 255);

        private static byte[] CreatePng(int width, int height, Func<int, int, SKColor> pixel)
        {
            using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bitmap.SetPixel(x, y, pixel(x, y));
                }
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static byte[] SolidPng(int width, int height, SKColor color)
        {
            return CreatePng(width, height, (_, _) => color);
        }

        // ---- constructor ----

        [TestMethod]
        public void DefaultThresholdIs128()
        {
            Assert.AreEqual(128, SkiaImageConverter.DefaultThreshold);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(128)]
        [DataRow(256)]
        public void ConstructorAcceptsThresholdsFromZeroToTwoFiftySix(int threshold)
        {
            Assert.IsNotNull(new SkiaImageConverter(threshold));
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(257)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void ConstructorRejectsThresholdsOutOfRange(int threshold)
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SkiaImageConverter(threshold));
        }

        // ---- ConvertImage ----

        [TestMethod]
        public void ConvertImageBlackAndWhite()
        {
            // 8x2: first row black, second row white
            var png = CreatePng(8, 2, (_, y) => y == 0 ? Black : White);

            var result = new SkiaImageConverter().ConvertImage(png);

            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00 }, result.RawData);
            Assert.AreEqual(1, result.BytesPerRow);
            Assert.AreEqual(2, result.BinaryByteCount);
        }

        [TestMethod]
        public void ConvertImagePacksBitsMostSignificantBitFirst()
        {
            // only pixel x == 0, 2 and 7 are black
            var png = CreatePng(8, 1, (x, _) => x is 0 or 2 or 7 ? Black : White);

            var result = new SkiaImageConverter().ConvertImage(png);

            CollectionAssert.AreEqual(new byte[] { 0b1010_0001 }, result.RawData);
        }

        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(7, 1)]
        [DataRow(8, 1)]
        [DataRow(9, 2)]
        [DataRow(10, 2)]
        [DataRow(16, 2)]
        [DataRow(17, 3)]
        public void ConvertImagePadsRowsToWholeBytes(int width, int expectedBytesPerRow)
        {
            var png = SolidPng(width, 3, Black);

            var result = new SkiaImageConverter().ConvertImage(png);

            Assert.AreEqual(expectedBytesPerRow, result.BytesPerRow);
            Assert.AreEqual(expectedBytesPerRow * 3, result.RawData.Length);
            Assert.AreEqual(result.RawData.Length, result.BinaryByteCount);

            // padding bits at the end of every row stay 0
            int usedBitsInLastByte = width - ((expectedBytesPerRow - 1) * 8);
            byte expectedLastByte = (byte)(0xFF << (8 - usedBitsInLastByte));
            for (int row = 0; row < 3; row++)
            {
                for (int i = 0; i < expectedBytesPerRow - 1; i++)
                {
                    Assert.AreEqual(0xFF, result.RawData[(row * expectedBytesPerRow) + i]);
                }

                Assert.AreEqual(expectedLastByte, result.RawData[(row * expectedBytesPerRow) + expectedBytesPerRow - 1]);
            }
        }

        [TestMethod]
        public void ConvertImageKeepsRowOrderAndPixelPositionsOnWideImages()
        {
            // 10x2, a single black pixel at (9, 1)
            var png = CreatePng(10, 2, (x, y) => x == 9 && y == 1 ? Black : White);

            var result = new SkiaImageConverter().ConvertImage(png);

            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x40 }, result.RawData);
        }

        [TestMethod]
        public void ConvertImageThresholdBoundaries()
        {
            byte[] GrayPng(byte gray) => SolidPng(1, 1, new SKColor(gray, gray, gray, 255));
            var converter = new SkiaImageConverter();

            // average luminance must be strictly below the threshold to become black
            Assert.AreEqual(0x80, converter.ConvertImage(GrayPng(127)).RawData[0]);
            Assert.AreEqual(0x00, converter.ConvertImage(GrayPng(128)).RawData[0]);
        }

        [TestMethod]
        public void ConvertImageUsesAverageOfColorChannels()
        {
            // (255 + 0 + 0) / 3 = 85 -> black, (255 + 255 + 0) / 3 = 170 -> white
            var converter = new SkiaImageConverter();

            Assert.AreEqual(0x80, converter.ConvertImage(SolidPng(1, 1, new SKColor(255, 0, 0))).RawData[0]);
            Assert.AreEqual(0x00, converter.ConvertImage(SolidPng(1, 1, new SKColor(255, 255, 0))).RawData[0]);
        }

        [TestMethod]
        public void ConvertImageHonorsCustomThreshold()
        {
            var gray = SolidPng(1, 1, new SKColor(100, 100, 100));

            Assert.AreEqual(0x80, new SkiaImageConverter().ConvertImage(gray).RawData[0]);
            Assert.AreEqual(0x80, new SkiaImageConverter(101).ConvertImage(gray).RawData[0]);
            Assert.AreEqual(0x00, new SkiaImageConverter(100).ConvertImage(gray).RawData[0]);
            Assert.AreEqual(0x00, new SkiaImageConverter(0).ConvertImage(SolidPng(1, 1, Black)).RawData[0]);
            Assert.AreEqual(0x80, new SkiaImageConverter(256).ConvertImage(SolidPng(1, 1, White)).RawData[0]);
        }

        [TestMethod]
        public void ConvertImageComposesTransparencyOntoWhite()
        {
            var converter = new SkiaImageConverter();

            // fully transparent pixels (whatever their color) are white
            Assert.AreEqual(0x00, converter.ConvertImage(SolidPng(1, 1, new SKColor(0, 0, 0, 0))).RawData[0]);
            // mostly opaque black stays black, mostly transparent black becomes white
            Assert.AreEqual(0x80, converter.ConvertImage(SolidPng(1, 1, new SKColor(0, 0, 0, 200))).RawData[0]);
            Assert.AreEqual(0x00, converter.ConvertImage(SolidPng(1, 1, new SKColor(0, 0, 0, 100))).RawData[0]);
            // the composition boundary of black on white is at about 50 % alpha
            Assert.AreEqual(0x80, converter.ConvertImage(SolidPng(1, 1, new SKColor(0, 0, 0, 128))).RawData[0]);
            Assert.AreEqual(0x00, converter.ConvertImage(SolidPng(1, 1, new SKColor(0, 0, 0, 127))).RawData[0]);
        }

        [TestMethod]
        public void ConvertImageRejectsInvalidInput()
        {
            var converter = new SkiaImageConverter();

            Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertImage(null));
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertImage([]));
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertImage([1, 2, 3, 4, 5, 6, 7, 8]));
        }

        [TestMethod]
        public void ConvertImageRejectsCorruptedImages()
        {
            var png = CreatePng(16, 16, (x, y) => (x * y) % 3 == 0 ? Black : White);
            var converter = new SkiaImageConverter();
            int rejected = 0;

            // flipping any single byte either leaves a decodable image or is reported as InvalidDataException
            for (int index = 0; index < png.Length; index++)
            {
                var corrupted = png.ToArray();
                corrupted[index] ^= 0xFF;
                try
                {
                    Assert.AreEqual(2, converter.ConvertImage(corrupted).BytesPerRow);
                }
                catch (InvalidDataException)
                {
                    rejected++;
                }
            }

            Assert.IsGreaterThan(0, rejected);
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertImage(png.Take(40).ToArray()));
        }

        // ---- ConvertToPng ----

        [TestMethod]
        public void ConvertToPngWithScaleOneKeepsPixels()
        {
            var source = CreatePng(3, 2, (x, y) => (x + y) % 2 == 0 ? Black : White);

            var png = new SkiaImageConverter().ConvertToPng(source, 1);

            using var decoded = SKBitmap.Decode(png);
            Assert.AreEqual(3, decoded.Width);
            Assert.AreEqual(2, decoded.Height);
            for (int y = 0; y < 2; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    Assert.AreEqual((x + y) % 2 == 0 ? Black : White, decoded.GetPixel(x, y));
                }
            }
        }

        [TestMethod]
        [DataRow(2.0, 20, 12)]
        [DataRow(0.5, 5, 3)]
        [DataRow(1.5, 15, 9)]
        [DataRow(0.01, 1, 1)]
        public void ConvertToPngScalesImage(double scale, int expectedWidth, int expectedHeight)
        {
            var source = SolidPng(10, 6, Black);

            var png = new SkiaImageConverter().ConvertToPng(source, scale);

            using var decoded = SKBitmap.Decode(png);
            Assert.AreEqual(expectedWidth, decoded.Width);
            Assert.AreEqual(expectedHeight, decoded.Height);
            Assert.AreEqual(Black, decoded.GetPixel(expectedWidth / 2, expectedHeight / 2));
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-1.0)]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void ConvertToPngRejectsInvalidScaleFactors(double scale)
        {
            var source = SolidPng(2, 2, Black);

            var exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SkiaImageConverter().ConvertToPng(source, scale));
            Assert.AreEqual("scaleFactor", exception.ParamName);
        }

        [TestMethod]
        public void ConvertToPngRejectsInvalidImageData()
        {
            var converter = new SkiaImageConverter();

            Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertToPng(null, 1));
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertToPng([1, 2, 3], 1));
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertToPng([1, 2, 3], 2));
        }

        // ---- ConvertGrfToPng ----

        [TestMethod]
        public void ConvertGrfToPngProducesTransparentBackgroundAndBlackPixels()
        {
            // 16x2 image: first row 0xF0 0x0F, second row 0x01 0x80
            var grf = new byte[] { 0xF0, 0x0F, 0x01, 0x80 };

            var png = new SkiaImageConverter().ConvertGrfToPng(grf, 2);

            using var decoded = SKBitmap.Decode(png);
            Assert.AreEqual(16, decoded.Width);
            Assert.AreEqual(2, decoded.Height);

            for (int x = 0; x < 16; x++)
            {
                bool expectedBlack = x is < 4 or >= 12;
                AssertPixel(decoded, x, 0, expectedBlack);
            }

            for (int x = 0; x < 16; x++)
            {
                bool expectedBlack = x is 7 or 8;
                AssertPixel(decoded, x, 1, expectedBlack);
            }
        }

        private static void AssertPixel(SKBitmap bitmap, int x, int y, bool expectedBlack)
        {
            SKColor pixel = bitmap.GetPixel(x, y);
            if (expectedBlack)
            {
                Assert.AreEqual(Black, pixel, $"pixel ({x},{y})");
            }
            else
            {
                Assert.AreEqual(0, pixel.Alpha, $"pixel ({x},{y})");
            }
        }

        [TestMethod]
        public void ConvertGrfToPngIgnoresTrailingPartialRow()
        {
            var png = new SkiaImageConverter().ConvertGrfToPng([0xFF, 0xFF, 0xFF, 0xFF, 0xFF], 2);

            using var decoded = SKBitmap.Decode(png);
            Assert.AreEqual(16, decoded.Width);
            Assert.AreEqual(2, decoded.Height);
        }

        [TestMethod]
        public void ConvertGrfRoundTrip()
        {
            // 13x3 image with a diagonal pattern; the 3 padding bits of every row are blank
            var source = CreatePng(13, 3, (x, y) => (x + y) % 3 == 0 ? Black : White);
            var converter = new SkiaImageConverter();

            var grf = converter.ConvertImage(source);
            var png = converter.ConvertGrfToPng(grf.RawData, grf.BytesPerRow);
            var roundTrip = converter.ConvertImage(png);

            Assert.AreEqual(2, grf.BytesPerRow);
            Assert.AreEqual(grf.BytesPerRow, roundTrip.BytesPerRow);
            CollectionAssert.AreEqual(grf.RawData, roundTrip.RawData);

            using var decoded = SKBitmap.Decode(png);
            Assert.AreEqual(16, decoded.Width);
            Assert.AreEqual(3, decoded.Height);
        }

        [TestMethod]
        public void ConvertGrfToPngRejectsInvalidInput()
        {
            var converter = new SkiaImageConverter();

            Assert.ThrowsExactly<ArgumentNullException>(() => converter.ConvertGrfToPng(null, 1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => converter.ConvertGrfToPng([0xFF], 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => converter.ConvertGrfToPng([0xFF], -1));
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertGrfToPng([0xFF], 2));
            Assert.ThrowsExactly<InvalidDataException>(() => converter.ConvertGrfToPng([], 1));
        }

        // ---- integration with the core library ----

        [TestMethod]
        public void ImplementsImageConverterContract()
        {
            IImageConverter converter = new SkiaImageConverter();
            var png = SolidPng(8, 1, Black);

            Assert.AreEqual("FF", converter.ConvertImage(png).RawData.Select(b => b.ToString("X2")).Aggregate(string.Concat));
            Assert.IsNotNull(converter.ConvertToPng(png, 1));
        }
    }
}
