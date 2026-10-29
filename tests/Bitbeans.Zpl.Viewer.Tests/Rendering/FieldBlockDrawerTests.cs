using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class FieldBlockDrawerTests
    {
        private static DrawerOptions Bitmap(params string[] fonts)
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider(fonts.Length == 0 ? ["A"] : fonts);
            return options;
        }

        /// <summary>
        /// Field block with the 5x9 test bitmap font "A" without expansion. Every glyph is a 5x9 box, advancing 6 pixels.
        /// </summary>
        private static string Fb(int width, int maxLines, int lineSpace, string justification, int indent, string text, string orientation = "N", string pos = "^FO50,50")
        {
            return $"^XA{pos}^AA{orientation},9,5^FB{width},{maxLines},{lineSpace},{justification},{indent}^FD{text}^FS^XZ";
        }

        private static SKBitmap Render(string zpl, DrawerOptions options = null, int dpmm = RenderHelper.Dpmm)
        {
            return RenderHelper.Render(zpl, options ?? Bitmap(), 100, 100, dpmm);
        }

        private static void AssertRect(int left, int top, int right, int bottom, SKRectI actual, int tolerance = 0, string message = null)
        {
            Assert.IsTrue(
                Math.Abs(actual.Left - left) <= tolerance &&
                Math.Abs(actual.Top - top) <= tolerance &&
                Math.Abs(actual.Right - right) <= tolerance &&
                Math.Abs(actual.Bottom - bottom) <= tolerance,
                $"{message}: expected [{left},{top},{right},{bottom}] but was {actual}");
        }

        private static SKRectI Band(SKBitmap bmp, int index)
        {
            (int top, int bottom) = RenderHelper.RowBands(bmp)[index];
            return RenderHelper.DarkBounds(bmp, 0, top, int.MaxValue, bottom).Value;
        }

        // ---------- bitmap font ----------

        [TestMethod]
        public void Bitmap_WordsAreWrappedAtTheBlockWidth()
        {
            using SKBitmap bmp = Render(Fb(60, 3, 3, "L", 0, "AAAA BBBB CCCC"));
            List<(int Top, int Bottom)> bands = RenderHelper.RowBands(bmp);
            Assert.AreEqual(2, bands.Count);
            AssertRect(50, 50, 103, 59, Band(bmp, 0), message: "line 1");
            AssertRect(50, 62, 73, 71, Band(bmp, 1), message: "line 2");
        }

        [TestMethod]
        public void Bitmap_MaximumLineCountLimitsTheLines()
        {
            // five lines of text, only two lines fit: the others are overprinted on the last line
            using SKBitmap bmp = Render(Fb(30, 2, 3, "L", 0, "AAAA BBBB CCCC DDDD EEEE"));
            Assert.AreEqual(2, RenderHelper.RowBands(bmp).Count);
        }

        [TestMethod]
        public void Bitmap_ZeroMaximumLineCountBehavesLikeOneLine()
        {
            ZplFieldBlock block = new("AAAA BBBB", 50, 50, 30, new ZplFont(5, 9, "A"), maxLineCount: 0);
            using SKBitmap bmp = RenderHelper.RenderElements([block], Bitmap(), 100, 100);
            Assert.AreEqual(1, RenderHelper.RowBands(bmp).Count);
        }

        [TestMethod]
        public void Bitmap_LineSpacingMovesTheLines()
        {
            using SKBitmap bmp = Render(Fb(30, 2, 10, "L", 0, "AAAA BBBB"));
            List<(int Top, int Bottom)> bands = RenderHelper.RowBands(bmp);
            Assert.AreEqual(2, bands.Count);
            Assert.AreEqual(50, bands[0].Top);
            Assert.AreEqual(50 + 9 + 10, bands[1].Top);
        }

        [TestMethod]
        public void Bitmap_ZplNewLineStartsANewLine()
        {
            using SKBitmap bmp = Render(Fb(60, 3, 3, "L", 0, "AA\\&BB"));
            Assert.AreEqual(2, RenderHelper.RowBands(bmp).Count);
            AssertRect(50, 62, 61, 71, Band(bmp, 1), message: "second line");

            // a new line directly after a word that already filled the line
            using SKBitmap afterWord = Render(Fb(60, 4, 3, "L", 0, "AAAA BBBBBB\\&C"));
            Assert.AreEqual(3, RenderHelper.RowBands(afterWord).Count);

            // a too wide first word in front of a new line
            using SKBitmap tooWide = Render(Fb(60, 3, 3, "L", 0, "BBBBBBBBBBBBB\\&C"));
            Assert.AreEqual(2, RenderHelper.RowBands(tooWide).Count);
            // clipped to the block
            Assert.AreEqual(109, Band(tooWide, 0).Right);
        }

        [TestMethod]
        public void Bitmap_LongWordsAreSplitWithAHyphen()
        {
            // 20 glyphs do not fit in the 60 px: 9 glyphs and a hyphen per line
            using SKBitmap bmp = Render(Fb(60, 5, 3, "L", 0, "AAAAAAAAAAAAAAAAAAAA"));
            List<(int Top, int Bottom)> bands = RenderHelper.RowBands(bmp);
            Assert.AreEqual(3, bands.Count);
            AssertRect(50, 50, 109, 59, Band(bmp, 0), message: "full line");
            AssertRect(50, 74, 61, 83, Band(bmp, 2), message: "rest");

            // after text in the same line: the earlier line is flushed first
            using SKBitmap after = Render(Fb(60, 6, 3, "L", 0, "CC AAAAAAAAAAAAAAAAAAAA"));
            Assert.AreEqual(4, RenderHelper.RowBands(after).Count);

            // the last part of the word shares its line with the following word
            using SKBitmap before = Render(Fb(60, 6, 3, "L", 0, "AAAAAAAAAAAAAAAAAAAA BB"));
            Assert.AreEqual(3, RenderHelper.RowBands(before).Count);
            AssertRect(50, 74, 50 + 6 * 2 + 6 + 11, 83, Band(before, 2), message: "AA BB");
        }

        [TestMethod]
        public void Bitmap_VeryNarrowBlocksSplitAfterEveryCharacter()
        {
            // not even a character with a hyphen fits in 8 px
            using SKBitmap narrow = Render(Fb(8, 10, 3, "L", 0, "AAA"));
            Assert.AreEqual(3, RenderHelper.RowBands(narrow).Count);

            // not even a single character fits in 3 px: one character is placed anyway
            using SKBitmap tiny = Render(Fb(3, 10, 3, "L", 0, "AA"));
            Assert.AreEqual(2, RenderHelper.RowBands(tiny).Count);
        }

        [TestMethod]
        public void Bitmap_TextJustification()
        {
            // line 2 is "CCCC": 24 px wide in a 60 px block
            using SKBitmap center = Render(Fb(60, 3, 3, "C", 0, "AAAA BBBB CCCC"));
            Assert.AreEqual(50 + 18, Band(center, 1).Left);

            using SKBitmap right = Render(Fb(60, 3, 3, "R", 0, "AAAA BBBB CCCC"));
            Assert.AreEqual(50 + 36, Band(right, 1).Left);
            // first line "AAAA BBBB" is 54 px wide
            Assert.AreEqual(50 + 6, Band(right, 0).Left);

            // justified lines (except the last) fill the whole width
            using SKBitmap justified = Render(Fb(60, 3, 3, "J", 0, "AAAA BBBB CCCC"));
            Assert.AreEqual(50, Band(justified, 0).Left);
            Assert.AreEqual(50 + 59, Band(justified, 0).Right);
            Assert.AreEqual(50, Band(justified, 1).Left);
            Assert.AreEqual(50 + 23, Band(justified, 1).Right);
        }

        [TestMethod]
        public void Bitmap_HangingIndentMovesFollowingLines()
        {
            using SKBitmap left = Render(Fb(60, 3, 3, "L", 10, "AAAA BBBB CCCC"));
            Assert.AreEqual(50, Band(left, 0).Left);
            Assert.AreEqual(60, Band(left, 1).Left);

            // right justified lines move to the left
            using SKBitmap right = Render(Fb(60, 3, 3, "R", 10, "AAAA BBBB CCCC"));
            Assert.AreEqual(50 + 36 - 10, Band(right, 1).Left);
        }

        [TestMethod]
        public void Bitmap_JustifiedLinesWithSingleWordsAndNarrowSpaceAreDrawnPlainly()
        {
            // the flushed line "CC" is only one word: nothing to justify
            using SKBitmap singleWord = Render(Fb(60, 5, 3, "J", 0, "CC AAAAAAAAAAAAAAAAAAAA"));
            Assert.AreEqual(50, Band(singleWord, 0).Left);
            Assert.AreEqual(50 + 11, Band(singleWord, 0).Right);

            // the indented second line has less room than its words need
            using SKBitmap tight = Render(Fb(60, 3, 3, "J", 20, "AAAA BBBB CCCC DDDD EEEE"));
            Assert.IsTrue(RenderHelper.RowBands(tight).Count >= 2);
            Assert.AreEqual(50 + 20, Band(tight, 1).Left);
        }

        [TestMethod]
        public void Bitmap_JustifiedLineSkipsEmptyWords()
        {
            // two spaces in a row create an empty word in the middle of the justified line
            using SKBitmap bmp = Render(Fb(80, 3, 3, "J", 0, "AA  BB CCCCCC DDDDDD"));
            Assert.IsTrue(RenderHelper.RowBands(bmp).Count >= 2);
            Assert.AreEqual(50, Band(bmp, 0).Left);
        }

        [TestMethod]
        public void Bitmap_FieldTypesetStartsAboveTheBaseline()
        {
            using SKBitmap bmp = Render(Fb(60, 2, 3, "L", 0, "AAAA BBBB", pos: "^FT50,100"));
            // (max - 1) * line height + baseline (7)
            Assert.AreEqual(100 - (12 + 7), Band(bmp, 0).Top);
        }

        [TestMethod]
        public void Bitmap_Orientations()
        {
            // "AA" is 11 px wide, one line is 9 px high; the block is rotated around different centers
            (string Orientation, int Left, int Top, int Right, int Bottom)[] expected =
            [
                ("N", 50, 50, 61, 59),
                ("R", 50, 50, 59, 61),
                ("I", 99, 50, 110, 59),
                ("B", 50, 99, 59, 110),
            ];
            foreach ((string orientation, int left, int top, int right, int bottom) in expected)
            {
                using SKBitmap bmp = Render(Fb(60, 1, 0, "L", 0, "AA", orientation));
                AssertRect(left, top, right, bottom, RenderHelper.DarkBounds(bmp).Value, 1, orientation);
            }
        }

        [TestMethod]
        public void Bitmap_OrientationsWithFieldTypeset()
        {
            foreach (string orientation in new[] { "N", "R", "I", "B" })
            {
                using SKBitmap bmp = Render(Fb(60, 1, 0, "L", 0, "AA", orientation, "^FT100,100"));
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), orientation);
            }
        }

        [TestMethod]
        public void Bitmap_ReversePrintInvertsTheBackground()
        {
            using SKBitmap bmp = Render("^XA^FO40,40^GB100,40,40^FS^FO50,50^FR^AAN,9,5^FB60,1,0,L,0^FDA^FS^XZ");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 50, 55));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 52, 55));
        }

        [TestMethod]
        public void Bitmap_HexEscapesAreReplaced()
        {
            using SKBitmap escaped = Render("^XA^FO50,50^AAN,9,5^FB60,1,0,L,0^FH^FDA_42^FS^XZ");
            using SKBitmap plain = Render("^XA^FO50,50^AAN,9,5^FB60,1,0,L,0^FDAB^FS^XZ");
            Assert.IsTrue(RenderHelper.SameImage(escaped, plain));
        }

        [TestMethod]
        public void Bitmap_DefaultPositionFollowsThePreviousElement()
        {
            ZplFont font = new(5, 9, "A");
            ZplFieldBlock first = new("AA", 50, 50, 60, font, 1);
            ZplFieldBlock second = (ZplFieldBlock)RenderHelper.WithDefaultPosition(new ZplFieldBlock("B", 0, 0, 60, font, 1));
            using SKBitmap bmp = RenderHelper.RenderElements([first, second], Bitmap(), 100, 100);
            // next default position: x + block width (60), y + block height (9)
            SKRectI? bounds = RenderHelper.DarkBounds(bmp, 110, 59);
            Assert.IsNotNull(bounds);
            Assert.AreEqual(110, bounds.Value.Left);
            Assert.AreEqual(59, bounds.Value.Top);
        }

        [TestMethod]
        public void Bitmap_EmptyTextDrawsNothing()
        {
            ZplFieldBlock block = new("", 50, 50, 60, new ZplFont(5, 9, "A"));
            using SKBitmap bmp = RenderHelper.RenderElements([block], Bitmap(), 100, 100);
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Bitmap_UnsupportedProviderOrFontFallsBackToTheOutline()
        {
            using SKBitmap outline = RenderHelper.Render("^XA^FO50,50^ACN,30,15^FB200,2,0,L,0^FDHello world^FS^XZ", null, 100, 100);

            // font 0 style names never use bitmap fonts
            DrawerOptions options = Bitmap("A", "0");
            using SKBitmap font0 = RenderHelper.Render("^XA^FO50,50^A0N,30,15^FB200,2,0,L,0^FDHello world^FS^XZ", options, 100, 100);
            Assert.IsTrue(RenderHelper.DarkBounds(font0).Value.Height > 15);

            // no data for the font and no data for font A
            using SKBitmap noData = RenderHelper.Render("^XA^FO50,50^ACN,30,15^FB200,2,0,L,0^FDHello world^FS^XZ", Bitmap("B"), 100, 100);
            Assert.IsTrue(RenderHelper.SameImage(outline, noData));

            // no provider at all
            DrawerOptions none = RenderHelper.Opaque();
            none.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            none.BitmapFontProvider = null;
            using SKBitmap noProvider = RenderHelper.Render("^XA^FO50,50^ACN,30,15^FB200,2,0,L,0^FDHello world^FS^XZ", none, 100, 100);
            Assert.IsTrue(RenderHelper.SameImage(outline, noProvider));
        }

        [TestMethod]
        public void Bitmap_FontWithoutDataUsesFontA()
        {
            TestBitmapFontProvider provider = new("A");
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = provider;
            using SKBitmap bmp = RenderHelper.Render("^XA^FO50,50^ABN,9,5^FB60,1,0,L,0^FDAA^FS^XZ", options, 100, 100);
            AssertRect(50, 50, 61, 59, RenderHelper.DarkBounds(bmp).Value, message: "font A glyphs");
            CollectionAssert.AreEqual(new[] { "B", "A" }, provider.Requests);
        }

        [TestMethod]
        public void Bitmap_ExpansionIsAppliedToGlyphsAndLines()
        {
            // expansion 2: glyphs are 10x18 and the lines are 18 + 4 apart
            using SKBitmap bmp = RenderHelper.Render("^XA^FO50,50^AAN,18,10^FB100,2,4,L,0^FDAA BB CC DD EE FF GG^FS^XZ", Bitmap(), 100, 100);
            List<(int Top, int Bottom)> bands = RenderHelper.RowBands(bmp);
            Assert.AreEqual(2, bands.Count);
            Assert.AreEqual(18, bands[0].Bottom - bands[0].Top);
            Assert.AreEqual(50 + 18 + 4, bands[1].Top);
        }

        // ---------- outline font ----------

        private static SKBitmap RenderOutline(string zpl)
        {
            return RenderHelper.Render(zpl, null, 100, 100);
        }

        private static string OutlineFb(int width, int maxLines, int lineSpace, string justification, int indent, string text, string orientation = "N", string pos = "^FO100,100")
        {
            return $"^XA{pos}^A0{orientation},30,30^FB{width},{maxLines},{lineSpace},{justification},{indent}^FD{text}^FS^XZ";
        }

        private const string LongText = "The quick brown fox jumps over the lazy dog and runs away";

        [TestMethod]
        public void Outline_TextIsWrappedIntoMultipleLines()
        {
            using SKBitmap one = RenderOutline(OutlineFb(500, 1, 0, "L", 0, "Short text"));
            using SKBitmap many = RenderOutline(OutlineFb(200, 6, 5, "L", 0, LongText));
            Assert.AreEqual(1, RenderHelper.RowBands(one).Count);
            Assert.IsTrue(RenderHelper.RowBands(many).Count >= 3);
            // the block never exceeds its width
            SKRectI bounds = RenderHelper.DarkBounds(many).Value;
            Assert.IsTrue(bounds.Left >= 98 && bounds.Right <= 301, bounds.ToString());
        }

        [TestMethod]
        public void Outline_MaximumLineCountAndLineSpacing()
        {
            using SKBitmap limited = RenderOutline(OutlineFb(200, 2, 10, "L", 0, LongText));
            Assert.AreEqual(2, RenderHelper.RowBands(limited).Count);

            using SKBitmap spaced = RenderOutline(OutlineFb(200, 2, 40, "L", 0, LongText));
            List<(int Top, int Bottom)> a = RenderHelper.RowBands(limited);
            List<(int Top, int Bottom)> b = RenderHelper.RowBands(spaced);
            Assert.AreEqual(a[1].Top - a[0].Top + 30, b[1].Top - b[0].Top, 3);

            ZplFieldBlock zero = new("AAA BBB CCC DDD", 100, 100, 100, new ZplFont(30, 30, "0"), maxLineCount: 0);
            using SKBitmap bmp = RenderHelper.RenderElements([zero], null, 100, 100);
            Assert.IsTrue(RenderHelper.RowBands(bmp).Count <= 2);
        }

        [TestMethod]
        public void Outline_Justification()
        {
            using SKBitmap center = RenderOutline(OutlineFb(300, 3, 5, "C", 0, "Center me please"));
            SKRectI c = RenderHelper.DarkBounds(center).Value;
            Assert.IsTrue(Math.Abs((c.Left + c.Right) / 2 - 250) <= 10, c.ToString());

            using SKBitmap right = RenderOutline(OutlineFb(300, 3, 5, "R", 0, "Right"));
            SKRectI r = RenderHelper.DarkBounds(right).Value;
            Assert.IsTrue(Math.Abs(r.Right - 400) <= 10, r.ToString());

            using SKBitmap justified = RenderOutline(OutlineFb(300, 3, 5, "J", 0, LongText));
            SKRectI first = Band(justified, 0);
            Assert.IsTrue(Math.Abs(first.Right - 400) <= 12, first.ToString());
            Assert.IsTrue(Math.Abs(first.Left - 100) <= 6, first.ToString());
        }

        [TestMethod]
        public void Outline_HangingIndent()
        {
            using SKBitmap left = RenderOutline(OutlineFb(200, 4, 5, "L", 30, LongText));
            Assert.IsTrue(Band(left, 1).Left >= Band(left, 0).Left + 25);

            using SKBitmap right = RenderOutline(OutlineFb(200, 4, 5, "R", 30, LongText));
            Assert.IsTrue(RenderHelper.RowBands(right).Count >= 2);
        }

        [TestMethod]
        public void Outline_NewLinesAndLongWords()
        {
            using SKBitmap newLine = RenderOutline(OutlineFb(300, 4, 5, "L", 0, "One\\&Two\\&Three"));
            Assert.AreEqual(3, RenderHelper.RowBands(newLine).Count);

            // a new line after a word that fits / does not fit the line
            using SKBitmap afterFit = RenderOutline(OutlineFb(200, 4, 5, "L", 0, "Mix of words\\&Next"));
            Assert.IsTrue(RenderHelper.RowBands(afterFit).Count >= 2);
            using SKBitmap afterNoFit = RenderOutline(OutlineFb(200, 5, 5, "L", 0, "Some words and thenwordwiththenewline\\&Next"));
            Assert.IsTrue(RenderHelper.RowBands(afterNoFit).Count >= 3);
            using SKBitmap firstNoFit = RenderOutline(OutlineFb(100, 4, 5, "L", 0, "Wordwhichistoowidetofit\\&Next"));
            Assert.IsTrue(RenderHelper.RowBands(firstNoFit).Count >= 2);

            // words wider than the block are cut
            using SKBitmap longWord = RenderOutline(OutlineFb(150, 6, 5, "L", 0, "Supercalifragilisticexpialidocious"));
            Assert.IsTrue(RenderHelper.RowBands(longWord).Count >= 3);
            using SKBitmap afterText = RenderOutline(OutlineFb(150, 6, 5, "L", 0, "Hi Supercalifragilisticexpialidocious end"));
            Assert.IsTrue(RenderHelper.RowBands(afterText).Count >= 3);
            using SKBitmap tiny = RenderOutline(OutlineFb(2, 6, 5, "L", 0, "WW"));
            Assert.IsNotNull(tiny);
        }

        [TestMethod]
        public void Outline_JustifiedLinesWithSingleWordsAreDrawnPlainly()
        {
            using SKBitmap bmp = RenderOutline(OutlineFb(200, 6, 5, "J", 0, "Hi Supercalifragilisticexpialidocious"));
            Assert.IsTrue(RenderHelper.RowBands(bmp).Count >= 2);

            // not enough room for the justification of the indented line
            using SKBitmap tight = RenderOutline(OutlineFb(200, 6, 5, "J", 150, LongText));
            Assert.IsTrue(RenderHelper.RowBands(tight).Count >= 2);

            // empty words in a justified line
            using SKBitmap spaces = RenderOutline(OutlineFb(300, 6, 5, "J", 0, "a  b  c d e f g h i j k l m n o p q r s t u v w x y z"));
            Assert.IsTrue(RenderHelper.RowBands(spaces).Count >= 1);
        }

        [TestMethod]
        public void Outline_FieldTypesetPlacesTheBlockAboveTheBaseline()
        {
            using SKBitmap bmp = RenderOutline(OutlineFb(300, 2, 10, "L", 0, "First line of text here that wraps around to the second line", pos: "^FT100,300"));
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(bounds.Bottom <= 305, bounds.ToString());
            Assert.IsTrue(bounds.Top < 300 - 30);
        }

        [TestMethod]
        public void Outline_Orientations()
        {
            using SKBitmap normal = RenderOutline(OutlineFb(300, 1, 0, "L", 0, "Rotate"));
            SKRectI n = RenderHelper.DarkBounds(normal).Value;
            foreach (string orientation in new[] { "R", "I", "B" })
            {
                using SKBitmap bmp = RenderOutline(OutlineFb(300, 1, 0, "L", 0, "Rotate", orientation));
                SKRectI b = RenderHelper.DarkBounds(bmp).Value;
                if (orientation == "I")
                {
                    Assert.IsTrue(Math.Abs(b.Width - n.Width) <= 2, orientation);
                    Assert.IsTrue(Math.Abs(b.Height - n.Height) <= 2, orientation);
                }
                else
                {
                    Assert.IsTrue(Math.Abs(b.Width - n.Height) <= 2, orientation);
                    Assert.IsTrue(Math.Abs(b.Height - n.Width) <= 2, orientation);
                }
            }

            foreach (string orientation in new[] { "N", "R", "I", "B" })
            {
                using SKBitmap bmp = RenderOutline(OutlineFb(300, 1, 0, "L", 0, "Rotate", orientation, "^FT200,200"));
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), orientation);
            }
        }

        [TestMethod]
        public void Outline_ReversePrintAndReplacements()
        {
            using SKBitmap reversed = RenderOutline("^XA^FO90,90^GB250,60,60^FS^FO100,100^FR^A0N,30,30^FB200,1,0,L,0^FDReverse^FS^XZ");
            Assert.IsTrue(RenderHelper.IsDark(reversed, 95, 95));
            Assert.IsTrue(RenderHelper.CountDark(reversed, 100, 100, 300, 140) < 200 * 40);

            using SKBitmap dash = RenderOutline(OutlineFb(300, 1, 0, "L", 0, "A-B"));
            DrawerOptions noDash = RenderHelper.Opaque();
            noDash.ReplaceDashWithEnDash = false;
            using SKBitmap plain = RenderHelper.Render(OutlineFb(300, 1, 0, "L", 0, "A-B"), noDash, 100, 100);
            Assert.IsTrue(RenderHelper.DarkBounds(dash).Value.Width > RenderHelper.DarkBounds(plain).Value.Width);

            DrawerOptions underscore = RenderHelper.Opaque();
            underscore.ReplaceUnderscoreWithEnSpace = true;
            using SKBitmap replaced = RenderHelper.Render(OutlineFb(300, 1, 0, "L", 0, "A_B"), underscore, 100, 100);
            using SKBitmap unreplaced = RenderOutline(OutlineFb(300, 1, 0, "L", 0, "A_B"));
            Assert.IsTrue(RenderHelper.CountDark(replaced) < RenderHelper.CountDark(unreplaced));

            // fonts other than 0 are untouched
            using SKBitmap fontA = RenderHelper.Render("^XA^FO100,100^AAN,30,30^FB300,1,0,L,0^FDA-B^FS^XZ", null, 100, 100);
            using SKBitmap fontANoDash = RenderHelper.Render("^XA^FO100,100^AAN,30,30^FB300,1,0,L,0^FDA-B^FS^XZ", noDash, 100, 100);
            Assert.IsTrue(RenderHelper.SameImage(fontA, fontANoDash));
        }

        [TestMethod]
        public void Outline_HexEscapesAndDefaultPosition()
        {
            using SKBitmap escaped = RenderOutline("^XA^FO100,100^A0N,30,30^FB300,1,0,L,0^FH^FDH_45LLO^FS^XZ");
            using SKBitmap plain = RenderOutline("^XA^FO100,100^A0N,30,30^FB300,1,0,L,0^FDHELLO^FS^XZ");
            Assert.IsTrue(RenderHelper.SameImage(escaped, plain));

            ZplFont font = new(30, 30, "0");
            ZplFieldBlock first = new("HELLO", 100, 100, 200, font, 1);
            ZplFieldBlock second = (ZplFieldBlock)RenderHelper.WithDefaultPosition(new ZplFieldBlock("WORLD", 0, 0, 200, font, 1));
            using SKBitmap both = RenderHelper.RenderElements([first, second], null, 100, 100);
            SKRectI all = RenderHelper.DarkBounds(both).Value;
            // the second block starts at the right of the first block
            Assert.IsTrue(all.Right > 300);
        }

        [TestMethod]
        public void FieldBlockDrawer_OnlyDrawsFieldBlocks()
        {
            FieldBlockElementDrawer drawer = new();
            ZplFont font = new(10, 10, "0");
            Assert.IsTrue(drawer.CanDraw(new ZplFieldBlock("A", 0, 0, 10, font)));
            Assert.IsFalse(drawer.CanDraw(new ZplTextField("A", 0, 0, font)));
            Assert.IsFalse(drawer.IsReverseDraw(new ZplTextField("A", 0, 0, font, reversePrint: true)));
            Assert.IsTrue(drawer.IsReverseDraw(new ZplFieldBlock("A", 0, 0, 10, font, reversePrint: true)));
            Assert.IsFalse(drawer.IsReverseDraw(new ZplFieldBlock("A", 0, 0, 10, font)));

            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                drawer.Prepare(new PrinterStorage(), canvas);
                SKPoint position = new(3, 4);
                Assert.AreEqual(position, drawer.Draw(new ZplTextField("A", 0, 0, font), new DrawerOptions(), position, InternationalFont.ZCP850, 8));
            }
        }

        [TestMethod]
        public void Bitmap_SeveralLongWordsInOneBlockAreAllSplit()
        {
            using SKBitmap bmp = Render(Fb(60, 10, 3, "L", 0, "AAAAAAAAAAAAAAAAAAAA BBBBBBBBBBBBBBBBBBBB"));

            // every long word needs more than two lines of ten characters, the hyphens end the lines
            Assert.IsTrue(RenderHelper.RowBands(bmp).Count >= 4);
            Assert.AreEqual(50, RenderHelper.DarkBounds(bmp).Value.Left);
        }

        [TestMethod]
        public void Outline_SeveralLongWordsInOneBlockAreAllSplit()
        {
            string longWord = new('W', 12);
            using SKBitmap one = RenderHelper.Render($"^XA^FO20,20^A0N,30,30^FB100,20,10,L,0^FD{longWord}^FS^XZ");
            using SKBitmap two = RenderHelper.Render($"^XA^FO20,20^A0N,30,30^FB100,20,10,L,0^FD{longWord} {longWord}^FS^XZ");

            int oneBands = RenderHelper.RowBands(one).Count;
            int twoBands = RenderHelper.RowBands(two).Count;
            Assert.IsTrue(oneBands >= 3, oneBands.ToString());
            Assert.IsTrue(twoBands >= 2 * oneBands - 1, $"{oneBands} {twoBands}");
        }

        [TestMethod]
        public void Bitmap_WordThatFitsOnlyWithoutTheTrailingGapStartsItsOwnLine()
        {
            // "AAA" is 17 pixels wide when drawn (3 * 6 - 1), but its advance width is 18
            using SKBitmap bmp = Render(Fb(17, 3, 3, "L", 0, "AAA"));

            Assert.AreEqual(new SKRectI(50, 50, 67, 59), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void Bitmap_FontExpansionFollowsHeightAndWidth()
        {
            // test font: 5x9 cell, advance 6; "AB" is 11 + 6 expanded
            (int height, int width, SKRectI expected)[] cases =
            [
                (18, 0, new SKRectI(50, 50, 72, 68)),
                (0, 10, new SKRectI(50, 50, 72, 68)),
                (18, 10, new SKRectI(50, 50, 72, 68)),
                (27, 15, new SKRectI(50, 50, 83, 77)),
                (9, 15, new SKRectI(50, 50, 83, 59)),
                (0, 0, new SKRectI(50, 50, 61, 59)),
            ];

            foreach ((int height, int width, SKRectI expected) in cases)
            {
                using SKBitmap bmp = Render($"^XA^FO50,50^AAN,{height},{width}^FB200,3,0,L,0^FDAB^FS^XZ");

                Assert.AreEqual(expected, RenderHelper.DarkBounds(bmp).Value, $"{height}x{width}");
            }
        }
    }
}
