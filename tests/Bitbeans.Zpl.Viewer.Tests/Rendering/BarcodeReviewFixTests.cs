using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System.Collections.Generic;
using System.Linq;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    /// <summary>
    /// Regression tests for the barcode review findings: GS1 check digit, reverse print, bitmap ownership,
    /// negative ^FT positions and field justification.
    /// </summary>
    [TestClass]
    public class BarcodeReviewFixTests
    {
        private const double LabelWidth = 100;
        private const double LabelHeight = 100;

        private sealed class ProbeDrawer : BarcodeDrawerBase
        {
            public override bool CanDraw(ZplElementBase element)
            {
                return false;
            }

            public void DrawProbe(SKBitmap bitmap, float x, float y, int width, int height, bool useFieldOrigin, FieldOrientation orientation)
            {
                this.DrawBarcode(bitmap, x, y, width, height, useFieldOrigin, orientation);
            }

            public static int Check(string payload)
            {
                return CalculateGs1CheckDigit(payload);
            }

            public (float X, float Y) Justify(float x, float y, int width, bool useFieldOrigin, FieldOrientation orientation, FieldJustification justification)
            {
                this.JustifyOrigin(ref x, ref y, width, useFieldOrigin, orientation, justification);
                return (x, y);
            }
        }

        private static SKBitmap Render(string zpl)
        {
            return RenderHelper.Render(zpl, null, LabelWidth, LabelHeight);
        }

        private static SKRectI Bounds(string zpl)
        {
            using SKBitmap bitmap = Render(zpl);
            SKRectI? bounds = RenderHelper.DarkBounds(bitmap);
            Assert.IsNotNull(bounds, zpl);
            return bounds.Value;
        }

        // ---- 2: GS1 check digit ----

        [TestMethod]
        public void Gs1CheckDigitMatchesKnownCodes()
        {
            Assert.AreEqual(9, ProbeDrawer.Check("9501093"));
            Assert.AreEqual(1, ProbeDrawer.Check("400638133393"));
            Assert.AreEqual(2, ProbeDrawer.Check("03600029145"));
            Assert.AreEqual(0, ProbeDrawer.Check("0000000"));
        }

        private static string Ean8Candidate(int checkDigit, string except)
        {
            for (int i = 1000000; i < 1000100; i++)
            {
                string payload = i.ToString();
                if (payload != except && ProbeDrawer.Check(payload) == checkDigit)
                {
                    return payload;
                }
            }

            throw new System.InvalidOperationException();
        }

        [TestMethod]
        public void Ean8InterpretationLineShowsTheRealCheckDigit()
        {
            // 9501093 -> GTIN-8 95010939; the old weights printed 95010931.
            const int moduleWidth = 3;
            string Zpl(string data) => $"^XA^BY{moduleWidth},3,80^FO40,120^B8N,80,Y,N^FD{data}^FS^XZ";

            // slot of the 8th digit: 3 module guard, 7 digits of 7 modules, 5 module center guard
            int slotLeft = 40 + 3 * moduleWidth + 7 * 7 * moduleWidth + 5 * moduleWidth;
            int slotWidth = 7 * moduleWidth;

            using SKBitmap actual = Render(Zpl("9501093"));
            using SKBitmap sameCheckDigit = Render(Zpl(Ean8Candidate(9, "9501093")));
            using SKBitmap otherCheckDigit = Render(Zpl(Ean8Candidate(1, "9501093")));

            // below the bars (bars: 120..200) plus the guard extension
            int top = 205;
            int bottom = 260;
            Assert.IsGreaterThan(0, RenderHelper.CountDark(actual, slotLeft, top, slotLeft + slotWidth, bottom));
            Assert.IsTrue(SameRegion(actual, sameCheckDigit, slotLeft, top, slotWidth, bottom - top));
            Assert.IsFalse(SameRegion(actual, otherCheckDigit, slotLeft, top, slotWidth, bottom - top));
        }

        private static bool SameRegion(SKBitmap a, SKBitmap b, int left, int top, int width, int height)
        {
            for (int y = top; y < top + height; y++)
            {
                for (int x = left; x < left + width; x++)
                {
                    if (RenderHelper.IsDark(a, x, y) != RenderHelper.IsDark(b, x, y))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // ---- 3: reverse print ----

        [TestMethod]
        public void FieldReversePrintDrawsBarsWhiteOnBlackBox()
        {
            const string zpl = "^XA^FO20,20^GB300,100,100^FS^FO30,30^FR^BCN,60,Y,N^FD123456^FS^XZ";
            using SKBitmap bitmap = Render(zpl);

            // the box is solid black outside of the barcode area
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 310, 60));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 25, 60));

            // inside the barcode area the spaces stay black and the bars turn white
            int light = 0;
            for (int x = 30; x < 150; x++)
            {
                if (RenderHelper.IsLight(bitmap, x, 60))
                {
                    light++;
                }
            }

            Assert.IsGreaterThan(20, light);

            // the interpretation line is white as well
            Assert.IsGreaterThan(0, CountLight(bitmap, 30, 96, 200, 118));
        }

        [TestMethod]
        public void WithoutReversePrintTheBoxStaysBlackUnderTheBarcode()
        {
            const string zpl = "^XA^FO20,20^GB300,100,100^FS^FO30,30^BCN,60,Y,N^FD123456^FS^XZ";
            using SKBitmap bitmap = Render(zpl);
            Assert.AreEqual(0, CountLight(bitmap, 30, 30, 300, 118));
        }

        [TestMethod]
        public void LabelReversePrintAppliesToBarcodes()
        {
            const string zpl = "^XA^LRY^FO20,20^GB300,100,100^FS^FO30,30^BCN,60,N^FD123456^FS^XZ";
            using SKBitmap bitmap = Render(zpl);
            Assert.IsGreaterThan(20, CountLight(bitmap, 30, 40, 200, 80));
        }

        [TestMethod]
        public void FieldReversePrintAppliesToMatrixBarcodes()
        {
            const string zpl = "^XA^FO20,20^GB300,300,300^FS^FO30,30^FR^BQN,2,5^FDQA,reverse^FS^XZ";
            using SKBitmap bitmap = Render(zpl);
            Assert.IsGreaterThan(50, CountLight(bitmap, 30, 30, 200, 200));
        }

        [TestMethod]
        public void ReversePrintOnlyAffectsTheFollowingField()
        {
            const string zpl = "^XA^FO20,20^GB300,100,100^FS^FO30,30^FR^BCN,60,N^FD123456^FS^FO30,150^BCN,60,N^FD123456^FS^XZ";
            using SKBitmap bitmap = Render(zpl);
            Assert.IsGreaterThan(20, RenderHelper.CountDark(bitmap, 30, 150, 200, 210));
        }

        [TestMethod]
        public void ReverseDrawIsOnlyRequestedForReversedFieldData()
        {
            Barcode128ElementDrawer drawer = new();
            Assert.IsFalse(drawer.IsReverseDraw(new ZplRaw("^XA")));
            Assert.IsFalse(drawer.IsReverseDraw(new ZplBarcode128("A", 0, 0)));
            Assert.IsTrue(drawer.IsReverseDraw(new ZplBarcode128("A", 0, 0, reversePrint: true)));
        }

        [TestMethod]
        public void AnalyzerPassesReversePrintToEveryBarcode()
        {
            string[] commands =
            [
                "^B1N,N,60,N,N", "^B3N,N,60,N,N", "^BAN,60,N,N,N", "^BCN,60,N,N", "^BEN,60,N,N", "^B8N,60,N,N",
                "^BXN,10,200", "^B2N,60,N,N,N", "^BIN,60,N,N", "^BD2", "^BQN,2,5", "^BUN,60,N,N,Y",
                "^B9N,60,N,N,Y", "^BSN,60,N,N", "^B7N,8,0,2,4", "^B0N,3", "^BKN,N,60,N,N", "^BMN,B,60,N,N,N", "^BLN,60,N",
            ];
            foreach (string command in commands)
            {
                ZplElementBase[] reversed = RenderHelper.Parse($"^XA^FO10,10^FR{command}^FD12345678^FS^XZ");
                ZplElementBase[] plain = RenderHelper.Parse($"^XA^FO10,10{command}^FD12345678^FS^XZ");
                ZplFieldDataElementBase reversedElement = (ZplFieldDataElementBase)reversed.Single(e => e is ZplFieldDataElementBase);
                ZplFieldDataElementBase plainElement = (ZplFieldDataElementBase)plain.Single(e => e is ZplFieldDataElementBase);
                Assert.IsTrue(reversedElement.ReversePrint, command);
                Assert.IsFalse(plainElement.ReversePrint, command);
            }
        }

        private static int CountLight(SKBitmap bitmap, int left, int top, int right, int bottom)
        {
            int count = 0;
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    if (RenderHelper.IsLight(bitmap, x, y))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // ---- 4/14: bitmap ownership ----

        [TestMethod]
        public void DrawBarcodeDrawsTheBitmapDirectlyAndDoesNotTakeOwnership()
        {
            using SKBitmap target = new(40, 40);
            using SKCanvas canvas = new(target);
            canvas.Clear(SKColors.White);
            using SKBitmap source = new(10, 10);
            source.Erase(SKColors.Black);

            ProbeDrawer drawer = new();
            drawer.Prepare(new PrinterStorage(), canvas);
            drawer.DrawProbe(source, 5, 5, 10, 10, true, FieldOrientation.Normal);

            Assert.IsTrue(RenderHelper.IsDark(target, 8, 8));
            Assert.IsTrue(RenderHelper.IsLight(target, 20, 20));

            // still usable: the drawer neither disposed nor kept it
            Assert.AreEqual(10, source.Width);
            Assert.AreEqual(SKColors.Black, source.GetPixel(0, 0));
            drawer.DrawProbe(source, 25, 25, 10, 10, true, FieldOrientation.Normal);
            Assert.IsTrue(RenderHelper.IsDark(target, 28, 28));
        }

        [TestMethod]
        public void BarcodeRenderingIsStableAcrossRepeatedDraws()
        {
            const string zpl = "^XA^FO20,20^BEN,60,Y,N^FD123456789012^FS^FO20,150^BQN,2,4^FDQA,ok^FS^XZ";
            using SKBitmap first = Render(zpl);
            using SKBitmap second = Render(zpl);
            Assert.IsTrue(RenderHelper.SameImage(first, second));
            Assert.IsGreaterThan(100, RenderHelper.CountDark(first));
        }

        // ---- 7: negative ^FT position ----

        [TestMethod]
        public void BottomAnchoredBarcodeAboveTheLabelIsClippedNotShifted()
        {
            // bottom at y=20, height 80 -> top at -60: only the lowest 20 rows are visible
            SKRectI bounds = Bounds("^XA^BY3,3,80^FT30,20^BCN,80,N^FD123456^FS^XZ");
            Assert.AreEqual(0, bounds.Top);
            Assert.AreEqual(20, bounds.Bottom);
        }

        [TestMethod]
        public void BottomAnchoredInterpretationLineAboveTheLabelIsNotShifted()
        {
            // the line below a barcode that ends above the label top stays out of the visible area
            using SKBitmap bitmap = Render("^XA^BY3,3,80^FT30,20^BCN,80,Y,N^FD123456^FS^XZ");
            SKRectI bounds = RenderHelper.DarkBounds(bitmap).Value;
            Assert.AreEqual(0, bounds.Top);
            Assert.IsLessThan(60, bounds.Bottom);
        }

        // ---- 8: field justification ----

        [TestMethod]
        public void RightJustifiedBarcodeEndsAtTheOrigin()
        {
            SKRectI left = Bounds("^XA^FT400,100,0^BCN,80,N^FD123456^FS^XZ");
            SKRectI right = Bounds("^XA^FT400,100,1^BCN,80,N^FD123456^FS^XZ");

            Assert.AreEqual(400, left.Left);
            Assert.AreEqual(400, right.Right);
            Assert.AreEqual(left.Width, right.Width);
            Assert.AreEqual(left.Top, right.Top);
        }

        [TestMethod]
        public void RightJustifiedBarcodeWithFieldOriginEndsAtTheOrigin()
        {
            SKRectI right = Bounds("^XA^FO200,100,1^BCN,80,N^FD123456^FS^XZ");
            Assert.AreEqual(200, right.Right);
            Assert.AreEqual(100, right.Top);
        }

        [TestMethod]
        public void AutoAndLeftJustificationKeepTheOrigin()
        {
            SKRectI none = Bounds("^XA^FT200,100^BCN,80,N^FD123456^FS^XZ");
            SKRectI auto = Bounds("^XA^FT200,100,2^BCN,80,N^FD123456^FS^XZ");
            SKRectI left = Bounds("^XA^FT200,100,0^BCN,80,N^FD123456^FS^XZ");
            Assert.AreEqual(none, auto);
            Assert.AreEqual(none, left);
        }

        [TestMethod]
        public void RightJustificationMovesRotatedFieldOriginBarcodeAlongTheReadingAxis()
        {
            foreach (string orientation in new[] { "R", "B" })
            {
                SKRectI left = Bounds($"^XA^FO200,300,0^BC{orientation},80,N^FD123456^FS^XZ");
                SKRectI right = Bounds($"^XA^FO200,300,1^BC{orientation},80,N^FD123456^FS^XZ");
                Assert.AreEqual(left.Left, right.Left, orientation);
                Assert.AreEqual(left.Height, right.Height, orientation);
                Assert.AreEqual(left.Top - left.Height, right.Top, orientation);
            }
        }

        [TestMethod]
        public void RightJustificationMovesRotatedFieldTypesetBarcodeAlongTheReadingAxis()
        {
            SKRectI left = Bounds("^XA^FT200,300,0^BCR,80,N^FD123456^FS^XZ");
            SKRectI right = Bounds("^XA^FT200,300,1^BCR,80,N^FD123456^FS^XZ");
            Assert.AreEqual(left.Left, right.Left);
            Assert.AreEqual(left.Top, right.Bottom);

            left = Bounds("^XA^FT400,300,0^BCI,80,N^FD123456^FS^XZ");
            right = Bounds("^XA^FT400,300,1^BCI,80,N^FD123456^FS^XZ");
            Assert.AreEqual(left.Right, right.Left);
            Assert.AreEqual(left.Top, right.Top);

            left = Bounds("^XA^FT400,300,0^BCB,80,N^FD123456^FS^XZ");
            right = Bounds("^XA^FT400,300,1^BCB,80,N^FD123456^FS^XZ");
            Assert.AreEqual(left.Bottom, right.Top);
            Assert.AreEqual(left.Left, right.Left);
        }

        [TestMethod]
        public void RightJustificationKeepsInvertedFieldOriginBarcodeOnTheXAxis()
        {
            SKRectI left = Bounds("^XA^FO300,100,0^BCI,80,N^FD123456^FS^XZ");
            SKRectI right = Bounds("^XA^FO300,100,1^BCI,80,N^FD123456^FS^XZ");
            Assert.AreEqual(left.Left - left.Width, right.Left);
            Assert.AreEqual(left.Top, right.Top);
        }

        [TestMethod]
        public void RightJustificationMovesTheInterpretationLineWithTheBarcode()
        {
            SKRectI left = Bounds("^XA^FO300,100,0^BCN,80,Y,N^FD123456^FS^XZ");
            SKRectI right = Bounds("^XA^FO300,100,1^BCN,80,Y,N^FD123456^FS^XZ");
            Assert.AreEqual(left.Left - left.Width, right.Left);
            Assert.AreEqual(left.Bottom, right.Bottom);
        }

        [TestMethod]
        [DataRow("^BXN,10,200", "^FDabc")]
        [DataRow("^BQN,2,5", "^FDQA,abc")]
        [DataRow("^B7N,8,0,2,4", "^FDabc")]
        [DataRow("^B0N,3", "^FDabc")]
        [DataRow("^BD2", "^FDabc")]
        [DataRow("^B2N,60,N,N,N", "^FD123456")]
        [DataRow("^BEN,60,N,N", "^FD123456789012")]
        [DataRow("^BUN,60,N,N,Y", "^FD12345678901")]
        public void RightJustificationAppliesToEveryBarcodeFamily(string command, string data)
        {
            string left = $"^XA^FO400,100,0{command}{data}^FS^XZ";
            string right = $"^XA^FO400,100,1{command}{data}^FS^XZ";
            SKRectI leftBounds = Bounds(left);
            SKRectI rightBounds = Bounds(right);
            Assert.AreEqual(400, leftBounds.Left, command);
            Assert.AreEqual(leftBounds.Width, rightBounds.Width, command);
            Assert.AreEqual(leftBounds.Left - leftBounds.Width, rightBounds.Left, command);
            Assert.AreEqual(leftBounds.Top, rightBounds.Top, command);
        }

        [TestMethod]
        public void AnalyzerRecordsTheJustificationOnBarcodeElements()
        {
            ZplFieldDataElementBase element = (ZplFieldDataElementBase)RenderHelper.Parse("^XA^FT200,100,1^BCN,80,N^FD123456^FS^XZ").Single(e => e is ZplFieldDataElementBase);
            Assert.AreEqual(FieldJustification.Right, element.FieldJustification);

            ZplFieldDataElementBase byDefault = (ZplFieldDataElementBase)RenderHelper.Parse("^XA^FT200,100^BCN,80,N^FD123456^FS^XZ").Single(e => e is ZplFieldDataElementBase);
            Assert.AreEqual(FieldJustification.None, byDefault.FieldJustification);
        }

        [TestMethod]
        public void JustifyOriginOnlyMovesRightJustifiedFields()
        {
            ProbeDrawer drawer = new();
            Assert.AreEqual((100f, 50f), drawer.Justify(100, 50, 30, true, FieldOrientation.Normal, FieldJustification.None));
            Assert.AreEqual((100f, 50f), drawer.Justify(100, 50, 30, true, FieldOrientation.Normal, FieldJustification.Left));
            Assert.AreEqual((100f, 50f), drawer.Justify(100, 50, 30, true, FieldOrientation.Normal, FieldJustification.Auto));
            Assert.AreEqual((70f, 50f), drawer.Justify(100, 50, 30, true, FieldOrientation.Normal, FieldJustification.Right));
            Assert.AreEqual((70f, 50f), drawer.Justify(100, 50, 30, true, FieldOrientation.Rotated180, FieldJustification.Right));
            Assert.AreEqual((70f, 50f), drawer.Justify(100, 50, 30, false, FieldOrientation.Normal, FieldJustification.Right));
            Assert.AreEqual((100f, 20f), drawer.Justify(100, 50, 30, false, FieldOrientation.Rotated90, FieldJustification.Right));
            Assert.AreEqual((130f, 50f), drawer.Justify(100, 50, 30, false, FieldOrientation.Rotated180, FieldJustification.Right));
            Assert.AreEqual((100f, 80f), drawer.Justify(100, 50, 30, false, FieldOrientation.Rotated270, FieldJustification.Right));
            Assert.AreEqual((100f, 20f), drawer.Justify(100, 50, 30, true, FieldOrientation.Rotated90, FieldJustification.Right));
            Assert.AreEqual((100f, 20f), drawer.Justify(100, 50, 30, true, FieldOrientation.Rotated270, FieldJustification.Right));
        }

        [TestMethod]
        public void LinearBarcodesJustifyWithTheirDrawers()
        {
            List<string> failures = [];
            string[] commands =
            [
                "^B1N,N,60,N,N", "^B3N,N,60,N,N", "^BAN,60,N,N,N", "^BCN,60,N,N", "^BEN,60,N,N", "^B8N,60,N,N",
                "^B2N,60,N,N,N", "^BIN,60,N,N", "^BUN,60,N,N,Y", "^B9N,60,N,N,Y", "^BSN,60,N,N", "^BKN,N,60,N,N",
                "^BMN,B,60,N,N,N", "^BLN,60,N",
            ];
            foreach (string command in commands)
            {
                SKRectI left = Bounds($"^XA^FO400,100,0{command}^FD1234567^FS^XZ");
                SKRectI right = Bounds($"^XA^FO400,100,1{command}^FD1234567^FS^XZ");
                // some symbologies end with a blank module that is part of the field width
                int shift = 400 - right.Left - left.Width;
                if (left.Left != 400 || shift < 0 || shift > 6 || right.Width != left.Width)
                {
                    failures.Add($"{command} {left} {right}");
                }
            }

            Assert.IsEmpty(failures, string.Join(", ", failures));
        }
    }
}
