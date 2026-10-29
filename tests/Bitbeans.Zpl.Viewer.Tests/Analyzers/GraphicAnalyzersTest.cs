using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Graphic commands (^GB, ^GC, ^GD, ^GE, ^GF), image commands (^IM, ^XG, ~DG, ~DY) and ^CI.
    /// </summary>
    [TestClass]
    public class GraphicAnalyzersTest
    {
        private const string Box8x8 = "FF818181818181FF";
        private const string PngSignatureHex = "89504E47";

        // ^GB w,h,t,c,r
        [TestMethod]
        public void GraphicBox_Defaults()
        {
            ZplGraphicBox box = AnalyzerTestHelper.Single<ZplGraphicBox>("^FO10,20^GB");

            Assert.AreEqual(10, box.PositionX);
            Assert.AreEqual(20, box.PositionY);
            Assert.AreEqual(1, box.Width);
            Assert.AreEqual(1, box.Height);
            Assert.AreEqual(1, box.BorderThickness);
            Assert.AreEqual(LineColor.Black, box.LineColor);
            Assert.AreEqual(0, box.CornerRounding);
            Assert.IsFalse(box.ReversePrint);
        }

        [TestMethod]
        public void GraphicBox_AllParameters()
        {
            ZplGraphicBox box = AnalyzerTestHelper.Single<ZplGraphicBox>("^FO10,20^GB100,50,4,W,3");

            Assert.AreEqual(100, box.Width);
            Assert.AreEqual(50, box.Height);
            Assert.AreEqual(4, box.BorderThickness);
            Assert.AreEqual(LineColor.White, box.LineColor);
            Assert.AreEqual(3, box.CornerRounding);
        }

        [TestMethod]
        public void GraphicBox_ThicknessIsTheDefaultForWidthAndHeight()
        {
            ZplGraphicBox box = AnalyzerTestHelper.Single<ZplGraphicBox>("^FO0,0^GB,,5");

            Assert.AreEqual(5, box.Width);
            Assert.AreEqual(5, box.Height);
            Assert.AreEqual(5, box.BorderThickness);
        }

        [TestMethod]
        public void GraphicBox_InvalidValuesUseDefaultsAndOtherColorIsBlack()
        {
            ZplGraphicBox box = AnalyzerTestHelper.Single<ZplGraphicBox>("^FO0,0^GBa,b,c,B,d");

            Assert.AreEqual(1, box.Width);
            Assert.AreEqual(1, box.Height);
            Assert.AreEqual(1, box.BorderThickness);
            Assert.AreEqual(LineColor.Black, box.LineColor);
            Assert.AreEqual(0, box.CornerRounding);
        }

        [TestMethod]
        public void GraphicBox_WithoutFieldOriginIsAtZero()
        {
            ZplGraphicBox box = AnalyzerTestHelper.Single<ZplGraphicBox>("^GB10,10,1");

            Assert.AreEqual(0, box.PositionX);
            Assert.AreEqual(0, box.PositionY);
        }

        [TestMethod]
        public void GraphicBox_FieldReverseAndLabelReverse()
        {
            Assert.IsTrue(AnalyzerTestHelper.Single<ZplGraphicBox>("^FO0,0^FR^GB10,10,1").ReversePrint);
            Assert.IsTrue(AnalyzerTestHelper.Single<ZplGraphicBox>("^LRY^FO0,0^GB10,10,1").ReversePrint);
        }

        [TestMethod]
        public void GraphicBox_FieldTypesetPosition()
        {
            ZplGraphicBox box = AnalyzerTestHelper.Single<ZplGraphicBox>("^FT30,40^GB10,10,1");

            Assert.AreEqual(30, box.PositionX);
            Assert.AreEqual(40, box.PositionY);
            Assert.IsNotNull(box.FieldTypeset);
            Assert.IsNull(box.FieldOrigin);
        }

        [TestMethod]
        public void GraphicBox_FieldTypesetWithoutCoordinatesUsesDefaultPosition()
        {
            Assert.IsTrue(AnalyzerTestHelper.Single<ZplGraphicBox>("^FT^GB10,10,1").UseDefaultPosition);
        }

        // ^GC d,t,c
        [TestMethod]
        public void GraphicCircle_Defaults()
        {
            ZplGraphicCircle circle = AnalyzerTestHelper.Single<ZplGraphicCircle>("^FO5,6^GC");

            Assert.AreEqual(5, circle.PositionX);
            Assert.AreEqual(6, circle.PositionY);
            Assert.AreEqual(3, circle.Diameter);
            Assert.AreEqual(1, circle.BorderThickness);
            Assert.AreEqual(LineColor.Black, circle.LineColor);
            Assert.IsFalse(circle.ReversePrint);
        }

        [TestMethod]
        public void GraphicCircle_AllParameters()
        {
            ZplGraphicCircle circle = AnalyzerTestHelper.Single<ZplGraphicCircle>("^FO5,6^FR^GC80,6,W");

            Assert.AreEqual(80, circle.Diameter);
            Assert.AreEqual(6, circle.BorderThickness);
            Assert.AreEqual(LineColor.White, circle.LineColor);
            Assert.IsTrue(circle.ReversePrint);
        }

        [TestMethod]
        public void GraphicCircle_InvalidValuesUseDefaults()
        {
            ZplGraphicCircle circle = AnalyzerTestHelper.Single<ZplGraphicCircle>("^GCa,b,B");

            Assert.AreEqual(3, circle.Diameter);
            Assert.AreEqual(1, circle.BorderThickness);
            Assert.AreEqual(LineColor.Black, circle.LineColor);
            Assert.AreEqual(0, circle.PositionX);
        }

        [TestMethod]
        public void GraphicCircle_FieldTypesetPosition()
        {
            ZplGraphicCircle circle = AnalyzerTestHelper.Single<ZplGraphicCircle>("^FT30,40^GC10");

            Assert.IsNotNull(circle.FieldTypeset);
        }

        // ^GD w,h,t,c,o
        [TestMethod]
        public void GraphicDiagonalLine_Defaults()
        {
            ZplGraphicDiagonalLine line = AnalyzerTestHelper.Single<ZplGraphicDiagonalLine>("^FO5,6^GD");

            Assert.AreEqual(3, line.Width);
            Assert.AreEqual(3, line.Height);
            Assert.AreEqual(1, line.BorderThickness);
            Assert.AreEqual(LineColor.Black, line.LineColor);
            Assert.IsTrue(line.RightLeaningDiagonal);
            Assert.IsFalse(line.ReversePrint);
        }

        [TestMethod]
        public void GraphicDiagonalLine_AllParameters()
        {
            ZplGraphicDiagonalLine line = AnalyzerTestHelper.Single<ZplGraphicDiagonalLine>("^FO5,6^FR^GD100,50,4,W,L");

            Assert.AreEqual(100, line.Width);
            Assert.AreEqual(50, line.Height);
            Assert.AreEqual(4, line.BorderThickness);
            Assert.AreEqual(LineColor.White, line.LineColor);
            Assert.IsFalse(line.RightLeaningDiagonal);
            Assert.IsTrue(line.ReversePrint);
        }

        [TestMethod]
        [DataRow("R", true)]
        [DataRow("L", false)]
        [DataRow("\\", false)]
        [DataRow("", true)]
        [DataRow("X", true)]
        public void GraphicDiagonalLine_Orientation(string value, bool rightLeaning)
        {
            ZplGraphicDiagonalLine line = AnalyzerTestHelper.Single<ZplGraphicDiagonalLine>("^FO0,0^GD10,10,1,B," + value);

            Assert.AreEqual(rightLeaning, line.RightLeaningDiagonal);
        }

        [TestMethod]
        public void GraphicDiagonalLine_ThicknessIsTheDefaultForWidthAndHeight()
        {
            ZplGraphicDiagonalLine line = AnalyzerTestHelper.Single<ZplGraphicDiagonalLine>("^GD,,7");

            Assert.AreEqual(7, line.Width);
            Assert.AreEqual(7, line.Height);
        }

        [TestMethod]
        public void GraphicDiagonalLine_InvalidValuesUseDefaults()
        {
            ZplGraphicDiagonalLine line = AnalyzerTestHelper.Single<ZplGraphicDiagonalLine>("^GDa,b,c");

            Assert.AreEqual(3, line.Width);
            Assert.AreEqual(3, line.Height);
            Assert.AreEqual(1, line.BorderThickness);
        }

        [TestMethod]
        public void GraphicDiagonalLine_FieldTypesetPosition()
        {
            Assert.IsNotNull(AnalyzerTestHelper.Single<ZplGraphicDiagonalLine>("^FT30,40^GD10,10").FieldTypeset);
        }

        // ^GE w,h,t,c
        [TestMethod]
        public void GraphicEllipse_Defaults()
        {
            ZplGraphicEllipse ellipse = AnalyzerTestHelper.Single<ZplGraphicEllipse>("^FO5,6^GE");

            Assert.AreEqual(1, ellipse.Width);
            Assert.AreEqual(1, ellipse.Height);
            Assert.AreEqual(1, ellipse.BorderThickness);
            Assert.AreEqual(LineColor.Black, ellipse.LineColor);
            Assert.IsFalse(ellipse.ReversePrint);
        }

        [TestMethod]
        public void GraphicEllipse_AllParameters()
        {
            ZplGraphicEllipse ellipse = AnalyzerTestHelper.Single<ZplGraphicEllipse>("^FO5,6^FR^GE100,50,4,W");

            Assert.AreEqual(100, ellipse.Width);
            Assert.AreEqual(50, ellipse.Height);
            Assert.AreEqual(4, ellipse.BorderThickness);
            Assert.AreEqual(LineColor.White, ellipse.LineColor);
            Assert.IsTrue(ellipse.ReversePrint);
        }

        [TestMethod]
        public void GraphicEllipse_ThicknessIsTheDefaultForWidthAndHeight()
        {
            ZplGraphicEllipse ellipse = AnalyzerTestHelper.Single<ZplGraphicEllipse>("^GE,,9,B");

            Assert.AreEqual(9, ellipse.Width);
            Assert.AreEqual(9, ellipse.Height);
            Assert.AreEqual(LineColor.Black, ellipse.LineColor);
        }

        [TestMethod]
        public void GraphicEllipse_InvalidValuesUseDefaults()
        {
            ZplGraphicEllipse ellipse = AnalyzerTestHelper.Single<ZplGraphicEllipse>("^GEa,b,c");

            Assert.AreEqual(1, ellipse.Width);
            Assert.AreEqual(1, ellipse.Height);
            Assert.AreEqual(1, ellipse.BorderThickness);
        }

        [TestMethod]
        public void GraphicEllipse_FieldTypesetPosition()
        {
            Assert.IsNotNull(AnalyzerTestHelper.Single<ZplGraphicEllipse>("^FT30,40^GE10,10").FieldTypeset);
        }

        // ^GF a,b,c,d,data
        [TestMethod]
        public void GraphicField_HexData()
        {
            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^FO10,20^GFA,8,8,1," + Box8x8);

            Assert.AreEqual(10, field.PositionX);
            Assert.AreEqual(20, field.PositionY);
            Assert.AreEqual('A', field.CompressionType);
            Assert.AreEqual(8, field.BinaryByteCount);
            Assert.AreEqual(8, field.GraphicFieldCount);
            Assert.AreEqual(1, field.BytesPerRow);
            Assert.AreEqual(Box8x8, field.Data);
        }

        [TestMethod]
        public void GraphicField_Z64Data()
        {
            string z64 = ZebraZ64CompressionHelper.Compress(Box8x8);

            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^FO0,0^GFA,8,8,1," + z64);

            Assert.AreEqual(z64, field.Data);
        }

        [TestMethod]
        public void GraphicField_B64Data()
        {
            string b64 = ZebraB64CompressionHelper.Compress(Box8x8);

            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^FO0,0^GFA,8,8,1," + b64);

            Assert.AreEqual(b64, field.Data);
        }

        [TestMethod]
        public void GraphicField_AcsCompressedData()
        {
            string acs = ZebraACSCompressionHelper.Compress(Box8x8, 1);

            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^FO0,0^GFA,8,8,1," + acs);

            Assert.AreEqual(acs, field.Data);
        }

        [TestMethod]
        public void GraphicField_InvalidNumbersAndFieldTypeset()
        {
            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^FT10,20^GFA,a,b,1," + Box8x8);

            Assert.AreEqual(0, field.BinaryByteCount);
            Assert.AreEqual(0, field.GraphicFieldCount);
            Assert.AreEqual(1, field.BytesPerRow);
            Assert.IsNotNull(field.FieldTypeset);
        }

        [TestMethod]
        public void GraphicField_InvalidBytesPerRowKeepsZero()
        {
            // Without a row width the image data cannot be interpreted: reported as an error, no element
            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze(AnalyzerTestHelper.Label("^FO0,0^GFA,8,8,x," + Box8x8));

            Assert.HasCount(1, info.Errors);
            Assert.IsEmpty(AnalyzerTestHelper.AllElements(info));
        }

        [TestMethod]
        public void GraphicField_WithoutFieldOriginIsAtZero()
        {
            ZplGraphicField field = AnalyzerTestHelper.Single<ZplGraphicField>("^GFA,8,8,1," + Box8x8);

            Assert.AreEqual(0, field.PositionX);
            Assert.AreEqual(0, field.PositionY);
        }

        [TestMethod]
        public void GraphicField_WithoutParametersIsReportedAsError()
        {
            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze(AnalyzerTestHelper.Label("^FO0,0^GFA"));

            Assert.HasCount(1, info.Errors);
        }

        [TestMethod]
        public void GraphicField_MissingDataIsReportedAsError()
        {
            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze(AnalyzerTestHelper.Label("^FO0,0^GFA,8"));

            Assert.HasCount(1, info.Errors);
        }

        // ^IM d:o.x
        [TestMethod]
        public void ImageMove_Parameters()
        {
            ZplImageMove image = AnalyzerTestHelper.Single<ZplImageMove>("^FO10,20^IMR:LOGO.GRF");

            Assert.AreEqual('R', image.StorageDevice);
            Assert.AreEqual("LOGO.GRF", image.ObjectName);
            Assert.AreEqual(10, image.PositionX);
            Assert.AreEqual(20, image.PositionY);
        }

        [TestMethod]
        public void ImageMove_WithoutFieldOriginAtZero()
        {
            ZplImageMove image = AnalyzerTestHelper.Single<ZplImageMove>("^IME:PIC.PNG");

            Assert.AreEqual('E', image.StorageDevice);
            Assert.AreEqual("PIC.PNG", image.ObjectName);
            Assert.AreEqual(0, image.PositionX);
            Assert.AreEqual(0, image.PositionY);
        }

        // ^XG d:o.x,mx,my
        [TestMethod]
        public void RecallGraphic_AllParameters()
        {
            ZplRecallGraphic image = AnalyzerTestHelper.Single<ZplRecallGraphic>("^FO10,20^XGR:LOGO.GRF,2,3");

            Assert.AreEqual('R', image.StorageDevice);
            Assert.AreEqual("LOGO.GRF", image.ImageName);
            Assert.AreEqual(2, image.MagnificationFactorX);
            Assert.AreEqual(3, image.MagnificationFactorY);
            Assert.AreEqual(10, image.PositionX);
        }

        [TestMethod]
        public void RecallGraphic_DefaultsToDeviceRAndNoMagnification()
        {
            ZplRecallGraphic image = AnalyzerTestHelper.Single<ZplRecallGraphic>("^XGLOGO.GRF");

            Assert.AreEqual('R', image.StorageDevice);
            Assert.AreEqual(1, image.MagnificationFactorX);
            Assert.AreEqual(1, image.MagnificationFactorY);
            Assert.AreEqual(0, image.PositionX);
        }

        [TestMethod]
        public void RecallGraphic_EmptyMagnificationKeepsDefault()
        {
            ZplRecallGraphic image = AnalyzerTestHelper.Single<ZplRecallGraphic>("^FT10,20^XGR:LOGO.GRF,,3");

            Assert.AreEqual(1, image.MagnificationFactorX);
            Assert.AreEqual(3, image.MagnificationFactorY);
            Assert.IsNotNull(image.FieldTypeset);
        }

        [TestMethod]
        public void RecallGraphic_InvalidMagnificationKeepsDefaultAndExtraParametersAreIgnored()
        {
            ZplRecallGraphic image = AnalyzerTestHelper.Single<ZplRecallGraphic>("^XGE:LOGO.GRF,x,2,9");

            Assert.AreEqual('E', image.StorageDevice);
            Assert.AreEqual(1, image.MagnificationFactorX);
            Assert.AreEqual(2, image.MagnificationFactorY);
        }

        [TestMethod]
        public void RecallGraphic_NameWithoutExtensionUsesGrf()
        {
            Assert.AreEqual("LOGO.GRF", AnalyzerTestHelper.Single<ZplRecallGraphic>("^XGLOGO").FileName);
        }

        // ~DG d:o.x,t,w,data
        [TestMethod]
        public void DownloadGraphics_StoresGraphicInPrinterStorage()
        {
            PrinterStorage storage = new();

            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze("~DGR:LOGO.GRF,8,1," + Box8x8, storage);

            Assert.IsEmpty(info.Errors);
            Assert.IsEmpty(info.UnknownCommands);
            byte[] stored = storage.GetFile('R', "LOGO.GRF");
            StringAssert.StartsWith(ByteHelper.BytesToHex(stored), PngSignatureHex);
        }

        [TestMethod]
        public void DownloadGraphics_WithoutDeviceUsesR()
        {
            PrinterStorage storage = new();

            AnalyzerTestHelper.Analyze("~DGIMG.GRF,8,1," + Box8x8, storage);

            Assert.IsNotEmpty(storage.GetFile('R', "IMG.GRF"));
        }

        [TestMethod]
        public void DownloadGraphics_InvalidCommandIsIgnored()
        {
            PrinterStorage storage = new();

            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze("~DGR:LOGO", storage);

            Assert.IsEmpty(info.Errors);
            Assert.IsEmpty(storage.GetFile('R', "LOGO"));
        }

        // ~DY d:f,b,x,t,w,data
        [TestMethod]
        public void DownloadObjects_StoresRawBytes()
        {
            PrinterStorage storage = new();

            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze("~DYE:OBJ,B,G,3,1,FF00AA", storage);

            Assert.IsEmpty(info.Errors);
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00, 0xAA }, storage.GetFile('E', "OBJ.GRF"));
        }

        [TestMethod]
        public void DownloadObjects_IncompleteCommandIsReportedAsError()
        {
            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze("~DYE:OBJ,B");

            Assert.HasCount(1, info.Errors);
        }

        [TestMethod]
        public void DownloadObjects_InvalidLengthsAreIgnored()
        {
            PrinterStorage storage = new();

            AnalyzerTestHelper.Analyze("~DYE:OBJ,B,G,x,y,0102", storage);

            CollectionAssert.AreEqual(new byte[] { 1, 2 }, storage.GetFile('E', "OBJ.GRF"));
        }

        // ^CI
        [TestMethod]
        [DataRow("^CI28", InternationalFont.UTF8)]
        [DataRow("^CI27", InternationalFont.ZCP1252)]
        [DataRow("^CI0", InternationalFont.ZCP850)]
        [DataRow("^CI13", InternationalFont.ZCP850_13)]
        public void ChangeInternationalFont_Encoding(string command, InternationalFont expected)
        {
            ZplChangeInternationalFont element = AnalyzerTestHelper.Single<ZplChangeInternationalFont>(command);

            Assert.AreEqual(expected, element.InternationalFont);
        }

        [TestMethod]
        [DataRow("^CI")]
        [DataRow("^CIabc")]
        [DataRow("^CI99")]
        public void ChangeInternationalFont_MissingOrUnknownEncodingIsCodePage850(string command)
        {
            ZplChangeInternationalFont element = AnalyzerTestHelper.Single<ZplChangeInternationalFont>(command);

            Assert.AreEqual(InternationalFont.ZCP850, element.InternationalFont);
        }

        [TestMethod]
        public void ChangeInternationalFont_RemapParametersDoNotChangeTheEncoding()
        {
            // ^CIa,s1,d1,...: only the first parameter selects the encoding
            ZplChangeInternationalFont element = AnalyzerTestHelper.Single<ZplChangeInternationalFont>("^CI28,35,36");

            Assert.AreEqual(InternationalFont.UTF8, element.InternationalFont);
        }
    }
}
