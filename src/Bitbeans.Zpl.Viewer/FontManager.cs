using Bitbeans.Zpl.Viewer.Properties;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>Resolves the typefaces used for text and interpretation lines from font stacks, registered typefaces and the system fonts.</summary>
    /// <remarks>
    /// The manager owns and caches the typefaces it creates itself (the default typefaces matched in the system fonts and
    /// the lazily created <see cref="TypefaceGS"/>); they are shared by all draws and released by <see cref="Dispose"/>. Typefaces passed to
    /// <see cref="RegisterTypeface"/> or returned by a custom <see cref="FontLoader"/> belong to the caller and are not disposed.
    /// The lazily created typefaces are thread safe, so a manager can be used by concurrent draws.
    /// </remarks>
    public class FontManager : IDisposable
    {
        private readonly object typefaceLock = new();
        private readonly List<SKTypeface> ownedTypefaces = [];
        private readonly Dictionary<string, IList<SKTypeface>> registeredTypefaces = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Looks a family up in the system fonts (replaceable for tests, e.g. to simulate a system without fonts).
        /// </summary>
        /// <remarks>The native library is loaded on the first lookup, not in a static initializer, so a missing
        /// <c>libSkiaSharp</c> does not fail the type initializer of <see cref="FontManager"/>.</remarks>
        internal Func<string, SKFontStyle, SKTypeface> SystemTypefaceMatcher { get; set; } =
            (familyName, fontStyle) => SKFontManager.Default.MatchFamily(familyName, fontStyle);

        /// <summary>
        /// Gets or sets the list of font family names used as the primary font stack for rendering text.
        /// </summary>
        /// <remarks>The font stack is ordered by preference; the first available font in the list will be
        /// used. Some fonts, such as 'Arial' or 'Helvetica', require that a Narrow or Condensed typeface
        /// is installed on the system for proper rendering.</remarks>
        public List<string> FontStack0 { get; set; } = [
            "Swis721 Cn BT",
            "TeX Gyre Heros Cn",
            "Nimbus Sans Narrow",
            "Roboto Condensed",
            "Helvetica",
            "Helvetica Neue",
            "Arial",
        ];

        /// <summary>
        /// Gets or sets the list of font family names used as the primary monospace font stack.
        /// </summary>
        /// <remarks>The order of font names determines the fallback sequence when rendering text. Modify
        /// this list to customize the preferred monospace fonts for your application.</remarks>
        public List<string> FontStackA { get; set; } = [
            "DejaVu Sans Mono",
            "Lucida Console",
            "Andale Mono",
            "Droid Sans Mono",
        ];

        /// <summary>
        /// Gets or sets the delegate used to load a font by name and return an SKTypeface instance.
        /// </summary>
        /// <remarks>The delegate should accept a font name as a string and return the corresponding
        /// SKTypeface. This property allows customization of font loading behavior, such as loading fonts from embedded
        /// resources or external files.</remarks>
        public Func<string, SKTypeface> FontLoader { get; set; }

        private SKTypeface typeface0;
        internal SKTypeface Typeface0 {
            get {
                lock (this.typefaceLock)
                {
                    this.ThrowIfDisposed();
                    if (this.typeface0 == null)
                    {
                        using SKFontStyle fontStyle0 = new(SKFontStyleWeight.Bold, SKFontStyleWidth.SemiCondensed, SKFontStyleSlant.Upright);
                        this.typeface0 = this.GetDefaultTypeface(this.FontStack0, fontStyle0);
                    }

                    return this.typeface0;
                }
            }
        }

        private SKTypeface typefaceA;
        internal SKTypeface TypefaceA {
            get {
                lock (this.typefaceLock)
                {
                    this.ThrowIfDisposed();
                    if (this.typefaceA == null)
                    {
                        using SKFontStyle fontStyleA = new(SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                        this.typefaceA = this.GetDefaultTypeface(this.FontStackA, fontStyleA);
                    }

                    return this.typefaceA;
                }
            }
        }

        private SKTypeface typefaceGS;

        /// <summary>Gets the typeface of the ZPL symbol font used by <c>^GS</c>. It is created on first access, owned by the manager and disposed by <see cref="Dispose"/>.</summary>
        /// <exception cref="ObjectDisposedException">The manager is disposed.</exception>
        public SKTypeface TypefaceGS
        {
            get
            {
                lock (this.typefaceLock)
                {
                    this.ThrowIfDisposed();
                    this.typefaceGS ??= SKTypeface.FromStream(new MemoryStream(Resources.ZplGS));
                    return this.typefaceGS;
                }
            }
        }

        private bool disposed;

        /// <summary>Initializes a new instance of the <see cref="FontManager"/> class; font <c>0</c> maps to the default typeface 0, every other font to the default typeface A.</summary>
        public FontManager() {
            this.FontLoader = (fontName) => {
                if (fontName == "0")
                {
                    return this.Typeface0;
                }

                return this.TypefaceA;
            };
        }

        /// <summary>
        /// Registers the specified typeface for use within the font manager, making it available for font selection by
        /// family name.
        /// </summary>
        /// <param name="typeface">The typeface to register. Cannot be null. The typeface's family name is used as the
        /// key for registration.</param>
        /// <exception cref="ObjectDisposedException">The manager is disposed.</exception>
        public void RegisterTypeface(SKTypeface typeface)
        {
            lock (this.typefaceLock)
            {
                this.ThrowIfDisposed();
                this.AddRegisteredTypeface(typeface);
            }
        }

        /// <summary>Disposes the typefaces owned by the manager (the system default typefaces and <see cref="TypefaceGS"/>). Registered typefaces are not disposed. The manager must not be used afterwards: accessing a typeface or registering one throws an <see cref="ObjectDisposedException"/>. Calling <see cref="Dispose"/> again has no effect.</summary>
        public void Dispose()
        {
            lock (this.typefaceLock)
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
                this.typefaceGS?.Dispose();
                this.typefaceGS = null;
                foreach (SKTypeface typeface in this.ownedTypefaces)
                {
                    typeface.Dispose();
                }

                this.ownedTypefaces.Clear();
                this.typeface0 = null;
                this.typefaceA = null;
            }

            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(this.disposed, this);
        }

        private void AddRegisteredTypeface(SKTypeface typeface)
        {
            if (this.registeredTypefaces.TryGetValue(typeface.FamilyName, out IList<SKTypeface> typefaces))
            {
                typefaces.Add(typeface);
            } else {
                this.registeredTypefaces[typeface.FamilyName] = [typeface];
            }
        }

        private SKTypeface GetDefaultTypeface(List<string> fontStack, SKFontStyle fontStyle)
        {
            foreach (string fontFamily in fontStack)
            {
                SKTypeface typeface = this.GetTypeFaceByFamily(fontFamily, fontStyle);
                if (typeface != null)
                {
                    return typeface;
                }
            }

            return this.MatchSystemTypeface(SKTypeface.Default.FamilyName, fontStyle) ?? SKTypeface.Default;
        }

        private SKTypeface GetTypeFaceByFamily(string familyName, SKFontStyle fontStyle)
        {
            if (this.registeredTypefaces.TryGetValue(familyName, out IList<SKTypeface> typefaces))
            {
                return typefaces
                    // group italic and oblique together
                    .OrderBy(t => Math.Abs(Convert.ToInt32(t.FontStyle.Slant != SKFontStyleSlant.Upright) - Convert.ToInt32(fontStyle.Slant != SKFontStyleSlant.Upright)))
                    .ThenBy(t => Math.Abs(t.FontStyle.Width - fontStyle.Width))
                    .ThenBy(t => Math.Abs(t.FontStyle.Weight - fontStyle.Weight))
                    .First();
            }

            return this.MatchSystemTypeface(familyName, fontStyle);
        }

        /// <summary>A typeface matched in the system fonts is a new object that the manager owns.</summary>
        private SKTypeface MatchSystemTypeface(string familyName, SKFontStyle fontStyle)
        {
            SKTypeface typeface = this.SystemTypefaceMatcher(familyName, fontStyle);
            if (typeface != null)
            {
                this.ownedTypefaces.Add(typeface);
            }

            return typeface;
        }

    }
}
