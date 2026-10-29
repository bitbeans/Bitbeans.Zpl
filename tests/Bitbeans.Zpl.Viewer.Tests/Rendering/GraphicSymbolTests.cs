using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class GraphicSymbolTests
    {
        private static SKBitmap R(string body, DrawerOptions options = null)
        {
            return RenderHelper.Render("^XA" + body + "^XZ", options);
        }

        private static DrawerOptions Bitmap(params string[] fonts)
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider(fonts);
            return options;
        }

        // ---------- outline (typeface) rendering ----------

        [TestMethod]
        public void Outline_DrawsSymbolAtOrigin()
        {
            using SKBitmap bmp = R("^FO100,100^GSN,60,60^FDA^FS");

            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(bounds.Left >= 98 && bounds.Left <= 130, $"left {bounds.Left}");
            Assert.IsTrue(bounds.Top >= 98 && bounds.Top < 160, $"top {bounds.Top}");
            Assert.IsTrue(bounds.Width > 20 && bounds.Height > 20, $"{bounds.Width}x{bounds.Height}");
            Assert.IsNull(RenderHelper.DarkBounds(bmp, 0, 0, 90, 400));
        }

        [TestMethod]
        public void Outline_DifferentSymbolsLookDifferent()
        {
            using SKBitmap registered = R("^FO100,100^GSN,60,60^FDA^FS");
            using SKBitmap copyright = R("^FO100,100^GSN,60,60^FDB^FS");
            using SKBitmap trademark = R("^FO100,100^GSN,60,60^FDC^FS");

            Assert.IsFalse(RenderHelper.SameImage(registered, copyright));
            Assert.IsFalse(RenderHelper.SameImage(copyright, trademark));
            Assert.IsFalse(RenderHelper.SameImage(registered, trademark));
        }

        [TestMethod]
        public void Outline_SizeFollowsHeightAndWidth()
        {
            using SKBitmap small = R("^FO100,100^GSN,30,30^FDA^FS");
            using SKBitmap large = R("^FO100,100^GSN,90,90^FDA^FS");
            using SKBitmap wide = R("^FO100,100^GSN,60,120^FDA^FS");
            using SKBitmap normal = R("^FO100,100^GSN,60,60^FDA^FS");

            SKRectI s = RenderHelper.DarkBounds(small).Value;
            SKRectI l = RenderHelper.DarkBounds(large).Value;
            SKRectI w = RenderHelper.DarkBounds(wide).Value;
            SKRectI n = RenderHelper.DarkBounds(normal).Value;
            Assert.IsTrue(s.Height < n.Height && n.Height < l.Height);
            Assert.IsTrue(s.Width < n.Width && n.Width < l.Width);
            Assert.IsTrue(w.Width > n.Width);
        }

        [TestMethod]
        public void Outline_RotationsSwapExtents()
        {
            SKRectI normal = RenderHelper.DarkBounds(R("^FO200,200^GSN,60,60^FDB^FS")).Value;
            SKRectI r90 = RenderHelper.DarkBounds(R("^FO200,200^GSR,60,60^FDB^FS")).Value;
            SKRectI r180 = RenderHelper.DarkBounds(R("^FO200,200^GSI,60,60^FDB^FS")).Value;
            SKRectI r270 = RenderHelper.DarkBounds(R("^FO200,200^GSB,60,60^FDB^FS")).Value;

            Assert.IsTrue(Math.Abs(r90.Width - normal.Height) <= 2, $"{r90} vs {normal}");
            Assert.IsTrue(Math.Abs(r90.Height - normal.Width) <= 2, $"{r90} vs {normal}");
            Assert.IsTrue(Math.Abs(r270.Width - normal.Height) <= 2, $"{r270} vs {normal}");
            Assert.IsTrue(Math.Abs(r270.Height - normal.Width) <= 2, $"{r270} vs {normal}");
            Assert.IsTrue(Math.Abs(r180.Width - normal.Width) <= 2, $"{r180} vs {normal}");
            Assert.IsTrue(Math.Abs(r180.Height - normal.Height) <= 2, $"{r180} vs {normal}");
        }

        [TestMethod]
        public void Outline_FieldTypesetRotationsSwapExtents()
        {
            SKRectI normal = RenderHelper.DarkBounds(R("^FT200,200^GSN,60,60^FDB^FS")).Value;
            SKRectI r90 = RenderHelper.DarkBounds(R("^FT200,200^GSR,60,60^FDB^FS")).Value;
            SKRectI r180 = RenderHelper.DarkBounds(R("^FT200,200^GSI,60,60^FDB^FS")).Value;
            SKRectI r270 = RenderHelper.DarkBounds(R("^FT200,200^GSB,60,60^FDB^FS")).Value;

            Assert.IsTrue(Math.Abs(r90.Width - normal.Height) <= 2, $"{r90} vs {normal}");
            Assert.IsTrue(Math.Abs(r270.Width - normal.Height) <= 2, $"{r270} vs {normal}");
            Assert.IsTrue(Math.Abs(r180.Width - normal.Width) <= 2, $"{r180} vs {normal}");
            // field typeset anchors the baseline at y: the normal symbol sits above it
            Assert.IsTrue(normal.Bottom <= 202, $"{normal}");
            // rotated by 90 degrees the symbol extends to the left of x, rotated by 270 to the right
            Assert.IsTrue(r90.Right >= 195, $"{r90}");
            Assert.IsTrue(r270.Left <= 205, $"{r270}");
        }

        [TestMethod]
        public void Outline_RightJustifiedEndsAtX()
        {
            // the analyzer does not hand the justification to graphic symbols, so set it on the elements
            SKRectI left = RenderHelper.DarkBounds(RenderHelper.RenderElements(
                [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 200, 100, 60, 60, fieldJustification: FieldJustification.Left)])).Value;
            SKRectI right = RenderHelper.DarkBounds(RenderHelper.RenderElements(
                [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 200, 100, 60, 60, fieldJustification: FieldJustification.Right)])).Value;

            Assert.IsTrue(left.Left >= 198, $"{left}");
            Assert.IsTrue(right.Right <= 202, $"{right}");
            Assert.AreEqual(left.Width, right.Width, 2);
        }

        [TestMethod]
        public void Outline_FieldTypesetRightJustifiedEndsAtX()
        {
            SKRectI left = RenderHelper.DarkBounds(RenderHelper.RenderElements(
                [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 200, 200, 60, 60, bottomToTop: true, fieldJustification: FieldJustification.Left)])).Value;
            SKRectI right = RenderHelper.DarkBounds(RenderHelper.RenderElements(
                [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 200, 200, 60, 60, bottomToTop: true, fieldJustification: FieldJustification.Right)])).Value;

            Assert.IsTrue(left.Left >= 198, $"{left}");
            Assert.IsTrue(right.Right <= 202, $"{right}");
        }

        [TestMethod]
        public void Outline_UsesDefaultPositionAfterPreviousSymbol()
        {
            ZplElementBase[] elements =
            [
                new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 50, 50, 60, 60),
                RenderHelper.WithDefaultPosition(new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 0, 0, 60, 60)),
            ];

            using SKBitmap bmp = RenderHelper.RenderElements(elements);
            using SKBitmap single = RenderHelper.RenderElements([elements[0]]);

            SKRectI first = RenderHelper.DarkBounds(single).Value;
            SKRectI all = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(all.Right > first.Right + 10, $"{all} vs {first}");
            Assert.AreEqual(first.Left, all.Left);
        }

        [TestMethod]
        public void Outline_NullCharacterDrawsNothing()
        {
            ZplElementBase[] elements = [new ZplGraphicSymbol((GraphicSymbolCharacter?)null, 50, 50, 60, 60)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements);

            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Outline_TextConstructorTakesFirstCharacter()
        {
            ZplGraphicSymbol symbol = new("C", 1, 2, 3, 4);
            ZplGraphicSymbol empty = new("", 1, 2, 3, 4);

            Assert.AreEqual(GraphicSymbolCharacter.TradeMark, symbol.Character);
            Assert.IsNull(empty.Character);
        }

        [TestMethod]
        public void Outline_UnsupportedElementIsIgnored()
        {
            GraphicSymbolElementDrawer drawer = new();
            ZplImageMove other = new(1, 2, 'R', "LOGO.GRF", string.Empty);

            Assert.IsFalse(drawer.CanDraw(other));
            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                drawer.Prepare(new PrinterStorage(), canvas);
                SKPoint current = new(3, 4);
                Assert.AreEqual(current, drawer.Draw(other, new DrawerOptions(), current, InternationalFont.ZCP850, 8));
            }
        }

        // ---------- bitmap rendering ----------

        [TestMethod]
        public void Bitmap_DrawsExpandedGlyphAtOrigin()
        {
            using SKBitmap bmp = R("^FO20,20^GSN,18,10^FDA^FS", Bitmap("GS"));

            // the test glyph is a 5x9 frame, expanded 2x2 to 10x18
            Assert.AreEqual(new SKRectI(20, 20, 30, 38), RenderHelper.DarkBounds(bmp).Value);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 20, 30));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 29, 30));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 25, 30));
        }

        [TestMethod]
        public void Bitmap_ExpansionDefaultsAreDerivedFromTheGivenSize()
        {
            // height only: the width expansion follows the height
            using SKBitmap heightOnly = R("^FO20,20^GSN,18,0^FDA^FS", Bitmap("GS"));
            // width only: the height expansion follows the width
            using SKBitmap widthOnly = R("^FO20,20^GSN,0,10^FDA^FS", Bitmap("GS"));
            // nothing: no expansion
            using SKBitmap none = R("^FO20,20^GSN,0,0^FDA^FS", Bitmap("GS"));

            Assert.AreEqual(new SKRectI(20, 20, 30, 38), RenderHelper.DarkBounds(heightOnly).Value);
            Assert.AreEqual(new SKRectI(20, 20, 30, 38), RenderHelper.DarkBounds(widthOnly).Value);
            Assert.AreEqual(new SKRectI(20, 20, 25, 29), RenderHelper.DarkBounds(none).Value);
        }

        [TestMethod]
        public void Bitmap_WidthAndHeightExpandIndependently()
        {
            using SKBitmap bmp = R("^FO20,20^GSN,27,15^FDA^FS", Bitmap("GS"));

            Assert.AreEqual(new SKRectI(20, 20, 35, 47), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void Bitmap_FieldTypesetAnchorsBaselineAtY()
        {
            using SKBitmap bmp = R("^FT20,100^GSN,18,10^FDA^FS", Bitmap("GS"));

            // top = y - baseline(7) * expansion(2)
            Assert.AreEqual(new SKRectI(20, 86, 30, 104), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void Bitmap_RightJustifiedEndsAtX()
        {
            ZplElementBase[] origin = [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 100, 20, 10, 18, fieldJustification: FieldJustification.Right)];
            ZplElementBase[] typeset = [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 100, 60, 10, 18, bottomToTop: true, fieldJustification: FieldJustification.Right)];

            using SKBitmap bmpOrigin = RenderHelper.RenderElements(origin, Bitmap("GS"));
            using SKBitmap bmpTypeset = RenderHelper.RenderElements(typeset, Bitmap("GS"));

            // total width = advance (5 + 1) * 2 = 12
            Assert.AreEqual(new SKRectI(88, 20, 98, 38), RenderHelper.DarkBounds(bmpOrigin).Value);
            Assert.AreEqual(new SKRectI(88, 46, 98, 64), RenderHelper.DarkBounds(bmpTypeset).Value);
        }

        [TestMethod]
        public void Bitmap_FieldOriginRotations()
        {
            using SKBitmap r90 = R("^FO20,20^GSR,18,10^FDA^FS", Bitmap("GS"));
            using SKBitmap r180 = R("^FO20,20^GSI,18,10^FDA^FS", Bitmap("GS"));
            using SKBitmap r270 = R("^FO20,20^GSB,18,10^FDA^FS", Bitmap("GS"));

            Assert.AreEqual(new SKRectI(20, 20, 38, 30), RenderHelper.DarkBounds(r90).Value);
            Assert.AreEqual(new SKRectI(22, 20, 32, 38), RenderHelper.DarkBounds(r180).Value);
            Assert.AreEqual(new SKRectI(20, 22, 38, 32), RenderHelper.DarkBounds(r270).Value);
        }

        [TestMethod]
        public void Bitmap_FieldTypesetRotations()
        {
            using SKBitmap r90 = R("^FT50,100^GSR,18,10^FDA^FS", Bitmap("GS"));
            using SKBitmap r180 = R("^FT50,100^GSI,18,10^FDA^FS", Bitmap("GS"));
            using SKBitmap r270 = R("^FT50,100^GSB,18,10^FDA^FS", Bitmap("GS"));

            Assert.AreEqual(new SKRectI(46, 100, 64, 110), RenderHelper.DarkBounds(r90).Value);
            Assert.AreEqual(new SKRectI(40, 96, 50, 114), RenderHelper.DarkBounds(r180).Value);
            Assert.AreEqual(new SKRectI(36, 90, 54, 100), RenderHelper.DarkBounds(r270).Value);
        }

        [TestMethod]
        public void Bitmap_UsesDefaultPositionAfterPreviousSymbol()
        {
            ZplElementBase[] elements =
            [
                new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 20, 20, 10, 18),
                RenderHelper.WithDefaultPosition(new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 0, 0, 10, 18)),
                new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 100, 100, 10, 18, bottomToTop: true),
                RenderHelper.WithDefaultPosition(new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 0, 0, 10, 18, bottomToTop: true)),
            ];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, Bitmap("GS"));

            // field origin: the next symbol starts right of the first one (advance 12) and below it (height 18)
            Assert.AreEqual(new SKRectI(20, 20, 42, 56), RenderHelper.DarkBounds(bmp, 0, 0, 80, 80).Value);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 32, 38));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 41, 55));
            // field typeset: the next symbol is at the same y (12 to the right), its baseline anchored there
            Assert.AreEqual(new SKRectI(100, 86, 122, 104), RenderHelper.DarkBounds(bmp, 80, 80).Value);
        }

        [TestMethod]
        public void Bitmap_MissingGlyphAdvancesWithoutDrawing()
        {
            // a font without glyphs: only the advance is applied, nothing is drawn
            ZplElementBase[] elements = [new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 20, 20, 10, 18)];
            DrawerOptions options = Bitmap("GS");
            options.BitmapFontProvider = new EmptyGlyphProvider();

            using SKBitmap bmp = RenderHelper.RenderElements(elements, options);

            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Bitmap_FallsBackToOutlineWhenFontIsMissing()
        {
            TestBitmapFontProvider provider = new("A");
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = provider;

            using SKBitmap bmp = R("^FO100,100^GSN,60,60^FDA^FS", options);
            using SKBitmap outline = R("^FO100,100^GSN,60,60^FDA^FS");

            CollectionAssert.Contains(provider.Requests, "GS");
            Assert.IsTrue(RenderHelper.SameImage(outline, bmp));
        }

        [TestMethod]
        public void Bitmap_FallsBackToOutlineWithoutProvider()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = null;

            using SKBitmap bmp = R("^FO100,100^GSN,60,60^FDA^FS", options);
            using SKBitmap outline = R("^FO100,100^GSN,60,60^FDA^FS");

            Assert.IsTrue(RenderHelper.SameImage(outline, bmp));
        }

        [TestMethod]
        public void OutlineMode_IgnoresTheBitmapProvider()
        {
            TestBitmapFontProvider provider = new("GS");
            DrawerOptions options = RenderHelper.Opaque();
            options.BitmapFontProvider = provider;

            using SKBitmap bmp = R("^FO100,100^GSN,60,60^FDA^FS", options);

            Assert.AreEqual(0, provider.Requests.Count);
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));
        }

        private sealed class EmptyGlyphProvider : Bitbeans.Zpl.Viewer.BitmapFonts.IZplBitmapFontProvider
        {
            public bool TryGet(string fontName, int printDensityDpmm, InternationalFont internationalFont, out Bitbeans.Zpl.Viewer.BitmapFonts.ZplBitmapFontData font)
            {
                Bitbeans.Zpl.Viewer.BitmapFonts.ZplBitmapFontMetrics metrics = new(fontName, 5, 9, 1, 7);
                font = new Bitbeans.Zpl.Viewer.BitmapFonts.ZplBitmapFontData(
                    metrics,
                    new System.Collections.Generic.Dictionary<int, Bitbeans.Zpl.Viewer.BitmapFonts.ZplBitmapGlyph>());
                return true;
            }
        }
    }
}
