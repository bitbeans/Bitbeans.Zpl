namespace Bitbeans.Zpl.Viewer
{
    /// <summary>Selects how text is rendered.</summary>
    public enum ZplTextRenderingMode
    {
        /// <summary>Text is drawn with outline fonts. This is the default.</summary>
        Outline = 0,
        /// <summary>Text is drawn with bitmap fonts from <see cref="ElementDrawers.DrawerOptions.BitmapFontProvider"/>.</summary>
        BitmapStrict = 1
    }
}

