using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Fields are finalized at ^FS: the barcode commands, ^BY defaults, position and field data are combined into elements.
    /// </summary>
    [TestClass]
    public class FieldSeparatorBarcodeTest
    {
        private const string Defaults = "^BY4,2.5,60^FO11,22";

        private static T Barcode<T>(string barcodeCommand, string content = "12345") where T : ZplBarcode
        {
            T barcode = AnalyzerTestHelper.Single<T>(Defaults + barcodeCommand + "^FD" + content + "^FS");

            Assert.AreEqual(content, barcode.Content);
            Assert.AreEqual(11, barcode.PositionX);
            Assert.AreEqual(22, barcode.PositionY);
            Assert.AreEqual(4, barcode.ModuleWidth);
            Assert.AreEqual(2.5, barcode.WideBarToNarrowBarWidthRatio);
            return barcode;
        }

        [TestMethod]
        [DataRow("^B1", typeof(ZplBarcode11))]
        [DataRow("^B3", typeof(ZplBarcode39))]
        [DataRow("^BA", typeof(ZplBarcode93))]
        [DataRow("^BC", typeof(ZplBarcode128))]
        [DataRow("^BE", typeof(ZplBarcodeEan13))]
        [DataRow("^B8", typeof(ZplBarcodeEan8))]
        [DataRow("^B2", typeof(ZplBarcodeInterleaved2of5))]
        [DataRow("^BI", typeof(ZplBarcodeIndustrial2of5))]
        [DataRow("^BU", typeof(ZplBarcodeUpcA))]
        [DataRow("^B9", typeof(ZplBarcodeUpcE))]
        [DataRow("^BS", typeof(ZplBarcodeUpcExtension))]
        [DataRow("^BK", typeof(ZplBarcodeAnsiCodabar))]
        [DataRow("^BM", typeof(ZplBarcodeMsi))]
        [DataRow("^BL", typeof(ZplBarcodeLogmars))]
        public void OmittedHeightUsesBarcodeFieldDefault(string command, System.Type expectedType)
        {
            ZplBarcode barcode = AnalyzerTestHelper.Single<ZplBarcode>(Defaults + command + "^FD123^FS");

            Assert.IsInstanceOfType(barcode, expectedType);
            Assert.AreEqual(60, barcode.Height);
            Assert.AreEqual(4, barcode.ModuleWidth);
            Assert.AreEqual(2.5, barcode.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual("123", barcode.Content);
            Assert.AreEqual(11, barcode.PositionX);
            Assert.AreEqual(22, barcode.PositionY);
            Assert.AreEqual(FieldOrientation.Normal, barcode.FieldOrientation);
            Assert.IsNull(barcode.HexadecimalIndicator);
        }

        [TestMethod]
        [DataRow("^B1N,N,33", typeof(ZplBarcode11))]
        [DataRow("^B3N,N,33", typeof(ZplBarcode39))]
        [DataRow("^BAN,33", typeof(ZplBarcode93))]
        [DataRow("^BCN,33", typeof(ZplBarcode128))]
        [DataRow("^BEN,33", typeof(ZplBarcodeEan13))]
        [DataRow("^B8N,33", typeof(ZplBarcodeEan8))]
        [DataRow("^B2N,33", typeof(ZplBarcodeInterleaved2of5))]
        [DataRow("^BIN,33", typeof(ZplBarcodeIndustrial2of5))]
        [DataRow("^BUN,33", typeof(ZplBarcodeUpcA))]
        [DataRow("^B9N,33", typeof(ZplBarcodeUpcE))]
        [DataRow("^BSN,33", typeof(ZplBarcodeUpcExtension))]
        [DataRow("^BKN,N,33", typeof(ZplBarcodeAnsiCodabar))]
        [DataRow("^BMN,B,33", typeof(ZplBarcodeMsi))]
        [DataRow("^BLN,33", typeof(ZplBarcodeLogmars))]
        public void ExplicitHeightOverridesBarcodeFieldDefault(string command, System.Type expectedType)
        {
            ZplBarcode barcode = AnalyzerTestHelper.Single<ZplBarcode>(Defaults + command + "^FD123^FS");

            Assert.IsInstanceOfType(barcode, expectedType);
            Assert.AreEqual(33, barcode.Height);
        }

        [TestMethod]
        [DataRow("^B1R", FieldOrientation.Rotated90)]
        [DataRow("^B3I", FieldOrientation.Rotated180)]
        [DataRow("^BCB", FieldOrientation.Rotated270)]
        public void BarcodeOrientationAndHexadecimalIndicator(string command, FieldOrientation expected)
        {
            ZplBarcode barcode = AnalyzerTestHelper.Single<ZplBarcode>(Defaults + "^FH\\" + command + "^FD123^FS");

            Assert.AreEqual(expected, barcode.FieldOrientation);
            Assert.AreEqual('\\', barcode.HexadecimalIndicator);
        }

        [TestMethod]
        public void Code11()
        {
            ZplBarcode11 barcode = Barcode<ZplBarcode11>("^B1N,Y,40,N,Y");

            Assert.AreEqual(Code11CheckDigitCount.One, barcode.CheckDigitCount);
            Assert.AreEqual(40, barcode.Height);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code39()
        {
            ZplBarcode39 barcode = Barcode<ZplBarcode39>("^B3N,Y,40,N,Y");

            Assert.IsTrue(barcode.Mod43CheckDigit);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code93()
        {
            ZplBarcode93 barcode = Barcode<ZplBarcode93>("^BAN,40,N,Y,Y");

            Assert.IsTrue(barcode.CheckDigit);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code128()
        {
            ZplBarcode128 barcode = Barcode<ZplBarcode128>("^BCN,40,N,Y,Y,A");

            Assert.AreEqual("A", barcode.Mode);
            Assert.IsTrue(barcode.UccCheckDigit);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Ean13AndEan8()
        {
            Assert.IsFalse(Barcode<ZplBarcodeEan13>("^BEN,40,N,Y").PrintInterpretationLine);
            Assert.IsTrue(Barcode<ZplBarcodeEan8>("^B8N,40,Y,Y").PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Interleaved2of5()
        {
            ZplBarcodeInterleaved2of5 barcode = Barcode<ZplBarcodeInterleaved2of5>("^B2N,40,N,Y,Y");

            Assert.IsTrue(barcode.Mod10CheckDigit);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Industrial2of5()
        {
            ZplBarcodeIndustrial2of5 barcode = Barcode<ZplBarcodeIndustrial2of5>("^BIN,40,N,Y");

            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void UpcA()
        {
            ZplBarcodeUpcA barcode = Barcode<ZplBarcodeUpcA>("^BUN,40,N,Y,N");

            Assert.IsFalse(barcode.PrintCheckDigit);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void UpcE()
        {
            ZplBarcodeUpcE barcode = Barcode<ZplBarcodeUpcE>("^B9N,40,N,Y,N");

            Assert.IsFalse(barcode.PrintCheckDigit);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void UpcExtension()
        {
            ZplBarcodeUpcExtension barcode = Barcode<ZplBarcodeUpcExtension>("^BSN,40,N,N");

            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsFalse(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void AnsiCodabar()
        {
            ZplBarcodeAnsiCodabar barcode = Barcode<ZplBarcodeAnsiCodabar>("^BKN,Y,40,N,Y,B,C");

            Assert.IsTrue(barcode.CheckDigit);
            Assert.AreEqual('B', barcode.StartCharacter);
            Assert.AreEqual('C', barcode.StopCharacter);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void AnsiCodabar_StartCharacterWithoutStopCharacter()
        {
            ZplBarcodeAnsiCodabar barcode = Barcode<ZplBarcodeAnsiCodabar>("^BKN,N,40,Y,N,D");

            Assert.AreEqual('D', barcode.StartCharacter);
            Assert.AreEqual('A', barcode.StopCharacter);
        }

        [TestMethod]
        public void AnsiCodabar_InvalidStartCharacterIsReportedAsError()
        {
            Models.AnalyzeInfo info = AnalyzerTestHelper.Analyze(AnalyzerTestHelper.Label(Defaults + "^BKN,N,40,Y,N,X,A^FD123^FS"));

            Assert.HasCount(1, info.Errors);
            Assert.IsEmpty(AnalyzerTestHelper.AllElements(info));
        }

        [TestMethod]
        public void Msi()
        {
            ZplBarcodeMsi barcode = Barcode<ZplBarcodeMsi>("^BMN,D,40,N,Y,Y");

            Assert.AreEqual(MsiBarcodeCheckDigitMode.Mod_1_11_1_10, barcode.CheckDigitMode);
            Assert.IsFalse(barcode.PrintInterpretationLine);
            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
            Assert.IsTrue(barcode.PrintCheckDigit);
        }

        [TestMethod]
        public void Logmars()
        {
            ZplBarcodeLogmars barcode = Barcode<ZplBarcodeLogmars>("^BLN,40,Y");

            Assert.IsTrue(barcode.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        [DataRow("^BMN,B")]
        [DataRow("^BLN")]
        [DataRow("^BCN")]
        public void FieldTypesetWithoutCoordinatesKeepsDefaultPosition(string command)
        {
            ZplBarcode barcode = AnalyzerTestHelper.Single<ZplBarcode>("^FT" + command + "^FD123^FS");

            Assert.IsTrue(barcode.UseDefaultPosition);
            Assert.IsNotNull(barcode.FieldTypeset);
            Assert.IsNull(barcode.FieldOrigin);
        }

        [TestMethod]
        [DataRow("^BMN,B")]
        [DataRow("^BLN")]
        [DataRow("^BCN")]
        public void FieldTypesetWithCoordinates(string command)
        {
            ZplBarcode barcode = AnalyzerTestHelper.Single<ZplBarcode>("^FT30,40" + command + "^FD123^FS");

            Assert.IsFalse(barcode.UseDefaultPosition);
            Assert.AreEqual(30, barcode.PositionX);
            Assert.AreEqual(40, barcode.PositionY);
            Assert.IsNotNull(barcode.FieldTypeset);
        }

        [TestMethod]
        public void DataMatrix()
        {
            ZplDataMatrix barcode = AnalyzerTestHelper.Single<ZplDataMatrix>(Defaults + "^BXR,7,200,20,22,4,#,2^FDabc^FS");

            Assert.AreEqual("abc", barcode.Content);
            Assert.AreEqual(FieldOrientation.Rotated90, barcode.FieldOrientation);
            Assert.AreEqual(7, barcode.Height);
            Assert.AreEqual(QualityLevel.ECC200, barcode.QualityLevel);
            Assert.AreEqual(20, barcode.Columns);
            Assert.AreEqual(22, barcode.Rows);
            Assert.AreEqual(4, barcode.Format);
            Assert.AreEqual('#', barcode.EscapeSequence);
            Assert.AreEqual(2, barcode.AspectRatio);
        }

        [TestMethod]
        public void DataMatrix_OmittedHeightUsesBarcodeFieldDefault()
        {
            Assert.AreEqual(60, AnalyzerTestHelper.Single<ZplDataMatrix>(Defaults + "^BXN^FDabc^FS").Height);
        }

        [TestMethod]
        public void MaxiCode()
        {
            ZplMaxiCode barcode = AnalyzerTestHelper.Single<ZplMaxiCode>(Defaults + "^BD4,2,5^FDabc^FS");

            Assert.AreEqual("abc", barcode.Content);
            Assert.AreEqual(4, barcode.Mode);
            Assert.AreEqual(2, barcode.Position);
            Assert.AreEqual(5, barcode.Total);
            Assert.AreEqual(11, barcode.PositionX);
        }

        [TestMethod]
        public void Pdf417()
        {
            ZplPDF417 barcode = AnalyzerTestHelper.Single<ZplPDF417>(Defaults + "^B7R,40,3,10,20,Y^FDabc^FS");

            Assert.AreEqual("abc", barcode.Content);
            Assert.AreEqual(FieldOrientation.Rotated90, barcode.FieldOrientation);
            Assert.AreEqual(40, barcode.Height);
            Assert.AreEqual(4, barcode.ModuleWidth);
            Assert.AreEqual(3, barcode.SecurityLevel);
            Assert.AreEqual(10, barcode.Columns);
            Assert.AreEqual(20, barcode.Rows);
            Assert.IsTrue(barcode.Compact);
        }

        [TestMethod]
        public void Pdf417_EmptyHeightUsesBarcodeFieldDefault()
        {
            Assert.AreEqual(60, AnalyzerTestHelper.Single<ZplPDF417>(Defaults + "^B7N,,2^FDabc^FS").Height);
        }

        [TestMethod]
        public void Aztec()
        {
            ZplAztecBarcode barcode = AnalyzerTestHelper.Single<ZplAztecBarcode>(Defaults + "^BOR,5,Y,25,Y,3,MYID^FDabc^FS");

            Assert.AreEqual("abc", barcode.Content);
            Assert.AreEqual(FieldOrientation.Rotated90, barcode.FieldOrientation);
            Assert.AreEqual(5, barcode.MagnificationFactor);
            Assert.IsTrue(barcode.ExtendedChannel);
            Assert.AreEqual(25, barcode.ErrorControl);
            // MenuSymbol is passed on, but ZplAztecBarcode (core) never stores it (core bug, not asserted here)
            Assert.AreEqual(3, barcode.SymbolCount);
            Assert.AreEqual("MYID", barcode.IdField);
        }

        [TestMethod]
        public void GraphicSymbol()
        {
            ZplGraphicSymbol symbol = AnalyzerTestHelper.Single<ZplGraphicSymbol>("^FO11,22^GSR,50,60^FDA^FS");

            Assert.AreEqual(GraphicSymbolCharacter.RegisteredTradeMark, symbol.Character);
            Assert.AreEqual(FieldOrientation.Rotated90, symbol.FieldOrientation);
            Assert.AreEqual(50, symbol.Height);
            Assert.AreEqual(60, symbol.Width);
            Assert.AreEqual(11, symbol.PositionX);
        }

        [TestMethod]
        public void BarcodeFieldDataIsConsumedByTheField()
        {
            // The second field has no barcode command: it must be a plain text field, not the barcode again
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^FO0,0^BCN^FD123^FS^FO0,50^FDtext^FS");

            Assert.HasCount(2, elements);
            Assert.IsInstanceOfType<ZplBarcode128>(elements[0]);
            Assert.IsInstanceOfType<ZplTextField>(elements[1]);
        }

        [TestMethod]
        public void BarcodeFieldDefaultsApplyToLaterFields()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^BY3,2.0,30^FO0,0^BCN^FD1^FS^FO0,50^B3N^FD2^FS");

            Assert.AreEqual(3, ((ZplBarcode)elements[0]).ModuleWidth);
            Assert.AreEqual(30, ((ZplBarcode)elements[0]).Height);
            Assert.AreEqual(3, ((ZplBarcode)elements[1]).ModuleWidth);
            Assert.AreEqual(30, ((ZplBarcode)elements[1]).Height);
        }
    }
}
