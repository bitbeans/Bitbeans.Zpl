using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Bitbeans.Zpl.Viewer.BitmapFonts
{
    /// <summary>A bitmap font: its metrics and the glyphs by Unicode code point.</summary>
    public sealed class ZplBitmapFontData
    {
        private readonly IReadOnlyDictionary<int, ZplBitmapGlyph> glyphs;

        /// <summary>Initializes a new instance of the <see cref="ZplBitmapFontData"/> class; the glyph dictionary is copied.</summary>
        /// <param name="metrics">The font metrics.</param>
        /// <param name="glyphs">The glyphs by Unicode code point.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metrics"/> or <paramref name="glyphs"/> is <see langword="null"/>.</exception>
        public ZplBitmapFontData(
            ZplBitmapFontMetrics metrics,
            IReadOnlyDictionary<int, ZplBitmapGlyph> glyphs)
        {
            this.Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
            this.glyphs = new ReadOnlyDictionary<int, ZplBitmapGlyph>(
                new Dictionary<int, ZplBitmapGlyph>(glyphs ?? throw new ArgumentNullException(nameof(glyphs))));
        }

        /// <summary>Gets the font metrics.</summary>
        public ZplBitmapFontMetrics Metrics { get; }

        /// <summary>Gets the glyphs by Unicode code point.</summary>
        public IReadOnlyDictionary<int, ZplBitmapGlyph> Glyphs => this.glyphs;

        /// <summary>Tries to get the glyph of a code point.</summary>
        /// <param name="codePoint">The Unicode code point.</param>
        /// <param name="glyph">The glyph if found; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the font contains the glyph; otherwise <see langword="false"/>.</returns>
        public bool TryGetGlyph(int codePoint, out ZplBitmapGlyph glyph)
        {
            return this.glyphs.TryGetValue(codePoint, out glyph);
        }
    }
}
