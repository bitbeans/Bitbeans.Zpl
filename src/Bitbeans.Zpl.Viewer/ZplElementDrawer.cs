using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Bitbeans.Zpl.Viewer.Helpers;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>Draws ZPL elements with the element drawers. The element drawers keep per-draw state, so every <c>Draw</c> call creates its own drawer instances; one <see cref="ZplElementDrawer"/> can be used by several threads at the same time (the <see cref="IPrinterStorage"/> and the <see cref="DrawerOptions"/> are shared and must not be modified while drawing).</summary>
    public class ZplElementDrawer
    {
        /// <summary>The largest label side in pixels.</summary>
        private const int MaxLabelPixels = 16384;

        /// <summary>The reverse probe is drawn with at most this many pixels per side.</summary>
        private const int ReverseProbePixels = 128;

        /// <summary>Probe pixels added around the area found by the reverse probe.</summary>
        private const int ReverseProbeMargin = 3;

        private readonly DrawerOptions drawerOptions;
        private readonly IPrinterStorage printerStorage;

        /// <summary>Initializes a new instance of the <see cref="ZplElementDrawer"/> class.</summary>
        /// <param name="printerStorage">The printer storage with the downloaded objects.</param>
        /// <param name="drawerOptions">The drawer options; default options if <see langword="null"/>.</param>
        public ZplElementDrawer(IPrinterStorage printerStorage, DrawerOptions drawerOptions = null)
        {
            this.drawerOptions = drawerOptions ?? new DrawerOptions();
            this.printerStorage = printerStorage;
        }

        /// <summary>
        /// Draws the label and returns it encoded in the <see cref="DrawerOptions.RenderFormat"/>.
        /// </summary>
        /// <param name="elements">Zpl elements</param>
        /// <param name="labelWidth">Label width in millimeter</param>
        /// <param name="labelHeight">Label height in millimeter</param>
        /// <param name="printDensityDpmm">Dots per millimeter</param>
        /// <param name="logger">Receives the errors of elements that cannot be drawn; such elements are skipped.</param>
        /// <returns>The encoded label image.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="elements"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="labelWidth"/>, <paramref name="labelHeight"/> or <paramref name="printDensityDpmm"/> is not a positive number, or the label is smaller than one pixel or larger than 16384 pixels per side.</exception>
        public byte[] Draw(
            ZplElementBase[] elements,
            double labelWidth = 101.6,
            double labelHeight = 152.4,
            int printDensityDpmm = 8,
            ILogger logger = null)
        {
            ArgumentNullException.ThrowIfNull(elements);
            (int labelImageWidth, int labelImageHeight) = GetLabelPixelSize(labelWidth, labelHeight, printDensityDpmm);

            SKImageInfo info = new(labelImageWidth, labelImageHeight);
            using SKSurface surface = SKSurface.Create(info);
            SKCanvas skCanvas = surface.Canvas;

            //make sure to have a transparent canvas for SKBlendMode.Xor to work properly
            skCanvas.Clear(SKColors.Transparent);
            this.DrawElements(skCanvas, elements, labelImageWidth, labelImageHeight, printDensityDpmm, logger ?? NullLogger.Instance);

            using SKImage image = surface.Snapshot();
            SKData imageData;

            //check if we need to apply an opaque background color
            if (drawerOptions.OpaqueBackground)
            {
                using SKSurface surfaceBackground = SKSurface.Create(info);
                SKCanvas skCanvasBackground = surfaceBackground.Canvas;
                skCanvasBackground.Clear(drawerOptions.BackgroundColor);
                skCanvasBackground.DrawImage(image, 0f, 0f, SKSamplingOptions.Default);

                using SKImage imageBackground = surfaceBackground.Snapshot();
                imageData = imageBackground.Encode(drawerOptions.RenderFormat, drawerOptions.RenderQuality);
            }
            else
            {
                imageData = image.Encode(drawerOptions.RenderFormat, drawerOptions.RenderQuality);
            }

            using (imageData)
            {
                return imageData.ToArray();
            }
        }

        /// <summary>
        /// Draws the label on the provided surface
        /// </summary>
        /// <param name="surface">Skia Surface</param>
        /// <param name="elements">Zpl elements</param>
        /// <param name="labelWidth">Label width in millimeter</param>
        /// <param name="labelHeight">Label height in millimeter</param>
        /// <param name="printDensityDpmm">Dots per millimeter</param>
        /// <param name="logger">Receives the errors of elements that cannot be drawn; such elements are skipped.</param>
        /// <exception cref="ArgumentNullException"><paramref name="surface"/> or <paramref name="elements"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="labelWidth"/>, <paramref name="labelHeight"/> or <paramref name="printDensityDpmm"/> is not a positive number, or the label is smaller than one pixel or larger than 16384 pixels per side.</exception>
        public void DrawSurface(SKSurface surface,
            IEnumerable<ZplElementBase> elements,
            double labelWidth = 101.6,
            double labelHeight = 152.4,
            int printDensityDpmm = 8,
            ILogger logger = null)
        {
            ArgumentNullException.ThrowIfNull(surface);
            ArgumentNullException.ThrowIfNull(elements);
            (int labelImageWidth, int labelImageHeight) = GetLabelPixelSize(labelWidth, labelHeight, printDensityDpmm);

            SKCanvas skCanvas = surface.Canvas;
            //This has an issue with AvaloniaUI making the window transparent.
            skCanvas.Clear(SKColors.Transparent);
            this.DrawElements(skCanvas, elements, labelImageWidth, labelImageHeight, printDensityDpmm, logger ?? NullLogger.Instance);
        }

        private static (int Width, int Height) GetLabelPixelSize(double labelWidth, double labelHeight, int printDensityDpmm)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(printDensityDpmm);
            return (
                ToPixels(labelWidth, printDensityDpmm, nameof(labelWidth)),
                ToPixels(labelHeight, printDensityDpmm, nameof(labelHeight)));
        }

        private static int ToPixels(double millimeter, int printDensityDpmm, string parameterName)
        {
            double pixels = millimeter * printDensityDpmm;

            // also rejects NaN
            if (!(pixels >= 0.5 && pixels < MaxLabelPixels + 0.5))
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    millimeter,
                    $"The label size must result in 1 to {MaxLabelPixels} pixels at {printDensityDpmm} dots per millimeter.");
            }

            return Convert.ToInt32(pixels);
        }

        /// <summary>
        /// Creates the element drawers; every draw call uses its own instances because the drawers keep the canvas of the current draw.
        /// </summary>
        private static ElementDrawerBase[] CreateElementDrawers()
        {
            return [
                new AztecBarcodeElementDrawer(),
                new Barcode11ElementDrawer(),
                new Barcode128ElementDrawer(),
                new Barcode39ElementDrawer(),
                new Barcode93ElementDrawer(),
                new BarcodeEAN13ElementDrawer(),
                new BarcodeEAN8ElementDrawer(),
                new BarcodeUpcAElementDrawer(),
                new BarcodeUpcEElementDrawer(),
                new BarcodeUpcExtensionElementDrawer(),
                new DataMatrixElementDrawer(),
                new FieldBlockElementDrawer(),
                new GraphicBoxElementDrawer(),
                new GraphicCircleElementDrawer(),
                new GraphicDiagonalLineElementDrawer(),
                new GraphicEllipseElementDrawer(),
                new GraphicFieldElementDrawer(),
                new GraphicSymbolElementDrawer(),
                new ImageMoveElementDrawer(),
                new Interleaved2of5BarcodeDrawer(),
                new Industrial2of5BarcodeDrawer(),
                new MaxiCodeElementDrawer(),
                new Pdf417ElementDrawer(),
                new QrCodeElementDrawer(),
                new RecallGraphicElementDrawer(),
                new TextFieldElementDrawer(),
                new BarcodeAnsiCodabarElementDrawer(),
                new BarcodeMsiElementDrawer(),
                new BarcodeLogmarsElementDrawer()
            ];
        }

        private void DrawElements(
            SKCanvas skCanvas,
            IEnumerable<ZplElementBase> elements,
            int labelImageWidth,
            int labelImageHeight,
            int printDensityDpmm,
            ILogger logger)
        {
            ElementDrawerBase[] elementDrawers = CreateElementDrawers();
            InternationalFont internationalFont = InternationalFont.ZCP850;
            SKPoint currentDefaultPosition = SKPoint.Empty;

            foreach (ZplElementBase element in elements)
            {
                if (element is ZplChangeInternationalFont changeFont)
                {
                    internationalFont = changeFont.InternationalFont;
                    continue;
                }

                try
                {
                    ElementDrawerBase drawer = elementDrawers.SingleOrDefault(o => o.CanDraw(element));
                    if (drawer == null)
                    {
                        continue;
                    }

                    if (drawer.IsReverseDraw(element))
                    {
                        //basically only ZplGraphicBox/Circle depending on requirements
                        currentDefaultPosition = this.DrawReverse(drawer, element, skCanvas, labelImageWidth, labelImageHeight, currentDefaultPosition, internationalFont, printDensityDpmm);
                        continue;
                    }

                    drawer.Prepare(printerStorage, skCanvas);
                    currentDefaultPosition = drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);
                }
                catch (Exception ex)
                {
                    if (element is ZplBarcode barcodeElement)
                        logger.LogError(ex, $"Error on zpl element \"{barcodeElement.Content}\": {ex.Message}");
                    else if (element is ZplDataMatrix dataMatrixElement)
                        logger.LogError(ex, $"Error on zpl element \"{dataMatrixElement.Content}\": {ex.Message}");
                    else
                    {
                        logger.LogError(ex, "Error on zpl element");
                    }
                }
            }
        }

        /// <summary>
        /// Draws a reverse print element: the element is drawn into a layer that is composited with <see cref="SKBlendMode.Xor"/>
        /// (inverting what is underneath). The layer is limited to the area of the element, which is found with a draw into a
        /// probe bitmap of at most 128 pixels per side, so the cost depends on the element and not on the label size.
        /// </summary>
        private SKPoint DrawReverse(
            ElementDrawerBase drawer,
            ZplElementBase element,
            SKCanvas skCanvas,
            int labelImageWidth,
            int labelImageHeight,
            SKPoint currentDefaultPosition,
            InternationalFont internationalFont,
            int printDensityDpmm)
        {
            // label pixels per probe pixel
            int probeScale = (Math.Max(labelImageWidth, labelImageHeight) + ReverseProbePixels - 1) / ReverseProbePixels;
            int probeWidth = (labelImageWidth + probeScale - 1) / probeScale;
            int probeHeight = (labelImageHeight + probeScale - 1) / probeScale;

            SKRectI probeArea;
            using (SKBitmap probe = new(new SKImageInfo(probeWidth, probeHeight)))
            {
                using (SKCanvas probeCanvas = new(probe))
                {
                    probeCanvas.Clear(SKColors.Transparent);
                    probeCanvas.Scale(1f / probeScale);
                    drawer.Prepare(printerStorage, probeCanvas);
                    drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);
                }

                probeArea = FindDrawnArea(probe);
            }

            // Without anything found (nothing visible or only details that vanish in the small probe) the layer covers the label.
            SKRect layerBounds = new(0, 0, labelImageWidth, labelImageHeight);
            if (!probeArea.IsEmpty)
            {
                layerBounds = SKRect.Intersect(
                    layerBounds,
                    new SKRect(
                        (probeArea.Left - ReverseProbeMargin) * probeScale,
                        (probeArea.Top - ReverseProbeMargin) * probeScale,
                        (probeArea.Right + ReverseProbeMargin) * probeScale,
                        (probeArea.Bottom + ReverseProbeMargin) * probeScale));
            }

            //use color inversion on an reverse draw white element
            using SKColorFilter inverter = drawer.IsWhiteDraw(element) ? CreateWhiteInverter() : null;
            using SKPaint layerPaint = new()
            {
                BlendMode = SKBlendMode.Xor,
                ColorFilter = inverter
            };

            int saveCount = skCanvas.SaveLayer(layerBounds, layerPaint);
            try
            {
                drawer.Prepare(printerStorage, skCanvas);
                return drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);
            }
            finally
            {
                skCanvas.RestoreToCount(saveCount);
            }
        }

        private static SKRectI FindDrawnArea(SKBitmap probe)
        {
            int left = int.MaxValue;
            int top = int.MaxValue;
            int right = -1;
            int bottom = -1;

            for (int y = 0; y < probe.Height; y++)
            {
                for (int x = 0; x < probe.Width; x++)
                {
                    if (probe.GetPixel(x, y).Alpha == 0)
                    {
                        continue;
                    }

                    left = Math.Min(left, x);
                    top = Math.Min(top, y);
                    right = Math.Max(right, x + 1);
                    bottom = Math.Max(bottom, y + 1);
                }
            }

            return right < 0 ? SKRectI.Empty : new SKRectI(left, top, right, bottom);
        }

        private static SKColorFilter CreateWhiteInverter()
        {
            float[] inverter = [
                -1f,  0f,  0f, 0f, 1f,
                0f, -1f,  0f, 0f, 1f,
                0f,  0f, -1f, 0f, 1f,
                0f,  0f,  0f, 1f, 0f
            ];
            return SKColorFilter.CreateColorMatrix(inverter);
        }
    }
}
