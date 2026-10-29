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
    public class ZplElementDrawer
    {
        /// <summary>
        /// The array of <see cref="IElementDrawer"/> to draw <see cref="ZplElementBase"/>
        /// </summary>
        private readonly IElementDrawer[] elementDrawers = [
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

        private readonly DrawerOptions drawerOptions;
        private readonly IPrinterStorage printerStorage;

        public ZplElementDrawer(IPrinterStorage printerStorage, DrawerOptions drawerOptions = null)
        {
            this.drawerOptions = drawerOptions ?? new DrawerOptions();
            this.printerStorage = printerStorage;
        }

        /// <summary>
        /// Draw the label on multiple canvases
        /// </summary>
        /// <param name="elements">Zpl elements</param>
        /// <param name="labelWidth">Label width in millimeter</param>
        /// <param name="labelHeight">Label height in millimeter</param>
        /// <param name="printDensityDpmm">Dots per millimeter</param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public byte[] Draw(
            ZplElementBase[] elements,
            double labelWidth = 101.6,
            double labelHeight = 152.4,
            int printDensityDpmm = 8,
            ILogger logger = null)
        {
            logger ??= NullLogger.Instance;
            int labelImageWidth = Convert.ToInt32(labelWidth * printDensityDpmm);
            int labelImageHeight = Convert.ToInt32(labelHeight * printDensityDpmm);

            //use SKNWayCanvas to be able to draw on multiple canvases
            using SKNWayCanvas skCanvas = new(labelImageWidth, labelImageHeight);

            //add Bitmap canvas
            SKImageInfo info = new(labelImageWidth, labelImageHeight);
            SKSurface surface = SKSurface.Create(info);
            using SKCanvas skImageCanvas = surface.Canvas;
            skCanvas.AddCanvas(skImageCanvas);

            //make sure to have a transparent canvas for SKBlendMode.Xor to work properly
            skCanvas.Clear(SKColors.Transparent);
            InternationalFont internationalFont = InternationalFont.ZCP850;
            SKPoint currentDefaultPosition = SKPoint.Empty;

            foreach (ZplElementBase element in elements)
            {
                if (element is ZplChangeInternationalFont changeFont)
                {
                    internationalFont = changeFont.InternationalFont;
                    continue;
                }

                IElementDrawer drawer = this.elementDrawers.SingleOrDefault(o => o.CanDraw(element));
                if (drawer == null)
                {
                    continue;
                }

                try
                {
                    if (drawer.IsReverseDraw(element))
                    {
                        //basically only ZplGraphicBox/Circle depending on requirements
                        using SKBitmap skBitmapInvert = new(labelImageWidth, labelImageHeight);
                        using SKCanvas skCanvasInvert = new(skBitmapInvert);
                        skCanvasInvert.Clear(SKColors.Transparent);

                        drawer.Prepare(printerStorage, skCanvasInvert);
                        currentDefaultPosition = drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);

                        //use color inversion on an reverse draw white element
                        if (drawer.IsWhiteDraw(element))
                        {
                            InvertDrawWhite(skCanvas, skBitmapInvert);
                        }
                        else
                        {
                            InvertDraw(skCanvas, skBitmapInvert);
                        }

                        continue;
                    }

                    drawer.Prepare(printerStorage, skCanvas);
                    currentDefaultPosition = drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);

                    continue;
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

            //check if we need to apply an opaque background color
            SKImage image = surface.Snapshot();
            if (drawerOptions.OpaqueBackground == true)
            {
                using SKSurface surfaceWhiteBg = SKSurface.Create(info);
                using SKCanvas skImageCanvasWhiteBg = surfaceWhiteBg.Canvas;
                skImageCanvasWhiteBg.Clear(drawerOptions.BackgroundColor);

                SKImage surfaceImage = surface.Snapshot();
                SKPaint paint = new()
                {
                    BlendMode = SKBlendMode.SrcOver
                };
                skImageCanvasWhiteBg.DrawImage(surfaceImage, 0f, 0f, SKSamplingOptions.Default, paint);

                image = surfaceWhiteBg.Snapshot();
            }

            SKData imageData = image.Encode(drawerOptions.RenderFormat, drawerOptions.RenderQuality);
            return imageData.ToArray();
        }

        /// <summary>
        /// Draw the label on the provided surface
        /// </summary>
        /// <param name="surface">Skia Surface</param>
        /// <param name="elements">Zpl elements</param>
        /// <param name="labelWidth">Label width in millimeter</param>
        /// <param name="labelHeight">Label height in millimeter</param>
        /// <param name="printDensityDpmm">Dots per millimeter</param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public void DrawSurface(SKSurface surface,
            IEnumerable<ZplElementBase> elements,
            double labelWidth = 101.6,
            double labelHeight = 152.4,
            int printDensityDpmm = 8,
            ILogger logger = null)
        {
            logger ??= NullLogger.Instance;
            int labelImageWidth = Convert.ToInt32(labelWidth * printDensityDpmm);
            int labelImageHeight = Convert.ToInt32(labelHeight * printDensityDpmm);

            SKCanvas skCanvas = surface.Canvas;
            //This has an issue with AvaloniaUI making the window transparent.
            skCanvas.Clear(SKColors.Transparent);
            InternationalFont internationalFont = InternationalFont.ZCP850;
            SKPoint currentDefaultPosition = SKPoint.Empty;

            foreach (ZplElementBase element in elements)
            {
                if (element is ZplChangeInternationalFont changeFont)
                {
                    internationalFont = changeFont.InternationalFont;
                    continue;
                }

                IElementDrawer drawer = this.elementDrawers.SingleOrDefault(o => o.CanDraw(element));
                if (drawer == null)
                {
                    continue;
                }

                try
                {
                    if (drawer.IsReverseDraw(element))
                    {
                        //basically only ZplGraphicBox/Circle depending on requirements
                        using SKBitmap skBitmapInvert = new(labelImageWidth, labelImageHeight);
                        using SKCanvas skCanvasInvert = new(skBitmapInvert);
                        skCanvasInvert.Clear(SKColors.Transparent);

                        drawer.Prepare(printerStorage, skCanvasInvert);
                        currentDefaultPosition = drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);

                        //use color inversion on an reverse draw white element
                        if (drawer.IsWhiteDraw(element))
                        {
                            InvertDrawWhite(skCanvas, skBitmapInvert);
                        }
                        else
                        {
                            InvertDraw(skCanvas, skBitmapInvert);
                        }

                        continue;
                    }

                    drawer.Prepare(printerStorage, skCanvas);
                    currentDefaultPosition = drawer.Draw(element, drawerOptions, currentDefaultPosition, internationalFont, printDensityDpmm);

                    continue;
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

        private static void InvertDraw(SKCanvas baseCanvas, SKBitmap bmToInvert)
        {
            using (SKPaint paint = new())
            {
                paint.BlendMode = SKBlendMode.Xor;
                baseCanvas.DrawBitmap(bmToInvert, 0, 0, SKSamplingOptions.Default, paint);
            }
        }

        private static void InvertDrawWhite(SKCanvas baseCanvas, SKBitmap bmToInvert)
        {
            using (SKPaint paint = new())
            {
                float[] inverter = [
                    -1f,  0f,  0f, 0f, 1f,
                    0f, -1f,  0f, 0f, 1f,
                    0f,  0f, -1f, 0f, 1f,
                    0f,  0f,  0f, 1f, 0f
                ];
                paint.ColorFilter = SKColorFilter.CreateColorMatrix(inverter);
                paint.BlendMode = SKBlendMode.Xor;
                baseCanvas.DrawBitmap(bmToInvert, 0, 0, SKSamplingOptions.Default, paint);
            }
        }
    }
}
