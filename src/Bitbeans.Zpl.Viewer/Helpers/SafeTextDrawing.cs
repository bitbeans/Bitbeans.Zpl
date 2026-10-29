using SkiaSharp;
using SkiaSharp.HarfBuzz;

using System;

namespace Bitbeans.Zpl.Viewer.Helpers
{
    /// <summary>Extension methods for drawing text without failing if shaping is not possible.</summary>
    public static class SafeTextDrawing
    {
        /// <summary>Draws shaped text with HarfBuzz and falls back to plain text drawing when shaping throws an <see cref="ArgumentNullException"/> for a missing asset.</summary>
        /// <param name="canvas">The canvas to draw on.</param>
        /// <param name="text">The text.</param>
        /// <param name="x">The X coordinate of the text.</param>
        /// <param name="y">The Y coordinate of the text baseline.</param>
        /// <param name="textAlign">The horizontal alignment of the text.</param>
        /// <param name="font">The font.</param>
        /// <param name="paint">The paint used for drawing.</param>
        public static void DrawShapedTextSafe(
            this SKCanvas canvas,
            string text,
            float x,
            float y,
            SKTextAlign textAlign,
            SKFont font,
            SKPaint paint)
        {
            try
            {
                canvas.DrawShapedText(text, x, y, textAlign, font, paint);
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "asset")
            {
                canvas.DrawText(text, x, y, textAlign, font, paint);
            }
        }
    }
}
