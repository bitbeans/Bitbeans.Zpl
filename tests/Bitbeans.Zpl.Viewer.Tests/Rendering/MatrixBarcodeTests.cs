using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using Microsoft.Extensions.Logging;

using SkiaSharp;

using System;

using ZXing;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class MatrixBarcodeTests
    {
        private static SKBitmap Render(string zpl, DrawerOptions options = null, ILogger logger = null, int dpmm = RenderHelper.Dpmm)
        {
            return RenderHelper.Render(zpl, options, 100, 100, dpmm, logger);
        }

        // ---------- all 2D drawers: shared behavior ----------

        // (name, ^B command with {0} = orientation, data, ZXing format or null)
        private static readonly (string Name, string Cmd, string Data, BarcodeFormat? Format, string Decoded)[] Cases =
        [
            ("Aztec", "^BO{0},4", "Aztec content 123", BarcodeFormat.AZTEC, "Aztec content 123"),
            ("DataMatrix", "^BX{0},6,200", "DataMatrix 123", BarcodeFormat.DATA_MATRIX, "DataMatrix 123"),
            ("Qr", "^BQ{0},2,5,Q,7", "QA,QR content 123", BarcodeFormat.QR_CODE, "QR content 123"),
            ("Pdf417", "^B7{0},12,2", "PDF417 content 123", BarcodeFormat.PDF_417, "PDF417 content 123"),
            ("MaxiCode", "^BD4", "MaxiCode content 123", null, null),
        ];

        private static string Zpl(string cmd, string data, string orientation = "N", int x = 100, int y = 120, bool hex = false)
        {
            return $"^XA^BY2,3,10^FO{x},{y}{string.Format(cmd, orientation)}{(hex ? "^FH" : "")}^FD{data}^FS^XZ";
        }

        [TestMethod]
        public void Matrix_RenderedSymbolsAreDecodable()
        {
            foreach ((string name, string cmd, string data, BarcodeFormat? format, string decoded) in Cases)
            {
                using SKBitmap bmp = Render(Zpl(cmd, data));
                Assert.IsNotNull(RenderHelper.DarkBounds(bmp), name);
                if (format != null)
                {
                    Assert.AreEqual(decoded, RenderHelper.Decode(bmp, format.Value), name);
                }
            }
        }

        [TestMethod]
        public void Matrix_SymbolStartsAtTheFieldOrigin()
        {
            foreach ((string name, string cmd, string data, _, _) in Cases)
            {
                using SKBitmap bmp = Render(Zpl(cmd, data));
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                Assert.IsTrue(Math.Abs(bounds.Left - 100) <= 3, $"{name}: {bounds}");
                // QR codes are moved down by the vertical quiet zone (10)
                int expectedTop = name == "Qr" ? 130 : 120;
                Assert.IsTrue(Math.Abs(bounds.Top - expectedTop) <= 8, $"{name}: {bounds}");
            }
        }

        [TestMethod]
        public void Matrix_RotatedSymbolsAreDecodable()
        {
            foreach ((string name, string cmd, string data, BarcodeFormat? format, string decoded) in Cases)
            {
                if (format == null || name == "Qr")
                {
                    // the analyzer always produces unrotated QR codes
                    continue;
                }

                foreach (string orientation in new[] { "R", "I", "B" })
                {
                    using SKBitmap bmp = Render(Zpl(cmd, data, orientation));
                    Assert.AreEqual(decoded, RenderHelper.Decode(bmp, format.Value), $"{name} {orientation}");
                }
            }
        }

        [TestMethod]
        public void Matrix_RotatedSymbolsKeepTheirSizeAndSwapExtent()
        {
            foreach ((string name, string cmd, string data, _, _) in Cases)
            {
                if (name == "Qr" || name == "MaxiCode")
                {
                    continue;
                }

                using SKBitmap normal = Render(Zpl(cmd, data));
                SKRectI n = RenderHelper.DarkBounds(normal).Value;
                using SKBitmap inverted = Render(Zpl(cmd, data, "I"));
                SKRectI i = RenderHelper.DarkBounds(inverted).Value;
                Assert.IsTrue(Math.Abs(i.Width - n.Width) <= 2, name);
                Assert.IsTrue(Math.Abs(i.Height - n.Height) <= 2, name);

                using SKBitmap rotated = Render(Zpl(cmd, data, "R"));
                SKRectI r = RenderHelper.DarkBounds(rotated).Value;
                Assert.IsTrue(Math.Abs(r.Width - n.Height) <= 2, name);
                Assert.IsTrue(Math.Abs(r.Height - n.Width) <= 2, name);
            }
        }

        [TestMethod]
        public void Matrix_FieldTypesetPlacesTheSymbolAbove()
        {
            foreach ((string name, string cmd, string data, _, _) in Cases)
            {
                using SKBitmap bmp = Render($"^XA^BY2,3,10^FT100,300{string.Format(cmd, "N")}^FD{data}^FS^XZ");
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                // the quiet zone of 10 is kept below QR codes
                int expectedBottom = name == "Qr" ? 290 : 300;
                Assert.IsTrue(Math.Abs(bounds.Bottom - expectedBottom) <= 8, $"{name}: {bounds}");
            }
        }

        [TestMethod]
        public void Matrix_HexadecimalEscapesAreReplaced()
        {
            foreach ((string name, string cmd, string data, _, _) in Cases)
            {
                // replace the 'c' in the last word by its hex escape
                string escaped = data.Replace("content", "_63ontent");
                using SKBitmap plain = Render(Zpl(cmd, data));
                using SKBitmap hex = Render(Zpl(cmd, escaped, hex: true));
                Assert.IsTrue(RenderHelper.SameImage(plain, hex), name);
            }
        }

        [TestMethod]
        public void Matrix_DefaultPositionStartsAtThePreviousElementsEnd()
        {
            foreach ((string name, string cmd, string data, _, _) in Cases)
            {
                ZplElementBase[] parsed = RenderHelper.Parse(Zpl(cmd, data, x: 0, y: 0));
                ZplElementBase[] elements =
                [
                    new ZplGraphicBox(100, 100, 20, 20, 20),
                    RenderHelper.WithDefaultPosition(parsed[0]),
                ];
                using SKBitmap bmp = RenderHelper.RenderElements(elements, null, 100, 100);
                SKRectI bounds = RenderHelper.DarkBounds(bmp, 121, 0).Value;
                Assert.IsTrue(Math.Abs(bounds.Left - 120) <= 3, $"{name}: {bounds}");
                Assert.IsTrue(bounds.Top >= 120 - 1, $"{name}: {bounds}");
            }
        }

        [TestMethod]
        public void Matrix_WrongElementTypesReturnTheCurrentPosition()
        {
            IElementDrawer[] drawers =
            [
                new AztecBarcodeElementDrawer(), new DataMatrixElementDrawer(), new QrCodeElementDrawer(),
                new Pdf417ElementDrawer(), new MaxiCodeElementDrawer(),
            ];
            ZplElementBase other = new ZplGraphicBox(1, 1, 5, 5);
            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                foreach (IElementDrawer drawer in drawers)
                {
                    drawer.Prepare(new PrinterStorage(), canvas);
                    SKPoint position = new(5, 6);
                    Assert.IsFalse(drawer.CanDraw(other));
                    Assert.AreEqual(position, drawer.Draw(other, new DrawerOptions(), position), drawer.GetType().Name);
                    Assert.AreEqual(position, drawer.Draw(other, new DrawerOptions(), position, InternationalFont.ZCP850), drawer.GetType().Name);
                    Assert.AreEqual(position, drawer.Draw(other, new DrawerOptions(), position, InternationalFont.ZCP850, 8), drawer.GetType().Name);
                }
            }
        }

        // ---------- Aztec ----------

        [TestMethod]
        public void Aztec_EmptyContentDrawsNothing()
        {
            using SKBitmap bmp = Render("^XA^FO100,100^BON,4^FD^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Aztec_MagnificationScalesTheSymbol()
        {
            using SKBitmap small = Render("^XA^FO100,100^BON,2^FDAztec^FS^XZ");
            using SKBitmap large = Render("^XA^FO100,100^BON,6^FDAztec^FS^XZ");
            SKRectI s = RenderHelper.DarkBounds(small).Value;
            SKRectI l = RenderHelper.DarkBounds(large).Value;
            Assert.AreEqual(s.Width * 3, l.Width);
            Assert.AreEqual(s.Height * 3, l.Height);
        }

        [TestMethod]
        public void Aztec_ErrorControlSelectsErrorCorrectionOrLayers()
        {
            // 0: default, 1-99: percentage of error correction, 101-104: compact layers,
            // 201-232: full range layers, 300: pure barcode, other values: ignored
            string[] controls = ["0", "10", "90", "101", "102", "104", "201", "210", "232", "300", "150"];
            int[] widths = new int[controls.Length];
            for (int i = 0; i < controls.Length; i++)
            {
                using SKBitmap bmp = Render($"^XA^FO100,100^BON,3,N,{controls[i]}^FDAztec error control^FS^XZ");
                SKRectI? bounds = RenderHelper.DarkBounds(bmp);
                if (controls[i] is "101" or "201")
                {
                    // a single layer is too small for this content
                    Assert.IsNull(bounds, controls[i]);
                    continue;
                }

                widths[i] = bounds.Value.Width;
                Assert.AreEqual("Aztec error control", RenderHelper.Decode(bmp, BarcodeFormat.AZTEC), controls[i]);
            }

            // a single layer works for a short content
            foreach (string control in new[] { "101", "201" })
            {
                using SKBitmap bmp = Render($"^XA^FO100,100^BON,3,N,{control}^FDHi^FS^XZ");
                Assert.AreEqual("Hi", RenderHelper.Decode(bmp, BarcodeFormat.AZTEC), control);
            }

            // more layers is bigger
            Assert.IsTrue(widths[8] > widths[7]);
            Assert.IsTrue(widths[7] > widths[6 + 0] || widths[7] > 0);
            // more error correction needs a bigger or equal symbol
            Assert.IsTrue(widths[2] >= widths[1]);
            // unknown / default error control behave the same
            Assert.AreEqual(widths[0], widths[10]);
        }

        // ---------- Data Matrix ----------

        [TestMethod]
        public void DataMatrix_ZeroHeightOrBlankContentDrawsNothing()
        {
            using SKBitmap zeroHeight = Render("^XA^FO100,100^BXN,0,200^FDHello^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(zeroHeight));

            using SKBitmap blank = Render("^XA^FO100,100^BXN,6,200^FD   ^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(blank));
        }

        [TestMethod]
        public void DataMatrix_ModuleSizeIsTheHeight()
        {
            using SKBitmap six = Render("^XA^FO100,100^BXN,6,200^FDHello^FS^XZ");
            using SKBitmap twelve = Render("^XA^FO100,100^BXN,12,200^FDHello^FS^XZ");
            SKRectI a = RenderHelper.DarkBounds(six).Value;
            SKRectI b = RenderHelper.DarkBounds(twelve).Value;
            Assert.AreEqual(a.Width * 2, b.Width);
            Assert.AreEqual(a.Height * 2, b.Height);
        }

        [TestMethod]
        public void DataMatrix_AspectRatioAndExplicitSizeSelectTheSymbolShape()
        {
            // 1 = square, 2 = rectangular
            using SKBitmap square = Render("^XA^FO100,100^BXN,6,200,,,6,~,1^FDHello^FS^XZ");
            using SKBitmap rect = Render("^XA^FO100,100^BXN,6,200,,,6,~,2^FDHello^FS^XZ");
            SKRectI s = RenderHelper.DarkBounds(square).Value;
            SKRectI r = RenderHelper.DarkBounds(rect).Value;
            Assert.AreEqual(s.Width, s.Height);
            Assert.IsTrue(r.Width > r.Height);
            Assert.AreEqual("Hello", RenderHelper.Decode(square, BarcodeFormat.DATA_MATRIX));
            Assert.AreEqual("Hello", RenderHelper.Decode(rect, BarcodeFormat.DATA_MATRIX));

            // explicit columns/rows: equal => square (18x18 modules), different => rectangular (16x48)
            using SKBitmap exactSquare = Render("^XA^FO100,100^BXN,6,200,18,18^FDHello^FS^XZ");
            SKRectI es = RenderHelper.DarkBounds(exactSquare).Value;
            Assert.AreEqual(18 * 6, es.Width);
            Assert.AreEqual(18 * 6, es.Height);

            using SKBitmap exactRect = Render("^XA^FO100,100^BXN,6,200,48,16^FDHello^FS^XZ");
            SKRectI er = RenderHelper.DarkBounds(exactRect).Value;
            Assert.AreEqual(48 * 6, er.Width);
            Assert.AreEqual(16 * 6, er.Height);
        }

        [TestMethod]
        public void DataMatrix_HandRolledGs1Prefix()
        {
            using SKBitmap gs1 = Render("^XA^FO100,100^BXN,6,200^FD_10109501101530003^FS^XZ");
            using SKBitmap plain = Render("^XA^FO100,100^BXN,6,200^FD0109501101530003^FS^XZ");
            Assert.IsNotNull(RenderHelper.DarkBounds(gs1));
            // GS1 mode adds a FNC1 so the symbols are different
            Assert.IsFalse(RenderHelper.SameImage(gs1, plain));
        }

        // ---------- QR ----------

        [TestMethod]
        public void Qr_EmptyContentDrawsNothing()
        {
            ZplQrCode empty = new("", 100, 100);
            using SKBitmap bmp = RenderHelper.RenderElements([empty], null, 100, 100);
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Qr_MagnificationAndVerticalQuietZone()
        {
            ZplQrCode small = new("Hello", 100, 100, magnificationFactor: 3, verticalQuietZone: 0);
            ZplQrCode large = new("Hello", 100, 100, magnificationFactor: 6, verticalQuietZone: 0);
            ZplQrCode quiet = new("Hello", 100, 100, magnificationFactor: 3, verticalQuietZone: 20);
            using SKBitmap s = RenderHelper.RenderElements([small], null, 100, 100);
            using SKBitmap l = RenderHelper.RenderElements([large], null, 100, 100);
            using SKBitmap q = RenderHelper.RenderElements([quiet], null, 100, 100);

            SKRectI sb = RenderHelper.DarkBounds(s).Value;
            SKRectI lb = RenderHelper.DarkBounds(l).Value;
            SKRectI qb = RenderHelper.DarkBounds(q).Value;
            Assert.AreEqual(sb.Width * 2, lb.Width);
            Assert.AreEqual(sb.Left, qb.Left);
            Assert.AreEqual(sb.Top + 20, qb.Top);
            Assert.AreEqual("Hello", RenderHelper.Decode(s, BarcodeFormat.QR_CODE));
        }

        [TestMethod]
        public void Qr_ErrorCorrectionLevelsAndMask()
        {
            ErrorCorrectionLevel[] levels =
            [
                ErrorCorrectionLevel.HighDensity,
                ErrorCorrectionLevel.Standard,
                ErrorCorrectionLevel.HighReliability,
                ErrorCorrectionLevel.UltraHighReliability,
                (ErrorCorrectionLevel)99,
            ];
            int[] widths = new int[levels.Length];
            for (int i = 0; i < levels.Length; i++)
            {
                ZplQrCode qr = new("Error correction test content", 100, 100, 2, 3, levels[i], 3);
                using SKBitmap bmp = RenderHelper.RenderElements([qr], null, 100, 100);
                widths[i] = RenderHelper.DarkBounds(bmp).Value.Width;
                Assert.AreEqual("Error correction test content", RenderHelper.Decode(bmp, BarcodeFormat.QR_CODE), levels[i].ToString());
            }

            Assert.IsTrue(widths[0] <= widths[1]);
            Assert.IsTrue(widths[1] <= widths[2]);
            Assert.IsTrue(widths[2] <= widths[3]);
            // unknown values fall back to M
            Assert.AreEqual(widths[1], widths[4]);
        }

        [TestMethod]
        public void Qr_Gs1PrefixIsRemovedFromTheContent()
        {
            ZplQrCode gs1 = new(">;>8" + "0109501101530003", 100, 100, 2, 4);
            ZplQrCode plain = new("0109501101530003", 100, 100, 2, 4);
            using SKBitmap a = RenderHelper.RenderElements([gs1], null, 100, 100);
            using SKBitmap b = RenderHelper.RenderElements([plain], null, 100, 100);
            Assert.IsNotNull(RenderHelper.DarkBounds(a));
            Assert.IsFalse(RenderHelper.SameImage(a, b));
        }

        [TestMethod]
        public void Qr_HexEscapesAreDecodedOnlyOnce()
        {
            // "_5F" is an escaped underscore, so "_5F31" is the literal text "_31", which must not be decoded again
            ZplQrCode qr = new("_5F31", 100, 100, 2, 4, hexadecimalIndicator: '_');
            using SKBitmap bmp = RenderHelper.RenderElements([qr], null, 100, 100);
            Assert.AreEqual("_31", RenderHelper.Decode(bmp, BarcodeFormat.QR_CODE));
        }

        [TestMethod]
        public void Qr_RotatedSymbolIsDecodable()
        {
            foreach (FieldOrientation orientation in new[] { FieldOrientation.Rotated90, FieldOrientation.Rotated180, FieldOrientation.Rotated270 })
            {
                ZplQrCode qr = new("Rotated", 100, 100, 2, 4, fieldOrientation: orientation);
                using SKBitmap bmp = RenderHelper.RenderElements([qr], null, 100, 100);
                Assert.AreEqual("Rotated", RenderHelper.Decode(bmp, BarcodeFormat.QR_CODE), orientation.ToString());
            }
        }

        // ---------- PDF417 ----------

        [TestMethod]
        public void Pdf417_ZeroHeightOrBlankContentDrawsNothing()
        {
            using SKBitmap zero = Render("^XA^BY2^FO100,100^B7N,0^FDHello^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(zero));

            using SKBitmap blank = Render("^XA^BY2^FO100,100^B7N,10^FD   ^FS^XZ");
            Assert.IsNull(RenderHelper.DarkBounds(blank));
        }

        [TestMethod]
        public void Pdf417_RowHeightAndModuleWidth()
        {
            // bar height 3 * ^BY equals the requested height of 6 -> no vertical scaling
            using SKBitmap natural = Render("^XA^BY2^FO100,100^B7N,6^FDHello PDF417^FS^XZ");
            using SKBitmap tall = Render("^XA^BY2^FO100,100^B7N,12^FDHello PDF417^FS^XZ");
            SKRectI n = RenderHelper.DarkBounds(natural).Value;
            SKRectI t = RenderHelper.DarkBounds(tall).Value;
            Assert.AreEqual(n.Width, t.Width);
            Assert.AreEqual(n.Height * 2, t.Height);
            Assert.AreEqual("Hello PDF417", RenderHelper.Decode(natural, BarcodeFormat.PDF_417));
            Assert.AreEqual("Hello PDF417", RenderHelper.Decode(tall, BarcodeFormat.PDF_417));

            using SKBitmap single = Render("^XA^BY1^FO100,100^B7N,3^FDHello PDF417^FS^XZ");
            using SKBitmap triple = Render("^XA^BY3^FO100,100^B7N,9^FDHello PDF417^FS^XZ");
            Assert.AreEqual(RenderHelper.DarkBounds(single).Value.Width * 3, RenderHelper.DarkBounds(triple).Value.Width);
        }

        [TestMethod]
        public void Pdf417_ColumnsAndRowsAreHonored()
        {
            string data = "The quick brown fox jumps over the lazy dog 0123456789";
            using SKBitmap none = Render($"^XA^BY2^FO100,100^B7N,6,2^FD{data}^FS^XZ");
            using SKBitmap cols = Render($"^XA^BY2^FO100,100^B7N,6,2,5^FD{data}^FS^XZ");
            using SKBitmap rows = Render($"^XA^BY2^FO100,100^B7N,6,2,,12^FD{data}^FS^XZ");
            using SKBitmap both = Render($"^XA^BY2^FO100,100^B7N,6,2,4,12^FD{data}^FS^XZ");

            foreach (SKBitmap bmp in new[] { none, cols, rows, both })
            {
                Assert.AreEqual(data, RenderHelper.Decode(bmp, BarcodeFormat.PDF_417));
            }

            // more columns make the symbol wider and lower
            Assert.IsTrue(RenderHelper.DarkBounds(cols).Value.Width > RenderHelper.DarkBounds(none).Value.Width);
            Assert.IsTrue(RenderHelper.DarkBounds(cols).Value.Height < RenderHelper.DarkBounds(none).Value.Height);
            // a row limit caps the height (6 dots per row); with fixed columns the rows are exact
            Assert.IsTrue(RenderHelper.DarkBounds(rows).Value.Height <= 12 * 6);
            Assert.AreEqual(12 * 6, RenderHelper.DarkBounds(both).Value.Height);
        }

        [TestMethod]
        public void Pdf417_SecurityLevelsAndCompact()
        {
            int previousWidth = 0;
            for (int level = 0; level <= 9; level++)
            {
                using SKBitmap bmp = Render($"^XA^BY2^FO100,100^B7N,6,{level}^FDSecurity level test 1234567890^FS^XZ");
                Assert.AreEqual("Security level test 1234567890", RenderHelper.Decode(bmp, BarcodeFormat.PDF_417), level.ToString());
                if (level == 0)
                {
                    previousWidth = RenderHelper.DarkBounds(bmp).Value.Height;
                }
                else if (level == 8)
                {
                    // more error correction needs more rows
                    Assert.IsTrue(RenderHelper.DarkBounds(bmp).Value.Height > previousWidth);
                }
            }

            using SKBitmap compact = Render("^XA^BY2^FO100,100^B7N,6,2,,,Y^FDCompact^FS^XZ");
            using SKBitmap normal = Render("^XA^BY2^FO100,100^B7N,6,2,,,N^FDCompact^FS^XZ");
            Assert.IsTrue(RenderHelper.DarkBounds(compact).Value.Width < RenderHelper.DarkBounds(normal).Value.Width);
        }

        [TestMethod]
        public void Pdf417_ZeroModuleWidthIsAnError()
        {
            ZplPDF417 element = new("Hello", 100, 100, 6, 0);
            ListLogger logger = new();
            using SKBitmap bmp = RenderHelper.RenderElements([element], null, 100, 100, logger: logger);
            Assert.IsNull(RenderHelper.DarkBounds(bmp));
            Assert.AreEqual(1, logger.Entries.Count);
            Assert.AreEqual(LogLevel.Error, logger.Entries[0].Level);
        }

        // ---------- MaxiCode ----------

        [TestMethod]
        public void MaxiCode_SymbolSizeFollowsThePrintDensity()
        {
            // the symbol is about 25 mm wide at every supported density, 6 and 24 use the generic formula
            foreach (int dpmm in new[] { 6, 8, 12, 24 })
            {
                using SKBitmap bmp = RenderHelper.Render("^XA^FO20,20^BD4^FDMaxiCode^FS^XZ", null, 50, 50, dpmm);
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                Assert.IsTrue(bounds.Width >= 24.5 * dpmm && bounds.Width <= 27 * dpmm, $"{dpmm}dpmm width {bounds.Width}");
                Assert.IsTrue(bounds.Height >= 23 * dpmm && bounds.Height <= 26 * dpmm, $"{dpmm}dpmm height {bounds.Height}");
                Assert.IsTrue(bounds.Left <= 20 + 2 * dpmm, $"{dpmm}");
            }
        }

        [TestMethod]
        public void MaxiCode_HasACentralBullseye()
        {
            using SKBitmap bmp = Render("^XA^FO100,100^BD4^FDMaxiCode^FS^XZ");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            int cx = bounds.Left + bounds.Width / 2;
            int cy = bounds.Top + bounds.Height / 2;
            // the center of the finder pattern is white, the first ring (radius 0.51..1.18 mm = 4..9 px) is black
            Assert.IsTrue(RenderHelper.IsLight(bmp, cx, cy));
            int ring = 0;
            for (int d = 4; d <= 9; d++)
            {
                if (RenderHelper.IsDark(bmp, cx + d, cy))
                {
                    ring++;
                }
            }

            Assert.IsTrue(ring > 0);
        }

        [TestMethod]
        public void MaxiCode_UnstructuredContentForMode2IsStillRendered()
        {
            using SKBitmap bmp = Render("^XA^FO100,100^BD2^FDnot a structured message^FS^XZ");
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void DataMatrix_ColumnsWithoutRowsDoNotFixTheSize()
        {
            ZplElementBase plain = new ZplDataMatrix("ABC123", 50, 50, 100, QualityLevel.ECC200);
            ZplElementBase columnsOnly = new ZplDataMatrix("ABC123", 50, 50, 100, QualityLevel.ECC200, columns: 24);

            using SKBitmap a = RenderHelper.RenderElements([plain]);
            using SKBitmap b = RenderHelper.RenderElements([columnsOnly]);

            Assert.IsNotNull(RenderHelper.DarkBounds(a));
            Assert.IsTrue(RenderHelper.SameImage(a, b));
        }
    }
}
