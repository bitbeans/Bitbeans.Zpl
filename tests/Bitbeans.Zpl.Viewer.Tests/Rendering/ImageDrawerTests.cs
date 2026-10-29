using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class ImageDrawerTests
    {
        // an 8x8 black frame, 1 byte per row: FF 81 81 81 81 81 81 FF
        private const string Frame8 = "FF818181818181FF";

        private static byte[] SolidPng(int width, int height)
        {
            using SKBitmap bitmap = new(width, height);
            bitmap.Erase(SKColors.Black);
            using SKImage image = SKImage.FromBitmap(bitmap);
            return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
        }

        private static string SolidPngHex(int width, int height)
        {
            return ByteHelper.BytesToHex(SolidPng(width, height));
        }

        private static IPrinterStorage StorageWith(int width, int height)
        {
            PrinterStorage storage = new();
            storage.AddFile('R', "LOGO.GRF", SolidPng(width, height));
            return storage;
        }

        private static SKPoint DrawDirect(ElementDrawerBase drawer, IPrinterStorage storage, ZplElementBase element, SKPoint current)
        {
            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                drawer.Prepare(storage, canvas);
                return drawer.Draw(element, new DrawerOptions(), current, InternationalFont.ZCP850, 8);
            }
        }

        // ---------- ^GF graphic field ----------

        [TestMethod]
        public void GraphicField_DrawsDecodedFrameAtOrigin()
        {
            using SKBitmap bmp = RenderHelper.Render("^XA^FO10,20^GFA,8,8,1," + Frame8 + "^FS^XZ");

            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(new SKRectI(10, 20, 18, 28), bounds);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 10, 24));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 17, 24));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 13, 24));
            Assert.AreEqual(28, RenderHelper.CountDark(bmp));
        }

        [TestMethod]
        public void GraphicField_FieldTypesetPlacesBottomAtY()
        {
            using SKBitmap bmp = RenderHelper.Render("^XA^FT10,50^GFA,8,8,1," + Frame8 + "^FS^XZ");

            Assert.AreEqual(new SKRectI(10, 42, 18, 50), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void GraphicField_FieldTypesetAboveTheLabelIsClipped()
        {
            using SKBitmap bmp = RenderHelper.Render("^XA^FT10,3^GFA,8,8,1," + Frame8 + "^FS^XZ");

            // the bottom stays at y=3, the part above the label is clipped
            Assert.AreEqual(new SKRectI(10, 0, 18, 3), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void GraphicField_UsesDefaultPositionAfterPreviousGraphic()
        {
            ZplElementBase[] elements =
            [
                new ZplGraphicField(30, 40, 8, 8, 1, "FFFFFFFFFFFFFFFF"),
                RenderHelper.WithDefaultPosition(new ZplGraphicField(0, 0, 2, 2, 1, "FFFF"))
            ];

            using SKBitmap bmp = RenderHelper.RenderElements(elements);

            Assert.IsTrue(RenderHelper.IsDark(bmp, 30, 40));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 37, 47));
            // the next default position is right below-right of the first graphic: (38, 48)
            Assert.AreEqual(new SKRectI(38, 48, 46, 50), RenderHelper.DarkBounds(bmp, 38, 48).Value);
            Assert.AreEqual(8 * 8 + 8 * 2, RenderHelper.CountDark(bmp));
        }

        [TestMethod]
        public void GraphicField_NextDefaultPositionWithoutFieldOriginKeepsY()
        {
            GraphicFieldElementDrawer drawer = new();
            ZplGraphicField element = new(10, 60, 8, 8, 1, "FFFFFFFFFFFFFFFF", bottomToTop: true);

            SKPoint next = DrawDirect(drawer, new PrinterStorage(), element, SKPoint.Empty);

            // field typeset: y is moved up by the image height, x advances by its width
            Assert.AreEqual(new SKPoint(18, 52), next);
        }

        [TestMethod]
        public void GraphicField_IgnoresOtherElements()
        {
            GraphicFieldElementDrawer drawer = new();
            ZplImageMove other = new(1, 2, 'R', "LOGO.GRF", string.Empty);
            SKPoint current = new(5, 6);

            Assert.IsFalse(drawer.CanDraw(other));
            Assert.AreEqual(current, DrawDirect(drawer, new PrinterStorage(), other, current));
        }

        // ---------- ~DG + ^IM image move ----------

        [TestMethod]
        public void ImageMove_DrawsDownloadedGraphic()
        {
            string zpl = "~DGR:LOGO.GRF,8,1," + Frame8 + "^XA^FO15,25^IMR:LOGO.GRF^FS^XZ";

            using SKBitmap bmp = RenderHelper.Render(zpl);

            Assert.AreEqual(new SKRectI(15, 25, 23, 33), RenderHelper.DarkBounds(bmp).Value);
            Assert.IsTrue(RenderHelper.IsLight(bmp, 18, 29));
        }

        [TestMethod]
        public void ImageMove_UnknownObjectDrawsNothing()
        {
            ListLogger logger = new();

            using SKBitmap bmp = RenderHelper.Render("^XA^FO15,25^IMR:NOPE.GRF^FS^XZ", logger: logger);

            Assert.IsNull(RenderHelper.DarkBounds(bmp));
            // regression: a missing object used to throw while decoding the empty data and was logged as an error
            Assert.AreEqual(0, logger.Entries.Count);
        }

        [TestMethod]
        public void ImageMove_UnknownObjectKeepsCurrentPosition()
        {
            ImageMoveElementDrawer drawer = new();
            ZplImageMove element = new(15, 25, 'R', "NOPE.GRF", string.Empty);
            SKPoint current = new(7, 9);

            Assert.AreEqual(current, DrawDirect(drawer, new PrinterStorage(), element, current));
        }

        [TestMethod]
        public void ImageMove_FieldTypesetPlacesBottomAtY()
        {
            ZplElementBase[] elements = [new ZplImageMove(20, 40, 'R', "LOGO.GRF", string.Empty, bottomToTop: true)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, storage: StorageWith(10, 6));

            Assert.AreEqual(new SKRectI(20, 34, 30, 40), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void ImageMove_FieldTypesetAboveTheLabelIsClipped()
        {
            ZplElementBase[] elements = [new ZplImageMove(20, 2, 'R', "LOGO.GRF", string.Empty, bottomToTop: true)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, storage: StorageWith(10, 6));

            Assert.AreEqual(new SKRectI(20, 0, 30, 2), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void ImageMove_UsesDefaultPositionAndAdvances()
        {
            ZplElementBase[] elements =
            [
                new ZplImageMove(30, 40, 'R', "LOGO.GRF", string.Empty),
                RenderHelper.WithDefaultPosition(new ZplImageMove(0, 0, 'R', "LOGO.GRF", string.Empty)),
                RenderHelper.WithDefaultPosition(new ZplImageMove(0, 0, 'R', "LOGO.GRF", string.Empty)),
            ];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, storage: StorageWith(10, 6));

            // first (30,40)-(40,46), the next one at (40,46), the third one at (50,52)
            Assert.IsTrue(RenderHelper.IsDark(bmp, 30, 40));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 40, 46));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 49, 51));
            Assert.AreEqual(new SKRectI(30, 40, 60, 58), RenderHelper.DarkBounds(bmp).Value);
            Assert.AreEqual(3 * 10 * 6, RenderHelper.CountDark(bmp));
        }

        [TestMethod]
        public void ImageMove_IgnoresOtherElements()
        {
            ImageMoveElementDrawer drawer = new();
            ZplRecallGraphic other = new(1, 2, 'R', "LOGO.GRF");
            SKPoint current = new(5, 6);

            Assert.IsFalse(drawer.CanDraw(other));
            Assert.AreEqual(current, DrawDirect(drawer, StorageWith(4, 4), other, current));
        }

        // ---------- ^XG recall graphic ----------

        [TestMethod]
        public void RecallGraphic_DrawsDownloadedGraphic()
        {
            string zpl = "~DGR:LOGO.GRF,8,1," + Frame8 + "^XA^FO15,25^XGR:LOGO.GRF,1,1^FS^XZ";

            using SKBitmap bmp = RenderHelper.Render(zpl);

            Assert.AreEqual(new SKRectI(15, 25, 23, 33), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void RecallGraphic_MagnificationScalesTheImage()
        {
            string zpl = "~DGR:LOGO.GRF,8,1," + Frame8 + "^XA^FO15,25^XGR:LOGO.GRF,3,2^FS^XZ";

            using SKBitmap bmp = RenderHelper.Render(zpl);

            // 8x8 magnified by 3 horizontally and 2 vertically
            Assert.AreEqual(new SKRectI(15, 25, 39, 41), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void RecallGraphic_UnknownImageDrawsNothing()
        {
            using SKBitmap bmp = RenderHelper.Render("^XA^FO15,25^XGR:NOPE.GRF,1,1^FS^XZ");

            Assert.IsNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void RecallGraphic_UnknownImageKeepsCurrentPosition()
        {
            RecallGraphicElementDrawer drawer = new();
            ZplRecallGraphic element = new(15, 25, 'R', "NOPE", 1, 1, useDefaultPosition: false);
            SKPoint current = new(7, 9);

            Assert.AreEqual(current, DrawDirect(drawer, new PrinterStorage(), element, current));
        }

        [TestMethod]
        public void RecallGraphic_FieldTypesetPlacesBottomAtY()
        {
            ZplElementBase[] elements = [new ZplRecallGraphic(20, 40, 'R', "LOGO.GRF", 2, 3, bottomToTop: true, useDefaultPosition: false)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, storage: StorageWith(5, 4));

            // 5x4 magnified by 2 and 3 is 10x12
            Assert.AreEqual(new SKRectI(20, 28, 30, 40), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void RecallGraphic_FieldTypesetAboveTheLabelIsClipped()
        {
            ZplElementBase[] elements = [new ZplRecallGraphic(20, 5, 'R', "LOGO.GRF", 2, 3, bottomToTop: true, useDefaultPosition: false)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, storage: StorageWith(5, 4));

            Assert.AreEqual(new SKRectI(20, 0, 30, 5), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void RecallGraphic_DefaultPositionFollowsPreviousElement()
        {
            ZplElementBase[] elements =
            [
                new ZplRecallGraphic(30, 40, 'R', "LOGO.GRF", 2, 2, useDefaultPosition: false),
                new ZplRecallGraphic(0, 0, 'R', "LOGO.GRF", 1, 1, useDefaultPosition: true),
            ];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, storage: StorageWith(5, 4));

            // first: (30,40)-(40,48), the second one starts at its end (40,48) and is 5x4
            Assert.AreEqual(new SKRectI(30, 40, 45, 52), RenderHelper.DarkBounds(bmp).Value);
            Assert.AreEqual(10 * 8 + 5 * 4, RenderHelper.CountDark(bmp));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 44, 51));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 39, 47));
        }

        [TestMethod]
        public void RecallGraphic_IgnoresOtherElements()
        {
            RecallGraphicElementDrawer drawer = new();
            ZplImageMove other = new(1, 2, 'R', "LOGO.GRF", string.Empty);
            SKPoint current = new(5, 6);

            Assert.IsFalse(drawer.CanDraw(other));
            Assert.AreEqual(current, DrawDirect(drawer, StorageWith(4, 4), other, current));
        }
    }
}
