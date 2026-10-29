using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Models;
using Bitbeans.Zpl.Viewer.Tests.Analyzers;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    /// <summary>Regression tests for the final pre-release review findings of the viewer.</summary>
    [TestClass]
    public class FinalReviewFixTests
    {
        private const string Frame8 = "FF818181818181FF";

        private static ZplTextField[] Texts(AnalyzeInfo info, int label)
        {
            return info.LabelInfos[label].ZplElements.OfType<ZplTextField>().ToArray();
        }

        private static DrawerOptions Bitmap()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider(["A"]);
            return options;
        }

        private static SKRectI Band(SKBitmap bmp, int index)
        {
            (int top, int bottom) = RenderHelper.RowBands(bmp)[index];
            return RenderHelper.DarkBounds(bmp, 0, top, int.MaxValue, bottom).Value;
        }

        private static int CountLight(SKBitmap bmp, int left, int top, int right, int bottom)
        {
            int count = 0;
            for (int y = top; y < bottom; y++)
            {
                for (int x = left; x < right; x++)
                {
                    if (RenderHelper.IsLight(bmp, x, y))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        // ---------- 1: default data of stored formats ----------

        [TestMethod]
        public void StoredFormat_FieldDataIsTheDefaultOfAllFieldsWithTheNumber()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:F.ZPL^FS^FO0,0^FN1^FS^FO0,40^FN1^FDHello^FS^XZ^XA^XFR:F.ZPL^FS^XZ");

            Assert.IsEmpty(info.Errors);
            ZplTextField[] texts = Texts(info, 0);
            Assert.HasCount(2, texts);
            Assert.AreEqual("Hello", texts[0].Text);
            Assert.AreEqual(0, texts[0].PositionY);
            Assert.AreEqual("Hello", texts[1].Text);
            Assert.AreEqual(40, texts[1].PositionY);
        }

        [TestMethod]
        public void StoredFormat_RecalledDataOverridesTheDefault()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:F.ZPL^FS^FO0,0^FN1^FS^FO0,40^FN1^FDHello^FS^FO0,80^FN2^FS^XZ" +
                "^XA^XFR:F.ZPL^FS^FN1^FDOverride^FS^XZ^XA^XFR:F.ZPL^FS^XZ");

            Assert.IsEmpty(info.Errors);
            ZplTextField[] first = Texts(info, 0);
            Assert.HasCount(2, first);
            Assert.AreEqual("Override", first[0].Text);
            Assert.AreEqual("Override", first[1].Text);

            // the stored format keeps its default for the next label; a field without any data is still omitted
            ZplTextField[] second = Texts(info, 1);
            Assert.HasCount(2, second);
            Assert.AreEqual("Hello", second[0].Text);
        }

        [TestMethod]
        public void StoredFormat_DefaultIsMergedOnTheModelLevel()
        {
            ZplTextField Text(string text, int y) => new(text, 10, y, ZplConstants.Font.Default);
            LabelInfo format = new()
            {
                DownloadFormatName = "FMT",
                ZplElements = [new ZplFieldNumber(1, Text(null, 10)), new ZplRecallFieldNumber(1, "default"), new ZplFieldNumber(2, Text(null, 20))],
            };

            List<LabelInfo> merged = new FormatMerger().MergeFormats([
                format,
                new LabelInfo { ZplElements = [new ZplRecallFormat("FMT")] },
                new LabelInfo { ZplElements = [new ZplRecallFormat("FMT"), new ZplRecallFieldNumber(1, "own")] },
            ]);

            Assert.HasCount(2, merged);
            Assert.HasCount(1, merged[0].ZplElements);
            Assert.AreEqual("default", ((ZplTextField)merged[0].ZplElements[0]).Text);
            Assert.AreEqual("own", ((ZplTextField)merged[1].ZplElements[0]).Text);
        }

        // ---------- 3: reverse field blocks ----------

        [TestMethod]
        public void ReverseFieldBlock_OverlappingLinesDoNotCancelOut_Outline()
        {
            const string box = "^FO40,40^GB200,80,80^FS^FO50,50^FR^A0N,40,40^FB100,1,0,L,0^FD";
            using SKBitmap doubled = RenderHelper.Render("^XA" + box + "A\\&A^FS^XZ");
            using SKBitmap single = RenderHelper.Render("^XA" + box + "A^FS^XZ");

            // antialiased edges add up where the lines overlap, so the images are similar but not identical
            int singleLight = CountLight(single, 50, 50, 100, 100);
            Assert.IsGreaterThan(0, singleLight, "The text must invert the black box");
            Assert.IsGreaterThan(singleLight / 2, CountLight(doubled, 50, 50, 100, 100), "The overlapping lines must not cancel out");
        }

        [TestMethod]
        public void ReverseFieldBlock_OverlappingLinesDoNotCancelOut_Bitmap()
        {
            const string box = "^FO40,40^GB200,80,80^FS^FO50,50^FR^AAN,9,5^FB100,1,0,L,0^FD";
            using SKBitmap doubled = RenderHelper.Render("^XA" + box + "A\\&A^FS^XZ", Bitmap());
            using SKBitmap single = RenderHelper.Render("^XA" + box + "A^FS^XZ", Bitmap());

            Assert.IsTrue(RenderHelper.SameImage(doubled, single));
            Assert.IsGreaterThan(0, CountLight(single, 50, 50, 100, 100), "The text must invert the black box");
        }

        // ---------- 4: ownership of the font manager ----------

        private static FontManager FontManagerOf(ZplElementDrawer drawer)
        {
            DrawerOptions options = (DrawerOptions)typeof(ZplElementDrawer)
                .GetField("drawerOptions", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(drawer);
            return options.FontManager;
        }

        [TestMethod]
        public void FontManager_TypefaceGsIsLazyAndCached()
        {
            using FontManager manager = new();

            SKTypeface typeface = manager.TypefaceGS;

            Assert.IsNotNull(typeface);
            Assert.AreSame(typeface, manager.TypefaceGS);
        }

        [TestMethod]
        public void FontManager_IsUnusableAfterDispose()
        {
            FontManager manager = new();
            _ = manager.TypefaceGS;
            _ = manager.FontLoader("0");
            manager.Dispose();
            manager.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.TypefaceGS);
            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.FontLoader("0"));
            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.FontLoader("A"));
            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.RegisterTypeface(SKTypeface.Default));
        }

        [TestMethod]
        public void FontManager_DisposeWithoutAccessWorks()
        {
            FontManager manager = new();

            manager.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.TypefaceGS);
        }

        [TestMethod]
        public void Drawer_DisposesTheFontManagerOfItsDefaultOptions()
        {
            ZplElementDrawer drawer = new(new PrinterStorage());
            FontManager manager = FontManagerOf(drawer);
            _ = manager.TypefaceGS;

            drawer.Dispose();
            drawer.Dispose();

            Assert.ThrowsExactly<ObjectDisposedException>(() => manager.TypefaceGS);
            Assert.ThrowsExactly<ObjectDisposedException>(() => drawer.Draw([]));
            using SKSurface surface = SKSurface.Create(new SKImageInfo(10, 10));
            Assert.ThrowsExactly<ObjectDisposedException>(() => drawer.DrawSurface(surface, []));
        }

        [TestMethod]
        public void Drawer_DoesNotDisposeCallerProvidedOptions()
        {
            using FontManager manager = new();
            ZplElementDrawer drawer = new(new PrinterStorage(), new DrawerOptions(manager));

            drawer.Dispose();

            Assert.IsNotNull(manager.TypefaceGS);
            Assert.ThrowsExactly<ObjectDisposedException>(() => drawer.Draw([]));
        }

        [TestMethod]
        public void Drawer_DefaultOptionsStillDraw()
        {
            using ZplElementDrawer drawer = new(new PrinterStorage());

            Assert.IsNotEmpty(drawer.Draw([new ZplTextField("A", 10, 10, ZplConstants.Font.Default)]));
        }

        // ---------- 5: ^GF ----------

        [TestMethod]
        public void GraphicField_OmittedFormatDefaultsToA()
        {
            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^FO0,0^GF,8,8,1," + Frame8);

            Assert.AreEqual('A', field.CompressionType);
            Assert.AreEqual(Frame8, field.Data);
        }

        [TestMethod]
        public void GraphicField_LowerCaseFormatIsAccepted()
        {
            Assert.AreEqual('A', AnalyzerTestHelper.Single<ZplGraphicField>("^FO0,0^GFa,8,8,1," + Frame8).CompressionType);
        }

        [TestMethod]
        public void GraphicField_BinaryFormatIsRejectedWithAnError()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(AnalyzerTestHelper.Label("^FO0,0^GFB,8,8,1," + Frame8));

            Assert.HasCount(1, info.Errors);
            StringAssert.Contains(info.Errors[0], "format B");
            Assert.IsEmpty(info.LabelInfos[0].ZplElements);
        }

        [TestMethod]
        public void GraphicField_OmittedFormatRendersLikeA()
        {
            using SKBitmap omitted = RenderHelper.Render("^XA^FO10,20^GF,8,8,1," + Frame8 + "^FS^XZ");
            using SKBitmap explicitA = RenderHelper.Render("^XA^FO10,20^GFA,8,8,1," + Frame8 + "^FS^XZ");

            Assert.IsTrue(RenderHelper.SameImage(omitted, explicitA));
            Assert.IsNotNull(RenderHelper.DarkBounds(omitted));
        }

        // ---------- 6: hanging indent of ^FB lines 2+ ----------

        private static string Fb(int width, string justification, int indent, string text)
        {
            return $"^XA^FO50,50^AAN,9,5^FB{width},3,3,{justification},{indent}^FD{text}^FS^XZ";
        }

        [TestMethod]
        public void Bitmap_CenteredLinesRespectTheHangingIndent()
        {
            // line 2 "CCCC" is 24 px wide: centered in the area from the indent (10) to the block width (60)
            using SKBitmap bmp = RenderHelper.Render(Fb(60, "C", 10, "AAAA BBBB CCCC"), Bitmap(), 100, 100);

            Assert.AreEqual(50 + 10 + 13, Band(bmp, 1).Left);
        }

        [TestMethod]
        public void Bitmap_RightLinesKeepTheRightEdgeWithAHangingIndent()
        {
            using SKBitmap bmp = RenderHelper.Render(Fb(60, "R", 10, "AAAA BBBB CCCC"), Bitmap(), 100, 100);

            Assert.AreEqual(50 + 60 - 24, Band(bmp, 1).Left);
        }

        [TestMethod]
        public void Bitmap_JustifiedLinesUseTheAreaAfterTheHangingIndent()
        {
            using SKBitmap bmp = RenderHelper.Render(Fb(60, "J", 10, "AAAA BBBB CC DD EEEE"), Bitmap(), 100, 100);

            // line 2 "CC DD" fills the 50 px after the indent
            Assert.AreEqual(50 + 10, Band(bmp, 1).Left);
            Assert.AreEqual(50 + 10 + 49, Band(bmp, 1).Right);
        }

        [TestMethod]
        [DataRow("C", 250)]
        [DataRow("R", 350)]
        [DataRow("L", 150)]
        public void Outline_FollowingLinesUseTheAreaAfterTheHangingIndent(string justification, int expected)
        {
            // second line "II" in a 300 px block with an indent of 100 px; the block starts at x = 50
            using SKBitmap bmp = RenderHelper.Render(
                $"^XA^FO50,50^A0N,30,30^FB300,3,0,{justification},100^FDIIII\\&II^FS^XZ", null, 100, 100);

            SKRectI line = Band(bmp, 1);

            int reference = justification switch
            {
                "C" => (line.Left + line.Right) / 2,
                "R" => line.Right,
                _ => line.Left,
            };
            Assert.IsLessThanOrEqualTo(8, Math.Abs(reference - expected), $"{justification}: expected about {expected} but was {reference}");
        }

        // ---------- ^BQ model and mask ----------

        [TestMethod]
        [DataRow("^BQN,1,3,Q,2", 1, 2)]
        [DataRow("^BQN,2,3,Q,0", 2, 0)]
        [DataRow("^BQN,3,3,Q,5", 2, 5)]
        [DataRow("^BQN,0,3,Q,8", 2, 7)]
        [DataRow("^BQN,2,3,Q,-1", 2, 7)]
        [DataRow("^BQN", 2, 7)]
        public void QrCode_InvalidModelAndMaskFallBackToTheDefaults(string command, int model, int mask)
        {
            QrCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(
                new QrCodeBarcodeZplCommandAnalyzer(), command);

            Assert.AreEqual(model, data.Model);
            Assert.AreEqual(mask, data.MaskValue);
        }
    }
}
