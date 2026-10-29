using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Parameter parsing of the barcode command analyzers (defaults per ZPL II programming guide).
    /// </summary>
    [TestClass]
    public class BarcodeAnalyzersTest
    {
        // ^BK ANSI Codabar: o,e,h,f,g,k,l
        [TestMethod]
        public void AnsiCodabar_Defaults()
        {
            AnsiCodabarFieldData data = AnalyzerTestHelper.FieldData<AnsiCodabarFieldData>(new AnsiCodabarBarcodeZplCommandAnalyzer(), "^BK");

            Assert.AreEqual(FieldOrientation.Normal, data.FieldOrientation);
            Assert.IsFalse(data.CheckDigit);
            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.AreEqual('A', data.StartCharacter);
            Assert.AreEqual('A', data.StopCharacter);
        }

        [TestMethod]
        public void AnsiCodabar_AllParameters()
        {
            AnsiCodabarFieldData data = AnalyzerTestHelper.FieldData<AnsiCodabarFieldData>(new AnsiCodabarBarcodeZplCommandAnalyzer(), "^BKR,Y,50,N,Y,B,C");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.IsTrue(data.CheckDigit);
            Assert.AreEqual(50, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.AreEqual('B', data.StartCharacter);
            Assert.AreEqual('C', data.StopCharacter);
        }

        [TestMethod]
        public void AnsiCodabar_StartCharacterWithoutStopCharacter()
        {
            // The start character is the sixth parameter; the stop character (seventh) is missing
            AnsiCodabarFieldData data = AnalyzerTestHelper.FieldData<AnsiCodabarFieldData>(new AnsiCodabarBarcodeZplCommandAnalyzer(), "^BKN,N,40,Y,N,D");

            Assert.AreEqual('D', data.StartCharacter);
            Assert.AreEqual('A', data.StopCharacter);
        }

        [TestMethod]
        public void AnsiCodabar_EmptyAndInvalidParametersUseDefaults()
        {
            AnsiCodabarFieldData empty = AnalyzerTestHelper.FieldData<AnsiCodabarFieldData>(new AnsiCodabarBarcodeZplCommandAnalyzer(), "^BK,,,,,,");
            Assert.AreEqual(FieldOrientation.Normal, empty.FieldOrientation);
            Assert.IsFalse(empty.CheckDigit);
            Assert.IsNull(empty.Height);
            Assert.IsTrue(empty.PrintInterpretationLine);
            Assert.IsFalse(empty.PrintInterpretationLineAboveCode);
            Assert.AreEqual('A', empty.StartCharacter);
            Assert.AreEqual('A', empty.StopCharacter);

            AnsiCodabarFieldData invalid = AnalyzerTestHelper.FieldData<AnsiCodabarFieldData>(new AnsiCodabarBarcodeZplCommandAnalyzer(), "^BKX,N,abc,N,N,AB,CD");
            Assert.IsNull(invalid.Height);
            Assert.AreEqual('A', invalid.StartCharacter);
            Assert.AreEqual('A', invalid.StopCharacter);
        }

        // ^B0 / ^BO Aztec: a,b,c,d,e,f,g
        [TestMethod]
        [DataRow(typeof(AztecBarcodeZplCommandAnalyzer))]
        [DataRow(typeof(AztecBarcodeAltZplCommandAnalyzer))]
        public void Aztec_Defaults(System.Type analyzerType)
        {
            IZplCommandAnalyzer analyzer = (IZplCommandAnalyzer)System.Activator.CreateInstance(analyzerType);
            AztecBarcodeFieldData data = AnalyzerTestHelper.FieldData<AztecBarcodeFieldData>(analyzer, ((ZplCommandAnalyzerBase)analyzer).PrinterCommandPrefix);

            Assert.AreEqual(FieldOrientation.Normal, data.FieldOrientation);
            Assert.AreEqual(2, data.MagnificationFactor);
            Assert.IsFalse(data.ExtendedChannel);
            Assert.AreEqual(0, data.ErrorControl);
            Assert.IsFalse(data.MenuSymbol);
            Assert.AreEqual(1, data.SymbolCount);
            Assert.IsNull(data.IdField);
        }

        [TestMethod]
        [DataRow(typeof(AztecBarcodeZplCommandAnalyzer))]
        [DataRow(typeof(AztecBarcodeAltZplCommandAnalyzer))]
        public void Aztec_AllParameters(System.Type analyzerType)
        {
            IZplCommandAnalyzer analyzer = (IZplCommandAnalyzer)System.Activator.CreateInstance(analyzerType);
            string prefix = ((ZplCommandAnalyzerBase)analyzer).PrinterCommandPrefix;
            AztecBarcodeFieldData data = AnalyzerTestHelper.FieldData<AztecBarcodeFieldData>(analyzer, prefix + "R,5,Y,25,Y,3,MYID");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(5, data.MagnificationFactor);
            Assert.IsTrue(data.ExtendedChannel);
            Assert.AreEqual(25, data.ErrorControl);
            Assert.IsTrue(data.MenuSymbol);
            Assert.AreEqual(3, data.SymbolCount);
            Assert.AreEqual("MYID", data.IdField);
        }

        [TestMethod]
        [DataRow(typeof(AztecBarcodeZplCommandAnalyzer))]
        [DataRow(typeof(AztecBarcodeAltZplCommandAnalyzer))]
        public void Aztec_InvalidNumbersUseDefaults(System.Type analyzerType)
        {
            IZplCommandAnalyzer analyzer = (IZplCommandAnalyzer)System.Activator.CreateInstance(analyzerType);
            string prefix = ((ZplCommandAnalyzerBase)analyzer).PrinterCommandPrefix;
            AztecBarcodeFieldData data = AnalyzerTestHelper.FieldData<AztecBarcodeFieldData>(analyzer, prefix + "N,x,N,y,N,z");

            Assert.AreEqual(2, data.MagnificationFactor);
            Assert.AreEqual(0, data.ErrorControl);
            Assert.AreEqual(1, data.SymbolCount);
        }

        // ^B1 Code 11: o,e,h,f,g
        [TestMethod]
        public void Code11_Defaults()
        {
            Code11BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code11BarcodeFieldData>(new Code11BarcodeZplCommandAnalyzer(), "^B1");

            Assert.AreEqual(Code11CheckDigitCount.Two, data.CheckDigitCount);
            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code11_AllParameters()
        {
            Code11BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code11BarcodeFieldData>(new Code11BarcodeZplCommandAnalyzer(), "^B1I,Y,70,N,Y");

            Assert.AreEqual(FieldOrientation.Rotated180, data.FieldOrientation);
            Assert.AreEqual(Code11CheckDigitCount.One, data.CheckDigitCount);
            Assert.AreEqual(70, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code11_NoCheckDigitMeansTwoDigitsAndInvalidHeightIsIgnored()
        {
            Code11BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code11BarcodeFieldData>(new Code11BarcodeZplCommandAnalyzer(), "^B1B,N,abc,,");

            Assert.AreEqual(FieldOrientation.Rotated270, data.FieldOrientation);
            Assert.AreEqual(Code11CheckDigitCount.Two, data.CheckDigitCount);
            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        // ^BC Code 128: o,h,f,g,e,m
        [TestMethod]
        public void Code128_Defaults()
        {
            Code128BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code128BarcodeFieldData>(new Code128BarcodeZplCommandAnalyzer(), "^BC");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.UccCheckDigit);
            Assert.AreEqual("N", data.Mode);
        }

        [TestMethod]
        public void Code128_AllParameters()
        {
            Code128BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code128BarcodeFieldData>(new Code128BarcodeZplCommandAnalyzer(), "^BCR,80,N,Y,Y,A");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(80, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.IsTrue(data.UccCheckDigit);
            Assert.AreEqual("A", data.Mode);
        }

        [TestMethod]
        public void Code128_EmptyAndInvalidParametersUseDefaults()
        {
            Code128BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code128BarcodeFieldData>(new Code128BarcodeZplCommandAnalyzer(), "^BC,abc,,,,");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.UccCheckDigit);
            Assert.AreEqual(string.Empty, data.Mode);
        }

        // ^B3 Code 39: o,e,h,f,g
        [TestMethod]
        public void Code39_Defaults()
        {
            Code39BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code39BarcodeFieldData>(new Code39BarcodeZplCommandAnalyzer(), "^B3");

            Assert.IsFalse(data.Mod43CheckDigit);
            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code39_AllParameters()
        {
            Code39BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code39BarcodeFieldData>(new Code39BarcodeZplCommandAnalyzer(), "^B3B,Y,90,N,Y");

            Assert.AreEqual(FieldOrientation.Rotated270, data.FieldOrientation);
            Assert.IsTrue(data.Mod43CheckDigit);
            Assert.AreEqual(90, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Code39_EmptyAndInvalidParametersUseDefaults()
        {
            Code39BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code39BarcodeFieldData>(new Code39BarcodeZplCommandAnalyzer(), "^B3N,,x,,");

            Assert.IsFalse(data.Mod43CheckDigit);
            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        // ^BA Code 93: o,h,f,g,e
        [TestMethod]
        public void Code93_Defaults()
        {
            Code93BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code93BarcodeFieldData>(new Code93BarcodeZplCommandAnalyzer(), "^BA");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.PrintCheckDigit);
        }

        [TestMethod]
        public void Code93_AllParameters()
        {
            Code93BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code93BarcodeFieldData>(new Code93BarcodeZplCommandAnalyzer(), "^BAI,60,N,Y,Y");

            Assert.AreEqual(FieldOrientation.Rotated180, data.FieldOrientation);
            Assert.AreEqual(60, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.IsTrue(data.PrintCheckDigit);
        }

        [TestMethod]
        public void Code93_InvalidHeightIsIgnored()
        {
            Code93BarcodeFieldData data = AnalyzerTestHelper.FieldData<Code93BarcodeFieldData>(new Code93BarcodeZplCommandAnalyzer(), "^BAN,x");

            Assert.IsNull(data.Height);
        }

        // ^BE EAN-13: o,h,f,g
        [TestMethod]
        public void Ean13_Defaults()
        {
            CodeEAN13BarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeEAN13BarcodeFieldData>(new CodeEAN13BarcodeZplCommandAnalyzer(), "^BE");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Ean13_AllParameters()
        {
            CodeEAN13BarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeEAN13BarcodeFieldData>(new CodeEAN13BarcodeZplCommandAnalyzer(), "^BER,55,N,Y");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(55, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Ean13_InvalidHeightIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<CodeEAN13BarcodeFieldData>(new CodeEAN13BarcodeZplCommandAnalyzer(), "^BEN,x").Height);
        }

        // ^B8 EAN-8: o,h,f,g
        [TestMethod]
        public void Ean8_Defaults()
        {
            CodeEAN8BarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeEAN8BarcodeFieldData>(new CodeEAN8BarcodeZplCommandAnalyzer(), "^B8");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Ean8_AllParameters()
        {
            CodeEAN8BarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeEAN8BarcodeFieldData>(new CodeEAN8BarcodeZplCommandAnalyzer(), "^B8B,45,N,Y");

            Assert.AreEqual(FieldOrientation.Rotated270, data.FieldOrientation);
            Assert.AreEqual(45, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Ean8_InvalidHeightIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<CodeEAN8BarcodeFieldData>(new CodeEAN8BarcodeZplCommandAnalyzer(), "^B8N,x").Height);
        }

        // ^BI Industrial 2 of 5: o,h,f,g
        [TestMethod]
        public void Industrial2of5_Defaults()
        {
            Industrial2of5BarcodeFieldData data = AnalyzerTestHelper.FieldData<Industrial2of5BarcodeFieldData>(new Industrial2of5BarcodeZplCommandAnalyzer(), "^BI");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Industrial2of5_AllParameters()
        {
            Industrial2of5BarcodeFieldData data = AnalyzerTestHelper.FieldData<Industrial2of5BarcodeFieldData>(new Industrial2of5BarcodeZplCommandAnalyzer(), "^BII,35,N,Y");

            Assert.AreEqual(FieldOrientation.Rotated180, data.FieldOrientation);
            Assert.AreEqual(35, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Industrial2of5_InvalidHeightIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<Industrial2of5BarcodeFieldData>(new Industrial2of5BarcodeZplCommandAnalyzer(), "^BIN,x").Height);
        }

        // ^B2 Interleaved 2 of 5: o,h,f,g,e
        [TestMethod]
        public void Interleaved2of5_Defaults()
        {
            Interleaved2of5BarcodeFieldData data = AnalyzerTestHelper.FieldData<Interleaved2of5BarcodeFieldData>(new Interleaved2of5BarcodeZplCommandAnalyzer(), "^B2");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.CalculateAndPrintMod10CheckDigit);
        }

        [TestMethod]
        public void Interleaved2of5_AllParameters()
        {
            Interleaved2of5BarcodeFieldData data = AnalyzerTestHelper.FieldData<Interleaved2of5BarcodeFieldData>(new Interleaved2of5BarcodeZplCommandAnalyzer(), "^B2R,65,N,Y,Y");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(65, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.IsTrue(data.CalculateAndPrintMod10CheckDigit);
        }

        [TestMethod]
        public void Interleaved2of5_InvalidHeightIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<Interleaved2of5BarcodeFieldData>(new Interleaved2of5BarcodeZplCommandAnalyzer(), "^B2N,x").Height);
        }

        // ^BL LOGMARS: o,h,g
        [TestMethod]
        public void Logmars_Defaults()
        {
            CodeLogmarsBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeLogmarsBarcodeFieldData>(new LogmarsZplCommandAnalyzer(), "^BL");

            Assert.IsNull(data.Height);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Logmars_AllParameters()
        {
            CodeLogmarsBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeLogmarsBarcodeFieldData>(new LogmarsZplCommandAnalyzer(), "^BLR,100,Y");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(100, data.Height);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void Logmars_EmptyInterpretationLineAboveCodeDefaultsToNo()
        {
            // ZPL II: ^BL g (print interpretation line above code) defaults to N
            CodeLogmarsBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeLogmarsBarcodeFieldData>(new LogmarsZplCommandAnalyzer(), "^BLN,x,");

            Assert.IsNull(data.Height);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        // ^BM MSI: o,e,h,f,g,h
        [TestMethod]
        public void Msi_Defaults()
        {
            CodeMsiBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeMsiBarcodeFieldData>(new MsiBarcodeZplCommandAnalyzer(), "^BM");

            Assert.AreEqual(MsiBarcodeCheckDigitMode.Mod1_10, data.CheckDigitSelection);
            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.PrintInterpretationLineWithCheckDigit);
        }

        [TestMethod]
        public void Msi_AllParameters()
        {
            CodeMsiBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeMsiBarcodeFieldData>(new MsiBarcodeZplCommandAnalyzer(), "^BMI,C,120,N,Y,Y");

            Assert.AreEqual(FieldOrientation.Rotated180, data.FieldOrientation);
            Assert.AreEqual(MsiBarcodeCheckDigitMode.Mod2_10, data.CheckDigitSelection);
            Assert.AreEqual(120, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.IsTrue(data.PrintInterpretationLineWithCheckDigit);
        }

        [TestMethod]
        [DataRow("A", MsiBarcodeCheckDigitMode.None)]
        [DataRow("B", MsiBarcodeCheckDigitMode.Mod1_10)]
        [DataRow("C", MsiBarcodeCheckDigitMode.Mod2_10)]
        [DataRow("D", MsiBarcodeCheckDigitMode.Mod_1_11_1_10)]
        [DataRow("", MsiBarcodeCheckDigitMode.Mod1_10)]
        [DataRow("Z", MsiBarcodeCheckDigitMode.Mod1_10)]
        public void Msi_CheckDigitSelection(string selection, MsiBarcodeCheckDigitMode expected)
        {
            CodeMsiBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeMsiBarcodeFieldData>(new MsiBarcodeZplCommandAnalyzer(), "^BMN," + selection);

            Assert.AreEqual(expected, data.CheckDigitSelection);
        }

        [TestMethod]
        public void Msi_EmptyAndInvalidParametersUseDefaults()
        {
            CodeMsiBarcodeFieldData data = AnalyzerTestHelper.FieldData<CodeMsiBarcodeFieldData>(new MsiBarcodeZplCommandAnalyzer(), "^BMN,B,x,,,");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.PrintInterpretationLineWithCheckDigit);
        }

        // ^BU UPC-A: o,h,f,g,e
        [TestMethod]
        public void UpcA_Defaults()
        {
            UpcABarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcABarcodeFieldData>(new UpcABarcodeZplCommandAnalyzer(), "^BU");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsTrue(data.PrintCheckDigit);
        }

        [TestMethod]
        public void UpcA_AllParameters()
        {
            UpcABarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcABarcodeFieldData>(new UpcABarcodeZplCommandAnalyzer(), "^BUR,75,N,Y,N");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(75, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.PrintCheckDigit);
        }

        [TestMethod]
        public void UpcA_InvalidHeightIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<UpcABarcodeFieldData>(new UpcABarcodeZplCommandAnalyzer(), "^BUN,x").Height);
        }

        // ^B9 UPC-E: o,h,f,g,e
        [TestMethod]
        public void UpcE_Defaults()
        {
            UpcEBarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcEBarcodeFieldData>(new UpcEBarcodeZplCommandAnalyzer(), "^B9");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
            Assert.IsTrue(data.PrintCheckDigit);
        }

        [TestMethod]
        public void UpcE_AllParameters()
        {
            UpcEBarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcEBarcodeFieldData>(new UpcEBarcodeZplCommandAnalyzer(), "^B9B,25,N,Y,N");

            Assert.AreEqual(FieldOrientation.Rotated270, data.FieldOrientation);
            Assert.AreEqual(25, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
            Assert.IsFalse(data.PrintCheckDigit);
        }

        [TestMethod]
        public void UpcE_InvalidHeightIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<UpcEBarcodeFieldData>(new UpcEBarcodeZplCommandAnalyzer(), "^B9N,x").Height);
        }

        // ^BS UPC/EAN extension: o,h,f,g
        [TestMethod]
        public void UpcExtension_Defaults()
        {
            UpcExtensionBarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcExtensionBarcodeFieldData>(new UpcExtensionBarcodeZplCommandAnalyzer(), "^BS");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void UpcExtension_AllParameters()
        {
            UpcExtensionBarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcExtensionBarcodeFieldData>(new UpcExtensionBarcodeZplCommandAnalyzer(), "^BSI,30,N,N");

            Assert.AreEqual(FieldOrientation.Rotated180, data.FieldOrientation);
            Assert.AreEqual(30, data.Height);
            Assert.IsFalse(data.PrintInterpretationLine);
            Assert.IsFalse(data.PrintInterpretationLineAboveCode);
        }

        [TestMethod]
        public void UpcExtension_EmptyAndInvalidParametersUseDefaults()
        {
            UpcExtensionBarcodeFieldData data = AnalyzerTestHelper.FieldData<UpcExtensionBarcodeFieldData>(new UpcExtensionBarcodeZplCommandAnalyzer(), "^BSN,x,,");

            Assert.IsNull(data.Height);
            Assert.IsTrue(data.PrintInterpretationLine);
            Assert.IsTrue(data.PrintInterpretationLineAboveCode);
        }

        // ^BX Data Matrix: o,h,s,c,r,f,g,a
        [TestMethod]
        public void DataMatrix_Defaults()
        {
            DataMatrixFieldData data = AnalyzerTestHelper.FieldData<DataMatrixFieldData>(new DataMatrixZplCommandAnalyzer(), "^BX");

            Assert.IsNull(data.Height);
            Assert.AreEqual(QualityLevel.ECC0, data.QualityLevel);
            Assert.IsNull(data.Columns);
            Assert.IsNull(data.Rows);
            Assert.AreEqual(6, data.Format);
            Assert.AreEqual('~', data.EscapeSequence);
            Assert.IsNull(data.AspectRatio);
        }

        [TestMethod]
        public void DataMatrix_AllParameters()
        {
            DataMatrixFieldData data = AnalyzerTestHelper.FieldData<DataMatrixFieldData>(new DataMatrixZplCommandAnalyzer(), "^BXR,5,200,20,22,4,#,2");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(5, data.Height);
            Assert.AreEqual(QualityLevel.ECC200, data.QualityLevel);
            Assert.AreEqual(20, data.Columns);
            Assert.AreEqual(22, data.Rows);
            Assert.AreEqual(4, data.Format);
            Assert.AreEqual('#', data.EscapeSequence);
            Assert.AreEqual(2, data.AspectRatio);
        }

        [TestMethod]
        [DataRow("0", QualityLevel.ECC0)]
        [DataRow("50", QualityLevel.ECC50)]
        [DataRow("80", QualityLevel.ECC80)]
        [DataRow("100", QualityLevel.ECC100)]
        [DataRow("140", QualityLevel.ECC140)]
        [DataRow("200", QualityLevel.ECC200)]
        [DataRow("", QualityLevel.ECC0)]
        [DataRow("77", QualityLevel.ECC0)]
        public void DataMatrix_QualityLevel(string quality, QualityLevel expected)
        {
            DataMatrixFieldData data = AnalyzerTestHelper.FieldData<DataMatrixFieldData>(new DataMatrixZplCommandAnalyzer(), "^BXN,5," + quality);

            Assert.AreEqual(expected, data.QualityLevel);
        }

        [TestMethod]
        public void DataMatrix_InvalidNumbersAndEmptyEscapeUseDefaults()
        {
            DataMatrixFieldData data = AnalyzerTestHelper.FieldData<DataMatrixFieldData>(new DataMatrixZplCommandAnalyzer(), "^BXN,a,200,b,c,d,,e");

            Assert.IsNull(data.Height);
            Assert.IsNull(data.Columns);
            Assert.IsNull(data.Rows);
            Assert.AreEqual(6, data.Format);
            Assert.AreEqual('~', data.EscapeSequence);
            Assert.IsNull(data.AspectRatio);
        }

        // ^B7 PDF417: o,h,s,c,r,t
        [TestMethod]
        public void Pdf417_Defaults()
        {
            PDF417FieldData data = AnalyzerTestHelper.FieldData<PDF417FieldData>(new PDF417ZplCommandAnalyzer(), "^B7");

            Assert.IsNull(data.Height);
            Assert.AreEqual(0, data.SecurityLevel);
            Assert.IsNull(data.Columns);
            Assert.IsNull(data.Rows);
            Assert.IsFalse(data.Compact);
        }

        [TestMethod]
        public void Pdf417_AllParameters()
        {
            PDF417FieldData data = AnalyzerTestHelper.FieldData<PDF417FieldData>(new PDF417ZplCommandAnalyzer(), "^B7R,40,3,10,20,Y");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(40, data.Height);
            Assert.AreEqual(3, data.SecurityLevel);
            Assert.AreEqual(10, data.Columns);
            Assert.AreEqual(20, data.Rows);
            Assert.IsTrue(data.Compact);
        }

        [TestMethod]
        public void Pdf417_DecimalHeightIsFloored()
        {
            Assert.AreEqual(12, AnalyzerTestHelper.FieldData<PDF417FieldData>(new PDF417ZplCommandAnalyzer(), "^B7N,12.7").Height);
        }

        [TestMethod]
        public void Pdf417_DecimalHeightBelowOneIsIgnored()
        {
            Assert.IsNull(AnalyzerTestHelper.FieldData<PDF417FieldData>(new PDF417ZplCommandAnalyzer(), "^B7N,0.5").Height);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("abc")]
        public void Pdf417_EmptyOrInvalidHeightKeepsDefaultHeight(string height)
        {
            // ZPL II: an empty parameter keeps the default (^BY height); it must not abort the whole command
            PDF417FieldData data = AnalyzerTestHelper.FieldData<PDF417FieldData>(new PDF417ZplCommandAnalyzer(), "^B7N," + height + ",x,y,z,");

            Assert.IsNull(data.Height);
            Assert.AreEqual(0, data.SecurityLevel);
            Assert.IsNull(data.Columns);
            Assert.IsNull(data.Rows);
            Assert.IsFalse(data.Compact);
        }

        // ^BQ QR code: a,b,c,d,e
        [TestMethod]
        public void QrCode_Defaults()
        {
            QrCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(new QrCodeBarcodeZplCommandAnalyzer(), "^BQ");

            Assert.AreEqual(2, data.Model);
            Assert.AreEqual(2, data.MagnificationFactor);
            Assert.AreEqual(ErrorCorrectionLevel.HighReliability, data.ErrorCorrection);
            Assert.AreEqual(7, data.MaskValue);
        }

        [TestMethod]
        public void QrCode_AllParameters()
        {
            QrCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(new QrCodeBarcodeZplCommandAnalyzer(), "^BQN,1,6,H,3");

            Assert.AreEqual(1, data.Model);
            Assert.AreEqual(6, data.MagnificationFactor);
            Assert.AreEqual(ErrorCorrectionLevel.UltraHighReliability, data.ErrorCorrection);
            Assert.AreEqual(3, data.MaskValue);
        }

        [TestMethod]
        public void QrCode_MagnificationIsCappedAt100()
        {
            Assert.AreEqual(100, AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(new QrCodeBarcodeZplCommandAnalyzer(), "^BQN,2,250").MagnificationFactor);
            Assert.AreEqual(100, AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(new QrCodeBarcodeZplCommandAnalyzer(), "^BQN,2,100").MagnificationFactor);
        }

        [TestMethod]
        [DataRow("H", ErrorCorrectionLevel.UltraHighReliability)]
        [DataRow("Q", ErrorCorrectionLevel.HighReliability)]
        [DataRow("M", ErrorCorrectionLevel.Standard)]
        [DataRow("L", ErrorCorrectionLevel.HighDensity)]
        [DataRow("", ErrorCorrectionLevel.Standard)]
        [DataRow("X", ErrorCorrectionLevel.Standard)]
        public void QrCode_ErrorCorrection(string correction, ErrorCorrectionLevel expected)
        {
            QrCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(new QrCodeBarcodeZplCommandAnalyzer(), "^BQN,2,2," + correction);

            Assert.AreEqual(expected, data.ErrorCorrection);
        }

        [TestMethod]
        public void QrCode_InvalidNumbersUseDefaults()
        {
            QrCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<QrCodeBarcodeFieldData>(new QrCodeBarcodeZplCommandAnalyzer(), "^BQN,a,b,Q,c");

            Assert.AreEqual(2, data.Model);
            Assert.AreEqual(2, data.MagnificationFactor);
            Assert.AreEqual(7, data.MaskValue);
        }

        // ^BD MaxiCode: mode,position,total
        [TestMethod]
        public void MaxiCode_Defaults()
        {
            MaxiCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<MaxiCodeBarcodeFieldData>(new MaxiCodeBarcodeZplCommandAnalyzer(), "^BD");

            Assert.AreEqual(2, data.Mode);
            Assert.AreEqual(1, data.Position);
            Assert.AreEqual(1, data.Total);
        }

        [TestMethod]
        public void MaxiCode_AllParameters()
        {
            MaxiCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<MaxiCodeBarcodeFieldData>(new MaxiCodeBarcodeZplCommandAnalyzer(), "^BD4,2,5");

            Assert.AreEqual(4, data.Mode);
            Assert.AreEqual(2, data.Position);
            Assert.AreEqual(5, data.Total);
        }

        [TestMethod]
        public void MaxiCode_InvalidPositionAndTotalUseDefaults()
        {
            MaxiCodeBarcodeFieldData data = AnalyzerTestHelper.FieldData<MaxiCodeBarcodeFieldData>(new MaxiCodeBarcodeZplCommandAnalyzer(), "^BD3,x,y");

            Assert.AreEqual(3, data.Mode);
            Assert.AreEqual(1, data.Position);
            Assert.AreEqual(1, data.Total);
        }

        [TestMethod]
        public void MaxiCode_InvalidModeReportsError()
        {
            string[] errors = AnalyzerTestHelper.Errors("^XA^FO10,10^BDabc^FDdata^FS^XZ");

            Assert.HasCount(1, errors);
            StringAssert.Contains(errors[0], "^BDabc");
        }

        // ^GS Graphic symbol: o,h,w
        [TestMethod]
        public void GraphicSymbol_DefaultsToFontSize()
        {
            VirtualPrinter printer = new();
            printer.SetFontHeight(40);
            printer.SetFontWidth(30);

            Assert.IsNull(new GraphicSymbolZplCommandAnalyzer().Analyze("^GS", printer, new PrinterStorage()));
            GraphicSymbolFieldData data = (GraphicSymbolFieldData)printer.NextElementFieldData;

            Assert.AreEqual(FieldOrientation.Normal, data.FieldOrientation);
            Assert.AreEqual(40, data.Height);
            Assert.AreEqual(30, data.Width);
        }

        [TestMethod]
        public void GraphicSymbol_AllParameters()
        {
            GraphicSymbolFieldData data = AnalyzerTestHelper.FieldData<GraphicSymbolFieldData>(new GraphicSymbolZplCommandAnalyzer(), "^GSR,50,60");

            Assert.AreEqual(FieldOrientation.Rotated90, data.FieldOrientation);
            Assert.AreEqual(50, data.Height);
            Assert.AreEqual(60, data.Width);
        }

        [TestMethod]
        public void GraphicSymbol_InvalidSizeUsesFontSize()
        {
            GraphicSymbolFieldData data = AnalyzerTestHelper.FieldData<GraphicSymbolFieldData>(new GraphicSymbolZplCommandAnalyzer(), "^GSN,x,y");

            Assert.AreEqual(0, data.Height);
            Assert.AreEqual(0, data.Width);
        }

        [TestMethod]
        public void Barcode_EmptyOrientationUsesPrinterDefaultOrientation()
        {
            VirtualPrinter printer = new();
            printer.SetFieldOrientation(FieldOrientation.Rotated180);

            new Code39BarcodeZplCommandAnalyzer().Analyze("^B3", printer, new PrinterStorage());

            Assert.AreEqual(FieldOrientation.Rotated180, ((Code39BarcodeFieldData)printer.NextElementFieldData).FieldOrientation);
        }
    }
}
