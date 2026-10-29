using Bitbeans.Zpl;
using Bitbeans.Zpl.Viewer.BitmapFonts;
using Bitbeans.Zpl.Viewer.Helpers;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;

using ZXing.Common;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Base clase for Barcode element drawers
    /// </summary>
    public abstract class BarcodeDrawerBase : ElementDrawerBase
    {
        /// <summary>
        /// Minimum acceptable magin between a barcode and its interpretation line, in pixels
        /// </summary>
        protected const float MIN_LABEL_MARGIN = 5f;
        /// <summary>The ZPL font used for the interpretation line.</summary>
        protected const string INTERPRETATION_LINE_FONT_NAME = "A";

        /// <summary>Draws the barcode image, rotated according to the field orientation.</summary>
        /// <param name="barcodeImageData">The encoded barcode image.</param>
        /// <param name="x">The X position of the barcode.</param>
        /// <param name="y">The Y position of the barcode.</param>
        /// <param name="barcodeWidth">The width of the barcode in pixels.</param>
        /// <param name="barcodeHeight">The height of the barcode in pixels.</param>
        /// <param name="useFieldOrigin"><see langword="true"/> if the field was positioned with <c>^FO</c>; <see langword="false"/> if it was positioned with <c>^FT</c> (bottom-left origin).</param>
        /// <param name="fieldOrientation">The field orientation.</param>
        protected void DrawBarcode(byte[] barcodeImageData, float x, float y, int barcodeWidth, int barcodeHeight, bool useFieldOrigin, Zpl.FieldOrientation fieldOrientation)
        {
            using (new SKAutoCanvasRestore(this.skCanvas))
            {
                SKMatrix matrix = GetRotationMatrix(x, y, barcodeWidth, barcodeHeight, useFieldOrigin, fieldOrientation);
                if (!useFieldOrigin)
                {
                    y -= barcodeHeight;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                if (matrix != SKMatrix.Empty)
                {
                    this.skCanvas.Concat(matrix);
                }

                this.skCanvas.DrawBitmap(SKBitmap.Decode(barcodeImageData), x, y, SKSamplingOptions.Default);
            }
        }

        /// <summary>Draws the interpretation line above or below the barcode using an outline font.</summary>
        /// <param name="interpretation">The human-readable interpretation text.</param>
        /// <param name="skFont">The font used to draw the text.</param>
        /// <param name="x">The X position of the barcode.</param>
        /// <param name="y">The Y position of the barcode.</param>
        /// <param name="barcodeWidth">The width of the barcode in pixels.</param>
        /// <param name="barcodeHeight">The height of the barcode in pixels.</param>
        /// <param name="useFieldOrigin"><see langword="true"/> if the field was positioned with <c>^FO</c>; <see langword="false"/> if it was positioned with <c>^FT</c> (bottom-left origin).</param>
        /// <param name="fieldOrientation">The field orientation.</param>
        /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the barcode.</param>
        /// <param name="options">The drawer options.</param>
        protected void DrawInterpretationLine(string interpretation, SKFont skFont, float x, float y, int barcodeWidth, int barcodeHeight, bool useFieldOrigin, Zpl.FieldOrientation fieldOrientation, bool printInterpretationLineAboveCode, DrawerOptions options)
        {
            using (new SKAutoCanvasRestore(this.skCanvas))
            {
                using SKPaint skPaint = new()
                {
                    IsAntialias = options.Antialias
                };

                SKMatrix matrix = GetRotationMatrix(x, y, barcodeWidth, barcodeHeight, useFieldOrigin, fieldOrientation);
                if (matrix != SKMatrix.Empty)
                {
                    this.skCanvas.Concat(matrix);
                }

                skFont.MeasureText(interpretation, out SKRect textBounds);
                x += (barcodeWidth - textBounds.Width) / 2;
                if (!useFieldOrigin)
                {
                    y -= barcodeHeight;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                float margin = Math.Max((skFont.Spacing - textBounds.Height) / 2, MIN_LABEL_MARGIN);
                if (printInterpretationLineAboveCode)
                {
                    this.skCanvas.DrawShapedTextSafe(interpretation, x, y - margin, SKTextAlign.Left, skFont, skPaint);
                }
                else
                {
                    this.skCanvas.DrawShapedTextSafe(
                        interpretation,
                        x,
                        y + barcodeHeight + textBounds.Height + margin,
                        SKTextAlign.Left,
                        skFont,
                        skPaint);
                }
            }
        }

        /// <summary>Draws the interpretation line above or below the barcode, using the bitmap font if strict bitmap rendering is enabled and a font is available, otherwise an outline font.</summary>
        /// <param name="interpretation">The human-readable interpretation text.</param>
        /// <param name="x">The X position of the barcode.</param>
        /// <param name="y">The Y position of the barcode.</param>
        /// <param name="barcodeWidth">The width of the barcode in pixels.</param>
        /// <param name="barcodeHeight">The height of the barcode in pixels.</param>
        /// <param name="useFieldOrigin"><see langword="true"/> if the field was positioned with <c>^FO</c>; <see langword="false"/> if it was positioned with <c>^FT</c> (bottom-left origin).</param>
        /// <param name="fieldOrientation">The field orientation.</param>
        /// <param name="printInterpretationLineAboveCode"><see langword="true"/> to print the interpretation line above the barcode.</param>
        /// <param name="options">The drawer options.</param>
        /// <param name="internationalFont">The active international character set (<c>^CI</c>).</param>
        /// <param name="printDensityDpmm">The print density in dots per millimeter.</param>
        /// <param name="moduleWidth">The module width in pixels.</param>
        protected void DrawBitmapInterpretationLine(
            string interpretation,
            float x,
            float y,
            int barcodeWidth,
            int barcodeHeight,
            bool useFieldOrigin,
            Zpl.FieldOrientation fieldOrientation,
            bool printInterpretationLineAboveCode,
            DrawerOptions options,
            InternationalFont internationalFont,
            int printDensityDpmm,
            int moduleWidth)
        {
            if (!TryGetBitmapInterpretationLineFont(options, internationalFont, printDensityDpmm, moduleWidth, out ZplBitmapFontData fontData, out ZplBitmapFontMetrics metrics))
            {
                float labelFontSize = Helpers.FontScale.GetBitmappedFontSize(INTERPRETATION_LINE_FONT_NAME, Math.Min(moduleWidth, 10), printDensityDpmm).Value;
                SKTypeface labelTypeFace = options.FontManager.FontLoader(INTERPRETATION_LINE_FONT_NAME);
                using SKFont labelFont = new(labelTypeFace, labelFontSize);
                this.DrawInterpretationLine(interpretation, labelFont, x, y, barcodeWidth, barcodeHeight, useFieldOrigin, fieldOrientation, printInterpretationLineAboveCode, options);
                return;
            }

            using (new SKAutoCanvasRestore(this.skCanvas))
            {
                SKMatrix matrix = GetRotationMatrix(x, y, barcodeWidth, barcodeHeight, useFieldOrigin, fieldOrientation);
                if (matrix != SKMatrix.Empty)
                {
                    this.skCanvas.Concat(matrix);
                }

                int textWidth = ZplBitmapFontRenderer.MeasureTextWidth(interpretation, metrics);
                float drawX = (float)Math.Round(x + (barcodeWidth - textWidth) / 2f);

                if (!useFieldOrigin)
                {
                    y -= barcodeHeight;
                    if (y < 0)
                    {
                        y = 0;
                    }
                }

                float drawY = printInterpretationLineAboveCode ?
                    y - MIN_LABEL_MARGIN - metrics.RenderedGlyphHeight :
                    y + barcodeHeight + MIN_LABEL_MARGIN;

                using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
                ZplBitmapFontRenderer renderer = new(this.skCanvas);
                renderer.DrawText(interpretation, fontData, metrics, drawX, drawY, paint);
            }
        }

        /// <summary>Tries to get the bitmap font and metrics for the interpretation line, scaled by the module width (at most 10).</summary>
        /// <param name="options">The drawer options.</param>
        /// <param name="internationalFont">The active international character set (<c>^CI</c>).</param>
        /// <param name="printDensityDpmm">The print density in dots per millimeter.</param>
        /// <param name="moduleWidth">The module width in pixels.</param>
        /// <param name="fontData">The bitmap font if available; otherwise <see langword="null"/>.</param>
        /// <param name="metrics">The scaled metrics if available; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if strict bitmap rendering is enabled and the provider has the font; otherwise <see langword="false"/>.</returns>
        protected bool TryGetBitmapInterpretationLineFont(
            DrawerOptions options,
            InternationalFont internationalFont,
            int printDensityDpmm,
            int moduleWidth,
            out ZplBitmapFontData fontData,
            out ZplBitmapFontMetrics metrics)
        {
            fontData = null;
            metrics = null;

            if (options.TextRenderingMode != ZplTextRenderingMode.BitmapStrict ||
                options.BitmapFontProvider == null ||
                !options.BitmapFontProvider.TryGet(INTERPRETATION_LINE_FONT_NAME, printDensityDpmm, internationalFont, out fontData))
            {
                return false;
            }

            int expansion = Math.Max(1, Math.Min(moduleWidth, 10));
            metrics = fontData.Metrics.WithExpansion(expansion, expansion);
            return true;
        }

        /// <summary>Draws a digit with the bitmap font.</summary>
        /// <param name="digit">The digit text.</param>
        /// <param name="fontData">The bitmap font data.</param>
        /// <param name="metrics">The font metrics.</param>
        /// <param name="x">The X coordinate of the left edge.</param>
        /// <param name="y">The Y coordinate of the top edge.</param>
        /// <param name="paint">The paint used for drawing.</param>
        protected void DrawBitmapDigit(
            string digit,
            ZplBitmapFontData fontData,
            ZplBitmapFontMetrics metrics,
            float x,
            float y,
            SKPaint paint)
        {
            ZplBitmapFontRenderer renderer = new(this.skCanvas);
            renderer.DrawText(digit, fontData, metrics, x, y, paint);
        }

        /// <summary>Gets the matrix that rotates a barcode around its anchor according to the field orientation.</summary>
        /// <param name="x">The X position of the barcode.</param>
        /// <param name="y">The Y position of the barcode.</param>
        /// <param name="width">The barcode width in pixels.</param>
        /// <param name="height">The barcode height in pixels.</param>
        /// <param name="useFieldOrigin"><see langword="true"/> if the field was positioned with <c>^FO</c>; <see langword="false"/> if it was positioned with <c>^FT</c> (bottom-left origin).</param>
        /// <param name="fieldOrientation">The field orientation.</param>
        /// <returns>The rotation matrix, or <see cref="SKMatrix.Empty"/> for the normal orientation.</returns>
        protected static SKMatrix GetRotationMatrix(float x, float y, int width, int height, bool useFieldOrigin, Zpl.FieldOrientation fieldOrientation)
        {
            SKMatrix matrix = SKMatrix.Empty;
            if (useFieldOrigin)
            {
                switch (fieldOrientation)
                {
                    case Zpl.FieldOrientation.Rotated90:
                        matrix = SKMatrix.CreateRotationDegrees(90, x + height / 2, y + height / 2);
                        break;
                    case Zpl.FieldOrientation.Rotated180:
                        matrix = SKMatrix.CreateRotationDegrees(180, x + width / 2, y + height / 2);
                        break;
                    case Zpl.FieldOrientation.Rotated270:
                        matrix = SKMatrix.CreateRotationDegrees(270, x + width / 2, y + width / 2);
                        break;
                    case Zpl.FieldOrientation.Normal:
                        break;
                }
            }
            else
            {
                switch (fieldOrientation)
                {
                    case Zpl.FieldOrientation.Rotated90:
                        matrix = SKMatrix.CreateRotationDegrees(90, x, y);
                        break;
                    case Zpl.FieldOrientation.Rotated180:
                        matrix = SKMatrix.CreateRotationDegrees(180, x, y);
                        break;
                    case Zpl.FieldOrientation.Rotated270:
                        matrix = SKMatrix.CreateRotationDegrees(270, x, y);
                        break;
                    case Zpl.FieldOrientation.Normal:
                        break;
                }
            }

            return matrix;
        }

        /// <summary>Converts a bar pattern to a bitmap: bars are black, spaces transparent, each module is scaled horizontally and the image is stretched to the height.</summary>
        /// <param name="array">The bar pattern, one entry per module (<see langword="true"/> is a bar).</param>
        /// <param name="height">The bitmap height in pixels.</param>
        /// <param name="moduleWidth">The width of one module in pixels.</param>
        /// <returns>The new bitmap; the caller disposes it.</returns>
        protected static SKBitmap BoolArrayToSKBitmap(bool[] array, int height, int moduleWidth = 1)
        {
            using SKBitmap image = new(array.Length, 1);
            for (int col = 0; col < array.Length; col++)
            {
                SKColor color = array[col] ? SKColors.Black : SKColors.Transparent;
                image.SetPixel(col, 0, color);
            }

            SKSamplingOptions sampling = new(SKFilterMode.Nearest);
            return image.Resize(new SKSizeI(image.Width * moduleWidth, height), sampling);
        }

        /// <summary>Like <see cref="BoolArrayToSKBitmap"/>, but only modules that are set in both the pattern and the mask are drawn as bars.</summary>
        /// <param name="array">The bar pattern.</param>
        /// <param name="mask">The mask; only modules set in the mask can be bars.</param>
        /// <param name="height">The bitmap height in pixels.</param>
        /// <param name="moduleWidth">The width of one module in pixels.</param>
        /// <returns>The new bitmap; the caller disposes it.</returns>
        protected static SKBitmap BoolArrayWithMaskToSKBitmap(bool[] array, bool[] mask, int height, int moduleWidth = 1)
        {
            using SKBitmap image = new(array.Length, 1);
            for (int col = 0; col < array.Length; col++)
            {
                SKColor color = array[col] && mask[col] ? SKColors.Black : SKColors.Transparent;
                image.SetPixel(col, 0, color);
            }

            SKSamplingOptions sampling = new(SKFilterMode.Nearest);
            return image.Resize(new SKSizeI(image.Width * moduleWidth, height), sampling);
        }

        /// <summary>Converts a two-dimensional symbol to a bitmap: set modules are black, others transparent, each module is scaled by the pixel scale.</summary>
        /// <param name="matrix">The bit matrix.</param>
        /// <param name="pixelScale">The size of one module in pixels.</param>
        /// <returns>The new bitmap; the caller disposes it.</returns>
        protected static SKBitmap BitMatrixToSKBitmap(BitMatrix matrix, int pixelScale)
        {
            using SKBitmap image = new(matrix.Width, matrix.Height);
            for (int row = 0; row < matrix.Height; row++)
            {
                for (int col = 0; col < matrix.Width; col++)
                {
                    SKColor color = matrix[col, row] ? SKColors.Black : SKColors.Transparent;
                    image.SetPixel(col, row, color);
                }
            }

            SKSamplingOptions sampling = new(SKFilterMode.Nearest);
            return image.Resize(new SKSizeI(image.Width * pixelScale, image.Height * pixelScale), sampling);
        }

        /// <summary>Rescales a bar pattern from single-module runs to the given widths: runs of one module become narrow, longer runs wide, and the final run is always narrow.</summary>
        /// <param name="array">The bar pattern with narrow (1 module) and wide (2 or more modules) runs.</param>
        /// <param name="wide">The wide bar width in modules.</param>
        /// <param name="narrow">The narrow bar width in modules.</param>
        /// <returns>The scaled bar pattern.</returns>
        protected static bool[] AdjustWidths(bool[] array, int wide, int narrow)
        {
            List<bool> result = [];
            bool last = true;
            int count = 0;
            foreach (bool current in array)
            {
                if (current != last)
                {
                    result.AddRange(Enumerable.Repeat(last, count == 1 ? narrow : wide));
                    last = current;
                    count = 0;
                }

                count += 1;
            }

            result.AddRange(Enumerable.Repeat(last, narrow));
            return result.ToArray();
        }
    }
}
