using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Models;
using Bitbeans.Zpl.Viewer.Tests.Analyzers;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    /// <summary>Regression tests for the verification review: omitted names, graphic names with extension, ~DY file names and disposing during a draw.</summary>
    [TestClass]
    public class VerificationFixTests
    {
        // an 8x8 black frame, 1 byte per row
        private const string Frame8 = "FF818181818181FF";

        private static string SolidPngHex(int width, int height)
        {
            using SKBitmap bitmap = new(width, height);
            bitmap.Erase(SKColors.Black);
            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            return ByteHelper.BytesToHex(data.ToArray());
        }

        // ---------- omitted format names ----------

        [TestMethod]
        [DataRow("^XA^DF^FS^FO0,0^FDX^FS^XZ^XA^XF^FS^XZ")]
        [DataRow("^XA^DFR:^FS^FO0,0^FDX^FS^XZ^XA^XFR:UNKNOWN.ZPL^FS^XZ")]
        [DataRow("^XA^DFUNKNOWN.ZPL^FS^FO0,0^FDX^FS^XZ^XA^XFR:^FS^XZ")]
        public void FormatWithoutNameIsStoredAndRecalledAsUnknown(string zpl)
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(zpl);

            Assert.IsEmpty(info.Errors, string.Join("\n", info.Errors));
            Assert.AreEqual("X", AnalyzerTestHelper.AllElements(info).OfType<ZplTextField>().Single().Text);
        }

        // ---------- ^XG with extension ----------

        [TestMethod]
        public void RecallGraphicWithLongNameAndExtensionIsDrawn()
        {
            string zpl = "~DGR:SAMPLE.GRF,8,1," + Frame8 + "^XA^FO15,25^XGR:SAMPLE.GRF,1,1^FS^XZ";

            Assert.IsEmpty(AnalyzerTestHelper.Errors(zpl));
            using SKBitmap bmp = RenderHelper.Render(zpl);
            Assert.AreEqual(new SKRectI(15, 25, 23, 33), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void CoreRecallGraphicFindsTheParsedDownload()
        {
            PrinterStorage storage = new();
            RenderHelper.Parse("~DGR:SAMPLE.GRF,8,1," + Frame8, storage);

            using SKBitmap bmp = RenderHelper.RenderElements([new ZplRecallGraphic(15, 25, 'R', "SAMPLE")], storage: storage);

            Assert.AreEqual(new SKRectI(15, 25, 23, 33), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        [DataRow("~DGR:,8,1,{0}^XA^FO15,25^XGR:,1,1^FS^XZ")]
        [DataRow("~DGR:.GRF,8,1,{0}^XA^FO15,25^XGR:UNKNOWN.GRF^FS^XZ")]
        [DataRow("~DGLOGO,8,1,{0}^XA^FO15,25^XGR:LOGO.GRF^FS^XZ")]
        [DataRow("~DGR:LOGO.XYZ,8,1,{0}^XA^FO15,25^XGLOGO^FS^XZ")]
        [DataRow("~DGR:LOGO,8,1,{0}^XA^FO15,25^IMR:LOGO.GRF^FS^XZ")]
        public void DownloadGraphicIsStoredAsGrfAndOmittedNamesAsUnknown(string format)
        {
            string zpl = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, Frame8);

            Assert.IsEmpty(AnalyzerTestHelper.Errors(zpl));
            using SKBitmap bmp = RenderHelper.Render(zpl);
            Assert.AreEqual(new SKRectI(15, 25, 23, 33), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void RecallGraphicNameThatIsTooLongIsReported()
        {
            Assert.HasCount(1, AnalyzerTestHelper.Errors("^XA^FO0,0^XGR:ABCDEFGHI.GRF,1,1^FS^XZ"));
        }

        // ---------- ~DY file names ----------

        [TestMethod]
        [DataRow("OBJ", "B", "OBJ.BMP")]
        [DataRow("OBJ", "E", "OBJ.TTE")]
        [DataRow("OBJ", "g", "OBJ.GRF")]
        [DataRow("OBJ", "P", "OBJ.PNG")]
        [DataRow("OBJ", "T", "OBJ.TTF")]
        [DataRow("OBJ", "X", "OBJ.PCX")]
        [DataRow("OBJ", "NRD", "OBJ.NRD")]
        [DataRow("OBJ", "Q", "OBJ.GRF")]
        [DataRow("OBJ", "C", "OBJ.WML")]
        [DataRow("OBJ", "F", "OBJ.HTM")]
        [DataRow("OBJ", "H", "OBJ.GET")]
        [DataRow("OBJ", "PAC", "OBJ.PAC")]
        [DataRow(".PNG", "G", "UNKNOWN.PNG")]
        [DataRow("OBJ", "", "OBJ.GRF")]
        [DataRow("OBJ.PNG", "G", "OBJ.PNG")]
        [DataRow("", "P", "UNKNOWN.PNG")]
        public void DownloadObjectIsStoredUnderNameAndExtension(string name, string extension, string fileName)
        {
            PrinterStorage storage = new();

            AnalyzeInfo info = AnalyzerTestHelper.Analyze($"~DYE:{name},B,{extension},2,1,0102", storage);

            Assert.IsEmpty(info.Errors, string.Join("\n", info.Errors));
            CollectionAssert.AreEqual(new byte[] { 1, 2 }, storage.GetFile('E', fileName));
        }

        [TestMethod]
        public void DownloadedPngIsFoundByImageMove()
        {
            string zpl = "~DYR:LOGO,P,P,1,," + SolidPngHex(6, 4) + "^XA^FO15,25^IMR:LOGO.PNG^FS^XZ";

            using SKBitmap bmp = RenderHelper.Render(zpl);

            Assert.AreEqual(new SKRectI(15, 25, 21, 29), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        [DataRow("^IMR:.PNG")]
        [DataRow("^IMR:UNKNOWN.PNG")]
        public void DownloadWithoutNameIsFoundByImageMoveWithoutName(string imageMove)
        {
            string zpl = "~DYR:,P,P,1,," + SolidPngHex(6, 4) + "^XA^FO15,25" + imageMove + "^FS^XZ";

            using SKBitmap bmp = RenderHelper.Render(zpl);

            Assert.AreEqual(new SKRectI(15, 25, 21, 29), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void DownloadedBmpIsDrawn()
        {
            // 1x1 black 24 bit BMP
            const string Bmp = "424D3A0000000000000036000000280000000100000001000000010018000000000004000000130B0000130B0000000000000000000000000000";

            using SKBitmap bmp = RenderHelper.Render("~DYR:DOT,B,B,58,," + Bmp + "^XA^FO10,10^IMR:DOT.BMP^FS^XZ");

            Assert.AreEqual(new SKRectI(10, 10, 11, 11), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void CoreImageMoveFindsTheParsedDownload()
        {
            PrinterStorage storage = new();
            RenderHelper.Parse("~DYR:LOGO,P,P,1,," + SolidPngHex(6, 4), storage);

            using SKBitmap bmp = RenderHelper.RenderElements([new ZplImageMove(15, 25, 'R', "LOGO", "PNG")], storage: storage);

            Assert.AreEqual(new SKRectI(15, 25, 21, 29), RenderHelper.DarkBounds(bmp).Value);
        }

        // ---------- disposing during a draw ----------

        private static IEnumerable<ZplElementBase> DisposeAfterFirstField(ZplElementDrawer drawer)
        {
            yield return new ZplTextField("A", 10, 10, new ZplFont(fontWidth: 40, fontHeight: 40));
            drawer.Dispose();
            yield return new ZplTextField("B", 10, 200, new ZplFont(fontWidth: 40, fontHeight: 40));
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void DisposeDuringDrawFinishesTheRunningDraw(bool callerOptions)
        {
            using FontManager callerManager = new();
            ZplElementDrawer drawer = callerOptions
                ? new(new PrinterStorage(), new DrawerOptions(callerManager))
                : new(new PrinterStorage());
            ListLogger logger = new();
            using SKSurface surface = SKSurface.Create(new SKImageInfo(400, 400));

            drawer.DrawSurface(surface, DisposeAfterFirstField(drawer), 50, 50, 8, logger);

            Assert.IsEmpty(logger.Entries, string.Join("\n", logger.Entries.Select(e => e.Message)));
            using SKImage image = surface.Snapshot();
            using SKBitmap bitmap = SKBitmap.FromImage(image);
            Assert.IsGreaterThan(0, RenderHelper.CountDark(bitmap, 0, 0, 400, 100));
            Assert.IsGreaterThan(0, RenderHelper.CountDark(bitmap, 0, 190, 400, 300));
            Assert.ThrowsExactly<ObjectDisposedException>(() => drawer.Draw([]));
            Assert.ThrowsExactly<ObjectDisposedException>(() => drawer.DrawSurface(surface, []));
            Assert.IsNotNull(callerManager.TypefaceGS);
        }

        [TestMethod]
        public void OwnedFontManagerIsReleasedWhenTheLastDrawEnds()
        {
            ZplElementDrawer drawer = new(new PrinterStorage());
            FontManager manager = ((DrawerOptions)typeof(ZplElementDrawer)
                .GetField("drawerOptions", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(drawer)).FontManager;
            using SKSurface surface = SKSurface.Create(new SKImageInfo(400, 400));

            drawer.DrawSurface(surface, DisposeAfterFirstField(drawer), 50, 50, 8);

            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.TypefaceGS);
        }
    }
}
