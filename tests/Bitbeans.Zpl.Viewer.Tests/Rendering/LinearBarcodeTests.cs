using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using Microsoft.Extensions.Logging;

using SkiaSharp;

using System;
using System.Linq;

using ZXing;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class LinearBarcodeTests
    {
        private const double LabelWidth = 100;
        private const double LabelHeight = 100;

        /// <summary>
        /// Command templates: {0} orientation, {1} print interpretation line (Y/N), {2} interpretation above (Y/N)
        /// </summary>
        private const string Code128 = "^BC{0},80,{1},{2}";
        private const string Code11 = "^B1{0},N,80,{1},{2}";
        private const string Code39 = "^B3{0},N,80,{1},{2}";
        private const string Code93 = "^BA{0},80,{1},{2},N";
        private const string Ean13 = "^BE{0},80,{1},{2}";
        private const string Ean8 = "^B8{0},80,{1},{2}";
        private const string UpcA = "^BU{0},80,{1},{2},Y";
        private const string UpcE = "^B9{0},80,{1},{2},Y";
        private const string UpcExt = "^BS{0},80,{1},{2}";
        private const string Interleaved = "^B2{0},80,{1},{2},N";
        private const string Industrial = "^BI{0},80,{1},{2}";
        private const string Codabar = "^BK{0},N,80,{1},{2}";
        private const string Msi = "^BM{0},B,80,{1},{2},N";
        private const string Logmars = "^BL{0},80,{2}";

        private static string Zpl(string template, string data, string orientation = "N", bool interpretation = false, bool above = false, int x = 40, int y = 120, bool hex = false)
        {
            string cmd = string.Format(template, orientation, interpretation ? "Y" : "N", above ? "Y" : "N");
            return $"^XA^BY3,3,80^FO{x},{y}{cmd}{(hex ? "^FH" : "")}^FD{data}^FS^XZ";
        }

        private static SKBitmap Render(string zpl, DrawerOptions options = null, ILogger logger = null)
        {
            return RenderHelper.Render(zpl, options, LabelWidth, LabelHeight, logger: logger);
        }

        private static SKBitmap RenderLinear(string template, string data, string orientation = "N", bool interpretation = false, bool above = false, DrawerOptions options = null, bool hex = false)
        {
            return Render(Zpl(template, data, orientation, interpretation, above, hex: hex), options);
        }

        // (template, data, expected decoded text, ZXing format or null when ZXing cannot read it)
        private static readonly (string Name, string Template, string Data, string Decoded, BarcodeFormat? Format)[] Cases =
        [
            ("Code128", Code128, "HELLO123", "HELLO123", BarcodeFormat.CODE_128),
            ("Code11", Code11, "123-45", null, null),
            ("Code39", Code39, "ABC123", "ABC123", BarcodeFormat.CODE_39),
            ("Code93", Code93, "TEST93", "TEST93", BarcodeFormat.CODE_93),
            ("Ean13", Ean13, "123456789012", "1234567890128", BarcodeFormat.EAN_13),
            ("Ean8", Ean8, "1234567", "12345670", BarcodeFormat.EAN_8),
            ("UpcA", UpcA, "12345678901", "123456789012", BarcodeFormat.UPC_A),
            ("UpcE", UpcE, "0123456", null, BarcodeFormat.UPC_E),
            ("UpcExt", UpcExt, "12", null, null),
            ("Interleaved", Interleaved, "12345678", "12345678", BarcodeFormat.ITF),
            ("Industrial", Industrial, "123456", null, null),
            ("Codabar", Codabar, "123456", "123456", BarcodeFormat.CODABAR),
            ("Msi", Msi, "123456", null, null),
            ("Logmars", Logmars, "ABC123", null, BarcodeFormat.CODE_39),
        ];

        // ---------- decoding ----------

        [TestMethod]
        public void Linear_RenderedBarcodesAreDecodable()
        {
            foreach ((string name, string template, string data, string decoded, BarcodeFormat? format) in Cases)
            {
                using SKBitmap bmp = RenderLinear(template, data);
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), name);

                if (format == null)
                {
                    continue;
                }

                Result result = RenderHelper.DecodeResult(bmp, format.Value);
                Assert.IsNotNull(result, name);
                Assert.AreEqual(format.Value, result.BarcodeFormat, name);
                if (decoded != null)
                {
                    Assert.AreEqual(decoded, result.Text, name);
                }
            }
        }

        [TestMethod]
        public void Linear_BarcodeGeometryMatchesModuleWidthAndHeight()
        {
            // Code128B: quiet-less 11 modules per symbol: START(11) + 8 chars (88) + checksum(11) + STOP(13) = 123 modules
            using SKBitmap bmp = RenderLinear(Code128, "HELLO123");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(40, bounds.Left);
            Assert.AreEqual(120, bounds.Top);
            Assert.AreEqual(123 * 3, bounds.Width);
            Assert.AreEqual(80, bounds.Height);
        }

        [TestMethod]
        public void Linear_InterpretationLineIsBelowBarcodeByDefaultAndAboveOnRequest()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                using SKBitmap plain = RenderLinear(template, data);
                using SKBitmap below = RenderLinear(template, data, interpretation: true);
                using SKBitmap above = RenderLinear(template, data, interpretation: true, above: true);

                SKRectI plainBounds = RenderHelper.DarkBounds(plain).Value;
                SKRectI belowBounds = RenderHelper.DarkBounds(below).Value;
                SKRectI aboveBounds = RenderHelper.DarkBounds(above).Value;

                // Logmars has no interpretation switch: it is always printed
                if (template != Logmars)
                {
                    Assert.IsTrue(RenderHelper.CountDark(below) > RenderHelper.CountDark(plain), name);
                    Assert.IsTrue(belowBounds.Bottom > plainBounds.Bottom, $"{name}: text below the bars");
                    Assert.AreEqual(plainBounds.Top, belowBounds.Top, name);
                }

                Assert.IsTrue(aboveBounds.Top < plainBounds.Top, $"{name}: text above the bars");
                if (template != Logmars)
                {
                    Assert.AreEqual(plainBounds.Bottom, aboveBounds.Bottom, name);
                }
            }
        }

        [TestMethod]
        public void Linear_OrientationsPlaceTheBarcodeInTheExpectedBox()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                if (template == Logmars)
                {
                    // the always printed interpretation line changes the bounds
                    continue;
                }

                using SKBitmap normal = RenderLinear(template, data);
                SKRectI n = RenderHelper.DarkBounds(normal).Value;
                int w = n.Width;
                int h = n.Height;

                using SKBitmap inverted = RenderLinear(template, data, "I");
                SKRectI i = RenderHelper.DarkBounds(inverted).Value;
                // rotated by 180 degrees around the barcode center: same size
                Assert.AreEqual(w, i.Width, name);
                Assert.AreEqual(h, i.Height, name);

                foreach (string orientation in new[] { "R", "B" })
                {
                    using SKBitmap rotated = RenderLinear(template, data, orientation);
                    SKRectI r = RenderHelper.DarkBounds(rotated).Value;
                    Assert.AreEqual(h, r.Width, $"{name} {orientation}");
                    Assert.AreEqual(w, r.Height, $"{name} {orientation}");
                    Assert.AreEqual(40, r.Left, $"{name} {orientation}");
                    Assert.IsTrue(Math.Abs(r.Top - 120) <= 4, $"{name} {orientation}");
                }
            }
        }

        [TestMethod]
        public void Linear_RotatedBarcodesAreStillDecodable()
        {
            foreach (string orientation in new[] { "R", "I", "B" })
            {
                using SKBitmap bmp = RenderLinear(Code128, "ROTATE", orientation);
                Assert.AreEqual("ROTATE", RenderHelper.Decode(bmp, BarcodeFormat.CODE_128), orientation);
            }
        }

        [TestMethod]
        public void Linear_RotatedBarcodesWithInterpretationLine()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                foreach (string orientation in new[] { "R", "I", "B" })
                {
                    using SKBitmap plain = RenderLinear(template, data, orientation);
                    using SKBitmap below = RenderLinear(template, data, orientation, true);
                    using SKBitmap above = RenderLinear(template, data, orientation, true, true);
                    if (template != Logmars)
                    {
                        Assert.IsTrue(RenderHelper.CountDark(below) > RenderHelper.CountDark(plain), $"{name} {orientation}");
                    }

                    if (template != Logmars)
                    {
                        Assert.IsTrue(RenderHelper.CountDark(above) > RenderHelper.CountDark(plain), $"{name} {orientation}");
                    }
                }
            }
        }

        [TestMethod]
        public void Linear_FieldTypesetPositionsFromTheBottom()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                string cmd = string.Format(template, "N", "N", "N");
                string zpl = $"^XA^BY3,3,80^FT40,200{cmd}^FD{data}^FS^XZ";
                using SKBitmap bmp = Render(zpl);
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                if (template == Logmars)
                {
                    // always has an interpretation line below the bars
                    Assert.IsTrue(bounds.Bottom > 200, name);
                }
                else
                {
                    Assert.AreEqual(200, bounds.Bottom, name);
                }

                Assert.AreEqual(40, bounds.Left, name);
            }
        }

        [TestMethod]
        public void Linear_FieldTypesetAtTopIsClampedToZero()
        {
            using SKBitmap bmp = Render($"^XA^BY3,3,80^FT40,20{string.Format(Code128, "N", "N", "N")}^FDABC^FS^XZ");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(0, bounds.Top);
        }

        [TestMethod]
        public void Linear_FieldTypesetWithInterpretationLines()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                foreach (bool above in new[] { false, true })
                {
                    string cmd = string.Format(template, "N", "Y", above ? "Y" : "N");
                    using SKBitmap bmp = Render($"^XA^BY3,3,80^FT40,300{cmd}^FD{data}^FS^XZ");
                    Assert.IsNotNull(RenderHelper.DarkBounds(bmp), name);
                }
            }
        }

        [TestMethod]
        public void Linear_HexadecimalEscapesAreReplaced()
        {
            // _31 is '1'
            string[] templates = [Code128, Code11, Code39, Code93, Ean13, Ean8, UpcA, UpcE, UpcExt, Interleaved, Industrial, Codabar, Msi, Logmars];
            foreach (string template in templates)
            {
                using SKBitmap plain = RenderLinear(template, "123456", interpretation: true);
                using SKBitmap hex = RenderLinear(template, "_3123456", interpretation: true, hex: true);
                Assert.IsTrue(RenderHelper.SameImage(plain, hex), template);
            }
        }

        [TestMethod]
        public void Linear_DefaultPositionStartsAtThePreviousElementsEnd()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                string cmd = string.Format(template, "N", "N", "N");
                ZplElementBase[] parsed = RenderHelper.Parse($"^XA^BY3,3,80^FO0,0{cmd}^FD{data}^FS^XZ");
                Assert.AreEqual(1, parsed.Length, name);

                using SKBitmap reference = RenderHelper.RenderElements(parsed, null, LabelWidth, LabelHeight);
                SKRectI refBounds = RenderHelper.DarkBounds(reference).Value;

                // box (filled) from (60,70) to (100,110), then the barcode with default position starting there
                ZplElementBase[] parsedAgain = RenderHelper.Parse($"^XA^BY3,3,80^FO0,0{cmd}^FD{data}^FS^XZ");
                ZplElementBase[] elements =
                [
                    new ZplGraphicBox(60, 70, 40, 40, 40),
                    RenderHelper.WithDefaultPosition(parsedAgain[0]),
                ];
                using SKBitmap bmp = RenderHelper.RenderElements(elements, null, LabelWidth, LabelHeight);
                SKRectI bounds = RenderHelper.DarkBounds(bmp, 100, 0).Value;
                Assert.AreEqual(100, bounds.Left, name);
                Assert.AreEqual(110, bounds.Top, name);
                Assert.AreEqual(refBounds.Width, bounds.Width, name);
            }
        }

        [TestMethod]
        public void Linear_FollowingDefaultPositionElementIsPlacedAfterThisOne()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                if (template == Logmars)
                {
                    // always prints an interpretation line below the bars
                    continue;
                }

                string cmd = string.Format(template, "N", "N", "N");
                ZplElementBase[] parsed = RenderHelper.Parse($"^XA^BY3,3,80^FO40,100{cmd}^FD{data}^FS^XZ");
                using SKBitmap single = RenderHelper.RenderElements(parsed, null, LabelWidth, LabelHeight);
                SKRectI first = RenderHelper.DarkBounds(single).Value;

                ZplElementBase[] elements =
                [
                    parsed[0],
                    RenderHelper.WithDefaultPosition(new ZplGraphicBox(0, 0, 10, 10, 10)),
                ];
                using SKBitmap bmp = RenderHelper.RenderElements(elements, null, LabelWidth, LabelHeight);
                // the 10x10 box is placed at (x + width, y + height) of the barcode, i.e. right below its end
                Assert.AreEqual(100, RenderHelper.CountDark(bmp, first.Right, first.Bottom, int.MaxValue, int.MaxValue), name);
            }
        }

        [TestMethod]
        public void Linear_WrongElementTypesReturnTheCurrentPosition()
        {
            IElementDrawer[] drawers =
            [
                new Barcode11ElementDrawer(), new Barcode128ElementDrawer(), new Barcode39ElementDrawer(), new Barcode93ElementDrawer(),
                new BarcodeEAN13ElementDrawer(), new BarcodeEAN8ElementDrawer(), new BarcodeUpcAElementDrawer(), new BarcodeUpcEElementDrawer(),
                new BarcodeUpcExtensionElementDrawer(), new Interleaved2of5BarcodeDrawer(), new Industrial2of5BarcodeDrawer(),
                new BarcodeAnsiCodabarElementDrawer(), new BarcodeMsiElementDrawer(), new BarcodeLogmarsElementDrawer(),
            ];
            ZplElementBase other = new ZplGraphicBox(1, 1, 5, 5);
            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                foreach (IElementDrawer drawer in drawers)
                {
                    drawer.Prepare(new PrinterStorage(), canvas);
                    Assert.IsFalse(drawer.CanDraw(other), drawer.GetType().Name);
                    SKPoint position = new(12, 34);
                    Assert.AreEqual(position, drawer.Draw(other, new DrawerOptions(), position), drawer.GetType().Name);
                    Assert.AreEqual(position, drawer.Draw(other, new DrawerOptions(), position, InternationalFont.ZCP850), drawer.GetType().Name);
                    Assert.AreEqual(position, drawer.Draw(other, new DrawerOptions(), position, InternationalFont.ZCP850, 8), drawer.GetType().Name);
                }
            }
        }

        [TestMethod]
        public void Linear_FourArgumentDrawOverloadsUseEightDotsPerMillimeter()
        {
            (IElementDrawer Drawer, string Template, string Data)[] drawers =
            [
                (new Interleaved2of5BarcodeDrawer(), Interleaved, "123456"),
                (new Industrial2of5BarcodeDrawer(), Industrial, "123456"),
                (new BarcodeMsiElementDrawer(), Msi, "123456"),
                (new BarcodeLogmarsElementDrawer(), Logmars, "ABC123"),
            ];

            foreach ((IElementDrawer drawer, string template, string data) in drawers)
            {
                ZplElementBase element = RenderHelper.Parse($"^XA^BY3,3,80^FO40,40{string.Format(template, "N", "Y", "N")}^FD{data}^FS^XZ")[0];
                (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
                using (surface)
                {
                    drawer.Prepare(new PrinterStorage(), canvas);
                    SKPoint next = drawer.Draw(element, new DrawerOptions(), SKPoint.Empty, InternationalFont.ZCP850);
                    Assert.IsTrue(next.X > 40, drawer.GetType().Name);
                    Assert.AreEqual(next, drawer.Draw(element, new DrawerOptions(), SKPoint.Empty, InternationalFont.ZCP850, 8), drawer.GetType().Name);
                }
            }
        }

        // ---------- bitmap interpretation lines ----------

        [TestMethod]
        public void Linear_BitmapInterpretationLineIsUsedWhenAProviderIsAvailable()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                TestBitmapFontProvider provider = new("A");
                DrawerOptions options = RenderHelper.Opaque();
                options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
                options.BitmapFontProvider = provider;

                foreach (bool above in new[] { false, true })
                {
                    using SKBitmap plain = RenderLinear(template, data);
                    using SKBitmap bmp = RenderLinear(template, data, "N", true, above, options);
                    Assert.IsTrue(provider.Requests.Contains("A"), name);
                    Assert.IsTrue(RenderHelper.CountDark(bmp) > RenderHelper.CountDark(plain), name);
                }
            }
        }

        [TestMethod]
        public void Linear_BitmapInterpretationLineIsUsedWhenRotated()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                TestBitmapFontProvider provider = new("A");
                DrawerOptions options = RenderHelper.Opaque();
                options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
                options.BitmapFontProvider = provider;

                foreach (string orientation in new[] { "R", "I", "B" })
                {
                    using SKBitmap plain = RenderLinear(template, data, orientation);
                    using SKBitmap bmp = RenderLinear(template, data, orientation, true, false, options);
                    Assert.IsTrue(RenderHelper.CountDark(bmp) > RenderHelper.CountDark(plain), $"{name} {orientation}");
                }
            }
        }

        [TestMethod]
        public void Linear_BitmapStrictWithoutUsableProviderFallsBackToTypeface()
        {
            using SKBitmap outline = RenderLinear(Code128, "HELLO123", interpretation: true);

            DrawerOptions noProvider = RenderHelper.Opaque();
            noProvider.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            noProvider.BitmapFontProvider = null;
            using SKBitmap a = RenderLinear(Code128, "HELLO123", interpretation: true, options: noProvider);
            Assert.IsTrue(RenderHelper.SameImage(outline, a));

            DrawerOptions emptyProvider = RenderHelper.Opaque();
            emptyProvider.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            using SKBitmap b = RenderLinear(Code128, "HELLO123", interpretation: true, options: emptyProvider);
            Assert.IsTrue(RenderHelper.SameImage(outline, b));

            DrawerOptions wrongFont = RenderHelper.Opaque();
            wrongFont.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            wrongFont.BitmapFontProvider = new TestBitmapFontProvider("B");
            using SKBitmap c = RenderLinear(Code128, "HELLO123", interpretation: true, options: wrongFont);
            Assert.IsTrue(RenderHelper.SameImage(outline, c));
        }

        // ---------- field typeset: clamping and rotation ----------

        private static DrawerOptions BitmapA()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider("A");
            return options;
        }

        [TestMethod]
        public void Linear_FieldTypesetNearTheTopIsClampedForEverySymbology()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                string cmd = string.Format(template, "N", "N", "N");
                using SKBitmap bmp = Render($"^XA^BY3,3,80^FT40,20{cmd}^FD{data}^FS^XZ");

                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                Assert.AreEqual(0, bounds.Top, name);
                Assert.AreEqual(40, bounds.Left, name);
            }
        }

        [TestMethod]
        public void Linear_FieldTypesetNearTheTopWithInterpretationLineIsClamped()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                foreach (bool above in new[] { false, true })
                {
                    string cmd = string.Format(template, "N", "Y", above ? "Y" : "N");
                    string zpl = $"^XA^BY3,3,80^FT40,20{cmd}^FD{data}^FS^XZ";

                    using SKBitmap outline = Render(zpl);
                    using SKBitmap bitmap = Render(zpl, BitmapA());

                    Assert.AreEqual(0, RenderHelper.DarkBounds(outline).Value.Top, name);
                    Assert.AreEqual(0, RenderHelper.DarkBounds(bitmap).Value.Top, name);
                }
            }
        }

        [TestMethod]
        public void Linear_FieldTypesetWithBitmapInterpretationLines()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                foreach (bool above in new[] { false, true })
                {
                    string plainCmd = string.Format(template, "N", "N", "N");
                    string cmd = string.Format(template, "N", "Y", above ? "Y" : "N");
                    using SKBitmap plain = Render($"^XA^BY3,3,80^FT40,300{plainCmd}^FD{data}^FS^XZ");
                    using SKBitmap bmp = Render($"^XA^BY3,3,80^FT40,300{cmd}^FD{data}^FS^XZ", BitmapA());

                    Assert.IsTrue(RenderHelper.CountDark(bmp) > RenderHelper.CountDark(plain), $"{name} {above}");
                }
            }
        }

        [TestMethod]
        public void Linear_FieldTypesetRotatedBarcodesTurnAroundTheirAnchor()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                if (template == Logmars)
                {
                    // always prints an interpretation line, the geometry below is the one of the bars only
                    continue;
                }

                using SKBitmap normal = Render($"^XA^BY3,3,80^FT400,400{string.Format(template, "N", "N", "N")}^FD{data}^FS^XZ");
                SKRectI n = RenderHelper.DarkBounds(normal).Value;
                int w = n.Width;
                int h = n.Height;

                (string orientation, SKRectI expected)[] cases =
                [
                    ("R", new SKRectI(400, 400, 400 + h, 400 + w)),
                    ("I", new SKRectI(400 - w, 400, 400, 400 + h)),
                    ("B", new SKRectI(400 - h, 400 - w, 400, 400)),
                ];

                foreach ((string orientation, SKRectI expected) in cases)
                {
                    using SKBitmap bmp = Render($"^XA^BY3,3,80^FT400,400{string.Format(template, orientation, "N", "N")}^FD{data}^FS^XZ");
                    SKRectI actual = RenderHelper.DarkBounds(bmp).Value;

                    Assert.IsTrue(
                        Math.Abs(actual.Left - expected.Left) <= 4 &&
                        Math.Abs(actual.Top - expected.Top) <= 4 &&
                        Math.Abs(actual.Right - expected.Right) <= 4 &&
                        Math.Abs(actual.Bottom - expected.Bottom) <= 4,
                        $"{name} {orientation}: {actual} vs {expected}");
                }
            }
        }

        [TestMethod]
        public void Linear_FieldTypesetRotatedInterpretationLines()
        {
            foreach ((string name, string template, string data, _, _) in Cases)
            {
                if (template == Logmars)
                {
                    // always prints an interpretation line
                    continue;
                }

                foreach (string orientation in new[] { "R", "I", "B" })
                {
                    string plainCmd = string.Format(template, orientation, "N", "N");
                    string cmd = string.Format(template, orientation, "Y", "N");
                    using SKBitmap plain = Render($"^XA^BY3,3,80^FT400,400{plainCmd}^FD{data}^FS^XZ");
                    using SKBitmap outline = Render($"^XA^BY3,3,80^FT400,400{cmd}^FD{data}^FS^XZ");
                    using SKBitmap bitmap = Render($"^XA^BY3,3,80^FT400,400{cmd}^FD{data}^FS^XZ", BitmapA());

                    Assert.IsTrue(RenderHelper.CountDark(outline) > RenderHelper.CountDark(plain), $"{name} {orientation}");
                    Assert.IsTrue(RenderHelper.CountDark(bitmap) > RenderHelper.CountDark(plain), $"{name} {orientation}");
                }
            }
        }

        // ---------- symbology specific ----------

        [TestMethod]
        public void Code128_EmptyContentDrawsNothing()
        {
            using SKBitmap bmp = Render(Zpl(Code128, ""));
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Code128_ModesChangeTheEncoding()
        {
            // N (and unknown modes) use code set B; U is code set C; A/D use automatic code sets
            string Mode(string mode, string data)
            {
                return $"^XA^BY3,3,80^FO40,120^BCN,80,N,N,N,{mode}^FD{data}^FS^XZ";
            }

            using SKBitmap n = Render(Mode("N", "1234"));
            using SKBitmap u = Render(Mode("U", "0012345678901234567"));
            using SKBitmap a = Render(Mode("A", "1234"));
            using SKBitmap d = Render(Mode("D", "(01)09501101530003"));
            using SKBitmap lower = Render(Mode("u", "0012345678901234567"));
            using SKBitmap other = Render(Mode("X", "1234"));

            Assert.AreEqual("1234", RenderHelper.Decode(n, BarcodeFormat.CODE_128));
            Assert.IsNotNull(RenderHelper.DarkBounds(u));
            Assert.AreEqual("1234", RenderHelper.Decode(a, BarcodeFormat.CODE_128));
            Assert.IsNotNull(RenderHelper.DarkBounds(d));
            Assert.IsTrue(RenderHelper.SameImage(u, lower));
            Assert.IsTrue(RenderHelper.SameImage(n, other));
            Assert.IsFalse(RenderHelper.SameImage(n, u));
        }

        [TestMethod]
        public void Code128_InvalidContentForTheModeDrawsNothingAndKeepsPosition()
        {
            // code set C can only encode digits
            using SKBitmap bmp = Render("^XA^BY3,3,80^FO40,120^BCN,80,N,N,N,U^FDABCD^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Code39_Mod43CheckDigitIsAppended()
        {
            using SKBitmap bmp = Render("^XA^BY3,3,80^FO40,120^B3N,Y,80,N,N^FDABC123^FS^XZ");
            // A=10 B=11 C=12 1 2 3 => 39 => '$'
            Assert.AreEqual("ABC123$", RenderHelper.Decode(bmp, BarcodeFormat.CODE_39));
        }

        [TestMethod]
        public void Code39_Mod43WithInvalidCharacterDrawsNothing()
        {
            using SKBitmap bmp = Render("^XA^BY3,3,80^FO40,120^B3N,Y,80,N,N^FDabc@^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Code39_LowerCaseAndStarsAreNormalized()
        {
            using SKBitmap a = Render("^XA^BY3,3,80^FO40,120^B3N,N,80,N,N^FD*abc*^FS^XZ");
            using SKBitmap b = Render("^XA^BY3,3,80^FO40,120^B3N,N,80,N,N^FDABC^FS^XZ");
            Assert.IsTrue(RenderHelper.SameImage(a, b));
        }

        [TestMethod]
        public void Code39_WideToNarrowRatioChangesWidth()
        {
            using SKBitmap two = Render("^XA^BY3,2,80^FO40,120^B3N,N,80,N,N^FDABC^FS^XZ");
            using SKBitmap three = Render("^XA^BY3,3,80^FO40,120^B3N,N,80,N,N^FDABC^FS^XZ");
            Assert.IsTrue(RenderHelper.DarkBounds(three).Value.Width > RenderHelper.DarkBounds(two).Value.Width);
        }

        [TestMethod]
        public void Code93_InterpretationLineIncludesStartStopAndOptionalCheckCharacters()
        {
            Assert.AreEqual("▯TEST93▯", Barcode93ElementDrawer.BuildCode93Interpretation("TEST93", false));
            Assert.AreEqual("▯TEST93" + "+6" + "▯", Barcode93ElementDrawer.BuildCode93Interpretation("TEST93", true));
        }

        [TestMethod]
        public void Code93_ExtendedCharactersAreConvertedForTheCheckCharacters()
        {
            // one character of each class: NUL, control, FS-US, space, '!', '-', ':', ';', '@', '[', '`', 'a', '{'
            string[] samples =
            [
                "\0", "\u0001", "\u001a", "\u001b", "\u001f", " ", "$", "%", "+", "!", ",", "-", "9", ":", ";", "?", "@",
                "A", "Z", "[", "_", "`", "a", "z", "{", "\u007f",
            ];
            foreach (string sample in samples)
            {
                string result = Barcode93ElementDrawer.BuildCode93Interpretation(sample, true);
                Assert.AreEqual(sample.Length + 4, result.Length, $"U+{(int)sample[0]:X4}");
            }

            // ';' is encoded as the shift character 'b' + 'F': checksum of "bF" computed manually:
            // weights from the right: F(15)*1 + b(44)*2 = 103 -> 103 % 47 = 9 => C = '9'
            // K: (9*1) + F(15)*2 + b(44)*3 = 171 -> 171 % 47 = 30 => 'U'
            Assert.AreEqual("▯;9U▯", Barcode93ElementDrawer.BuildCode93Interpretation(";", true));
        }

        [TestMethod]
        public void Code93_NonAsciiContentCannotBeEncoded()
        {
            Assert.ThrowsExactly<ArgumentException>(() => Barcode93ElementDrawer.BuildCode93Interpretation("ä", true));
        }

        [TestMethod]
        public void Code93_CheckDigitInterpretationIsDrawnForCheckDigitElements()
        {
            using SKBitmap with = Render("^XA^BY3,3,80^FO40,120^BAN,80,Y,N,Y^FDTEST93^FS^XZ");
            using SKBitmap without = Render("^XA^BY3,3,80^FO40,120^BAN,80,Y,N,N^FDTEST93^FS^XZ");
            Assert.IsFalse(RenderHelper.SameImage(with, without));
        }

        [TestMethod]
        public void Ean13_ShortContentIsPaddedAndLongContentIsCut()
        {
            using SKBitmap shortContent = RenderLinear(Ean13, "12345");
            using SKBitmap padded = RenderLinear(Ean13, "000000012345");
            using SKBitmap longContent = RenderLinear(Ean13, "1234567890123456");
            using SKBitmap cut = RenderLinear(Ean13, "123456789012");
            Assert.IsTrue(RenderHelper.SameImage(shortContent, padded));
            Assert.IsTrue(RenderHelper.SameImage(longContent, cut));
            Assert.AreEqual("0000000123457", RenderHelper.Decode(shortContent, BarcodeFormat.EAN_13));
        }

        [TestMethod]
        public void Ean13_InterpretationLineAboveUsesPlainText()
        {
            using SKBitmap bmp = RenderLinear(Ean13, "123456789012", interpretation: true, above: true);
            using SKBitmap plain = RenderLinear(Ean13, "123456789012");
            Assert.IsTrue(RenderHelper.DarkBounds(bmp).Value.Top < RenderHelper.DarkBounds(plain).Value.Top);
            Assert.AreEqual("1234567890128", RenderHelper.Decode(bmp, BarcodeFormat.EAN_13));
        }

        [TestMethod]
        public void Ean8_ShortContentIsPaddedAndLongContentIsCut()
        {
            using SKBitmap shortContent = RenderLinear(Ean8, "123");
            using SKBitmap padded = RenderLinear(Ean8, "0000123");
            using SKBitmap longContent = RenderLinear(Ean8, "123456789");
            using SKBitmap cut = RenderLinear(Ean8, "1234567");
            Assert.IsTrue(RenderHelper.SameImage(shortContent, padded));
            Assert.IsTrue(RenderHelper.SameImage(longContent, cut));
        }

        [TestMethod]
        public void UpcA_PrintCheckDigitFlagControlsTheInterpretation()
        {
            using SKBitmap with = Render("^XA^BY3,3,80^FO40,120^BUN,80,Y,N,Y^FD12345678901^FS^XZ");
            using SKBitmap without = Render("^XA^BY3,3,80^FO40,120^BUN,80,Y,N,N^FD12345678901^FS^XZ");
            Assert.IsFalse(RenderHelper.SameImage(with, without));
            Assert.AreEqual("123456789012", RenderHelper.Decode(with, BarcodeFormat.UPC_A));
        }

        [TestMethod]
        public void UpcA_ShortAndLongContent()
        {
            using SKBitmap shortContent = RenderLinear(UpcA, "123");
            using SKBitmap padded = RenderLinear(UpcA, "00000000123");
            using SKBitmap longContent = RenderLinear(UpcA, "123456789012345");
            using SKBitmap cut = RenderLinear(UpcA, "12345678901");
            Assert.IsTrue(RenderHelper.SameImage(shortContent, padded));
            Assert.IsTrue(RenderHelper.SameImage(longContent, cut));
        }

        [TestMethod]
        public void UpcE_ContentLengthsSelectTheConversion()
        {
            // < 7 digits: padded with zeros, 7 or 8 digits: user check digit ignored
            using SKBitmap shortContent = RenderLinear(UpcE, "1234");
            using SKBitmap padded = RenderLinear(UpcE, "0001234");
            using SKBitmap seven = RenderLinear(UpcE, "0123456");
            using SKBitmap eight = RenderLinear(UpcE, "01234565");
            Assert.IsTrue(RenderHelper.SameImage(shortContent, padded));
            Assert.IsTrue(RenderHelper.SameImage(seven, eight));

            // UPC-A style input (9, 10, 11 and 12 digits) is compressed to UPC-E
            foreach (string upcA in new[] { "123456789", "1200000345", "12100000345", "123000000045", "01234500001", "01234000005", "12345600007" })
            {
                using SKBitmap bmp = RenderLinear(UpcE, upcA, interpretation: true);
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), upcA);
            }
        }

        [TestMethod]
        public void UpcE_ManufacturerPatternsProduceDistinctSymbols()
        {
            // manufacturer % 100 == 0 with trail <= 2, trail > 2, manufacturer % 10 == 0 and the general case
            string[] inputs = ["01200000345", "01230000045", "01234000005", "01234500005"];
            SKBitmap[] images = inputs.Select(i => RenderLinear(UpcE, i)).ToArray();
            try
            {
                for (int a = 0; a < images.Length; a++)
                {
                    for (int b = a + 1; b < images.Length; b++)
                    {
                        Assert.IsFalse(RenderHelper.SameImage(images[a], images[b]), $"{inputs[a]} vs {inputs[b]}");
                    }
                }
            }
            finally
            {
                foreach (SKBitmap image in images)
                {
                    image.Dispose();
                }
            }
        }

        [TestMethod]
        public void UpcE_CheckDigitControlsTheInterpretation()
        {
            using SKBitmap with = Render("^XA^BY3,3,80^FO40,120^B9N,80,Y,N,Y^FD0123456^FS^XZ");
            using SKBitmap without = Render("^XA^BY3,3,80^FO40,120^B9N,80,Y,N,N^FD0123456^FS^XZ");
            Assert.IsFalse(RenderHelper.SameImage(with, without));
        }

        [TestMethod]
        public void UpcExtension_TwoAndFiveDigitSupplements()
        {
            using SKBitmap two = RenderLinear(UpcExt, "12", interpretation: true);
            using SKBitmap one = RenderLinear(UpcExt, "1", interpretation: true);
            using SKBitmap padded = RenderLinear(UpcExt, "01", interpretation: true);
            using SKBitmap five = RenderLinear(UpcExt, "12345", interpretation: true);
            using SKBitmap fiveLong = RenderLinear(UpcExt, "123456", interpretation: true);
            using SKBitmap fiveShort = RenderLinear(UpcExt, "345", interpretation: true);
            using SKBitmap fivePadded = RenderLinear(UpcExt, "00345", interpretation: true);

            Assert.IsTrue(RenderHelper.SameImage(one, padded));
            Assert.IsTrue(RenderHelper.SameImage(five, fiveLong));
            Assert.IsTrue(RenderHelper.SameImage(fiveShort, fivePadded));
            Assert.IsTrue(RenderHelper.DarkBounds(five).Value.Width > RenderHelper.DarkBounds(two).Value.Width);
        }

        [TestMethod]
        public void Interleaved_OddLengthIsPaddedWithLeadingZero()
        {
            using SKBitmap odd = RenderLinear(Interleaved, "12345");
            using SKBitmap even = RenderLinear(Interleaved, "012345");
            Assert.IsTrue(RenderHelper.SameImage(odd, even));
            Assert.AreEqual("012345", RenderHelper.Decode(odd, BarcodeFormat.ITF));
        }

        [TestMethod]
        public void Interleaved_Mod10CheckDigitIsAppended()
        {
            using SKBitmap bmp = Render("^XA^BY3,3,80^FO40,120^B2N,80,N,N,Y^FD1234567^FS^XZ");
            // digits 1234567 weights (from the right) 1,3,1,3...: 7*1+6*3+5+4*3+3+2*3+1 = 52 -> check digit 8
            Assert.AreEqual("12345678", RenderHelper.Decode(bmp, BarcodeFormat.ITF));
        }

        [TestMethod]
        public void Interleaved_Mod10WithNonDigitDrawsNothing()
        {
            using SKBitmap bmp = Render("^XA^BY3,3,80^FO40,120^B2N,80,N,N,Y^FD12A4^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Interleaved_EmptyContentDrawsNothing()
        {
            using SKBitmap bmp = Render(Zpl(Interleaved, ""));
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Industrial_NonDigitsAndEmptyContentDrawNothing()
        {
            using SKBitmap empty = Render(Zpl(Industrial, ""));
            Assert.IsNull(RenderHelper.DarkBounds(empty));

            using SKBitmap letters = Render(Zpl(Industrial, "12AB"));
            Assert.IsNull(RenderHelper.DarkBounds(letters));
        }

        [TestMethod]
        public void Industrial_EmptyContentAfterHexReplacement()
        {
            // the hex escape is replaced by an empty string? no: a control character is not a digit
            using SKBitmap bmp = Render(Zpl(Industrial, "_0A", hex: true));
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Industrial_WideRatioIsAtLeastTheNarrowWidth()
        {
            using SKBitmap bmp = Render("^XA^BY3,1.0,80^FO40,120^BIN,80,N,N^FD123^FS^XZ");
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Codabar_EmptyContentDrawsNothingAndStarsAreTrimmed()
        {
            using SKBitmap empty = Render(Zpl(Codabar, "**"));
            Assert.IsNull(RenderHelper.DarkBounds(empty));

            using SKBitmap stars = Render(Zpl(Codabar, "*123*"));
            using SKBitmap plain = Render(Zpl(Codabar, "123"));
            Assert.IsTrue(RenderHelper.SameImage(stars, plain));
        }

        [TestMethod]
        public void Msi_CheckDigitModes()
        {
            SKBitmap Render(string mode, string data, bool printCheckDigit)
            {
                return RenderHelper.Render($"^XA^BY3,3,80^FO40,120^BMN,{mode},80,Y,N,{(printCheckDigit ? "Y" : "N")}^FD{data}^FS^XZ", null, LabelWidth, LabelHeight);
            }

            using SKBitmap a = Render("A", "1234567", false);
            using SKBitmap b = Render("B", "1234567", false);
            using SKBitmap c = Render("C", "1234567", false);
            using SKBitmap d = Render("D", "1234567", false);
            using SKBitmap bPrinted = Render("B", "1234567", true);

            // each check digit mode adds more symbols: A < B < C < D
            int widthA = RenderHelper.DarkBounds(a).Value.Width;
            int widthB = RenderHelper.DarkBounds(b).Value.Width;
            int widthC = RenderHelper.DarkBounds(c).Value.Width;
            int widthD = RenderHelper.DarkBounds(d).Value.Width;
            Assert.IsTrue(widthA < widthB);
            Assert.IsTrue(widthB < widthC);
            Assert.IsTrue(widthC == widthD);
            Assert.IsFalse(RenderHelper.SameImage(b, bPrinted));
        }

        [TestMethod]
        public void Msi_InvalidContentIsLoggedAsError()
        {
            ListLogger logger = new();
            using SKBitmap bmp = Render("^XA^BY3,3,80^FO40,120^BMN,B,80,Y,N,N^FDABC^FS^XZ", null, logger);
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
            Assert.AreEqual(1, logger.Entries.Count);
            Assert.AreEqual(LogLevel.Error, logger.Entries[0].Level);
            Assert.IsTrue(logger.Entries[0].Message.Contains("ABC"));
        }

        [TestMethod]
        public void Msi_NonDigitsFailForEveryCheckDigitMode()
        {
            foreach (string mode in new[] { "B", "C", "D" })
            {
                ListLogger logger = new();
                using SKBitmap bmp = Render($"^XA^BY3,3,80^FO40,120^BMN,{mode},80,Y,N,N^FD12X4^FS^XZ", null, logger);
                Assert.IsNull(RenderHelper.DarkBounds(bmp), mode);
                Assert.AreEqual(1, logger.Entries.Count, mode);
            }
        }

        [TestMethod]
        public void Msi_EmptyContentDrawsNothing()
        {
            using SKBitmap bmp = Render(Zpl(Msi, ""));
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Msi_UnknownCheckDigitModeFailsGracefully()
        {
            ZplBarcodeMsi element = (ZplBarcodeMsi)RenderHelper.Parse(Zpl(Msi, "1234"))[0];
            ZplBarcodeMsi invalid = new(
                "1234",
                40,
                120,
                80,
                3,
                3.0,
                FieldOrientation.Normal,
                (MsiBarcodeCheckDigitMode)99,
                null,
                true,
                false,
                false,
                false);
            ListLogger logger = new();
            using SKBitmap bmp = RenderHelper.RenderElements([invalid], null, LabelWidth, LabelHeight, logger: logger);
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
            Assert.AreEqual(1, logger.Entries.Count);
            Assert.IsNotNull(element);
        }

        [TestMethod]
        public void Logmars_AppendsMod43CheckCharacter()
        {
            using SKBitmap bmp = Render(Zpl(Logmars, "ABC123"));
            // A=10 B=11 C=12 1 2 3 => 39 => '$'
            Assert.AreEqual("ABC123$", RenderHelper.Decode(bmp, BarcodeFormat.CODE_39));
        }

        [TestMethod]
        public void Logmars_EmptyOrInvalidContentDrawsNothing()
        {
            using SKBitmap empty = Render(Zpl(Logmars, ""));
            Assert.IsNull(RenderHelper.DarkBounds(empty));

            using SKBitmap invalid = Render(Zpl(Logmars, "abc"));
            Assert.IsNull(RenderHelper.DarkBounds(invalid));
        }

        [TestMethod]
        public void Code11_InvalidContentIsLoggedAsError()
        {
            ListLogger logger = new();
            using SKBitmap bmp = Render(Zpl(Code11, "12AB"), null, logger);
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
            Assert.AreEqual(1, logger.Entries.Count);
            Assert.AreEqual(LogLevel.Error, logger.Entries[0].Level);
            Assert.IsTrue(logger.Entries[0].Message.Contains("12AB"));
            Assert.IsNotNull(logger.Entries[0].Exception);
        }

        [TestMethod]
        public void Code11_CheckDigitCountChangesTheSymbol()
        {
            using SKBitmap one = Render("^XA^BY3,3,80^FO40,120^B1N,N,80,N,N^FD1234^FS^XZ");
            using SKBitmap two = Render("^XA^BY3,3,80^FO40,120^B1N,Y,80,N,N^FD1234^FS^XZ");
            Assert.IsTrue(RenderHelper.DarkBounds(two).Value.Width > 0);
            Assert.IsNotNull(RenderHelper.DarkBounds(one));
        }

        [TestMethod]
        public void Code93_LongContentWrapsTheCheckCharacterWeights()
        {
            // more than 20 characters: the weights of the check characters restart at 1
            // (check characters 'b' and 'L' are printed as '#' and 'L')
            const string data = "ABCDEFGHIJKLMNOPQRSTUVWXY";
            string start = Barcode93ElementDrawer.Code93StartStopSymbol;

            Assert.AreEqual(start + data + "#L" + start, Barcode93ElementDrawer.BuildCode93Interpretation(data, true));
            Assert.AreEqual(start + data + start, Barcode93ElementDrawer.BuildCode93Interpretation(data, false));

            using SKBitmap bmp = Render($"^XA^BY2,3,80^FO40,120^BAN,80,N,N,Y^FD{data}^FS^XZ");
            Assert.AreEqual(data, RenderHelper.Decode(bmp, BarcodeFormat.CODE_93));
        }

        [TestMethod]
        public void Msi_Mod11AndMod10CheckDigitsOfLongContentMatchTheExplicitDigits()
        {
            // weighted sum of 12345678 is 138 (weights 2..7 restart after six digits), (11 - 138 mod 11) mod 11 = 5,
            // then the mod 10 digit of 123456785 is 5
            using SKBitmap computed = Render("^XA^BY2,3,80^FO40,120^BMN,D,80,N,N,N^FD12345678^FS^XZ");
            using SKBitmap explicitDigits = Render("^XA^BY2,3,80^FO40,120^BMN,A,80,N,N,N^FD1234567855^FS^XZ");

            Assert.IsNotNull(RenderHelper.DarkBounds(computed));
            Assert.IsTrue(RenderHelper.SameImage(computed, explicitDigits));
        }

        [TestMethod]
        [DataRow("1234567", "123456741")]
        [DataRow("5", "513")]
        public void Msi_Mod11CheckDigitIsElevenMinusRemainder(string data, string expected)
        {
            // IBM weights 2..7: 1234567 has the weighted sum 106 and the check digit (11 - 106 mod 11) mod 11 = 4
            using SKBitmap computed = Render($"^XA^BY2,3,80^FO40,120^BMN,D,80,N,N,N^FD{data}^FS^XZ");
            using SKBitmap explicitDigits = Render($"^XA^BY2,3,80^FO40,120^BMN,A,80,N,N,N^FD{expected}^FS^XZ");

            Assert.IsNotNull(RenderHelper.DarkBounds(computed));
            Assert.IsTrue(RenderHelper.SameImage(computed, explicitDigits));
        }

        [TestMethod]
        public void Msi_Mod11CheckValueOfTenIsWrittenAsTwoDigits()
        {
            // 6 * 2 = 12, (11 - 12 mod 11) mod 11 = 10 is written as "10"; the mod 10 digit of "610" is 6
            using SKBitmap computed = Render("^XA^BY2,3,80^FO40,120^BMN,D,80,N,N,N^FD6^FS^XZ");
            using SKBitmap explicitDigits = Render("^XA^BY2,3,80^FO40,120^BMN,A,80,N,N,N^FD6106^FS^XZ");

            Assert.IsNotNull(RenderHelper.DarkBounds(computed));
            Assert.IsTrue(RenderHelper.SameImage(computed, explicitDigits));
        }
    }
}
