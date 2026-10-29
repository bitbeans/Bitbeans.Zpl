using Bitbeans.Zpl.Viewer.BitmapFonts;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>Options that control how labels are rendered: output format, background, text and font handling.</summary>
    public class DrawerOptions
    {
        /// <summary>
        /// Gets or sets the image format used when rendering output.
        /// </summary>
        public SKEncodedImageFormat RenderFormat { get; set; } = SKEncodedImageFormat.Png;

        /// <summary>
        /// Gets or sets the quality level used when rendering images in formats that support lossy compression.
        /// </summary>
        public int RenderQuality { get; set; } = 80;

        /// <summary>
        /// Applies label over a white background after rendering all elements
        /// </summary>
        public bool OpaqueBackground { get; set; } = false;

        /// <summary>
        /// Gets or sets the solid background color used when <see cref="OpaqueBackground"/> is enabled.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>Gets or sets a value indicating whether hyphens in text are replaced with en dashes. The default is <see langword="true"/>.</summary>
        public bool ReplaceDashWithEnDash { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether underscores in text should be replaced with en space.
        /// </summary>
        public bool ReplaceUnderscoreWithEnSpace { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether antialiasing is enabled.
        /// </summary>
        public bool Antialias { get; set; } = true;

        /// <summary>
        /// Gets or sets the text rendering path. Outline rendering is the default for compatibility.
        /// </summary>
        public ZplTextRenderingMode TextRenderingMode { get; set; } = ZplTextRenderingMode.Outline;

        /// <summary>
        /// Gets or sets the externally supplied bitmap font provider used by strict bitmap text rendering.
        /// </summary>
        public IZplBitmapFontProvider BitmapFontProvider { get; set; } = EmptyZplBitmapFontProvider.Instance;

        /// <summary>Gets the font manager used to resolve typefaces.</summary>
        public FontManager FontManager { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="DrawerOptions"/> class.</summary>
        public DrawerOptions() : this(new FontManager()) { }

        /// <summary>Initializes a new instance of the <see cref="DrawerOptions"/> class with the given font manager.</summary>
        /// <param name="fontManager">The font manager used to resolve typefaces.</param>
        public DrawerOptions(FontManager fontManager)
        {
            this.FontManager = fontManager;
        }
    }
}
