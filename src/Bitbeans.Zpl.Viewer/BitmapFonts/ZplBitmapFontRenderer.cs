using SkiaSharp;

using System;
using System.Text;

namespace Bitbeans.Zpl.Viewer.BitmapFonts
{
    /// <summary>Draws text from bitmap font data onto a canvas, dot by dot without antialiasing.</summary>
    public sealed class ZplBitmapFontRenderer
    {
        private readonly SKCanvas canvas;

        /// <summary>Initializes a new instance of the <see cref="ZplBitmapFontRenderer"/> class.</summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <exception cref="ArgumentNullException"><paramref name="canvas"/> is <see langword="null"/>.</exception>
        public ZplBitmapFontRenderer(SKCanvas canvas)
        {
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }

        /// <summary>Measures the width of a text: the advance of the font metrics for every character.</summary>
        /// <param name="text">The text.</param>
        /// <param name="metrics">The font metrics.</param>
        /// <returns>The width in pixels; 0 for a null or empty text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="metrics"/> is <see langword="null"/>.</exception>
        public static int MeasureTextWidth(string text, ZplBitmapFontMetrics metrics)
        {
            ArgumentNullException.ThrowIfNull(metrics);

            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int width = 0;
            foreach (Rune _ in text.EnumerateRunes())
            {
                width += metrics.Advance;
            }

            return width;
        }

        /// <summary>Creates a paint for bitmap text: no antialiasing and, for reverse print, XOR blending.</summary>
        /// <param name="reversePrint"><see langword="true"/> to blend with XOR (reverse print).</param>
        /// <returns>The new paint; the caller disposes it.</returns>
        public static SKPaint CreatePaint(bool reversePrint)
        {
            return new SKPaint
            {
                IsAntialias = false,
                Style = SKPaintStyle.Fill,
                BlendMode = reversePrint ?
                    SKBlendMode.Xor :
                    SKBlendMode.SrcOver
            };
        }

        /// <summary>Draws a text; characters without a glyph in the font only advance the position.</summary>
        /// <param name="text">The text.</param>
        /// <param name="fontData">The bitmap font data.</param>
        /// <param name="metrics">The font metrics.</param>
        /// <param name="x">The X coordinate of the left edge.</param>
        /// <param name="y">The Y coordinate of the top edge.</param>
        /// <param name="paint">The paint used for drawing.</param>
        /// <returns>The width of the drawn text in pixels; 0 for a null or empty text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="fontData"/>, <paramref name="metrics"/> or <paramref name="paint"/> is <see langword="null"/>.</exception>
        public int DrawText(
            string text,
            ZplBitmapFontData fontData,
            ZplBitmapFontMetrics metrics,
            float x,
            float y,
            SKPaint paint)
        {
            ArgumentNullException.ThrowIfNull(fontData);
            ArgumentNullException.ThrowIfNull(metrics);
            ArgumentNullException.ThrowIfNull(paint);

            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            float currentX = Snap(x);
            float top = Snap(y);
            int renderedWidth = 0;

            foreach (Rune rune in text.EnumerateRunes())
            {
                if (fontData.TryGetGlyph(rune.Value, out ZplBitmapGlyph glyph))
                {
                    this.DrawGlyph(glyph, metrics, currentX, top, paint);
                }

                currentX += metrics.Advance;
                renderedWidth += metrics.Advance;
            }

            return renderedWidth;
        }

        /// <summary>Draws a single glyph, each set dot as a rectangle of the expansion size.</summary>
        /// <param name="glyph">The glyph.</param>
        /// <param name="metrics">The font metrics.</param>
        /// <param name="x">The X coordinate of the left edge.</param>
        /// <param name="y">The Y coordinate of the top edge.</param>
        /// <param name="paint">The paint used for drawing.</param>
        /// <exception cref="ArgumentNullException"><paramref name="glyph"/>, <paramref name="metrics"/> or <paramref name="paint"/> is <see langword="null"/>.</exception>
        public void DrawGlyph(
            ZplBitmapGlyph glyph,
            ZplBitmapFontMetrics metrics,
            float x,
            float y,
            SKPaint paint)
        {
            ArgumentNullException.ThrowIfNull(glyph);
            ArgumentNullException.ThrowIfNull(metrics);
            ArgumentNullException.ThrowIfNull(paint);

            float left = Snap(x);
            float top = Snap(y);
            float pixelWidth = SnapDimension(metrics.ExpansionX);
            float pixelHeight = SnapDimension(metrics.ExpansionY);
            int drawWidth = Math.Min(glyph.Width, metrics.MatrixWidth);
            int drawHeight = Math.Min(glyph.Height, metrics.MatrixHeight);

            for (int row = 0; row < drawHeight; row++)
            {
                for (int column = 0; column < drawWidth; column++)
                {
                    if (!glyph.IsSet(column, row))
                    {
                        continue;
                    }

                    this.canvas.DrawRect(
                        left + column * pixelWidth,
                        top + row * pixelHeight,
                        pixelWidth,
                        pixelHeight,
                        paint);
                }
            }
        }

        private static float Snap(float value)
        {
            return MathF.Round(value);
        }

        private static float SnapDimension(float value)
        {
            return Math.Max(1, MathF.Round(value));
        }
    }
}
