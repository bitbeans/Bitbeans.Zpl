using System;

namespace Bitbeans.Zpl.Viewer.BitmapFonts
{
    /// <summary>The dimensions of a bitmap font: character matrix, spacing and expansion factors in dots.</summary>
    public sealed class ZplBitmapFontMetrics
    {
        /// <summary>Initializes a new instance of the <see cref="ZplBitmapFontMetrics"/> class.</summary>
        /// <param name="name">The font name.</param>
        /// <param name="matrixWidth">The width of the character matrix in dots.</param>
        /// <param name="matrixHeight">The height of the character matrix in dots.</param>
        /// <param name="intercharacterGap">The gap between two characters in dots (before expansion).</param>
        /// <param name="baseline">The baseline position in dots.</param>
        /// <param name="expansionX">The horizontal expansion factor, each dot is drawn this many pixels wide.</param>
        /// <param name="expansionY">The vertical expansion factor, each dot is drawn this many pixels high.</param>
        /// <param name="lineSpacing">The additional spacing between lines in pixels.</param>
        /// <exception cref="ArgumentException"><paramref name="name"/> is null, empty or white space.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="matrixWidth"/>, <paramref name="matrixHeight"/>, <paramref name="expansionX"/> or <paramref name="expansionY"/> is not positive, or <paramref name="intercharacterGap"/> or <paramref name="baseline"/> is negative.</exception>
        public ZplBitmapFontMetrics(
            string name,
            int matrixWidth,
            int matrixHeight,
            int intercharacterGap,
            int baseline,
            int expansionX = 1,
            int expansionY = 1,
            int lineSpacing = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Font name is required.", nameof(name));
            }

            if (matrixWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matrixWidth), matrixWidth, "Matrix width must be positive.");
            }

            if (matrixHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(matrixHeight), matrixHeight, "Matrix height must be positive.");
            }

            if (intercharacterGap < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(intercharacterGap), intercharacterGap, "Intercharacter gap cannot be negative.");
            }

            if (baseline < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseline), baseline, "Baseline cannot be negative.");
            }

            if (expansionX <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expansionX), expansionX, "Horizontal expansion must be positive.");
            }

            if (expansionY <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expansionY), expansionY, "Vertical expansion must be positive.");
            }

            this.Name = name;
            this.MatrixWidth = matrixWidth;
            this.MatrixHeight = matrixHeight;
            this.IntercharacterGap = intercharacterGap;
            this.Baseline = baseline;
            this.ExpansionX = expansionX;
            this.ExpansionY = expansionY;
            this.LineSpacing = lineSpacing;
        }

        /// <summary>Gets the font name.</summary>
        public string Name { get; }

        /// <summary>Gets the width of the character matrix in dots.</summary>
        public int MatrixWidth { get; }

        /// <summary>Gets the height of the character matrix in dots.</summary>
        public int MatrixHeight { get; }

        /// <summary>Gets the gap between two characters in dots (before expansion).</summary>
        public int IntercharacterGap { get; }

        /// <summary>Gets the baseline position in dots.</summary>
        public int Baseline { get; }

        /// <summary>Gets the horizontal expansion factor.</summary>
        public int ExpansionX { get; }

        /// <summary>Gets the vertical expansion factor.</summary>
        public int ExpansionY { get; }

        /// <summary>Gets the additional spacing between lines in pixels.</summary>
        public int LineSpacing { get; }

        /// <summary>Gets the horizontal distance from one character to the next: (matrix width + intercharacter gap) times the horizontal expansion.</summary>
        public int Advance => (this.MatrixWidth + this.IntercharacterGap) * this.ExpansionX;

        /// <summary>Gets the width of a rendered glyph: matrix width times the horizontal expansion.</summary>
        public int RenderedGlyphWidth => this.MatrixWidth * this.ExpansionX;

        /// <summary>Gets the height of a rendered glyph: matrix height times the vertical expansion.</summary>
        public int RenderedGlyphHeight => this.MatrixHeight * this.ExpansionY;

        /// <summary>Gets the height of a text line: rendered glyph height plus line spacing.</summary>
        public int LineHeight => this.RenderedGlyphHeight + this.LineSpacing;

        /// <summary>Creates a copy of these metrics with different expansion factors and line spacing.</summary>
        /// <param name="expansionX">The horizontal expansion factor.</param>
        /// <param name="expansionY">The vertical expansion factor.</param>
        /// <param name="lineSpacing">The additional spacing between lines in pixels.</param>
        /// <returns>The new metrics.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="expansionX"/> or <paramref name="expansionY"/> is not positive.</exception>
        public ZplBitmapFontMetrics WithExpansion(int expansionX, int expansionY, int lineSpacing = 0)
        {
            return new ZplBitmapFontMetrics(
                this.Name,
                this.MatrixWidth,
                this.MatrixHeight,
                this.IntercharacterGap,
                this.Baseline,
                expansionX,
                expansionY,
                lineSpacing);
        }
    }
}

