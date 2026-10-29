using System.Collections.Generic;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    [TestClass]
    public class ZplAnalyzerTest
    {
        private sealed class RecordingFormatMerger : IFormatMerger
        {
            public List<LabelInfo> Received { get; private set; }

            public List<LabelInfo> MergeFormats(List<LabelInfo> rawLabelInfos)
            {
                this.Received = rawLabelInfos;
                return [];
            }
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("\r\n\t\n")]
        public void BlankInputHasNoLabels(string zpl)
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(zpl);

            Assert.IsEmpty(info.LabelInfos);
            Assert.IsEmpty(info.Errors);
            Assert.IsEmpty(info.UnknownCommands);
        }

        [TestMethod]
        public void SingleLabel()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO10,20^FDhello^FS^XZ");

            Assert.HasCount(1, info.LabelInfos);
            Assert.IsNull(info.LabelInfos[0].DownloadFormatName);
            Assert.HasCount(1, info.LabelInfos[0].ZplElements);
        }

        [TestMethod]
        public void WhitespaceAndNewlinesBetweenCommandsAreIgnored()
        {
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("\r\n^FO10,20\r\n^FDhel\nlo^FS\r\n");

            Assert.AreEqual("hello", ((ZplTextField)elements[0]).Text);
        }

        [TestMethod]
        public void MultipleLabelsStartWithEmptyElementList()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO10,20^FDone^FS^XZ^XA^FO10,20^FDtwo^FS^FO10,40^FDthree^FS^XZ");

            Assert.HasCount(2, info.LabelInfos);
            Assert.HasCount(1, info.LabelInfos[0].ZplElements);
            Assert.HasCount(2, info.LabelInfos[1].ZplElements);
            Assert.AreEqual("two", ((ZplTextField)info.LabelInfos[1].ZplElements[0]).Text);
        }

        [TestMethod]
        public void StartAndEndCommandsAreCaseInsensitive()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^xa^FO10,20^FDone^FS^xz");

            Assert.HasCount(1, info.LabelInfos);
            Assert.HasCount(1, info.LabelInfos[0].ZplElements);
        }

        [TestMethod]
        public void LabelWithoutEndCommandIsNotReturned()
        {
            Assert.IsEmpty(AnalyzerTestHelper.Analyze("^XA^FO10,20^FDone^FS").LabelInfos);
        }

        [TestMethod]
        public void PendingFieldIsFinalizedAtLabelEnd()
        {
            // The last command is not ^FS: the field is evaluated forcibly at ^XZ
            ZplElementBase[] elements = AnalyzerTestHelper.Elements("^FO10,20^FDhello");

            Assert.AreEqual("hello", ((ZplTextField)elements[0]).Text);
        }

        [TestMethod]
        public void FieldSeparatorBeforeLabelEndIsNotRepeated()
        {
            Assert.HasCount(1, AnalyzerTestHelper.Elements("^FO10,20^FDhello^FS"));
        }

        [TestMethod]
        public void CommentBeforeLabelEndDoesNotHideTheFieldSeparator()
        {
            Assert.HasCount(1, AnalyzerTestHelper.Elements("^FO10,20^FDhello^FS^FX"));
        }

        [TestMethod]
        public void CommentsAreCollectedAndDoNotProduceElements()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FX a comment^FO10,20^FDhello^FS^XZ");

            Assert.IsEmpty(info.UnknownCommands);
            Assert.HasCount(1, info.LabelInfos[0].ZplElements);
        }

        [TestMethod]
        public void UnknownCommandsAreCollected()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^ZZ1,2^FO10,20^FDhello^FS~QQ^XZ");

            CollectionAssert.AreEqual(new[] { "^ZZ1,2", "~QQ" }, info.UnknownCommands);
            Assert.IsEmpty(info.Errors);
            Assert.HasCount(1, info.LabelInfos[0].ZplElements);
        }

        [TestMethod]
        public void TooShortCommandsAreUnknown()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^X^FO10,20^FDa^FS^XZ");

            CollectionAssert.AreEqual(new[] { "^X" }, info.UnknownCommands);
        }

        [TestMethod]
        public void BlankTextBetweenCommandsIsIgnored()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("  ^XA  ^FO10,20^FDa^FS  ^XZ  ");

            Assert.IsEmpty(info.UnknownCommands);
            Assert.HasCount(1, info.LabelInfos);
        }

        [TestMethod]
        public void FailingCommandIsReportedAndAnalysisContinues()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO10,20^BDx^FDabc^FS^FO10,60^FDok^FS^XZ");

            Assert.HasCount(1, info.Errors);
            StringAssert.StartsWith(info.Errors[0], "Cannot analyze command ^BDx");
            // the data of the failed barcode command is still a text field, the following field is analyzed too
            Assert.HasCount(2, info.LabelInfos[0].ZplElements);
            Assert.AreEqual("ok", ((ZplTextField)info.LabelInfos[0].ZplElements[1]).Text);
        }

        [TestMethod]
        public void ChangeCaretCommandChangesTheCommandPrefix()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CC#^FO10,20#FDa^b#FS#XZ");

            Assert.AreEqual("a^b", ((ZplTextField)info.LabelInfos[0].ZplElements[0]).Text);
        }

        [TestMethod]
        public void ChangeTildeCommandChangesTheCommandPrefix()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CT+^FO10,20^FDa~b^FS^XZ");

            Assert.AreEqual("a~b", ((ZplTextField)info.LabelInfos[0].ZplElements[0]).Text);
        }

        [TestMethod]
        public void ChangeCommandsWithoutParameterUseTheNextCommandPrefix()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CC^FO10,20^FDa^FS^CT~^XZ");

            Assert.IsEmpty(info.Errors);
            Assert.HasCount(1, info.LabelInfos);
        }

        [TestMethod]
        public void ChangedTildeCharacterStartsTildeCommands()
        {
            PrinterStorage storage = new();

            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CT+^XZ^XA+DGR:A.GRF,8,1,FF818181818181FF^XZ", storage);

            Assert.IsEmpty(info.UnknownCommands);
            Assert.IsNotEmpty(storage.GetFile('R', "A.GRF"));
        }

        [TestMethod]
        public void TildeCommandsAreRecognized()
        {
            PrinterStorage storage = new();

            AnalyzerTestHelper.Analyze("^XA~DGR:A.GRF,8,1,FF818181818181FF^XZ", storage);

            Assert.IsNotEmpty(storage.GetFile('R', "A.GRF"));
        }

        [TestMethod]
        public void TildeInDataMatrixEscapeParameterIsNotACommandStart()
        {
            ZplDataMatrix matrix = AnalyzerTestHelper.Single<ZplDataMatrix>("^FO0,0^BXN,5,200,,,6,~^FDabc^FS");

            Assert.AreEqual('~', matrix.EscapeSequence);
        }

        [TestMethod]
        public void TildeInLowerCaseDataMatrixCommandIsNotACommandStart()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^FO0,0^bxN,5,200,,,6,~^FDabc^FS^XZ");

            // ^bx is not a known (case sensitive) command, but the escape character must stay part of it
            CollectionAssert.AreEqual(new[] { "^bxN,5,200,,,6,~" }, info.UnknownCommands);
        }

        [TestMethod]
        public void TildeInDataMatrixCommandWithCustomCaret()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CC#^FO0,0#BXN,5,200,,,6,~#FDabc#FS#XZ");

            Assert.IsEmpty(info.Errors);
            Assert.IsEmpty(info.UnknownCommands);
            Assert.IsInstanceOfType<ZplDataMatrix>(info.LabelInfos[0].ZplElements[0]);
        }

        [TestMethod]
        public void TildeAfterOtherCommandsStartsANewCommand()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^BCN~QQ^XZ");

            CollectionAssert.AreEqual(new[] { "~QQ" }, info.UnknownCommands);
        }

        [TestMethod]
        public void DownloadFormatAndRecallFormatMergeTemplates()
        {
            const string Zpl =
                "^XA^DFR:FMT.ZPL^FS^FO10,20^A0N,30,20^FN1^FS^FO10,60^BCN,40^FN2^FS^XZ" +
                "^XA^XFR:FMT.ZPL^FN1^FDhello^FS^FN2^FD123^FS^XZ";

            AnalyzeInfo info = AnalyzerTestHelper.Analyze(Zpl);

            Assert.IsEmpty(info.Errors);
            Assert.HasCount(1, info.LabelInfos);
            ZplElementBase[] elements = info.LabelInfos[0].ZplElements;
            Assert.HasCount(2, elements);
            ZplTextField text = (ZplTextField)elements[0];
            Assert.AreEqual("hello", text.Text);
            Assert.AreEqual(10, text.PositionX);
            Assert.AreEqual("0", text.Font.FontName);
            ZplBarcode128 barcode = (ZplBarcode128)elements[1];
            Assert.AreEqual("123", barcode.Content);
            Assert.AreEqual(40, barcode.Height);
        }

        [TestMethod]
        public void RecallOfUnknownFormatIsAnError()
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^XFR:NONE.ZPL^XZ");

            Assert.HasCount(1, info.Errors);
        }

        [TestMethod]
        public void DownloadFormatNameIsResetByNextLabel()
        {
            // The second label is not a format: it must be returned although the first one was named
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^DFR:A.ZPL^FS^XZ^XA^FO10,20^FDx^FS^XZ");

            Assert.HasCount(1, info.LabelInfos);
            Assert.HasCount(1, info.LabelInfos[0].ZplElements);
        }

        [TestMethod]
        public void CustomFormatMergerIsUsed()
        {
            RecordingFormatMerger merger = new();

            AnalyzeInfo info = new ZplAnalyzer(new PrinterStorage(), merger).Analyze("^XA^FO1,1^FDa^FS^XZ^XA^DFR:B.ZPL^FS^XZ");

            Assert.HasCount(2, merger.Received);
            Assert.AreEqual("R:B", merger.Received[1].DownloadFormatName);
            Assert.IsEmpty(info.LabelInfos);
        }

        [TestMethod]
        public void PrinterStateIsKeptAcrossLabels()
        {
            // ^CF and ^BY are not reset by a new label (they are printer settings)
            AnalyzeInfo info = AnalyzerTestHelper.Analyze("^XA^CF0,40,30^BY5^XZ^XA^FO10,20^FDhello^FS^FO10,80^BCN^FD1^FS^XZ");

            ZplTextField text = (ZplTextField)info.LabelInfos[1].ZplElements[0];
            Assert.AreEqual(40, text.Font.FontHeight);
            Assert.AreEqual(5, ((ZplBarcode)info.LabelInfos[1].ZplElements[1]).ModuleWidth);
        }
    }
}
