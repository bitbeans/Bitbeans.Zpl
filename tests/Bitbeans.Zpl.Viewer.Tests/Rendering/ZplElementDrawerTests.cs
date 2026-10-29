using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using Microsoft.Extensions.Logging;

using SkiaSharp;

using System;
using System.Linq;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class ZplElementDrawerTests
    {
        private const string Shapes =
            "^XA^FO20,20^GB120,80,80^FS^FO60,40^FR^GB40,40,40^FS^FO200,20^GB80,80,80^FS^FO220,40^FR^GB40,40,40,W^FS^FO20,200^GC100,10^FS^XZ";

        private static byte[] DrawBytes(string zpl, DrawerOptions options, double width = 50, double height = 50, int dpmm = 8, ILogger logger = null)
        {
            PrinterStorage storage = new();
            ZplAnalyzer analyzer = new(storage);
            ZplElementBase[] elements = analyzer.Analyze(zpl).LabelInfos.SelectMany(l => l.ZplElements).ToArray();
            return new ZplElementDrawer(storage, options).Draw(elements, width, height, dpmm, logger);
        }

        // ---------- Draw: output ----------

        [TestMethod]
        public void Draw_ReturnsPngSizedByLabelAndDensity()
        {
            byte[] bytes = DrawBytes("^XA^XZ", new DrawerOptions(), 25.5, 12, 12);

            using SKBitmap bmp = SKBitmap.Decode(bytes);
            Assert.AreEqual(306, bmp.Width);
            Assert.AreEqual(144, bmp.Height);
            CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes.Take(4).ToArray());
        }

        [TestMethod]
        public void Draw_DefaultLabelSizeIsFourBySixInches()
        {
            PrinterStorage storage = new();
            byte[] bytes = new ZplElementDrawer(storage).Draw([]);

            using SKBitmap bmp = SKBitmap.Decode(bytes);
            Assert.AreEqual(813, bmp.Width);
            Assert.AreEqual(1219, bmp.Height);
        }

        [TestMethod]
        public void Draw_TransparentBackgroundByDefault()
        {
            using SKBitmap bmp = SKBitmap.Decode(DrawBytes("^XA^FO20,20^GB20,20,20^FS^XZ", new DrawerOptions()));

            Assert.AreEqual(0, bmp.GetPixel(200, 200).Alpha);
            Assert.AreEqual(0, bmp.GetPixel(0, 0).Alpha);
            Assert.AreEqual(255, bmp.GetPixel(30, 30).Alpha);
            Assert.AreEqual(0, bmp.GetPixel(30, 30).Red);
        }

        [TestMethod]
        public void Draw_OpaqueBackgroundUsesBackgroundColor()
        {
            DrawerOptions options = new() { OpaqueBackground = true, BackgroundColor = new SKColor(200, 10, 20) };

            using SKBitmap bmp = SKBitmap.Decode(DrawBytes("^XA^FO20,20^GB20,20,20^FS^XZ", options));

            Assert.AreEqual(new SKColor(200, 10, 20), bmp.GetPixel(200, 200));
            Assert.AreEqual(new SKColor(200, 10, 20), bmp.GetPixel(0, 0));
            Assert.AreEqual(SKColors.Black, bmp.GetPixel(30, 30));
        }

        [TestMethod]
        public void Draw_OpaqueBackgroundIsWhiteByDefault()
        {
            DrawerOptions options = new() { OpaqueBackground = true };

            using SKBitmap bmp = SKBitmap.Decode(DrawBytes("^XA^XZ", options));

            Assert.AreEqual(SKColors.White, bmp.GetPixel(10, 10));
        }

        [TestMethod]
        public void Draw_JpegFormat()
        {
            DrawerOptions options = new() { RenderFormat = SKEncodedImageFormat.Jpeg, RenderQuality = 95, OpaqueBackground = true };

            byte[] bytes = DrawBytes("^XA^FO20,20^GB100,100,100^FS^XZ", options);

            CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8, 0xFF }, bytes.Take(3).ToArray());
            using SKBitmap bmp = SKBitmap.Decode(bytes);
            Assert.AreEqual(400, bmp.Width);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 60));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 200, 200));
        }

        [TestMethod]
        public void Draw_WebpFormat()
        {
            DrawerOptions options = new() { RenderFormat = SKEncodedImageFormat.Webp, RenderQuality = 100 };

            byte[] bytes = DrawBytes("^XA^FO20,20^GB100,100,100^FS^XZ", options);

            Assert.AreEqual("RIFF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
            Assert.AreEqual("WEBP", System.Text.Encoding.ASCII.GetString(bytes, 8, 4));
        }

        // ---------- Draw: element handling ----------

        [TestMethod]
        public void Draw_ElementsWithoutDrawerAreSkipped()
        {
            ZplElementBase[] elements =
            [
                new ZplRaw("^PW400"),
                new ZplGraphicBox(10, 10, 20, 20, 20),
            ];

            using SKBitmap bmp = RenderHelper.RenderElements(elements);

            Assert.AreEqual(new SKRectI(10, 10, 30, 30), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void Draw_InternationalFontChangeAppliesToLaterText()
        {
            using SKBitmap latin = RenderHelper.Render("^XA^CI27^FO20,20^A0N,40,40^FDä^FS^XZ");
            using SKBitmap utf = RenderHelper.Render("^XA^CI28^FO20,20^A0N,40,40^FDÃ¤^FS^XZ");

            Assert.IsNotNull(RenderHelper.DarkBounds(latin));
            Assert.IsNotNull(RenderHelper.DarkBounds(utf));
        }

        [TestMethod]
        public void Draw_ReverseDrawsInvertTheLayersBelow()
        {
            using SKBitmap bmp = RenderHelper.Render(Shapes);

            // black box with a field-reversed hole
            Assert.IsTrue(RenderHelper.IsDark(bmp, 30, 30));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 80, 60));
            // the white reverse draw keeps the box black and does not paint over the hole border
            Assert.IsTrue(RenderHelper.IsDark(bmp, 210, 30));
        }

        [TestMethod]
        public void Draw_ThrowingElementsAreLoggedAndSkipped()
        {
            PrinterStorage storage = new();
            storage.AddFile('R', "BROKEN.GRF", [1, 2, 3]);
            ZplElementBase[] elements =
            [
                new ZplImageMove(10, 80, 'R', "BROKEN.GRF", string.Empty, bottomToTop: true),
                new ZplDataMatrix(new string('9', 200), 100, 100, 100, QualityLevel.ECC200, columns: 10, rows: 10),
                new ZplGraphicBox(10, 10, 20, 20, 20),
            ];
            ListLogger logger = new();

            using SKBitmap bmp = RenderHelper.RenderElements(elements, logger: logger, storage: storage);

            Assert.AreEqual(2, logger.Entries.Count);
            Assert.IsTrue(logger.Entries.All(e => e.Level == LogLevel.Error));
            Assert.AreEqual("Error on zpl element", logger.Entries[0].Message);
            Assert.IsNotNull(logger.Entries[0].Exception);
            Assert.IsTrue(logger.Entries[1].Message.StartsWith("Error on zpl element \"" + new string('9', 200) + "\""));
            // the following element is still drawn
            Assert.AreEqual(new SKRectI(10, 10, 30, 30), RenderHelper.DarkBounds(bmp).Value);
        }

        // ---------- DrawSurface ----------

        private static SKBitmap DrawOnSurface(string zpl, ILogger logger = null, int width = 400, int height = 400)
        {
            PrinterStorage storage = new();
            ZplElementBase[] elements = new ZplAnalyzer(storage).Analyze(zpl).LabelInfos.SelectMany(l => l.ZplElements).ToArray();
            using SKSurface surface = SKSurface.Create(new SKImageInfo(width, height));
            new ZplElementDrawer(storage, new DrawerOptions()).DrawSurface(surface, elements, 50, 50, 8, logger);
            using SKImage image = surface.Snapshot();
            return SKBitmap.FromImage(image);
        }

        [TestMethod]
        public void DrawSurface_MatchesDraw()
        {
            using SKBitmap viaDraw = SKBitmap.Decode(DrawBytes(Shapes, new DrawerOptions()));
            using SKBitmap viaSurface = DrawOnSurface(Shapes);

            Assert.IsTrue(RenderHelper.SameImage(viaDraw, viaSurface));
            Assert.AreEqual(0, viaSurface.GetPixel(399, 399).Alpha);
        }

        [TestMethod]
        public void DrawSurface_ClearsTheSurfaceFirst()
        {
            PrinterStorage storage = new();
            using SKSurface surface = SKSurface.Create(new SKImageInfo(100, 100));
            surface.Canvas.Clear(SKColors.Red);

            new ZplElementDrawer(storage).DrawSurface(surface, [new ZplGraphicBox(10, 10, 20, 20, 20)], 12.5, 12.5);

            using SKImage image = surface.Snapshot();
            using SKBitmap bmp = SKBitmap.FromImage(image);
            Assert.AreEqual(0, bmp.GetPixel(60, 60).Alpha);
            Assert.AreEqual(SKColors.Black, bmp.GetPixel(15, 15));
        }

        [TestMethod]
        public void DrawSurface_SkipsUnknownElementsAndHandlesFontChanges()
        {
            PrinterStorage storage = new();
            using SKSurface surface = SKSurface.Create(new SKImageInfo(100, 100));
            ZplElementBase[] elements =
            [
                new ZplChangeInternationalFont(InternationalFont.UTF8),
                new ZplRaw("^PW400"),
                new ZplGraphicBox(10, 10, 20, 20, 20),
            ];

            new ZplElementDrawer(storage).DrawSurface(surface, elements, 12.5, 12.5);

            using SKImage image = surface.Snapshot();
            using SKBitmap bmp = SKBitmap.FromImage(image);
            Assert.AreEqual(SKColors.Black, bmp.GetPixel(15, 15));
        }

        [TestMethod]
        public void DrawSurface_LogsThrowingElementsAndContinues()
        {
            PrinterStorage storage = new();
            storage.AddFile('R', "BROKEN.GRF", [1, 2, 3]);
            ZplElementBase[] elements =
            [
                new ZplImageMove(10, 80, 'R', "BROKEN.GRF", string.Empty, bottomToTop: true),
                new ZplDataMatrix(new string('9', 200), 100, 100, 100, QualityLevel.ECC200, columns: 10, rows: 10),
                new ZplGraphicBox(10, 10, 20, 20, 20),
            ];
            ListLogger logger = new();
            using SKSurface surface = SKSurface.Create(new SKImageInfo(100, 100));

            new ZplElementDrawer(storage).DrawSurface(surface, elements, 12.5, 12.5, 8, logger);

            Assert.AreEqual(2, logger.Entries.Count);
            Assert.AreEqual("Error on zpl element", logger.Entries[0].Message);
            Assert.IsTrue(logger.Entries[1].Message.StartsWith("Error on zpl element \"" + new string('9', 200) + "\""));
            using SKImage image = surface.Snapshot();
            using SKBitmap bmp = SKBitmap.FromImage(image);
            Assert.AreEqual(SKColors.Black, bmp.GetPixel(15, 15));
        }

        [TestMethod]
        public void DrawSurface_LogsBarcodeContentOfFailedBarcodes()
        {
            PrinterStorage storage = new();
            ZplElementBase[] elements = new ZplAnalyzer(storage)
                .Analyze("^XA^BY3,3,80^FO40,120^BMN,B,80,Y,N,N^FDABC^FS^XZ").LabelInfos.SelectMany(l => l.ZplElements).ToArray();
            ListLogger logger = new();
            using SKSurface surface = SKSurface.Create(new SKImageInfo(400, 400));

            new ZplElementDrawer(storage).DrawSurface(surface, elements, 50, 50, 8, logger);

            Assert.AreEqual(1, logger.Entries.Count);
            Assert.IsTrue(logger.Entries[0].Message.Contains("ABC"));
        }

        [TestMethod]
        public void DrawSurface_WorksWithoutLogger()
        {
            PrinterStorage storage = new();
            storage.AddFile('R', "BROKEN.GRF", [1, 2, 3]);
            using SKSurface surface = SKSurface.Create(new SKImageInfo(100, 100));

            new ZplElementDrawer(storage).DrawSurface(
                surface,
                [new ZplImageMove(10, 80, 'R', "BROKEN.GRF", string.Empty, bottomToTop: true)],
                12.5,
                12.5);

            using SKImage image = surface.Snapshot();
            using SKBitmap bmp = SKBitmap.FromImage(image);
            Assert.AreEqual(0, bmp.GetPixel(50, 50).Alpha);
        }
    }
}
