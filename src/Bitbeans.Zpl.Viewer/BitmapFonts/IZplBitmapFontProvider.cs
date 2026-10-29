using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.BitmapFonts
{
    /// <summary>Supplies bitmap font data for strict bitmap text rendering (<see cref="ZplTextRenderingMode.BitmapStrict"/>).</summary>
    public interface IZplBitmapFontProvider
    {
        /// <summary>Tries to get the bitmap font for a ZPL font name, print density and character set.</summary>
        /// <param name="fontName">The ZPL font name (for example <c>A</c>).</param>
        /// <param name="printDensityDpmm">The print density in dots per millimeter.</param>
        /// <param name="internationalFont">The active international character set (<c>^CI</c>).</param>
        /// <param name="font">The font data if one is available; otherwise <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if a font is available; otherwise <see langword="false"/>.</returns>
        bool TryGet(
            string fontName,
            int printDensityDpmm,
            InternationalFont internationalFont,
            out ZplBitmapFontData font);
    }
}

