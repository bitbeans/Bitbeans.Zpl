using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using System;
using System.Linq;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class ZplElementTests
    {
        private static readonly ZplRenderOptions Scaled = new() { SourcePrintDpi = 200, TargetPrintDpi = 300 };

        // ---- positioning ----

        [TestMethod]
        public void FieldOrigin()
        {
            var origin = new ZplFieldOrigin(10, 20);
            Assert.AreEqual("^FO10,20", origin.ToZplString());
            Assert.AreEqual("^FO15,30", origin.ToZplString(Scaled));
            Assert.AreEqual("^FO10,20,0", new ZplFieldOrigin(10, 20, FieldJustification.Left).ToZplString());
            Assert.AreEqual("^FO10,20,1", new ZplFieldOrigin(10, 20, FieldJustification.Right).ToZplString());
            Assert.AreEqual("^FO10,20,2", new ZplFieldOrigin(10, 20, FieldJustification.Auto).ToZplString());
        }

        [TestMethod]
        public void FieldOriginOffsetReturnsNewInstance()
        {
            var origin = new ZplFieldOrigin(10, 20, FieldJustification.Right);
            var moved = origin.Offset(5, -5);

            Assert.AreNotSame(origin, moved);
            Assert.AreEqual(10, origin.PositionX);
            Assert.AreEqual(15, moved.PositionX);
            Assert.AreEqual(15, moved.PositionY);
            Assert.AreEqual(FieldJustification.Right, moved.FieldJustification);
            Assert.AreEqual("^FO15,15,1", moved.ToZplString());
        }

        [TestMethod]
        public void FieldTypeset()
        {
            Assert.AreEqual("^FT10,20", new ZplFieldTypeset(10, 20).ToZplString());
            Assert.AreEqual("^FT15,30", new ZplFieldTypeset(10, 20).ToZplString(Scaled));
            Assert.AreEqual("^FT10,20,2", new ZplFieldTypeset(10, 20, FieldJustification.Auto).ToZplString());
            Assert.AreEqual("^FT", new ZplFieldTypeset(10, 20, useDefaultPosition: true).ToZplString());
            Assert.AreEqual("^FT,,1", new ZplFieldTypeset(10, 20, FieldJustification.Right, true).ToZplString());
        }

        [TestMethod]
        public void FieldTypesetWithoutX()
        {
            Assert.AreEqual("^FT,20", new ZplFieldTypeset(10, 20, useDefaultPositionX: true).ToZplString());
            Assert.AreEqual("^FT,30,1", new ZplFieldTypeset(10, 20, FieldJustification.Right, useDefaultPositionX: true).ToZplString(Scaled));
            Assert.IsTrue(new ZplFieldTypeset(10, 20, useDefaultPositionX: true).Offset(1, 1).UseDefaultPositionX);
            Assert.IsFalse(new ZplFieldTypeset(10, 20, useDefaultPosition: true, useDefaultPositionX: true).UseDefaultPositionX);
        }

        [TestMethod]
        public void FieldTypesetOffsetKeepsAllSettings()
        {
            var typeset = new ZplFieldTypeset(10, 20, FieldJustification.Left, true);
            var moved = typeset.Offset(1, 2);

            Assert.AreEqual(11, moved.PositionX);
            Assert.AreEqual(22, moved.PositionY);
            Assert.AreEqual(FieldJustification.Left, moved.FieldJustification);
            Assert.IsTrue(moved.UseDefaultPosition);
            Assert.AreEqual("^FT,,0", moved.ToZplString());
            Assert.AreEqual("^FT11,22", new ZplFieldTypeset(10, 20).Offset(1, 2).ToZplString());
        }

        [TestMethod]
        public void PositionedElementUsesOriginOrTypeset()
        {
            var origin = new ZplGraphicBox(10, 20, 1, 1);
            Assert.AreEqual(10, origin.PositionX);
            Assert.AreEqual(20, origin.PositionY);
            Assert.IsNotNull(origin.FieldOrigin);
            Assert.IsNull(origin.FieldTypeset);
            Assert.IsFalse(origin.UseDefaultPosition);

            var typeset = new ZplGraphicBox(10, 20, 1, 1, bottomToTop: true, useDefaultPosition: true);
            Assert.AreEqual(10, typeset.PositionX);
            Assert.AreEqual(20, typeset.PositionY);
            Assert.IsNull(typeset.FieldOrigin);
            Assert.IsNotNull(typeset.FieldTypeset);
            Assert.IsTrue(typeset.UseDefaultPosition);
            Assert.AreEqual("^FT\n^GB1,1,1,B,0^FS", typeset.ToZplString());
        }

        // ---- simple commands ----

        [TestMethod]
        public void Raw()
        {
            var raw = new ZplRaw("^FO1,1^FDX^FS");
            Assert.AreEqual("^FO1,1^FDX^FS", raw.RawContent);
            Assert.AreEqual("^FO1,1^FDX^FS", raw.ToZplString());
        }

        [TestMethod]
        public void ChangeInternationalFont()
        {
            Assert.AreEqual("^CI0", new ZplChangeInternationalFont(InternationalFont.ZCP850).ToZplString());
            Assert.AreEqual("^CI13", new ZplChangeInternationalFont(InternationalFont.ZCP850_13).ToZplString());
        }

        [TestMethod]
        public void Font()
        {
            Assert.AreEqual("^A0N,30,30", new ZplFont().ToZplString());
            Assert.AreEqual("^AER,40,20", new ZplFont(20, 40, "E", FieldOrientation.Rotated90).ToZplString());
            Assert.AreEqual("^A0N,60,30", new ZplFont(20, 40).ToZplString(new ZplRenderOptions { SourcePrintDpi = 100, TargetPrintDpi = 150 }));
        }

        [TestMethod]
        public void FontIdentifier()
        {
            var element = new ZplFontIdentifier('E', "R", "ARIAL.TTF");
            Assert.AreEqual("^CWE,R:ARIAL.TTF", element.ToZplString());
            Assert.AreEqual('E', element.FontReplaceLetter);
            Assert.AreEqual("R", element.Device);
            Assert.AreEqual("ARIAL.TTF", element.FontFileName);
        }

        [TestMethod]
        public void FieldOrientationElement()
        {
            Assert.AreEqual("^FWN", new ZplFieldOrientation(FieldOrientation.Normal).ToZplString());
            Assert.AreEqual("^FWR,1", new ZplFieldOrientation(FieldOrientation.Rotated90, FieldJustification.Right).ToZplString());
        }

        [TestMethod]
        public void RecallFormatRendersXfCommand()
        {
            var element = new ZplRecallFormat("R:FORMAT.ZPL");
            Assert.AreEqual("R:FORMAT.ZPL", element.FormatName);
            Assert.AreEqual("^XFR:FORMAT.ZPL^FS", element.ToZplString());
        }

        [TestMethod]
        public void FieldNumber()
        {
            var text = new ZplTextField("X", 10, 20, new ZplFont());
            var element = new ZplFieldNumber(3, text);

            Assert.AreEqual(3, element.Number);
            Assert.AreSame(text, element.FormatElement);
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FN3^FS", element.ToZplString());
        }

        [TestMethod]
        public void FieldNumberRequiresFormatElement()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFieldNumber(1, new ZplRaw("^FDX^FS")));
        }

        [TestMethod]
        public void RecallFieldNumber()
        {
            var element = new ZplRecallFieldNumber(2, "a^b~c\nd");
            Assert.AreEqual(2, element.Number);
            Assert.AreEqual("a^b~c\nd", element.Text);
            Assert.AreEqual("^FN2\n^FH^FDa_5Eb_7Ec d^FS", element.ToZplString());
        }

        [TestMethod]
        public void RecallFieldNumberPlainTextHasNoFieldHexadecimal()
        {
            Assert.AreEqual("^FN2\n^FDa_b\\c^FS", new ZplRecallFieldNumber(2, "a_b\\c").ToZplString());
        }

        [TestMethod]
        public void RecallFieldNumberTreatsNullTextAsEmpty()
        {
            var element = new ZplRecallFieldNumber(3, null);
            Assert.IsNull(element.Text);
            Assert.AreEqual("^FN3\n^FD^FS", element.ToZplString());
        }

        [TestMethod]
        public void FieldNumberRejectsNullAndNonFormatElements()
        {
            var nullElement = Assert.ThrowsExactly<ArgumentNullException>(() => new ZplFieldNumber(1, null));
            Assert.AreEqual("formatElement", nullElement.ParamName);

            var invalid = Assert.ThrowsExactly<ArgumentException>(() => new ZplFieldNumber(1, new ZplRaw("^XA")));
            Assert.AreEqual("formatElement", invalid.ParamName);
            Assert.Contains("IFormatElement", invalid.Message);
        }

        [TestMethod]
        public void ReferenceGrid()
        {
            var lines = new ZplReferenceGrid().Render().ToList();

            Assert.HasCount(30 * 30 * 8, lines);
            CollectionAssert.AreEqual(
                new[] { "^FO0,0", "^GB100,100,1,B,0^FS", "^A0N,20,0", "^FO10,30", "^FDX:0^FS", "^A0N,20,0", "^FO10,50", "^FDY:0^FS", "^FO0,100" },
                lines.Take(9).ToArray());
            Assert.AreEqual("^FDY:2900^FS", lines[^1]);
        }

        // ---- graphics ----

        [TestMethod]
        public void GraphicBox()
        {
            var box = new ZplGraphicBox(10, 20, 300, 200, 5, LineColor.White, 3);
            Assert.AreEqual(300, box.Width);
            Assert.AreEqual(200, box.Height);
            Assert.AreEqual(3, box.CornerRounding);
            Assert.AreEqual(5, box.BorderThickness);
            Assert.AreEqual(LineColor.White, box.LineColor);
            Assert.IsFalse(box.ReversePrint);
            Assert.AreEqual("^FO10,20\n^GB300,200,5,W,3^FS", box.ToZplString());
            //the corner rounding is an index (0-8), not a dot value, and is never scaled
            Assert.AreEqual("^FO15,30\n^GB450,300,8,W,3^FS", box.ToZplString(Scaled));
        }

        [TestMethod]
        public void GraphicBoxValidatesCornerRounding()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplGraphicBox(0, 0, 1, 1, cornerRounding: -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplGraphicBox(0, 0, 1, 1, cornerRounding: 9));
            Assert.AreEqual(8, new ZplGraphicBox(0, 0, 1, 1, cornerRounding: 8).CornerRounding);
            Assert.AreEqual(0, new ZplGraphicBox(0, 0, 1, 1, cornerRounding: 0).CornerRounding);
        }

        [TestMethod]
        public void GraphicCircle()
        {
            Assert.AreEqual("^FO10,20\n^GC100,1,B^FS", new ZplGraphicCircle(10, 20, 100).ToZplString());
            Assert.AreEqual("^FT10,20\n^GC100,5,W^FS", new ZplGraphicCircle(10, 20, 100, 5, LineColor.White, bottomToTop: true).ToZplString());
            Assert.AreEqual(100, new ZplGraphicCircle(10, 20, 100).Diameter);
            Assert.AreEqual("^FO15,30\n^GC150,8,B^FS", new ZplGraphicCircle(10, 20, 100, 5).ToZplString(Scaled));
        }

        [TestMethod]
        public void GraphicDiagonalLine()
        {
            var right = new ZplGraphicDiagonalLine(10, 20, 100, 50, 5);
            Assert.IsTrue(right.RightLeaningDiagonal);
            Assert.AreEqual("^FO10,20\n^GD100,50,5,B,R^FS", right.ToZplString());

            var left = new ZplGraphicDiagonalLine(10, 20, 100, 50, 5, LineColor.White, false);
            Assert.AreEqual("^FO10,20\n^GD100,50,5,W,L^FS", left.ToZplString());
            Assert.AreEqual(100, left.Width);
            Assert.AreEqual(50, left.Height);
        }

        [TestMethod]
        public void GraphicEllipse()
        {
            var ellipse = new ZplGraphicEllipse(10, 20, 300, 100, 10, LineColor.White);
            Assert.AreEqual(300, ellipse.Width);
            Assert.AreEqual(100, ellipse.Height);
            Assert.AreEqual("^FO10,20\n^GE300,100,10,W^FS", ellipse.ToZplString());
        }

        [TestMethod]
        public void GraphicField()
        {
            var field = new ZplGraphicField(10, 20, 4, 4, 2, "FF00FF00");
            Assert.AreEqual('A', field.CompressionType);
            Assert.AreEqual(4, field.BinaryByteCount);
            Assert.AreEqual(4, field.GraphicFieldCount);
            Assert.AreEqual(2, field.BytesPerRow);
            Assert.AreEqual("FF00FF00", field.Data);
            Assert.AreEqual("^FO10,20\n^GFA,4,4,2,FF00FF00^FS", field.ToZplString());
            Assert.AreEqual("^FT10,20\n^GFB,4,4,2,00^FS", new ZplGraphicField(10, 20, 4, 4, 2, "00", true, false, 'B').ToZplString());
        }

        [TestMethod]
        public void GraphicSymbol()
        {
            var symbol = new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 10, 20, 50, 40);
            Assert.AreEqual(GraphicSymbolCharacter.Copyright, symbol.Character);
            Assert.AreEqual(50, symbol.Width);
            Assert.AreEqual(40, symbol.Height);
            Assert.AreEqual(FieldOrientation.Normal, symbol.FieldOrientation);
            Assert.AreEqual("^FO10,20\n^GSN,40,50^FDB^FS", symbol.ToZplString());
            Assert.AreEqual("^FO15,30\n^GSR,60,75^FDE^FS", new ZplGraphicSymbol(GraphicSymbolCharacter.CanadianStandardsAssociationApproval, 10, 20, 50, 40, FieldOrientation.Rotated90).ToZplString(Scaled));
        }

        [TestMethod]
        public void GraphicSymbolWithJustification()
        {
            Assert.AreEqual("^FO10,20,1\n^GSN,40,50^FDB^FS", new ZplGraphicSymbol(GraphicSymbolCharacter.Copyright, 10, 20, 50, 40, fieldJustification: FieldJustification.Right).ToZplString());
            Assert.AreEqual("^FT10,20,0\n^GSN,40,50^FDC^FS", new ZplGraphicSymbol("C", 10, 20, 50, 40, bottomToTop: true, fieldJustification: FieldJustification.Left).ToZplString());
        }

        [TestMethod]
        public void GraphicSymbolFromText()
        {
            Assert.AreEqual(GraphicSymbolCharacter.TradeMark, new ZplGraphicSymbol("CD", 1, 2, 3, 4).Character);
            Assert.IsNull(new ZplGraphicSymbol("", 1, 2, 3, 4).Character);
            Assert.IsNull(new ZplGraphicSymbol((string)null, 1, 2, 3, 4).Character);
            Assert.AreEqual("^FO1,2\n^GSI,4,3^FD^FS", new ZplGraphicSymbol((string)null, 1, 2, 3, 4, FieldOrientation.Rotated180).ToZplString());
            Assert.AreEqual("^FT1,2\n^GSN,4,3^FDA^FS", new ZplGraphicSymbol("A", 1, 2, 3, 4, bottomToTop: true).ToZplString());
        }

        [TestMethod]
        public void ImageMove()
        {
            var move = new ZplImageMove(10, 20, 'R', "SAMPLE", "PNG");
            Assert.AreEqual('R', move.StorageDevice);
            Assert.AreEqual("SAMPLE", move.ObjectName);
            Assert.AreEqual("PNG", move.Extension);
            Assert.AreEqual("^FO10,20\n^IMR:SAMPLE.PNG^FS", move.ToZplString());
        }

        [TestMethod]
        public void RecallGraphic()
        {
            var recall = new ZplRecallGraphic(10, 20, 'R', "SAMPLE");
            Assert.AreEqual('R', recall.StorageDevice);
            Assert.AreEqual("SAMPLE", recall.ImageName);
            Assert.AreEqual(1, recall.MagnificationFactorX);
            Assert.AreEqual(1, recall.MagnificationFactorY);
            Assert.AreEqual("^FO10,20\n^XGR:SAMPLE.GRF,1,1^FS", recall.ToZplString());
            //bottomToTop without useDefaultPosition renders the coordinates, like every other element
            Assert.AreEqual("^FT10,20\n^XGE:IMG.GRF,2,3^FS", new ZplRecallGraphic(10, 20, 'E', "IMG", 2, 3, true).ToZplString());
            Assert.AreEqual("^FT\n^XGE:IMG.GRF,2,3^FS", new ZplRecallGraphic(10, 20, 'E', "IMG", 2, 3, true, true).ToZplString());
        }

        [TestMethod]
        public void RecallGraphicRejectsNamesLongerThanEightCharacters()
        {
            Assert.IsNotNull(new ZplRecallGraphic(0, 0, 'R', "12345678"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallGraphic(0, 0, 'R', "123456789"));
        }

        // ---- downloads ----

        [TestMethod]
        public void DownloadGraphicsNone()
        {
            var converter = new FakeImageConverter();
            var image = new byte[] { 9, 9 };
            var download = new ZplDownloadGraphics('R', "SAMPLE", image, converter, ZplCompressionScheme.None);

            Assert.AreEqual("SAMPLE", download.ImageName);
            Assert.AreSame(image, download.ImageData);
            Assert.AreEqual('R', download.StorageDevice);
            Assert.AreEqual("~DGR:SAMPLE.GRF,6,2,\nFF000000FF00", download.ToZplString());
            Assert.IsEmpty(converter.ScaleFactors);
            Assert.AreSame(image, converter.ConvertedImages.Single());
        }

        [TestMethod]
        public void DownloadGraphicsAcsIsDefault()
        {
            var converter = new FakeImageConverter();
            var download = new ZplDownloadGraphics('E', "SAMPLE", [1], converter);

            Assert.AreEqual("~DGE:SAMPLE.GRF,6,2,\nHF,,HF,", download.ToZplString());
        }

        [TestMethod]
        public void DownloadGraphicsZ64()
        {
            var converter = new FakeImageConverter();
            var lines = new ZplDownloadGraphics('R', "SAMPLE", [1], converter, ZplCompressionScheme.Z64).Render().ToList();

            Assert.AreEqual("~DGR:SAMPLE.GRF,6,2,", lines[0]);
            StringAssert.StartsWith(lines[1], ":Z64:");
            CollectionAssert.AreEqual(converter.RawData, ZebraZ64CompressionHelper.Uncompress(lines[1]));
        }

        [TestMethod]
        public void DownloadGraphicsB64()
        {
            var converter = new FakeImageConverter();
            var lines = new ZplDownloadGraphics('R', "SAMPLE", [1], converter, ZplCompressionScheme.B64).Render().ToList();

            Assert.AreEqual("~DGR:SAMPLE.GRF,6,2,", lines[0]);
            Assert.AreEqual(ZebraB64CompressionHelper.Compress(converter.RawData), lines[1]);
            CollectionAssert.AreEqual(converter.RawData, ZebraB64CompressionHelper.Uncompress(lines[1]));
        }

        [TestMethod]
        public void DownloadGraphicsRejectsUnknownCompressionScheme()
        {
            var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new ZplDownloadGraphics('R', "SAMPLE", [1], new FakeImageConverter(), (ZplCompressionScheme)99));
            Assert.AreEqual("compressionScheme", ex.ParamName);
        }

        [TestMethod]
        public void DownloadGraphicsScalesImageBeforeConversion()
        {
            var converter = new FakeImageConverter();
            var download = new ZplDownloadGraphics('R', "SAMPLE", [1], converter, ZplCompressionScheme.None);

            download.Render(Scaled).ToList();

            Assert.AreEqual(1.5, converter.ScaleFactors.Single());
            CollectionAssert.AreEqual(converter.Png, converter.ConvertedImages.Single());
        }

        [TestMethod]
        public void DownloadGraphicsValidatesArguments()
        {
            var converter = new FakeImageConverter();
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplDownloadGraphics('R', null, [1], converter));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplDownloadGraphics('R', "A", null, converter));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplDownloadGraphics('R', "A", [1], null));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics('R', "123456789", [1], converter));
            Assert.IsNotNull(new ZplDownloadGraphics('R', "12345678", [1], converter));
        }

        [TestMethod]
        public void DownloadObjects()
        {
            var converter = new FakeImageConverter { Png = [0x01, 0xAB, 0xFF] };
            var image = new byte[] { 7 };
            var download = new ZplDownloadObjects('R', "SAMPLE.PNG", image, converter);

            Assert.AreEqual("SAMPLE.PNG", download.ObjectName);
            Assert.AreSame(image, download.ImageData);
            Assert.AreEqual("~DYR:SAMPLE.PNG,P,P,3,,01ABFF", download.ToZplString());
            Assert.AreEqual(1.0, converter.ScaleFactors.Single());

            download.Render(Scaled).ToList();
            Assert.AreEqual(1.5, converter.ScaleFactors[1]);
        }

        [TestMethod]
        public void DownloadObjectsValidatesArguments()
        {
            var converter = new FakeImageConverter();
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplDownloadObjects('R', null, [1], converter));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplDownloadObjects('R', "A", null, converter));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplDownloadObjects('R', "A", [1], null));
        }
    }
}
