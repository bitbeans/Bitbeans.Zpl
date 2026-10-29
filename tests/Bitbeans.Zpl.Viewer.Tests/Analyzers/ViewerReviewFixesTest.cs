using System.Collections.Generic;
using System.Linq;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Regression tests for the viewer parser review findings (formats, escape characters, field numbers,
    /// stored objects, printer defaults and parameter validation).
    /// </summary>
    [TestClass]
    public class ViewerReviewFixesTest
    {
        private const string Grf1 = "FF818181818181FF";
        private const string Grf2 = "0000000000000000";

        private static ZplTextField[] Texts(AnalyzeInfo info, int label)
        {
            return info.LabelInfos[label].ZplElements.OfType<ZplTextField>().ToArray();
        }

        [TestMethod]
        public void SpaceAsHexadecimalIndicatorIsKept()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^FH ^FD 41^FS^XZ");

            Assert.IsEmpty(info.Errors);
            ZplTextField text = Texts(info, 0)[0];
            Assert.AreEqual(' ', text.HexadecimalIndicator);
            Assert.AreEqual(" 41", text.Text);
        }

        [TestMethod]
        public void GraphicSymbolKeepsTheFieldJustification()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO100,20,1^GSN,18,10^FDC^FS^FWN,1^FT100,60^GSN,18,10^FDC^FS^XZ");

            ZplGraphicSymbol[] symbols = info.LabelInfos[0].ZplElements.OfType<ZplGraphicSymbol>().ToArray();
            Assert.HasCount(2, symbols);
            Assert.AreEqual(FieldJustification.Right, symbols[0].FieldOrigin.FieldJustification);
            Assert.AreEqual(FieldJustification.Right, symbols[1].FieldTypeset.FieldJustification);
        }

        // 1: unknown ^XF
        [TestMethod]
        public void RecallOfUnknownFormatIsAnErrorAndOtherLabelsAreKept()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^FDok^FS^XZ^XA^XFR:MISSING.ZPL^FS^XZ");

            Assert.HasCount(1, info.Errors);
            StringAssert.Contains(info.Errors[0], "R:MISSING.ZPL");
            Assert.HasCount(2, info.LabelInfos);
            Assert.AreEqual("ok", Texts(info, 0)[0].Text);
            Assert.IsEmpty(info.LabelInfos[1].ZplElements);
        }

        [TestMethod]
        public void LabelWithUnknownFormatKeepsItsOwnElements()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^XFE:NONE.ZPL^FS^FO0,0^FDown^FS^XZ");

            Assert.HasCount(1, info.Errors);
            Assert.AreEqual("own", Texts(info, 0)[0].Text);
        }

        [TestMethod]
        public void MergerReportsUnknownFormatsInTheErrorCollection()
        {
            List<string> errors = [];
            LabelInfo label = new() { ZplElements = [new ZplRecallFormat("R:NONE.ZPL")] };

            List<LabelInfo> merged = new FormatMerger().MergeFormats([label], errors);

            Assert.HasCount(1, merged);
            Assert.HasCount(1, errors);
        }

        // 2: formats on different devices
        [TestMethod]
        public void FormatsOnDifferentDevicesDoNotCollide()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:A.ZPL^FS^FO0,0^FDone^FS^XZ^XA^DFE:A.ZPL^FS^FO0,0^FDtwo^FS^XZ^XA^XFE:A.ZPL^FS^XZ^XA^XFR:A.ZPL^FS^XZ");

            Assert.IsEmpty(info.Errors);
            Assert.HasCount(2, info.LabelInfos);
            Assert.AreEqual("two", Texts(info, 0)[0].Text);
            Assert.AreEqual("one", Texts(info, 1)[0].Text);
        }

        [TestMethod]
        public void LaterFormatWithTheSameKeyReplacesTheEarlierOne()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:A.ZPL^FS^FO0,0^FDold^FS^XZ^XA^DFA.ZPL^FS^FO0,0^FDnew^FS^XZ^XA^XFR:A.ZPL^FS^XZ");

            Assert.IsEmpty(info.Errors);
            Assert.AreEqual("new", Texts(info, 0)[0].Text);
        }

        [TestMethod]
        public void FormatKeyIsCaseInsensitiveAndDefaultsToDeviceR()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFe:Fmt.zpl^FS^FO0,0^FDx^FS^XZ^XA^XFE:FMT.ZPL^FS^XZ^XA^XFFMT^FS^XZ");

            // The second recall finds the format, the third one uses the default device R: and does not
            Assert.HasCount(1, info.Errors);
            Assert.AreEqual("x", Texts(info, 0)[0].Text);
            Assert.IsEmpty(info.LabelInfos[1].ZplElements);
        }

        // 3: Data Matrix escape character in field data
        [TestMethod]
        public void DataMatrixEscapeInFieldDataIsNotACommand()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^BXN,5,200,,,6,~^FD~1ABC^FS^XZ");

            Assert.IsEmpty(info.Errors);
            Assert.IsEmpty(info.UnknownCommands);
            ZplDataMatrix matrix = (ZplDataMatrix)info.LabelInfos[0].ZplElements[0];
            Assert.AreEqual("~1ABC", matrix.Content);
        }

        [TestMethod]
        public void DataMatrixDefaultEscapeInFieldVariableIsNotACommand()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^BXN,5,200^FV~1ABC^FS^XZ");

            Assert.IsEmpty(info.UnknownCommands);
            Assert.AreEqual("~1ABC", ((ZplDataMatrix)info.LabelInfos[0].ZplElements[0]).Content);
        }

        [TestMethod]
        public void DataMatrixEscapeWithCustomCaret()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CC#^FO0,0#BXN,5,200,,,6,~#FD~1ABC#FS#XZ");

            Assert.IsEmpty(info.UnknownCommands);
            Assert.AreEqual("~1ABC", ((ZplDataMatrix)info.LabelInfos[0].ZplElements[0]).Content);
        }

        [TestMethod]
        public void DataMatrixWithEmptyEscapeParameterUsesTheDefault()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^BXN,5,200,,,6,,^FD~1A^FS^XZ");

            Assert.IsEmpty(info.UnknownCommands);
            Assert.AreEqual("~1A", ((ZplDataMatrix)info.LabelInfos[0].ZplElements[0]).Content);
        }

        [TestMethod]
        public void TildeInFieldDataOfOtherSymbologiesStillStartsACommand()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^FDabc~QQ^FS^XZ");

            CollectionAssert.AreEqual(new[] { "~QQ" }, info.UnknownCommands);
        }

        [TestMethod]
        public void DataMatrixEscapeEndsWithTheField()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^BXN,5,200^FDa^FS^FO0,0^FDb~QQ^FS^XZ");

            CollectionAssert.AreEqual(new[] { "~QQ" }, info.UnknownCommands);
        }

        [TestMethod]
        public void DataMatrixEscapeEndsWithTheLabel()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^BXN,5,200^FDa^XZ^XA^FDb~QQ^XZ");

            CollectionAssert.AreEqual(new[] { "~QQ" }, info.UnknownCommands);
        }

        [TestMethod]
        public void DataMatrixEscapeEndsWithTheLabelStart()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^BXN,5,200^XA^FDb~QQ^XZ");

            CollectionAssert.AreEqual(new[] { "~QQ" }, info.UnknownCommands);
        }

        [TestMethod]
        public void NonFieldDataCommandsOfADataMatrixFieldKeepTheEscape()
        {
            // ^FH is neither a field separator nor field data: the escape stays active for the following ^FD
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^BXN,5,200^FH^FD~1A^FS^XZ");

            Assert.IsEmpty(info.UnknownCommands);
        }

        [TestMethod]
        public void TildeCommandInsideADataMatrixFieldStillStartsCommands()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^BXN,5,200^FH~QQ~RR^XZ");

            CollectionAssert.AreEqual(new[] { "~QQ", "~RR" }, info.UnknownCommands);
        }

        // 4: ^FN with ^FD in the same field
        [TestMethod]
        public void FieldNumberWithDataDefinesTheFieldAndProvidesTheData()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^FN1^FS^FO0,40^FN1^FDABC^FS^XZ");

            Assert.IsEmpty(info.Errors);
            ZplTextField[] texts = Texts(info, 0);
            Assert.HasCount(2, texts);
            Assert.AreEqual("ABC", texts[0].Text);
            Assert.AreEqual(0, texts[0].PositionY);
            Assert.AreEqual("ABC", texts[1].Text);
            Assert.AreEqual(40, texts[1].PositionY);
        }

        [TestMethod]
        public void FieldNumberWithDataKeepsTheBarcode()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO5,6^BCN,50^FN3^FD123^FS^XZ");

            ZplBarcode128 barcode = (ZplBarcode128)info.LabelInfos[0].ZplElements.Single();
            Assert.AreEqual("123", barcode.Content);
            Assert.AreEqual(50, barcode.Height);
        }

        [TestMethod]
        public void FieldNumberDataWithoutPositionOnlyFillsTheFormat()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:F.ZPL^FS^FO0,0^FN1^FS^XZ^XA^XFR:F.ZPL^FS^FN1^FDhello^FS^XZ");

            Assert.IsEmpty(info.Errors);
            Assert.AreEqual("hello", Texts(info, 0).Single().Text);
        }

        [TestMethod]
        public void FieldNumberWithDataInAFormatStaysATemplate()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:F.ZPL^FS^FO0,0^FN1^FDdefault^FS^XZ^XA^XFR:F.ZPL^FS^FN1^FDhello^FS^XZ");

            Assert.AreEqual("hello", Texts(info, 0).Single().Text);
        }

        // 5: ^FN with a prompt
        [TestMethod]
        [DataRow("^FN1\"Name\"", 1)]
        [DataRow("^FN12\"A,B\"", 12)]
        [DataRow("^FN\"Name\"", 0)]
        [DataRow("^FN99999999999", 0)]
        [DataRow("^FN 7", 7)]
        public void FieldNumberPromptIsIgnored(string command, int expected)
        {
            VirtualPrinter printer = new();

            AnalyzerTestHelper.Run(new FieldNumberCommandAnalyzer(), command, printer);

            Assert.AreEqual(expected, printer.NextFieldNumber);
        }

        [TestMethod]
        public void FieldNumberWithPromptReceivesData()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(
                "^XA^DFR:F.ZPL^FS^FO0,0^FN1\"Name\"^FS^XZ^XA^XFR:F.ZPL^FS^FN1^FDhello^FS^XZ");

            Assert.AreEqual("hello", Texts(info, 0).Single().Text);
        }

        // 6: stored objects are replaced
        [TestMethod]
        public void SecondDownloadedGraphicReplacesTheFirst()
        {
            PrinterStorage storage = new();
            AnalyzerTestHelper.Analyze("~DGR:A.GRF,8,1," + Grf1, storage);
            byte[] first = storage.GetFile('R', "A.GRF");

            AnalyzerTestHelper.Analyze("~DGR:A.GRF,8,1," + Grf2, storage);

            Assert.IsNotEmpty(first);
            CollectionAssert.AreNotEqual(first, storage.GetFile('R', "A.GRF"));
        }

        [TestMethod]
        public void SecondDownloadedObjectReplacesTheFirst()
        {
            PrinterStorage storage = new();
            AnalyzerTestHelper.Analyze("~DYE:OBJ,B,G,3,1,FF00AA", storage);
            AnalyzerTestHelper.Analyze("~DYE:OBJ,B,G,3,1,010203", storage);

            CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, storage.GetFile('E', "OBJ"));
        }

        // 7: ^CF keeps the last dimensions
        [TestMethod]
        public void ChangeDefaultFontKeepsDimensionsThatAreNotSupplied()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CF0,40,30^CF0^FO0,0^FDa^FS^CFA,50^FO0,0^FDb^FS^XZ");

            ZplTextField[] texts = Texts(info, 0);
            Assert.AreEqual(40, texts[0].Font.FontHeight);
            Assert.AreEqual(30, texts[0].Font.FontWidth);
            Assert.AreEqual("A", texts[1].Font.FontName);
            Assert.AreEqual(50, texts[1].Font.FontHeight);
            Assert.AreEqual(30, texts[1].Font.FontWidth);
        }

        // 8: ^FW is only a default for fields without an explicit orientation
        [TestMethod]
        public void FieldOrientationDoesNotOverwriteExplicitFontOrientation()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^A0N,30,30^FWR^FDx^FS");

            Assert.AreEqual(FieldOrientation.Normal, text.Font.FieldOrientation);
        }

        [TestMethod]
        [DataRow("N", FieldOrientation.Normal)]
        [DataRow("R", FieldOrientation.Rotated90)]
        [DataRow("I", FieldOrientation.Rotated180)]
        [DataRow("B", FieldOrientation.Rotated270)]
        public void FieldOrientationDoesNotOverwriteAnyExplicitFontOrientation(string orientation, FieldOrientation expected)
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^A0" + orientation + ",30,30^FWN^FDx^FS");

            Assert.AreEqual(expected, text.Font.FieldOrientation);
        }

        [TestMethod]
        public void FieldOrientationAppliesToFontWithoutOrientation()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^A0,30,30^FWR^FDx^FS");

            Assert.AreEqual(FieldOrientation.Rotated90, text.Font.FieldOrientation);
        }

        [TestMethod]
        public void FontOrientationFlagIsResetWithTheField()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^A0N,30,30^FDa^FS^FWR^FO0,0^A0,30,30^FDb^FS^XZ");

            ZplTextField[] texts = Texts(info, 0);
            Assert.AreEqual(FieldOrientation.Normal, texts[0].Font.FieldOrientation);
            Assert.AreEqual(FieldOrientation.Rotated90, texts[1].Font.FieldOrientation);
        }

        // 9: ^FT with omitted y
        [TestMethod]
        public void FieldTypesetWithoutYContinuesAtTheLastY()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FT10,50^FDa^FS^FT30^FDb^FS^FT40,^FDc^FS^XZ");

            ZplTextField[] texts = Texts(info, 0);
            Assert.AreEqual(50, texts[1].PositionY);
            Assert.AreEqual(30, texts[1].PositionX);
            Assert.IsFalse(texts[1].UseDefaultPosition);
            Assert.AreEqual(50, texts[2].PositionY);
            Assert.AreEqual(40, texts[2].PositionX);
        }

        [TestMethod]
        public void FieldTypesetWithoutYDoesNotAddTheLabelHomeTwice()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^LH5,6^FT10,20^FDa^FS^FT30^FDb^FS^XZ");

            ZplTextField[] texts = Texts(info, 0);
            Assert.AreEqual(26, texts[0].PositionY);
            Assert.AreEqual(26, texts[1].PositionY);
            Assert.AreEqual(35, texts[1].PositionX);
        }

        [TestMethod]
        public void LastYIsTakenFromFieldOrigin()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO1,77^FDa^FS^FT30^FDb^FS^XZ");

            Assert.AreEqual(77, Texts(info, 0)[1].PositionY);
        }

        // 10: CR and LF are ignored in the data stream, also inside ^FD
        [TestMethod]
        public void LineBreaksInFieldDataAreIgnored()
        {
            ZplTextField text = AnalyzerTestHelper.Single<ZplTextField>("^FO0,0^FDab\r\ncd\nef^FS");

            Assert.AreEqual("abcdef", text.Text);
        }

        // 12: parameter bounds
        [TestMethod]
        [DataRow("NaN")]
        [DataRow("-NaN")]
        [DataRow("Infinity")]
        [DataRow("-Infinity")]
        public void BarcodeRatioMustBeFinite(string ratio)
        {
            VirtualPrinter printer = new();
            AnalyzerTestHelper.Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY5,2.5,50", printer);

            AnalyzerTestHelper.Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY2," + ratio + ",50", printer);

            Assert.AreEqual(2.5, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(2, printer.BarcodeInfo.ModuleWidth);
        }

        [TestMethod]
        public void FieldBlockParametersAreClampedToTheDocumentedRanges()
        {
            VirtualPrinter printer = new();

            AnalyzerTestHelper.Run(new FieldBlockZplCommandAnalyzer(), "^FB100,2147483647", printer);
            Assert.AreEqual(9999, printer.NextElementFieldBlock.MaximumNumberOfLinesInTextBlock);

            AnalyzerTestHelper.Run(new FieldBlockZplCommandAnalyzer(), "^FB99999,0,-99999,L,-5", printer);
            Assert.AreEqual(32000, printer.NextElementFieldBlock.WidthOfTextBlockLine);
            Assert.AreEqual(1, printer.NextElementFieldBlock.MaximumNumberOfLinesInTextBlock);
            Assert.AreEqual(-9999, printer.NextElementFieldBlock.AddOrDeleteSpaceBetweenLines);
            Assert.AreEqual(0, printer.NextElementFieldBlock.HangingIndentOfTheSecondAndRemainingLines);

            AnalyzerTestHelper.Run(new FieldBlockZplCommandAnalyzer(), "^FB-5,3,99999,L,99999", printer);
            Assert.AreEqual(0, printer.NextElementFieldBlock.WidthOfTextBlockLine);
            Assert.AreEqual(9999, printer.NextElementFieldBlock.AddOrDeleteSpaceBetweenLines);
            Assert.AreEqual(9999, printer.NextElementFieldBlock.HangingIndentOfTheSecondAndRemainingLines);
        }
    }
}
