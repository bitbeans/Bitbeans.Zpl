using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.SkiaSharp;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Models;

using SkiaSharp;

using System.Collections.Generic;
using System.Linq;

namespace Bitbeans.Zpl.Viewer.Tests.Documentation
{
    /// <summary>
    /// Executes the code samples of README.md and docs/*.md so the documentation stays correct.
    /// Keep the samples here in sync with the documentation.
    /// </summary>
    [TestClass]
    public class DocumentationExamplesTests
    {
        [TestMethod]
        public void Readme_QuickStart_CreatesAndRendersLabel()
        {
            var elements = new List<ZplElementBase>
            {
                new ZplTextField("Hello ZPL", 50, 50, new ZplFont(fontWidth: 40, fontHeight: 40)),
                new ZplBarcode128("123ABC", 50, 120),
                new ZplGraphicBox(30, 30, 500, 300, borderThickness: 4),
            };

            string zpl = new ZplEngine(elements).ToZplString(new ZplRenderOptions());

            StringAssert.StartsWith(zpl, "^XA");
            StringAssert.Contains(zpl, "^FDHello ZPL^FS");
            StringAssert.Contains(zpl, "^BC");

            IPrinterStorage printerStorage = new PrinterStorage();
            var analyzer = new ZplAnalyzer(printerStorage);
            using var drawer = new ZplElementDrawer(printerStorage);

            AnalyzeInfo analyzeInfo = analyzer.Analyze(zpl);
            Assert.HasCount(1, analyzeInfo.LabelInfos);
            Assert.IsEmpty(analyzeInfo.Errors);

            byte[] png = drawer.Draw(analyzeInfo.LabelInfos[0].ZplElements, labelWidth: 101.6, labelHeight: 152.4, printDensityDpmm: 8);
            using SKBitmap bitmap = SKBitmap.Decode(png);
            Assert.AreEqual(813, bitmap.Width);
            Assert.AreEqual(1219, bitmap.Height);
            Assert.AreEqual(255, bitmap.GetPixel(30, 100).Alpha, "Left border of the box must be drawn");
        }

        [TestMethod]
        public void Labels_EngineWithRawCode_RendersAllElements()
        {
            var engine = new ZplEngine
            {
                new ZplTextField("Shipping label", 50, 40, new ZplFont(fontWidth: 40, fontHeight: 40)),
                new ZplGraphicBox(30, 20, 740, 1160, borderThickness: 4),
                new ZplBarcode128("1Z999AA10123456784", 60, 120, height: 150),
                new ZplQrCode("https://example.com", 500, 120, magnificationFactor: 5),
            };

            engine.AddRawZplCode("^FO50,400^GB700,3,3^FS");

            string zpl = engine.ToZplString(new ZplRenderOptions { AddEmptyLineBeforeElementStart = true });

            StringAssert.Contains(zpl, "^FO50,400^GB700,3,3^FS");
            StringAssert.Contains(zpl, "^BQN,2,5");
            StringAssert.EndsWith(zpl, "^XZ");
        }

        [TestMethod]
        public void Labels_Scaling_ScalesCoordinates()
        {
            var options = new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 300 };
            string zpl = new ZplEngine { new ZplGraphicBox(203, 203, 100, 100) }.ToZplString(options);

            StringAssert.Contains(zpl, "^FO300,300");
        }

        [TestMethod]
        public void Labels_Comments_AreRenderedAsFieldComment()
        {
            var field = new ZplTextField("AAA", 50, 100, ZplConstants.Font.Default);
            field.Comments.Add("An important field");

            string zpl = new ZplEngine { field }.ToZplString(new ZplRenderOptions { DisplayComments = true });

            StringAssert.Contains(zpl, "^FX");
            StringAssert.Contains(zpl, "//An important field");
        }

        [TestMethod]
        public void Labels_TextVariants_Render()
        {
            var text = "[_~^][LineBreak\n][The quick fox jumps over the lazy dog.]";
            var font = new ZplFont(fontWidth: 50, fontHeight: 50);

            var engine = new ZplEngine
            {
                new ZplTextField(text, 10, 10, font),
                new ZplTextField(text, 10, 50, font, hexadecimalIndicator: '_'),
                new ZplSingleLineFieldBlock(text, 10, 150, 500, font),
                new ZplFieldBlock(text, 10, 300, 400, font, maxLineCount: 2),
                new ZplTextBlock(text, 10, 600, 400, 100, font),
            };

            string zpl = engine.ToZplString(new ZplRenderOptions());

            // '_' is the ^FH default and therefore not repeated
            StringAssert.Contains(zpl, "^FH^FD[_5F_7E_5E][LineBreak");
            StringAssert.Contains(zpl, "^FB400,2");
        }

