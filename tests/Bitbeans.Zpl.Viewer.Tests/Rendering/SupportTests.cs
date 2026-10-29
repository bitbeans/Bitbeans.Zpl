using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Helpers;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class SupportTests
    {
        private sealed class Probe : ElementDrawerBase
        {
            public override bool CanDraw(ZplElementBase element)
            {
                return true;
            }

            public SKPoint Next(float x, float y, float width, float height, bool useFieldOrigin, FieldOrientation orientation, SKPoint current)
            {
                return this.CalculateNextDefaultPosition(x, y, width, height, useFieldOrigin, orientation, current);
            }
        }

        // ---------- ElementDrawerBase ----------

        [TestMethod]
        public void NextPosition_FieldOriginAdvancesByTheElementSize()
        {
            Probe probe = new();
            SKPoint current = new(-1, -2);

            Assert.AreEqual(new SKPoint(60, 90), probe.Next(10, 20, 50, 70, true, FieldOrientation.Normal, current));
            Assert.AreEqual(new SKPoint(10, 90), probe.Next(10, 20, 50, 70, true, FieldOrientation.Rotated90, current));
            Assert.AreEqual(new SKPoint(-40, 20), probe.Next(10, 20, 50, 70, true, FieldOrientation.Rotated180, current));
            Assert.AreEqual(new SKPoint(10, -50), probe.Next(10, 20, 50, 70, true, FieldOrientation.Rotated270, current));
        }

        [TestMethod]
        public void NextPosition_FieldTypesetAdvancesAlongTheWidth()
        {
            Probe probe = new();
            SKPoint current = new(-1, -2);

            Assert.AreEqual(new SKPoint(60, 20), probe.Next(10, 20, 50, 70, false, FieldOrientation.Normal, current));
            Assert.AreEqual(new SKPoint(10, 70), probe.Next(10, 20, 50, 70, false, FieldOrientation.Rotated90, current));
            Assert.AreEqual(new SKPoint(-40, 20), probe.Next(10, 20, 50, 70, false, FieldOrientation.Rotated180, current));
            Assert.AreEqual(new SKPoint(10, -30), probe.Next(10, 20, 50, 70, false, FieldOrientation.Rotated270, current));
        }

        [TestMethod]
        public void NextPosition_UnknownOrientationKeepsTheCurrentPosition()
        {
            Probe probe = new();
            SKPoint current = new(-1, -2);

            Assert.AreEqual(current, probe.Next(10, 20, 50, 70, true, (FieldOrientation)99, current));
            Assert.AreEqual(current, probe.Next(10, 20, 50, 70, false, (FieldOrientation)99, current));
        }

        [TestMethod]
        public void DrawerBase_DefaultsDoNothing()
        {
            Probe probe = new();
            DrawerOptions options = new();
            SKPoint current = new(3, 4);
            ZplElementBase element = new ZplGraphicBox(1, 2, 3, 4, 1);

            Assert.IsFalse(probe.IsReverseDraw(element));
            Assert.IsFalse(probe.IsWhiteDraw(element));
            Assert.IsFalse(probe.ForceBitmapDraw(element));
            Assert.AreEqual(current, probe.Draw(element, options, current));
            Assert.AreEqual(current, probe.Draw(element, options, current, InternationalFont.ZCP850));
            Assert.AreEqual(current, probe.Draw(element, options, current, InternationalFont.ZCP850, 8));
        }

        [TestMethod]
        public void DrawerBase_PrepareStoresStorageAndCanvas()
        {
            Probe probe = new();
            PrinterStorage storage = new();
            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface(10, 10);
            using (surface)
            {
                probe.Prepare(storage, canvas);

                Assert.AreSame(storage, probe.printerStorage);
                Assert.AreSame(canvas, probe.skCanvas);
            }
        }

        // ---------- SafeTextDrawing ----------

        [TestMethod]
        public void SafeText_DrawsShapedText()
        {
            using SKBitmap bitmap = new(200, 80);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKFont font = new(new FontManager().TypefaceGS, 40);
            using SKPaint paint = new() { Color = SKColors.Black, IsAntialias = true };

            canvas.DrawShapedTextSafe("A", 20, 60, SKTextAlign.Left, font, paint);

            SKRectI bounds = RenderHelper.DarkBounds(bitmap).Value;
            Assert.IsTrue(bounds.Left >= 18 && bounds.Left < 40, bounds.ToString());
            Assert.IsTrue(bounds.Bottom <= 62, bounds.ToString());
            Assert.IsTrue(bounds.Height > 10, bounds.ToString());
        }

        [TestMethod]
        public void SafeText_FallsBackToPlainTextWithoutFontAsset()
        {
            using SKBitmap bitmap = new(200, 80);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKFont font = new(SKTypeface.Empty, 40);
            using SKPaint paint = new() { Color = SKColors.Black };

            // the empty typeface has no font data to shape with: must not throw
            canvas.DrawShapedTextSafe("A", 20, 60, SKTextAlign.Left, font, paint);
        }

        [TestMethod]
        public void SafeText_OtherNullArgumentsStillThrow()
        {
            using SKBitmap bitmap = new(20, 20);
            using SKCanvas canvas = new(bitmap);
            using SKFont font = new(new FontManager().TypefaceGS, 10);

            Assert.ThrowsExactly<ArgumentNullException>(() => canvas.DrawShapedTextSafe("A", 1, 10, SKTextAlign.Left, font, null));
        }

        // ---------- ImageHelper ----------

        [TestMethod]
        public void ImageHelper_HexDataIsDecodedDirectly()
        {
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00, 0xAB }, ImageHelper.GetImageBytes("FF00aB", 3));
        }

        [TestMethod]
        public void ImageHelper_Z64DataIsInflated()
        {
            byte[] raw = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 0, 0, 0, 0, 0];
            string z64 = ZebraZ64CompressionHelper.Compress(raw);

            StringAssert.StartsWith(z64, ":Z64:");
            CollectionAssert.AreEqual(raw, ImageHelper.GetImageBytes(z64, 2));
        }

        [TestMethod]
        public void ImageHelper_B64DataIsDecoded()
        {
            byte[] raw = [9, 8, 7, 6, 5, 4, 3, 2, 1];
            string b64 = ZebraB64CompressionHelper.Compress(raw);

            StringAssert.StartsWith(b64, ":B64:");
            CollectionAssert.AreEqual(raw, ImageHelper.GetImageBytes(b64, 3));
        }

        [TestMethod]
        public void ImageHelper_AcsDataIsExpandedPerRow()
        {
            // ',' fills the rest of the row with zeros
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00 }, ImageHelper.GetImageBytes("FF,", 2));
            // 'H' repeats the next character twice
            CollectionAssert.AreEqual(new byte[] { 0xAA, 0xAA }, ImageHelper.GetImageBytes("HAHA", 2));
        }

        // ---------- UnitsHelper ----------

        [TestMethod]
        public void UnitsHelper_ConvertsMillimetersToInches()
        {
            Assert.AreEqual(1.0, UnitsHelper.ConvertMillimetersToInches(25.4), 1e-12);
            Assert.AreEqual(4.0, UnitsHelper.ConvertMillimetersToInches(101.6), 1e-12);
            Assert.AreEqual(0.0, UnitsHelper.ConvertMillimetersToInches(0));
        }
    }
}
