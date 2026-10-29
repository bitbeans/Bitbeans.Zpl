using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Models;

using Microsoft.Extensions.Logging;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;

using ZXing;
using ZXing.SkiaSharp;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    /// <summary>
    /// A logger collecting everything that is logged, for assertions.
    /// </summary>
    internal sealed class ListLogger : ILogger
    {
        public List<(LogLevel Level, string Message, Exception Exception)> Entries { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            this.Entries.Add((logLevel, formatter(state, exception), exception));
        }
    }

    /// <summary>
    /// Helpers for rendering ZPL to bitmaps and inspecting the pixels.
    /// </summary>
    internal static class RenderHelper
    {
        public const int Dpmm = 8;

        /// <summary>
        /// Drawer options with an opaque white background, so that pixels are either black or white.
        /// </summary>
        public static DrawerOptions Opaque()
        {
            return new DrawerOptions { OpaqueBackground = true };
        }

        public static ZplElementBase[] Parse(string zpl, IPrinterStorage storage = null)
        {
            storage ??= new PrinterStorage();
            ZplAnalyzer analyzer = new(storage);
            AnalyzeInfo info = analyzer.Analyze(zpl);
            return info.LabelInfos.SelectMany(l => l.ZplElements).ToArray();
        }

        /// <summary>
        /// Render a ZPL string (single label) into a bitmap. The default label is 50x50mm at 8 dpmm = 400x400 pixel.
        /// </summary>
        public static SKBitmap Render(
            string zpl,
            DrawerOptions options = null,
            double widthMm = 50,
            double heightMm = 50,
            int dpmm = Dpmm,
            ILogger logger = null,
            IPrinterStorage storage = null)
        {
            storage ??= new PrinterStorage();
            options ??= Opaque();
            ZplAnalyzer analyzer = new(storage);
            AnalyzeInfo info = analyzer.Analyze(zpl);
            ZplElementDrawer drawer = new(storage, options);
            byte[] png = drawer.Draw(info.LabelInfos.SelectMany(l => l.ZplElements).ToArray(), widthMm, heightMm, dpmm, logger);
            return SKBitmap.Decode(png);
        }

        public static SKBitmap RenderElements(
            ZplElementBase[] elements,
            DrawerOptions options = null,
            double widthMm = 50,
            double heightMm = 50,
            int dpmm = Dpmm,
            ILogger logger = null,
            IPrinterStorage storage = null)
        {
            storage ??= new PrinterStorage();
            options ??= Opaque();
            ZplElementDrawer drawer = new(storage, options);
            return SKBitmap.Decode(drawer.Draw(elements, widthMm, heightMm, dpmm, logger));
        }

        /// <summary>
        /// The analyzer never produces elements with default positioning, so flip the flag by reflection
        /// (the same state can be reached through the public element constructors).
        /// </summary>
        public static ZplElementBase WithDefaultPosition(ZplElementBase element)
        {
            typeof(ZplPositionedElementBase)
                .GetProperty(nameof(ZplPositionedElementBase.UseDefaultPosition))
                .SetValue(element, true);
            return element;
        }

        /// <summary>
        /// Some elements (e.g. the graphic symbol) cannot be given a field justification through their constructors,
        /// so replace the position element by reflection to reach the justified drawing code.
        /// </summary>
        public static ZplPositionedElementBase WithJustification(ZplPositionedElementBase element, FieldJustification justification)
        {
            if (element.FieldOrigin != null)
            {
                typeof(ZplPositionedElementBase)
                    .GetProperty(nameof(ZplPositionedElementBase.FieldOrigin))
                    .SetValue(element, new ZplFieldOrigin(element.PositionX, element.PositionY, justification));
            }
            else
            {
                typeof(ZplPositionedElementBase)
                    .GetProperty(nameof(ZplPositionedElementBase.FieldTypeset))
                    .SetValue(element, new ZplFieldTypeset(element.PositionX, element.PositionY, justification));
            }

            return element;
        }

        public static bool SameImage(SKBitmap a, SKBitmap b)
        {
            return a.Width == b.Width && a.Height == b.Height && a.Bytes.AsSpan().SequenceEqual(b.Bytes);
        }

        /// <summary>
        /// Contiguous groups of pixel rows that contain dark pixels (e.g. the text lines), as [top, bottom).
        /// </summary>
        public static List<(int Top, int Bottom)> RowBands(SKBitmap bitmap)
        {
            List<(int Top, int Bottom)> bands = [];
            int start = -1;
            for (int y = 0; y < bitmap.Height; y++)
            {
                bool any = CountDark(bitmap, 0, y, bitmap.Width, y + 1) > 0;
                if (any && start < 0)
                {
                    start = y;
                }
                else if (!any && start >= 0)
                {
                    bands.Add((start, y));
                    start = -1;
                }
            }

            if (start >= 0)
            {
                bands.Add((start, bitmap.Height));
            }

            return bands;
        }

        public static bool IsDark(SKBitmap bitmap, int x, int y)
        {
            SKColor c = bitmap.GetPixel(x, y);
            return c.Alpha > 127 && c.Red < 128 && c.Green < 128 && c.Blue < 128;
        }

        public static bool IsLight(SKBitmap bitmap, int x, int y)
        {
            return !IsDark(bitmap, x, y);
        }

        /// <summary>
        /// Number of dark pixels in the region [left, right) x [top, bottom)
        /// </summary>
        public static int CountDark(SKBitmap bitmap, int left, int top, int right, int bottom)
        {
            int count = 0;
            for (int y = Math.Max(top, 0); y < Math.Min(bottom, bitmap.Height); y++)
            {
                for (int x = Math.Max(left, 0); x < Math.Min(right, bitmap.Width); x++)
                {
                    if (IsDark(bitmap, x, y))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public static int CountDark(SKBitmap bitmap)
        {
            return CountDark(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }

        /// <summary>
        /// Bounding box (right and bottom exclusive) of the dark pixels, or null if there are none.
        /// </summary>
        public static SKRectI? DarkBounds(SKBitmap bitmap, int left = 0, int top = 0, int right = int.MaxValue, int bottom = int.MaxValue)
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = -1, maxY = -1;
            for (int y = Math.Max(top, 0); y < Math.Min(bottom, bitmap.Height); y++)
            {
                for (int x = Math.Max(left, 0); x < Math.Min(right, bitmap.Width); x++)
                {
                    if (!IsDark(bitmap, x, y))
                    {
                        continue;
                    }

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }

            if (maxX < 0)
            {
                return null;
            }

            return new SKRectI(minX, minY, maxX + 1, maxY + 1);
        }

        /// <summary>
        /// Decode a barcode with ZXing and return the text, or null if nothing could be decoded.
        /// </summary>
        public static string Decode(SKBitmap bitmap, params BarcodeFormat[] formats)
        {
            Result result = DecodeResult(bitmap, formats);
            return result?.Text;
        }

        /// <summary>
        /// Copy of the dark region with a white margin: some ZXing detectors search from the image center
        /// and fail on small symbols in a big image.
        /// </summary>
        public static SKBitmap CropToContent(SKBitmap bitmap, int margin = 24)
        {
            SKRectI? bounds = DarkBounds(bitmap);
            if (bounds == null)
            {
                return bitmap.Copy();
            }

            SKRectI b = bounds.Value;
            SKBitmap cropped = new(b.Width + 2 * margin, b.Height + 2 * margin);
            using SKCanvas canvas = new(cropped);
            canvas.Clear(SKColors.White);
            canvas.DrawBitmap(bitmap, SKRect.Create(b.Left, b.Top, b.Width, b.Height), SKRect.Create(margin, margin, b.Width, b.Height), SKSamplingOptions.Default);
            return cropped;
        }

        public static Result DecodeResult(SKBitmap source, params BarcodeFormat[] formats)
        {
            using SKBitmap bitmap = CropToContent(source);
            BarcodeReader reader = new()
            {
                AutoRotate = true,
                Options =
                {
                    TryHarder = true,
                    TryInverted = true,
                    PureBarcode = false,
                }
            };

            if (formats.Length > 0)
            {
                reader.Options.PossibleFormats = formats;
            }

            return reader.Decode(bitmap);
        }

        /// <summary>
        /// A canvas-backed surface for calling drawers directly.
        /// </summary>
        public static (SKSurface Surface, SKCanvas Canvas) CreateSurface(int width = 400, int height = 400)
        {
            SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
            surface.Canvas.Clear(SKColors.White);
            return (surface, surface.Canvas);
        }
    }
}
