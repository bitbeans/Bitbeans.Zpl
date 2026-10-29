using System;

namespace Bitbeans.Zpl.Viewer.BitmapFonts
{
    /// <summary>A bitmap font glyph: a monochrome pixel matrix for one Unicode code point.</summary>
    public sealed class ZplBitmapGlyph
    {
        private readonly bool[] pixels;

        /// <summary>Initializes a new instance of the <see cref="ZplBitmapGlyph"/> class; the pixel data is copied.</summary>
        /// <param name="codePoint">The Unicode code point.</param>
        /// <param name="width">The glyph width in dots.</param>
        /// <param name="height">The glyph height in dots.</param>
        /// <param name="pixels">The pixels row by row, <see langword="true"/> for a set dot.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pixels"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The length of <paramref name="pixels"/> is not <paramref name="width"/> times <paramref name="height"/>.</exception>
        public ZplBitmapGlyph(int codePoint, int width, int height, bool[] pixels)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Glyph width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Glyph height must be positive.");
            }

            ArgumentNullException.ThrowIfNull(pixels);

            if (pixels.Length != width * height)
            {
                throw new ArgumentException("Pixel data length must equal width multiplied by height.", nameof(pixels));
            }

            this.CodePoint = codePoint;
            this.Width = width;
            this.Height = height;
            this.pixels = (bool[])pixels.Clone();
        }

        /// <summary>Gets the Unicode code point of the glyph.</summary>
        public int CodePoint { get; }

        /// <summary>Gets the glyph width in dots.</summary>
        public int Width { get; }

        /// <summary>Gets the glyph height in dots.</summary>
        public int Height { get; }

        /// <summary>Gets whether a dot of the glyph is set.</summary>
        /// <param name="x">The column, zero-based.</param>
        /// <param name="y">The row, zero-based.</param>
        /// <returns><see langword="true"/> if the dot is set.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="x"/> or <paramref name="y"/> is outside the glyph.</exception>
        public bool IsSet(int x, int y)
        {
            if (x < 0 || x >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, "X coordinate is outside the glyph.");
            }

            if (y < 0 || y >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, "Y coordinate is outside the glyph.");
            }

            return this.pixels[(y * this.Width) + x];
        }

        /// <summary>Gets a copy of the pixel data.</summary>
        /// <returns>The pixels row by row, <see langword="true"/> for a set dot.</returns>
        public bool[] GetPixels()
        {
            return (bool[])this.pixels.Clone();
        }
    }
}

