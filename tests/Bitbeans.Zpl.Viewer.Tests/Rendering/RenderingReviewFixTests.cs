using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;
using Bitbeans.Zpl.Viewer.ElementDrawers;
using Bitbeans.Zpl.Viewer.Helpers;
using Bitbeans.Zpl.Viewer.Models;

using Microsoft.Extensions.Logging;

using SkiaSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    /// <summary>Regression tests for the review findings of the non-barcode rendering.</summary>
    [TestClass]
    public class RenderingReviewFixTests
    {
        private const string Frame8 = "FF818181818181FF";

        private static SKBitmap R(string zpl, DrawerOptions options = null, double widthMm = 50, double heightMm = 50)
        {
            return RenderHelper.Render("^XA" + zpl + "^XZ", options, widthMm, heightMm);
        }

        private static PrinterStorage StorageWithLogo(int width, int height)
        {
            using SKBitmap bitmap = new(width, height);
            bitmap.Erase(SKColors.Black);
            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            PrinterStorage storage = new();
            storage.AddFile('R', "LOGO.GRF", data.ToArray());
            return storage;
        }

        // ---------- 4: native resources ----------

        [TestMethod]
        public void FontManager_DisposeReleasesTheOwnedTypefacesOnly()
        {
            FontManager manager = new();
            SKTypeface owned = SKTypeface.FromStream(new MemoryStream(Bitbeans.Zpl.Viewer.Properties.Resources.ZplGS));
            SKTypeface registered = SKTypeface.FromStream(new MemoryStream(Bitbeans.Zpl.Viewer.Properties.Resources.ZplGS));
            manager.RegisterTypeface(registered);
            manager.SystemTypefaceMatcher = (family, style) => family == "System" ? owned : null;
            manager.FontStack0 = ["System"];
            manager.FontStackA = [registered.FamilyName];
            SKTypeface symbols = manager.TypefaceGS;

            Assert.AreSame(owned, manager.Typeface0);
            Assert.AreSame(registered, manager.TypefaceA);

            manager.Dispose();

            Assert.AreEqual(IntPtr.Zero, owned.Handle);
            Assert.AreEqual(IntPtr.Zero, symbols.Handle);
            Assert.AreNotEqual(IntPtr.Zero, registered.Handle);
            registered.Dispose();
        }

        [TestMethod]
        public void FontManager_TheCachedTypefaceIsSharedByConcurrentCallers()
        {
            FontManager manager = new();
            SKTypeface[] typefaces = new SKTypeface[32];

            Parallel.For(0, typefaces.Length, i => typefaces[i] = manager.Typeface0);

            Assert.IsTrue(typefaces.All(t => ReferenceEquals(t, typefaces[0])));
            manager.Dispose();
        }

        // ---------- 5: concurrent draws ----------

        [TestMethod]
        public void Draw_ConcurrentCallsOnOneInstanceMatchSequentialResults()
        {
            PrinterStorage storage = new();
            ZplElementDrawer drawer = new(storage, RenderHelper.Opaque());
            ZplElementBase[][] labels = Enumerable.Range(0, 24).Select(i => RenderHelper.Parse(
                $"^XA^FO{10 + i},20^A0N,30,30^FDLabel {i}^FS^FO20,{60 + i}^GB{80 + i},30,3^FS^FO30,120^FR^GB100,40,40^FS^FO20,200^A0N,20,20^FB150,3,0,L,20^FDSome text {i} that wraps^FS^XZ", storage)).ToArray();

            byte[][] sequential = labels.Select(l => drawer.Draw(l, 50, 50, 8)).ToArray();
            byte[][] parallel = new byte[labels.Length][];
            Parallel.For(0, labels.Length, new ParallelOptions { MaxDegreeOfParallelism = 8 }, i =>
            {
                for (int repeat = 0; repeat < 3; repeat++)
                {
                    parallel[i] = drawer.Draw(labels[i], 50, 50, 8);
                }
            });

            for (int i = 0; i < labels.Length; i++)
            {
                CollectionAssert.AreEqual(sequential[i], parallel[i], $"label {i}");
            }
        }

        // ---------- 6: partial ^FT ----------

        [TestMethod]
        public void FieldTypeset_OmittedXWithExplicitYContinuesAtTheEndOfThePreviousField()
        {
            using SKBitmap partial = R("^FT20,40^A0N,20,20^FDA^FS^FT,100^A0N,20,20^FDB^FS");
            using SKBitmap first = R("^FT20,40^A0N,20,20^FDA^FS");

            SKRectI second = RenderHelper.DarkBounds(partial, 0, 60, 400, 120).Value;
            SKRectI firstBounds = RenderHelper.DarkBounds(first).Value;
            Assert.IsTrue(second.Bottom is >= 97 and <= 101, second.ToString());
            Assert.IsTrue(second.Left >= firstBounds.Right - 2 && second.Left < firstBounds.Right + 12, $"{second} after {firstBounds}");
            Assert.AreEqual(firstBounds, RenderHelper.DarkBounds(partial, 0, 0, 400, 60).Value);
        }

        [TestMethod]
        public void FieldTypeset_OmittedXWithExplicitYAlsoWorksForFieldBlocks()
        {
            using SKBitmap bmp = R("^FT20,40^A0N,20,20^FDA^FS^FT,100^A0N,20,20^FB100,1,0,L,0^FDB^FS");

            SKRectI second = RenderHelper.DarkBounds(bmp, 0, 60, 400, 120).Value;
            Assert.IsTrue(second.Bottom is >= 97 and <= 101, second.ToString());
            Assert.IsTrue(second.Left > 25);
        }

        [TestMethod]
        public void FieldTypeset_FullyOmittedCoordinatesKeepTheDefaultPosition()
        {
            using SKBitmap bmp = R("^FT20,40^A0N,20,20^FDA^FS^FT^A0N,20,20^FDB^FS");

            SKRectI second = RenderHelper.DarkBounds(bmp, 28, 0, 400, 400).Value;
            Assert.IsTrue(second.Bottom is >= 37 and <= 41, second.ToString());
        }

        [TestMethod]
        public void FieldTypeset_AnalyzerFlagsTheOmittedCoordinates()
        {
            (string Zpl, bool Default, bool DefaultX, int Y)[] cases =
            [
                ("^FT,100", false, true, 100),
                ("^FT,100,1", false, true, 100),
                ("^FTabc,100", false, true, 100),
                ("^FT,", true, false, 0),
                ("^FT", true, false, 0),
                ("^FT,abc", true, false, 0),
                ("^FT20,abc", true, false, 0),
                ("^FT20,", false, false, 0),
                ("^FT20,50", false, false, 50),
            ];

            foreach ((string zpl, bool useDefault, bool useDefaultX, int y) in cases)
            {
                ZplTextField field = (ZplTextField)RenderHelper.Parse($"^XA^FT30,70^A0N,20,20^FDA^FS{zpl}^A0N,20,20^FDB^FS^XZ")[1];

                Assert.AreEqual(useDefault && !useDefaultX, field.UseDefaultPosition, zpl);
                Assert.AreEqual(useDefaultX, field.UseDefaultPositionX, zpl);
                Assert.IsNotNull(field.FieldTypeset, zpl);
                if (useDefaultX)
                {
                    Assert.AreEqual(y, field.PositionY, zpl);
                }
            }
        }

        [TestMethod]
        public void FieldTypeset_PartialPositionKeepsTheLabelHomeAndIsRemembered()
        {
            ZplElementBase[] elements = RenderHelper.Parse("^XA^LH10,10^FT,100^A0N,20,20^FDA^FS^FT50^A0N,20,20^FDB^FS^XZ");

            ZplTextField first = (ZplTextField)elements[0];
            ZplTextField second = (ZplTextField)elements[1];
            Assert.AreEqual(110, first.PositionY);
            Assert.IsTrue(first.UseDefaultPositionX);
            // an omitted y continues with the y of the last explicit position
            Assert.AreEqual(110, second.PositionY);
        }

        [TestMethod]
        public void FieldTypeset_ModelRendersOmittedXAndKeepsItWhenOffset()
        {
            ZplFieldTypeset typeset = new(0, 100, FieldJustification.None, useDefaultPositionX: true);

            Assert.AreEqual("^FT,100", typeset.ToZplString());
            Assert.AreEqual("^FT,100,1", new ZplFieldTypeset(0, 100, FieldJustification.Right, useDefaultPositionX: true).ToZplString());
            Assert.IsTrue(typeset.Offset(1, 2).UseDefaultPositionX);
            Assert.AreEqual("^FT,102", typeset.Offset(1, 2).ToZplString());
            Assert.IsFalse(new ZplFieldTypeset(0, 100, FieldJustification.None, true, true).UseDefaultPositionX);

            ZplTextField field = new("A", 0, 100, new ZplFont(20, 20, "0"), bottomToTop: true, useDefaultPositionX: true);
            Assert.IsTrue(field.UseDefaultPositionX);
            Assert.IsFalse(new ZplTextField("A", 0, 100, new ZplFont(20, 20, "0"), useDefaultPositionX: true).UseDefaultPositionX);
            Assert.IsFalse(new ZplTextField("A", 0, 100, new ZplFont(20, 20, "0"), bottomToTop: true, useDefaultPosition: true, useDefaultPositionX: true).UseDefaultPositionX);
            Assert.IsTrue(new ZplFieldBlock("A", 0, 100, 50, new ZplFont(20, 20, "0"), bottomToTop: true, useDefaultPositionX: true).UseDefaultPositionX);
        }

        // ---------- 7: ^FT bottom-anchored graphics keep their position ----------

        [TestMethod]
        public void FieldTypeset_GraphicsAboveTheLabelAreClippedAndNotMoved()
        {
            string[] zpl =
            [
                "^FT20,5^GB100,20,2^FS",
                "^FT20,5^GC20,2^FS",
                "^FT20,5^GE100,20,2^FS",
                "^FT20,5^GD100,20,8,B,L^FS",
            ];

            foreach (string z in zpl)
            {
                using SKBitmap bmp = R(z);
                SKRectI bounds = RenderHelper.DarkBounds(bmp) ?? throw new AssertFailedException("nothing drawn: " + z);
                Assert.AreEqual(0, bounds.Top, z);
                Assert.IsTrue(bounds.Bottom is >= 4 and <= 5, $"{z}: {bounds}");
            }
        }

        [TestMethod]
        public void FieldTypeset_ExactBottomOfTheBox()
        {
            using SKBitmap bmp = R("^FT20,5^GB100,20,20^FS");

            Assert.AreEqual(new SKRectI(20, 0, 120, 5), RenderHelper.DarkBounds(bmp).Value);
        }

        // ---------- 8: justification ----------

        [TestMethod]
        public void Justification_RightMakesThePositionTheRightEdgeOfGraphics()
        {
            (string Zpl, int Left, int Right)[] cases =
            [
                ("^FO200,20,1^GB100,20,20^FS", 100, 200),
                ("^FT200,60,1^GB100,20,20^FS", 100, 200),
                ("^FO200,20,1^GC40,40^FS", 160, 200),
                ("^FO200,20,1^GE80,40,40^FS", 120, 200),
                ("^FO200,20,1^GD60,40,4,B,L^FS", 140, 200),
                ("^FO200,20,1^GFA,8,8,1," + Frame8 + "^FS", 192, 200),
                ("^FO200,20,0^GB100,20,20^FS", 200, 300),
            ];

            foreach ((string zpl, int left, int right) in cases)
            {
                using SKBitmap bmp = R(zpl, widthMm: 50, heightMm: 50);
                using SKBitmap wide = RenderHelper.Render("^XA" + zpl + "^XZ", null, 50, 50);
                SKRectI bounds = RenderHelper.DarkBounds(wide).Value;
                Assert.IsTrue(Math.Abs(bounds.Left - left) <= 1, $"{zpl}: {bounds}");
                Assert.IsTrue(Math.Abs(bounds.Right - Math.Min(right, 400)) <= 1, $"{zpl}: {bounds}");
            }
        }

        [TestMethod]
        public void Justification_RightAppliesToStoredImages()
        {
            PrinterStorage storage = StorageWithLogo(10, 6);

            using SKBitmap move = RenderHelper.Render("^XA^FO200,20,1^IMR:LOGO.GRF^FS^XZ", storage: storage);
            using SKBitmap recall = RenderHelper.Render("^XA^FO200,20,1^XGR:LOGO.GRF,2,1^FS^XZ", storage: storage);

            Assert.AreEqual(new SKRectI(190, 20, 200, 26), RenderHelper.DarkBounds(move).Value);
            Assert.AreEqual(new SKRectI(180, 20, 200, 26), RenderHelper.DarkBounds(recall).Value);
        }

        [TestMethod]
        public void Justification_RightAppliesToFieldBlocks()
        {
            using SKBitmap bmp = R("^FO200,20,1^A0N,20,20^FB100,1,0,L,0^FDHI^FS");
            using SKBitmap left = R("^FO100,20,0^A0N,20,20^FB100,1,0,L,0^FDHI^FS");

            Assert.IsTrue(RenderHelper.SameImage(bmp, left));
        }

        [TestMethod]
        public void Justification_ModelPassesItToThePositionCommand()
        {
            Assert.AreEqual("^FO10,20,1", new ZplGraphicBox(10, 20, 5, 5, fieldJustification: FieldJustification.Right).FieldOrigin.ToZplString());
            Assert.AreEqual(FieldJustification.Right, new ZplGraphicCircle(10, 20, 5, fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.Right, new ZplGraphicEllipse(10, 20, 5, 5, bottomToTop: true, fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.Right, new ZplGraphicDiagonalLine(10, 20, 5, 5, fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.Right, new ZplGraphicField(10, 20, 1, 1, 1, "FF", fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.Right, new ZplImageMove(10, 20, 'R', "A", "GRF", fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.Right, new ZplRecallGraphic(10, 20, 'R', "A", fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.Right, new ZplFieldBlock("A", 10, 20, 50, new ZplFont(20, 20, "0"), fieldJustification: FieldJustification.Right).FieldJustification);
            Assert.AreEqual(FieldJustification.None, new ZplGraphicBox(10, 20, 5, 5).FieldJustification);
        }

        [TestMethod]
        public void Justification_DefaultFromFieldWriteIsUsed()
        {
            ZplGraphicBox box = (ZplGraphicBox)RenderHelper.Parse("^XA^FWN,1^FO200,20^GB100,20,20^FS^XZ")[0];

            Assert.AreEqual(FieldJustification.Right, box.FieldJustification);
        }

        // ---------- 9: white ellipse ----------

        [TestMethod]
        public void Ellipse_WhiteLineColorIsDrawnWhite()
        {
            using SKBitmap bmp = R("^FO20,20^GB100,100,100^FS^FO20,20^GE100,100,4,W^FS");

            Assert.IsTrue(RenderHelper.IsDark(bmp, 70, 70));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 22, 70));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 70, 22));
        }

        // ---------- 10: graphic field data ----------

        [TestMethod]
        public void GraphicField_RawZplDataIsDrawn()
        {
            using SKBitmap bmp = RenderHelper.RenderElements([new ZplGraphicField(20, 20, 1, 1, 1, "FF")]);

            Assert.AreEqual(new SKRectI(20, 20, 28, 21), RenderHelper.DarkBounds(bmp).Value);
        }

        [TestMethod]
        public void GraphicField_CompressedDataIsDrawn()
        {
            string acs = Bitbeans.Zpl.Helpers.ZebraACSCompressionHelper.Compress(Frame8, 1);
            string b64 = Bitbeans.Zpl.Helpers.ZebraB64CompressionHelper.Compress(Frame8);
            string z64 = Bitbeans.Zpl.Helpers.ZebraZ64CompressionHelper.Compress(Frame8);

            foreach (string data in new[] { Frame8, acs, b64, z64 })
            {
                using SKBitmap bmp = RenderHelper.RenderElements([new ZplGraphicField(10, 10, 8, 8, 1, data)]);
                Assert.AreEqual(new SKRectI(10, 10, 18, 18), RenderHelper.DarkBounds(bmp).Value, data);
                Assert.AreEqual(28, RenderHelper.CountDark(bmp), data);
            }
        }

        [TestMethod]
        public void GraphicField_ParsedElementRendersValidZpl()
        {
            ZplGraphicField parsed = (ZplGraphicField)RenderHelper.Parse("^XA^FO10,10^GFA,8,8,1," + Frame8 + "^FS^XZ")[0];

            string zpl = parsed.ToZplString();

            StringAssert.Contains(zpl, "^GFA,8,8,1," + Frame8 + "^FS");
            using SKBitmap original = RenderHelper.RenderElements([parsed]);
            using SKBitmap again = RenderHelper.Render("^XA" + zpl + "^XZ");
            Assert.IsTrue(RenderHelper.SameImage(original, again));
            Assert.AreEqual(28, RenderHelper.CountDark(original));
        }

        [TestMethod]
        public void GraphicField_UndrawableDataIsLoggedAndTheOtherElementsAreDrawn()
        {
            ListLogger logger = new();
            ZplElementBase[] elements = [new ZplGraphicField(10, 10, 1, 1, 0, "FF"), new ZplGraphicBox(50, 50, 20, 20, 20)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, logger: logger);

            Assert.AreEqual(1, logger.Entries.Count(e => e.Level == LogLevel.Error));
            Assert.AreEqual(400, RenderHelper.CountDark(bmp));
        }

        [TestMethod]
        public void GraphicField_AnalyzerRejectsDataSmallerThanOneRow()
        {
            AnalyzeInfo info = new ZplAnalyzer(new PrinterStorage()).Analyze("^XA^FO0,0^GFA,2,2,2,FF^FS^XZ");

            Assert.HasCount(1, info.Errors);
        }

        // ---------- 11: hanging indent ----------

        [TestMethod]
        public void HangingIndent_FollowingLinesAreWrappedWithinTheRemainingWidth_Outline()
        {
            using SKBitmap plain = R("^FO20,20^A0N,20,20^FB100,9,0,L,0^FDAA AA AA AA AA AA AA AA^FS");
            using SKBitmap indented = R("^FO20,20^A0N,20,20^FB100,9,0,L,60^FDAA AA AA AA AA AA AA AA^FS");

            List<(int Top, int Bottom)> plainBands = RenderHelper.RowBands(plain);
            List<(int Top, int Bottom)> indentedBands = RenderHelper.RowBands(indented);

            // narrower following lines need more lines, but no word is clipped
            Assert.IsTrue(indentedBands.Count > plainBands.Count);
            Assert.AreEqual(RenderHelper.CountDark(plain), RenderHelper.CountDark(indented), RenderHelper.CountDark(plain) * 0.01);
        }

        [TestMethod]
        public void HangingIndent_FollowingLinesAreWrappedWithinTheRemainingWidth_Bitmap()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider(["A"]);

            using SKBitmap bmp = RenderHelper.Render("^XA^FO50,50^AAN,9,5^FB60,5,3,L,30^FDAA BB CC DD EE FF^FS^XZ", options, 100, 100);

            List<(int Top, int Bottom)> bands = RenderHelper.RowBands(bmp);
            Assert.AreEqual(3, bands.Count);
            SKRectI second = RenderHelper.DarkBounds(bmp, 0, bands[1].Top, int.MaxValue, bands[1].Bottom).Value;
            Assert.AreEqual(80, second.Left);
            Assert.IsTrue(second.Right <= 110, second.ToString());
        }

        [TestMethod]
        public void HangingIndent_WordLongerThanTheFollowingLinesIsSplit()
        {
            DrawerOptions options = RenderHelper.Opaque();
            options.TextRenderingMode = ZplTextRenderingMode.BitmapStrict;
            options.BitmapFontProvider = new TestBitmapFontProvider(["A"]);

            using SKBitmap bmp = RenderHelper.Render("^XA^FO10,10^AAN,9,5^FB100,9,3,L,70^FDAAAA AAAAAAAAAAAAAAAA^FS^XZ", options, 100, 100);

            // 30 dots remain: the long word is split into chunks of at most 4 glyphs
            foreach ((int top, int bottom) in RenderHelper.RowBands(bmp).Skip(1))
            {
                SKRectI band = RenderHelper.DarkBounds(bmp, 0, top, int.MaxValue, bottom).Value;
                Assert.IsTrue(band.Left >= 80 && band.Right <= 110, band.ToString());
            }
        }

        // ---------- 12: ^FH and surrogate pairs ----------

        [TestMethod]
        public void HexEscapes_SurrogatePairsInLiteralTextAreKept()
        {
            Assert.AreEqual("A\U0001F600B", "A\U0001F600B".ReplaceHexEscapes('_', InternationalFont.UTF8));
            Assert.AreEqual("\U0001F600", "\U0001F600".ReplaceHexEscapes('_', InternationalFont.UTF16_BE));
            Assert.AreEqual("\U0001F600", "_F0_9F_98_80".ReplaceHexEscapes('_', InternationalFont.UTF8));
            Assert.AreEqual("A\U0001F600ä", "A\U0001F600_C3_A4".ReplaceHexEscapes('_', InternationalFont.UTF8));
            Assert.AreEqual("ä\U0001F600", "_C3_A4\U0001F600".ReplaceHexEscapes('_', InternationalFont.UTF8));
        }

        [TestMethod]
        public void HexEscapes_PlainTextAndInvalidEscapesStayUnchanged()
        {
            Assert.AreEqual("A_G1B", "A_G1B".ReplaceHexEscapes('_', InternationalFont.UTF8));
            Assert.AreEqual("A_4", "A_4".ReplaceHexEscapes('_', InternationalFont.ZCP850));
            Assert.AreEqual("AB", "A_42".ReplaceHexEscapes('_', InternationalFont.ZCP850));
        }

        // ---------- 13: diagonal lines stay within width and height ----------

        [TestMethod]
        public void Diagonal_StaysWithinTheSpecifiedWidthAndHeight()
        {
            foreach (string orientation in new[] { "R", "L" })
            {
                using SKBitmap bmp = R($"^FO50,50^GD100,100,10,B,{orientation}^FS");
                SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
                Assert.IsTrue(bounds.Left >= 50 && bounds.Right <= 150, $"{orientation}: {bounds}");
                Assert.IsTrue(bounds.Top >= 50 && bounds.Bottom <= 150, $"{orientation}: {bounds}");
                Assert.IsTrue(bounds.Width >= 98 && bounds.Height >= 98, $"{orientation}: {bounds}");
            }
        }

        [TestMethod]
        public void Diagonal_ThicknessLargerThanTheWidthFillsTheWidth()
        {
            using SKBitmap bmp = R("^FO50,50^GD20,100,40,B,L^FS");

            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(bounds.Left >= 50 && bounds.Right <= 70, bounds.ToString());
        }

        // ---------- 14: reverse fields ----------

        [TestMethod]
        public void Reverse_OnALargeLabelInvertsOnlyTheField()
        {
            using SKBitmap bmp = RenderHelper.Render("^XA^FO1990,1990^GB50,50,50^FS^FO2000,2000^FR^GB30,30,30^FS^FO100,100^FR^GB10,10,10^FS^XZ", null, 300, 300);

            Assert.IsTrue(RenderHelper.IsDark(bmp, 1995, 1995));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 2010, 2010));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 2035, 2035));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 105, 105));
        }

        [TestMethod]
        public void Reverse_FieldOutsideTheLabelDrawsNothingAndDoesNotFail()
        {
            using SKBitmap bmp = R("^FO5000,5000^FR^GB30,30,30^FS^FO10,10^GB10,10,10^FS");

            Assert.AreEqual(100, RenderHelper.CountDark(bmp));
        }

        [TestMethod]
        public void Reverse_TinyDetailsStillInvert()
        {
            using SKBitmap bmp = R("^FO0,0^GB400,400,400^FS^FO100,100^FR^GB1,1,1^FS");

            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 101, 100));
        }

        // ---------- 15: errors ----------

        [TestMethod]
        public void Draw_InvalidLabelSizeIsRejectedAtTheApiBoundary()
        {
            ZplElementDrawer drawer = new(new PrinterStorage());

            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], double.NaN, 50, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 50, double.PositiveInfinity, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], -5, 50, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 50, 0, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 0.01, 50, 8));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 50, 50, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 50, 50, -8));
            Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 5000, 50, 8));
            ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], double.NaN, 50, 8));
            Assert.AreEqual("labelWidth", ex.ParamName);
            Assert.AreEqual("labelHeight", Assert.Throws<ArgumentOutOfRangeException>(() => drawer.Draw([], 50, -1, 8)).ParamName);
        }

        [TestMethod]
        public void Draw_NullArgumentsAreRejected()
        {
            ZplElementDrawer drawer = new(new PrinterStorage());
            (SKSurface surface, SKCanvas _) = RenderHelper.CreateSurface();
            using (surface)
            {
                Assert.Throws<ArgumentNullException>(() => drawer.Draw(null));
                Assert.Throws<ArgumentNullException>(() => drawer.DrawSurface(null, []));
                Assert.Throws<ArgumentNullException>(() => drawer.DrawSurface(surface, null));
                Assert.Throws<ArgumentOutOfRangeException>(() => drawer.DrawSurface(surface, [], double.NaN));
            }
        }

        [TestMethod]
        public void Draw_ElementThatCannotBeAssignedToADrawerIsLoggedAndSkipped()
        {
            ListLogger logger = new();
            ZplElementBase[] elements = [null, new ZplGraphicBox(10, 10, 20, 20, 20)];

            using SKBitmap bmp = RenderHelper.RenderElements(elements, logger: logger);

            Assert.AreEqual(1, logger.Entries.Count(e => e.Level == LogLevel.Error));
            Assert.AreEqual(400, RenderHelper.CountDark(bmp));
        }

        [TestMethod]
        public void DrawSurface_ReverseFieldsAndErrorsAreHandled()
        {
            ListLogger logger = new();
            ZplElementDrawer drawer = new(new PrinterStorage());
            ZplElementBase[] elements = RenderHelper.Parse("^XA^FO0,0^GB100,100,100^FS^FO10,10^FR^GB30,30,30^FS^XZ");
            (SKSurface surface, SKCanvas _) = RenderHelper.CreateSurface();
            using (surface)
            {
                drawer.DrawSurface(surface, [.. elements, null], 50, 50, 8, logger);

                using SKImage image = surface.Snapshot();
                using SKBitmap bmp = SKBitmap.FromImage(image);
                Assert.IsTrue(RenderHelper.IsDark(bmp, 5, 5));
                Assert.AreEqual(1, logger.Entries.Count(e => e.Level == LogLevel.Error));
            }
        }
    }
}
