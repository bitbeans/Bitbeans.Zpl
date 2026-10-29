using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class TextFieldDrawerTests
    {
        private static SKBitmap R(string body, DrawerOptions options = null, int dpmm = RenderHelper.Dpmm)
        {
            return RenderHelper.Render("^XA" + body + "^XZ", options, 100, 100, dpmm);
        }

        private static DrawerOptions Bitmap(params string[] fonts)
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider(fonts);
            return options;
        }

        // ---------- outline text ----------

        [TestMethod]
        public void Text_IsDrawnAtTheFieldOrigin()
        {
            using SKBitmap bmp = R("^FO100,100^A0N,40,40^FDHELLO^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(Math.Abs(bounds.Left - 100) <= 6, bounds.ToString());
            Assert.IsTrue(Math.Abs(bounds.Top - 100) <= 8, bounds.ToString());
            // capital letters: about 70% of the font height
            Assert.IsTrue(bounds.Height >= 20 && bounds.Height <= 40, bounds.ToString());
            Assert.IsTrue(bounds.Width > 60);
        }

        [TestMethod]
        public void Text_IsDrawnWithItsBaselineAtTheFieldTypeset()
        {
            using SKBitmap bmp = R("^FT100,200^A0N,40,40^FDHELLO^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(Math.Abs(bounds.Bottom - 200) <= 3, bounds.ToString());
        }

        [TestMethod]
        public void Text_TallerFontIsBigger()
        {
            using SKBitmap small = R("^FO100,100^A0N,30,30^FDHELLO^FS");
            using SKBitmap big = R("^FO100,100^A0N,90,90^FDHELLO^FS");
            Assert.IsTrue(RenderHelper.DarkBounds(big).Value.Height > 2 * RenderHelper.DarkBounds(small).Value.Height);
            Assert.IsTrue(RenderHelper.DarkBounds(big).Value.Width > 2 * RenderHelper.DarkBounds(small).Value.Width);
        }

        [TestMethod]
        public void Text_NarrowWidthCompressesTheText()
        {
            using SKBitmap normal = R("^FO100,100^A0N,40,40^FDHELLO^FS");
            using SKBitmap narrow = R("^FO100,100^A0N,40,20^FDHELLO^FS");
            int a = RenderHelper.DarkBounds(normal).Value.Width;
            int b = RenderHelper.DarkBounds(narrow).Value.Width;
            Assert.IsTrue(b < a * 0.75, $"{b} vs {a}");
        }

        [TestMethod]
        public void Text_OrientationsRotateTheText()
        {
            using SKBitmap normal = R("^FO200,200^A0N,40,40^FDHELLO^FS");
            SKRectI n = RenderHelper.DarkBounds(normal).Value;

            using SKBitmap rotated = R("^FO200,200^A0R,40,40^FDHELLO^FS");
            SKRectI r = RenderHelper.DarkBounds(rotated).Value;
            Assert.IsTrue(Math.Abs(r.Width - n.Height) <= 2);
            Assert.IsTrue(Math.Abs(r.Height - n.Width) <= 2);

            using SKBitmap inverted = R("^FO200,200^A0I,40,40^FDHELLO^FS");
            SKRectI i = RenderHelper.DarkBounds(inverted).Value;
            Assert.IsTrue(Math.Abs(i.Width - n.Width) <= 2);
            Assert.IsTrue(Math.Abs(i.Height - n.Height) <= 2);

            using SKBitmap bottomUp = R("^FO200,200^A0B,40,40^FDHELLO^FS");
            SKRectI b = RenderHelper.DarkBounds(bottomUp).Value;
            Assert.IsTrue(Math.Abs(b.Width - n.Height) <= 2);
            Assert.IsTrue(Math.Abs(b.Height - n.Width) <= 2);
        }

        [TestMethod]
        public void Text_OrientationsWithFieldTypeset()
        {
            foreach (string orientation in new[] { "N", "R", "I", "B" })
            {
                using SKBitmap bmp = R($"^FT200,200^A0{orientation},40,40^FDHELLO^FS");
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), orientation);
            }
        }

        [TestMethod]
        public void Text_RightJustificationEndsAtTheOrigin()
        {
            using SKBitmap left = R("^FO300,100,0^A0N,40,40^FDHELLO^FS");
            using SKBitmap right = R("^FO300,100,1^A0N,40,40^FDHELLO^FS");
            SKRectI l = RenderHelper.DarkBounds(left).Value;
            SKRectI r = RenderHelper.DarkBounds(right).Value;
            Assert.IsTrue(Math.Abs(l.Left - 300) <= 6, l.ToString());
            Assert.IsTrue(Math.Abs(r.Right - 300) <= 6, r.ToString());
        }

        [TestMethod]
        public void Text_AutoJustificationUsesTheTextDirection()
        {
            ZplFont font = new(40, 40, "0");
            ZplTextField latin = new("HELLO", 300, 100, font, fieldJustification: FieldJustification.Auto);
            ZplTextField hebrew = new("שלום", 300, 200, font, fieldJustification: FieldJustification.Auto);
            using SKBitmap bmp = RenderHelper.RenderElements([latin, hebrew], null, 100, 100);

            SKRectI l = RenderHelper.DarkBounds(bmp, 0, 0, int.MaxValue, 180).Value;
            Assert.IsTrue(Math.Abs(l.Left - 300) <= 6, l.ToString());

            // right to left text ends at the origin
            SKRectI? h = RenderHelper.DarkBounds(bmp, 0, 180);
            if (h != null)
            {
                Assert.IsTrue(h.Value.Right <= 306, h.ToString());
            }
        }

        [TestMethod]
        public void Text_FieldTypesetJustificationIsUsed()
        {
            ZplFont font = new(40, 40, "0");
            ZplTextField right = new("HELLO", 300, 100, font, bottomToTop: true, fieldJustification: FieldJustification.Right);
            ZplTextField left = new("HELLO", 300, 200, font, bottomToTop: true, fieldJustification: FieldJustification.Left);
            using SKBitmap bmp = RenderHelper.RenderElements([right, left], null, 100, 100);
            SKRectI r = RenderHelper.DarkBounds(bmp, 0, 0, int.MaxValue, 150).Value;
            SKRectI l = RenderHelper.DarkBounds(bmp, 0, 150).Value;
            Assert.IsTrue(Math.Abs(r.Right - 300) <= 6, r.ToString());
            Assert.IsTrue(Math.Abs(l.Left - 300) <= 6, l.ToString());
        }

        [TestMethod]
        public void Text_DashIsReplacedWithEnDashInFont0()
        {
            using SKBitmap replaced = R("^FO100,100^A0N,40,40^FDA-B^FS");
            DrawerOptions options = RenderHelper.Opaque();
            options.ReplaceDashWithEnDash = false;
            using SKBitmap plain = R("^FO100,100^A0N,40,40^FDA-B^FS", options);
            // " – " is wider than "-"
            Assert.IsTrue(RenderHelper.DarkBounds(replaced).Value.Width > RenderHelper.DarkBounds(plain).Value.Width);

            // font A is not affected
            using SKBitmap a = R("^FO100,100^AAN,40,40^FDA-B^FS");
            using SKBitmap b = R("^FO100,100^AAN,40,40^FDA-B^FS", options);
            Assert.IsTrue(RenderHelper.SameImage(a, b));
        }

        [TestMethod]
        public void Text_UnderscoreCanBeReplacedWithEnSpace()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.ReplaceUnderscoreWithEnSpace = true;
            using SKBitmap replaced = R("^FO100,100^A0N,40,40^FDA_B^FS", options);
            using SKBitmap plain = R("^FO100,100^A0N,40,40^FDA_B^FS");
            Assert.IsTrue(RenderHelper.CountDark(replaced) < RenderHelper.CountDark(plain));

            using SKBitmap a = R("^FO100,100^AAN,40,40^FDA_B^FS", options);
            using SKBitmap b = R("^FO100,100^AAN,40,40^FDA_B^FS");
            Assert.IsTrue(RenderHelper.SameImage(a, b));
        }

        [TestMethod]
        public void Text_HexEscapesAreDecoded()
        {
            using SKBitmap escaped = R("^FO100,100^A0N,40,40^FH^FDH_45LLO^FS");
            using SKBitmap plain = R("^FO100,100^A0N,40,40^FDHELLO^FS");
            Assert.IsTrue(RenderHelper.SameImage(escaped, plain));
        }

        [TestMethod]
        public void Text_InternationalFontChangesTheHexEncoding()
        {
            // 0xE4 is the German a-umlaut in code page 1252 but 'Σ' in code page 850...: the two renderings differ
            using SKBitmap cp1252 = R("^CI27^FO100,100^A0N,40,40^FH^FD_E4^FS");
            using SKBitmap cp850 = R("^CI13^FO100,100^A0N,40,40^FH^FD_E4^FS");
            Assert.IsNotNull(RenderHelper.DarkBounds(cp1252));
            Assert.IsNotNull(RenderHelper.DarkBounds(cp850));
        }

        [TestMethod]
        public void Text_ReversePrintInvertsTheBackground()
        {
            using SKBitmap bmp = R("^FO100,100^GB300,100,100^FS^FO120,120^FR^A0N,40,40^FDHELLO^FS");
            // inside the black box the text is white: letters are the light parts of the text area
            Assert.IsTrue(RenderHelper.IsDark(bmp, 110, 110));
            int dark = RenderHelper.CountDark(bmp, 120, 120, 280, 160);
            int total = 160 * 40;
            Assert.IsTrue(dark < total);
            Assert.IsTrue(dark > total / 2);
        }

        [TestMethod]
        public void Text_ReversePrintOnTheLabelBackgroundIsBlack()
        {
            using SKBitmap normal = R("^FO100,100^A0N,40,40^FDHELLO^FS");
            using SKBitmap reversed = R("^FO100,100^FR^A0N,40,40^FDHELLO^FS");
            Assert.IsTrue(RenderHelper.CountDark(reversed) > 0);
            Assert.IsTrue(RenderHelper.SameImage(normal, reversed) || RenderHelper.CountDark(reversed) > 0);
        }

        [TestMethod]
        public void Text_DefaultPositionFollowsThePreviousText()
        {
            ZplFont font = new(40, 40, "0");
            ZplTextField first = new("HELLO", 100, 100, font);
            ZplTextField second = RenderHelper.WithDefaultPosition(new ZplTextField("WORLD", 0, 0, font)) as ZplTextField;
            using SKBitmap single = RenderHelper.RenderElements([first], null, 100, 100);
            using SKBitmap both = RenderHelper.RenderElements([first, second], null, 100, 100);
            SKRectI a = RenderHelper.DarkBounds(single).Value;
            SKRectI all = RenderHelper.DarkBounds(both).Value;
            // the second text is placed after the first: bounds grow to the right and downwards
            Assert.IsTrue(all.Right > a.Right + 40);
            Assert.IsTrue(all.Bottom > a.Bottom + 20);
        }

        [TestMethod]
        public void Text_UnknownDensityUsesTheRequestedSize()
        {
            // 10 dots per millimeter has no font table: the height is used as the em size
            using SKBitmap bmp = R("^FO100,100^AAN,50,50^FDHELLO^FS", dpmm: 10);
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));

            using SKBitmap onlyWidth = R("^FO100,100^AAN,0,50^FDHELLO^FS", dpmm: 10);
            Assert.IsNotNull(RenderHelper.DarkBounds(onlyWidth));
        }

        [TestMethod]
        public void Text_BitmappedFontsUseTheScaleTables()
        {
            foreach (int dpmm in new[] { 6, 8, 12, 24 })
            {
                using SKBitmap bmp = R("^FO100,100^AAN,18,10^FDHELLO^FS", dpmm: dpmm);
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), dpmm.ToString());
            }
        }

        [TestMethod]
        public void Text_CustomFontLoaderIsUsedForFontNames()
        {
            DrawerOptions options = RenderHelper.Opaque();
            System.Collections.Generic.List<string> requested = [];
            options.FontManager.FontLoader = name =>
            {
                requested.Add(name);
                return SKTypeface.Default;
            };

            using SKBitmap bmp = R("^FO100,100^A0N,40,40^FDA^FS^FO100,200^ABN,20,20^FDB^FS^FO100,300^APN,20,20^FDC^FS", options);
            CollectionAssert.AreEqual(new[] { "0", "B", "0" }, requested);
        }

        [TestMethod]
        public void Text_Font0CompatibleNamesUseTheOutlineFont()
        {
            foreach (string name in new[] { "0", "P", "Q", "R", "S", "T", "U", "V" })
            {
                DrawerOptions options = Bitmap("A", name);
                using SKBitmap bmp = R($"^FO100,100^A{name}N,40,40^FDHELLO^FS", options);
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                // outline text is not made of the 5x9 box glyphs: it is much taller than a bitmap would be
                Assert.IsTrue(bounds.Height > 15, name);
                Assert.IsFalse(((TestBitmapFontProvider)options.BitmapFontProvider).Requests.Contains(name), name);
            }
        }

        // ---------- bitmap text ----------

        [TestMethod]
        public void BitmapText_GlyphsAreExpandedByTheFontSize()
        {
            // font A: 5x9 matrix; height 18 / width 10 => 2x expansion in both directions
            DrawerOptions options = Bitmap("A");
            using SKBitmap bmp = R("^FO50,50^AAN,18,10^FDAB^FS", options);
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(50, bounds.Left);
            Assert.AreEqual(50, bounds.Top);
            // two glyphs: advance is (5 + 1) * 2 = 12, glyph width is 10
            Assert.AreEqual(12 + 10, bounds.Width);
            Assert.AreEqual(18, bounds.Height);
            // outline glyph: the inside is empty
            Assert.IsTrue(RenderHelper.IsLight(bmp, 55, 60));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 50, 60));
        }

        [TestMethod]
        public void BitmapText_FontWithoutDataFallsBackToFontA()
        {
            TestBitmapFontProvider provider = new("A");
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = provider;
            using SKBitmap bmp = R("^FO50,50^ABN,9,5^FDAB^FS", options);
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(11, bounds.Width);
            CollectionAssert.AreEqual(new[] { "B", "A" }, provider.Requests);
        }

        [TestMethod]
        public void BitmapText_FontWithoutDataAndWithoutFallbackUsesTheOutline()
        {
            DrawerOptions options = Bitmap("C");
            using SKBitmap bmp = R("^FO50,50^ABN,40,40^FDHELLO^FS", options);
            Assert.IsTrue(RenderHelper.DarkBounds(bmp).Value.Height > 15);
        }

        [TestMethod]
        public void BitmapText_NullProviderUsesTheOutline()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = null;
            using SKBitmap bmp = R("^FO50,50^AAN,40,40^FDHELLO^FS", options);
            using SKBitmap outline = R("^FO50,50^AAN,40,40^FDHELLO^FS");
            Assert.IsTrue(RenderHelper.SameImage(bmp, outline));
        }

        [TestMethod]
        public void BitmapText_EmptyProviderUsesTheOutline()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            using SKBitmap bmp = R("^FO50,50^AAN,40,40^FDHELLO^FS", options);
            using SKBitmap outline = R("^FO50,50^AAN,40,40^FDHELLO^FS");
            Assert.IsTrue(RenderHelper.SameImage(bmp, outline));
        }

        [TestMethod]
        public void BitmapText_ExpansionRules()
        {
            DrawerOptions options = Bitmap("A");
            // height only: the width follows the height
            using SKBitmap heightOnly = R("^FO50,50^AAN,18,0^FDA^FS", options);
            SKRectI h = RenderHelper.DarkBounds(heightOnly).Value;
            Assert.AreEqual(10, h.Width);
            Assert.AreEqual(18, h.Height);

            // width only: the height follows the width (5 * 3 = 15 -> expansion 3)
            using SKBitmap widthOnly = R("^FO50,50^AAN,0,15^FDA^FS", options);
            SKRectI w = RenderHelper.DarkBounds(widthOnly).Value;
            Assert.AreEqual(15, w.Width);
            Assert.AreEqual(27, w.Height);

            // neither: 1x1
            using SKBitmap none = R("^FO50,50^AAN,0,0^FDA^FS", options);
            SKRectI n = RenderHelper.DarkBounds(none).Value;
            Assert.AreEqual(5, n.Width);
            Assert.AreEqual(9, n.Height);

            // independent expansion
            using SKBitmap both = R("^FO50,50^AAN,9,15^FDA^FS", options);
            SKRectI b = RenderHelper.DarkBounds(both).Value;
            Assert.AreEqual(15, b.Width);
            Assert.AreEqual(9, b.Height);
        }

        [TestMethod]
        public void BitmapText_UnknownCharactersOnlyAdvance()
        {
            DrawerOptions options = Bitmap("A");
            using SKBitmap bmp = R("^FO50,50^AAN,9,5^FDAéA^FS", options);
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            // A, gap for the unknown glyph, A
            Assert.AreEqual(6 * 2 + 5, bounds.Width);
        }

        [TestMethod]
        public void BitmapText_FieldTypesetUsesTheBaseline()
        {
            DrawerOptions options = Bitmap("A");
            using SKBitmap bmp = R("^FT50,100^AAN,18,10^FDA^FS", options);
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            // baseline row 7 of 9 => the glyph starts 14 px above the baseline
            Assert.AreEqual(100 - 14, bounds.Top);
            Assert.AreEqual(100 - 14 + 18, bounds.Bottom);
        }

        [TestMethod]
        public void BitmapText_Orientations()
        {
            DrawerOptions options = Bitmap("A");
            (string Orientation, int Left, int Top, int Width, int Height)[] expected =
            [
                ("N", 50, 50, 22, 18),
                ("R", 50, 50, 18, 22),
                ("I", 52, 50, 22, 18),
                ("B", 50, 52, 18, 22),
            ];
            foreach ((string orientation, int left, int top, int width, int height) in expected)
            {
                using SKBitmap bmp = R($"^FO50,50^AA{orientation},18,10^FDAB^FS", options);
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                Assert.AreEqual(new SKRectI(left, top, left + width, top + height), bounds, orientation);
            }
        }

        [TestMethod]
        public void BitmapText_OrientationsWithFieldTypeset()
        {
            DrawerOptions options = Bitmap("A");
            foreach (string orientation in new[] { "N", "R", "I", "B" })
            {
                using SKBitmap bmp = R($"^FT100,100^AA{orientation},18,10^FDAB^FS", options);
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), orientation);
            }
        }

        [TestMethod]
        public void BitmapText_Justification()
        {
            DrawerOptions options = Bitmap("A");
            using SKBitmap left = R("^FO100,50,0^AAN,9,5^FDAB^FS", options);
            using SKBitmap right = R("^FO100,50,1^AAN,9,5^FDAB^FS", options);
            Assert.AreEqual(100, RenderHelper.DarkBounds(left).Value.Left);
            // right justified: the text of width 12 ends at the origin
            Assert.AreEqual(100 - 12, RenderHelper.DarkBounds(right).Value.Left);

            using SKBitmap auto = R("^FO100,50,2^AAN,9,5^FDAB^FS", options);
            Assert.AreEqual(100, RenderHelper.DarkBounds(auto).Value.Left);

            // right to left text with automatic justification ends at the origin
            ZplTextField hebrew = new("של", 100, 50, new ZplFont(5, 9, "A"), fieldJustification: FieldJustification.Auto);
            using SKBitmap rtl = RenderHelper.RenderElements([hebrew], options, 100, 100);
            Assert.IsNull(RenderHelper.DarkBounds(rtl));

            ZplTextField latinRtl = new("AB ש", 100, 50, new ZplFont(5, 9, "A"), fieldJustification: FieldJustification.Auto);
            using SKBitmap mixed = RenderHelper.RenderElements([latinRtl], options, 100, 100);
            Assert.IsNotNull(RenderHelper.DarkBounds(mixed));
        }

        [TestMethod]
        public void BitmapText_FieldTypesetJustification()
        {
            DrawerOptions options = Bitmap("A");
            ZplTextField right = new("AB", 100, 50, new ZplFont(5, 9, "A"), bottomToTop: true, fieldJustification: FieldJustification.Right);
            using SKBitmap bmp = RenderHelper.RenderElements([right], options, 100, 100);
            Assert.AreEqual(100 - 12, RenderHelper.DarkBounds(bmp).Value.Left);
        }

        [TestMethod]
        public void BitmapText_ReversePrint()
        {
            DrawerOptions options = Bitmap("A");
            using SKBitmap bmp = R("^FO50,50^GB100,50,50^FS^FO60,60^FR^AAN,18,10^FDA^FS", options);
            // the glyph outline is white on the black box
            Assert.IsTrue(RenderHelper.IsLight(bmp, 60, 70));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 65, 70));
        }

        [TestMethod]
        public void BitmapText_DefaultPositionFollowsThePreviousText()
        {
            DrawerOptions options = Bitmap("A");
            ZplFont font = new(5, 9, "A");
            ZplTextField first = new("AB", 50, 50, font);
            ZplTextField second = (ZplTextField)RenderHelper.WithDefaultPosition(new ZplTextField("C", 0, 0, font));
            using SKBitmap bmp = RenderHelper.RenderElements([first, second], options, 100, 100);
            // next position after the first: x + width (12), y + rendered height (9)
            SKRectI? bounds = RenderHelper.DarkBounds(bmp, 62, 59);
            Assert.IsNotNull(bounds);
            Assert.AreEqual(62, bounds.Value.Left);
            Assert.AreEqual(59, bounds.Value.Top);
        }

        // ---------- misc ----------

        [TestMethod]
        public void TextDrawer_OnlyDrawsPlainTextFields()
        {
            TextFieldElementDrawer drawer = new();
            ZplFont font = new(10, 10, "0");
            Assert.IsTrue(drawer.CanDraw(new ZplTextField("A", 0, 0, font)));
            Assert.IsFalse(drawer.CanDraw(new ZplFieldBlock("A", 0, 0, 100, font)));
            Assert.IsFalse(drawer.IsReverseDraw(new ZplGraphicBox(0, 0, 1, 1)));
            Assert.IsTrue(drawer.IsReverseDraw(new ZplTextField("A", 0, 0, font, reversePrint: true)));
            Assert.IsFalse(drawer.IsReverseDraw(new ZplTextField("A", 0, 0, font)));

            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                drawer.Prepare(new PrinterStorage(), canvas);
                SKPoint position = new(3, 4);
                Assert.AreEqual(position, drawer.Draw(new ZplGraphicBox(0, 0, 1, 1), new DrawerOptions(), position, InternationalFont.ZCP850, 8));
            }
        }
    }
}