        [TestMethod]
        public void Labels_Template_IsMergedByViewer()
        {
            var template = new ZplEngine
            {
                new ZplRaw("^DFR:LABEL.ZPL^FS"),
                new ZplFieldNumber(1, new ZplTextField(string.Empty, 50, 50, ZplConstants.Font.Default)),
            };

            var data = new ZplEngine
            {
                new ZplRaw("^XFR:LABEL.ZPL^FS"),
                new ZplRecallFieldNumber(1, "Filled in"),
            };

            string zpl = template.ToZplString(new ZplRenderOptions()) + data.ToZplString(new ZplRenderOptions());

            AnalyzeInfo analyzeInfo = new ZplAnalyzer(new PrinterStorage()).Analyze(zpl);

            ZplTextField merged = analyzeInfo.LabelInfos
                .SelectMany(o => o.ZplElements)
                .OfType<ZplTextField>()
                .Single();
            Assert.AreEqual("Filled in", merged.Text);
        }

        [TestMethod]
        public void Images_DownloadGraphicsAndObjects_UseConverter()
        {
            var converter = new SkiaImageConverter();
            byte[] imageData = CreatePng();

            var graphics = new ZplEngine
            {
                new ZplDownloadGraphics('R', "SAMPLE", imageData, converter),
                new ZplRecallGraphic(100, 100, 'R', "SAMPLE"),
            };
            string graphicsZpl = graphics.ToZplString(new ZplRenderOptions { SourcePrintDpi = 203, TargetPrintDpi = 300 });
            StringAssert.Contains(graphicsZpl, "~DGR:SAMPLE.GRF,");
            StringAssert.Contains(graphicsZpl, "^XGR:SAMPLE.GRF");

            string z64 = new ZplEngine
            {
                new ZplDownloadGraphics('R', "SAMPLE", imageData, converter, ZplCompressionScheme.Z64),
            }.ToZplString(new ZplRenderOptions());
            StringAssert.Contains(z64, ":Z64:");

            var objects = new ZplEngine
            {
                new ZplDownloadObjects('R', "SAMPLE.PNG", imageData, converter),
                new ZplImageMove(100, 100, 'R', "SAMPLE", "PNG"),
            };
            string objectsZpl = objects.ToZplString(new ZplRenderOptions());
            StringAssert.Contains(objectsZpl, "~DYR:SAMPLE.PNG,P,P,");
            StringAssert.Contains(objectsZpl, "^IMR:SAMPLE.PNG");
        }

        [TestMethod]
        public void Viewer_DrawerOptionsAndFontLoader_Render()
        {
            var options = new DrawerOptions
            {
                RenderFormat = SKEncodedImageFormat.Png,
                RenderQuality = 80,
                OpaqueBackground = true,
                BackgroundColor = SKColors.White,
                Antialias = true,
                ReplaceDashWithEnDash = true,
                ReplaceUnderscoreWithEnSpace = false,
                TextRenderingMode = ZplTextRenderingMode.Outline,
            };
            options.FontManager.FontLoader = fontName => fontName switch
            {
                "0" => SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright),
                _ => SKTypeface.Default,
            };

            IPrinterStorage printerStorage = new PrinterStorage();
            var drawer = new ZplElementDrawer(printerStorage, options);
            AnalyzeInfo analyzeInfo = new ZplAnalyzer(printerStorage).Analyze("^XA^FT100,100^A0N,67,0^FDTest label^FS^XZ");

            byte[] png = drawer.Draw(analyzeInfo.LabelInfos[0].ZplElements);
            using SKBitmap bitmap = SKBitmap.Decode(png);

            Assert.AreEqual(SKColors.White, bitmap.GetPixel(5, 5), "Opaque background expected");
        }

        [TestMethod]
        public void ViewerPackageReadme_SampleRenders()
        {
            IPrinterStorage printerStorage = new PrinterStorage();
            var analyzer = new ZplAnalyzer(printerStorage);
            using var drawer = new ZplElementDrawer(printerStorage);

            AnalyzeInfo analyzeInfo = analyzer.Analyze("^XA^FO50,50^A0N,50,50^FDHello^FS^FO50,120^BCN,100^FD12345^FS^XZ");

            Assert.IsEmpty(analyzeInfo.UnknownCommands);
            Assert.AreEqual(2, analyzeInfo.LabelInfos[0].ZplElements.Length);
            Assert.IsNotEmpty(drawer.Draw(analyzeInfo.LabelInfos[0].ZplElements));
        }

        private static byte[] CreatePng()
        {
            using var bitmap = new SKBitmap(40, 20);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
                using var paint = new SKPaint { Color = SKColors.Black };
                canvas.DrawRect(5, 5, 20, 10, paint);
            }

            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}
