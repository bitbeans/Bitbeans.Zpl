using System.Globalization;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Analyzers that only change the state of the virtual printer (^BY, ^CF, ^FW, ^LH, ...).
    /// </summary>
    [TestClass]
    public class PrinterStateAnalyzersTest
    {
        private static VirtualPrinter Run(IZplCommandAnalyzer analyzer, string command, VirtualPrinter printer = null)
        {
            printer ??= new VirtualPrinter();
            Assert.IsNull(AnalyzerTestHelper.Run(analyzer, command, printer));
            return printer;
        }

        // ^BY w,r,h
        [TestMethod]
        public void BarcodeFieldDefault_NoParametersKeepsDefaults()
        {
            VirtualPrinter printer = Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY");

            Assert.AreEqual(2, printer.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(3.0, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(10, printer.BarcodeInfo.Height);
        }

        [TestMethod]
        public void BarcodeFieldDefault_AllParameters()
        {
            VirtualPrinter printer = Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY4,2.5,80");

            Assert.AreEqual(4, printer.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(2.5, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(80, printer.BarcodeInfo.Height);
        }

        [TestMethod]
        public void BarcodeFieldDefault_ValuesAreClampedToTheAllowedRange()
        {
            VirtualPrinter low = Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY0,1.0,0");
            Assert.AreEqual(1, low.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(2.0, low.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(1, low.BarcodeInfo.Height);

            VirtualPrinter high = Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY11,3.5,5");
            Assert.AreEqual(10, high.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(3.0, high.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(5, high.BarcodeInfo.Height);
        }

        [TestMethod]
        public void BarcodeFieldDefault_RatioIsRoundedToOneDecimal()
        {
            VirtualPrinter printer = Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY2,2.34");

            Assert.AreEqual(2.3, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
        }

        [TestMethod]
        public void BarcodeFieldDefault_InvalidParametersKeepPreviousValues()
        {
            VirtualPrinter printer = new();
            Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY5,2.5,50", printer);
            Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BYa,b,c", printer);

            Assert.AreEqual(5, printer.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(2.5, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(50, printer.BarcodeInfo.Height);
        }

        [TestMethod]
        public void BarcodeFieldDefault_RatioIsCultureInvariant()
        {
            // ZPL always uses '.' as decimal separator, whatever the culture of the machine is
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                VirtualPrinter printer = Run(new BarCodeFieldDefaultZplCommandAnalyzer(), "^BY2,2.5");

                Assert.AreEqual(2.5, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        // ^CF f,h,w
        [TestMethod]
        public void ChangeAlphanumericDefaultFont_NoParameters()
        {
            VirtualPrinter printer = Run(new ChangeAlphanumericDefaultFontZplCommandAnalyzer(), "^CF");

            Assert.AreEqual("A", printer.FontName);
            Assert.AreEqual(9, printer.FontHeight);
            Assert.AreEqual(0, printer.FontWidth);
        }

        [TestMethod]
        public void ChangeAlphanumericDefaultFont_AllParameters()
        {
            VirtualPrinter printer = Run(new ChangeAlphanumericDefaultFontZplCommandAnalyzer(), "^CF0,40,30");

            Assert.AreEqual("0", printer.FontName);
            Assert.AreEqual(40, printer.FontHeight);
            Assert.AreEqual(30, printer.FontWidth);
        }

        [TestMethod]
        public void ChangeAlphanumericDefaultFont_EmptyFontKeepsCurrentFont()
        {
            VirtualPrinter printer = new();
            printer.SetFontName("D");

            Run(new ChangeAlphanumericDefaultFontZplCommandAnalyzer(), "^CF,25,x", printer);

            Assert.AreEqual("D", printer.FontName);
            Assert.AreEqual(25, printer.FontHeight);
            Assert.AreEqual(0, printer.FontWidth);
        }

        [TestMethod]
        public void ChangeAlphanumericDefaultFont_InvalidHeightFallsBackToDefault()
        {
            VirtualPrinter printer = Run(new ChangeAlphanumericDefaultFontZplCommandAnalyzer(), "^CFB,x,12");

            Assert.AreEqual("B", printer.FontName);
            Assert.AreEqual(9, printer.FontHeight);
            Assert.AreEqual(12, printer.FontWidth);
        }

        // ^FW r,z
        [TestMethod]
        public void FieldOrientation_NoParametersKeepsOrientationAndJustification()
        {
            VirtualPrinter printer = Run(new FieldOrientationZplCommandAnalyzer(), "^FW");

            Assert.AreEqual(FieldOrientation.Normal, printer.FieldOrientation);
            Assert.AreEqual(FieldJustification.None, printer.FieldJustification);
        }

        [TestMethod]
        [DataRow("N", FieldOrientation.Normal)]
        [DataRow("R", FieldOrientation.Rotated90)]
        [DataRow("I", FieldOrientation.Rotated180)]
        [DataRow("B", FieldOrientation.Rotated270)]
        public void FieldOrientation_Orientations(string value, FieldOrientation expected)
        {
            VirtualPrinter printer = Run(new FieldOrientationZplCommandAnalyzer(), "^FW" + value);

            Assert.AreEqual(expected, printer.FieldOrientation);
        }

        [TestMethod]
        [DataRow("0", FieldJustification.Left)]
        [DataRow("1", FieldJustification.Right)]
        [DataRow("2", FieldJustification.Auto)]
        public void FieldOrientation_Justifications(string value, FieldJustification expected)
        {
            VirtualPrinter printer = Run(new FieldOrientationZplCommandAnalyzer(), "^FWN," + value);

            Assert.AreEqual(expected, printer.FieldJustification);
        }

        [TestMethod]
        public void FieldOrientation_InvalidValuesKeepCurrentSettings()
        {
            VirtualPrinter printer = new();
            Run(new FieldOrientationZplCommandAnalyzer(), "^FWR,1", printer);
            Run(new FieldOrientationZplCommandAnalyzer(), "^FWX,9", printer);

            Assert.AreEqual(FieldOrientation.Rotated90, printer.FieldOrientation);
            Assert.AreEqual(FieldJustification.Right, printer.FieldJustification);
        }

        // ^LH x,y
        [TestMethod]
        public void LabelHome_Parameters()
        {
            VirtualPrinter printer = Run(new LabelHomeZplCommandAnalyzer(), "^LH30,40");

            Assert.AreEqual(30, printer.LabelHomePosition.X);
            Assert.AreEqual(40, printer.LabelHomePosition.Y);
        }

        [TestMethod]
        public void LabelHome_MissingAndInvalidParametersDefaultToZero()
        {
            VirtualPrinter missing = Run(new LabelHomeZplCommandAnalyzer(), "^LH");
            Assert.AreEqual(0, missing.LabelHomePosition.X);
            Assert.AreEqual(0, missing.LabelHomePosition.Y);

            VirtualPrinter invalid = Run(new LabelHomeZplCommandAnalyzer(), "^LHa,b");
            Assert.AreEqual(0, invalid.LabelHomePosition.X);
            Assert.AreEqual(0, invalid.LabelHomePosition.Y);

            VirtualPrinter onlyX = Run(new LabelHomeZplCommandAnalyzer(), "^LH7");
            Assert.AreEqual(7, onlyX.LabelHomePosition.X);
            Assert.AreEqual(0, onlyX.LabelHomePosition.Y);
        }

        // ^LR a
        [TestMethod]
        public void LabelReversePrint_Parameters()
        {
            Assert.IsTrue(Run(new LabelReversePrintZplCommandAnalyzer(), "^LRY").LabelReverse);
            Assert.IsFalse(Run(new LabelReversePrintZplCommandAnalyzer(), "^LRN").LabelReverse);
            Assert.IsFalse(Run(new LabelReversePrintZplCommandAnalyzer(), "^LR").LabelReverse);
        }

        [TestMethod]
        public void LabelReversePrint_CanBeSwitchedOffAgain()
        {
            VirtualPrinter printer = new();
            Run(new LabelReversePrintZplCommandAnalyzer(), "^LRY", printer);
            Run(new LabelReversePrintZplCommandAnalyzer(), "^LRN", printer);

            Assert.IsFalse(printer.LabelReverse);
        }

        // ^FR
        [TestMethod]
        public void FieldReversePrint_SetsReverseForNextElement()
        {
            VirtualPrinter printer = Run(new FieldReversePrintZplCommandAnalyzer(), "^FR");

            Assert.IsTrue(printer.NextElementFieldReverse);
        }

        // ^FH c
        [TestMethod]
        public void FieldHexadecimal_DefaultAndCustomIndicator()
        {
            Assert.AreEqual('_', Run(new FieldHexadecimalZplCommandAnalyzer(), "^FH").NextElementFieldHexadecimalIndicator);
            Assert.AreEqual('\\', Run(new FieldHexadecimalZplCommandAnalyzer(), "^FH\\").NextElementFieldHexadecimalIndicator);
        }

        // ^FN n
        [TestMethod]
        public void FieldNumber_Parameters()
        {
            Assert.AreEqual(12, Run(new FieldNumberCommandAnalyzer(), "^FN12").NextFieldNumber);
            Assert.AreEqual(0, Run(new FieldNumberCommandAnalyzer(), "^FN").NextFieldNumber);
            Assert.AreEqual(0, Run(new FieldNumberCommandAnalyzer(), "^FNabc").NextFieldNumber);
        }

        // ^FX comment
        [TestMethod]
        public void Comment_IsCollectedWithoutPrefix()
        {
            VirtualPrinter printer = Run(new CommentZplCommandAnalyzer(), "^FX first comment");
            Run(new CommentZplCommandAnalyzer(), "^FX second", printer);

            CollectionAssert.AreEqual(new[] { " first comment", " second" }, printer.Comments);
        }

        // ^A f,o,h,w
        [TestMethod]
        public void ScalableBitmappedFont_AllParameters()
        {
            VirtualPrinter printer = Run(new ScalableBitmappedFontZplCommandAnalyzer(), "^A0R,40,30");

            Assert.AreEqual("0", printer.NextFont.FontName);
            Assert.AreEqual(FieldOrientation.Rotated90, printer.NextFont.FieldOrientation);
            Assert.AreEqual(40, printer.NextFont.FontHeight);
            Assert.AreEqual(30, printer.NextFont.FontWidth);
        }

        [TestMethod]
        public void ScalableBitmappedFont_MissingParametersUsePrinterDefaults()
        {
            VirtualPrinter printer = new();
            printer.SetFontHeight(22);
            printer.SetFontWidth(11);
            printer.SetFieldOrientation(FieldOrientation.Rotated180);

            Run(new ScalableBitmappedFontZplCommandAnalyzer(), "^AB", printer);

            Assert.AreEqual("B", printer.NextFont.FontName);
            Assert.AreEqual(FieldOrientation.Rotated180, printer.NextFont.FieldOrientation);
            Assert.AreEqual(22, printer.NextFont.FontHeight);
            Assert.AreEqual(11, printer.NextFont.FontWidth);
        }

        [TestMethod]
        public void ScalableBitmappedFont_InvalidSizeUsesPrinterDefaults()
        {
            VirtualPrinter printer = new();
            printer.SetFontHeight(22);
            printer.SetFontWidth(11);

            Run(new ScalableBitmappedFontZplCommandAnalyzer(), "^AAN,x,y", printer);

            Assert.AreEqual(22, printer.NextFont.FontHeight);
            Assert.AreEqual(11, printer.NextFont.FontWidth);
        }

        // ^FD / ^FV
        [TestMethod]
        public void FieldData_SetsContent()
        {
            VirtualPrinter printer = Run(new FieldDataZplCommandAnalyzer(), "^FDhello, world");

            Assert.AreEqual("hello, world", printer.NextElementFieldData.Content);
        }

        [TestMethod]
        public void FieldData_KeepsBarcodeFieldDataWhenSettingContent()
        {
            VirtualPrinter printer = new();
            Run(new Code39BarcodeZplCommandAnalyzer(), "^B3N", printer);

            Run(new FieldDataZplCommandAnalyzer(), "^FD123", printer);

            Assert.IsInstanceOfType<Code39BarcodeFieldData>(printer.NextElementFieldData);
            Assert.AreEqual("123", printer.NextElementFieldData.Content);
        }

        [TestMethod]
        public void FieldData_AfterFieldNumberReturnsRecallElement()
        {
            VirtualPrinter printer = new();
            printer.SetNextFieldNumber(4);

            ZplElementBase element = AnalyzerTestHelper.Run(new FieldDataZplCommandAnalyzer(), "^FDvalue", printer);

            ZplRecallFieldNumber recall = (ZplRecallFieldNumber)element;
            Assert.AreEqual(4, recall.Number);
            Assert.AreEqual("value", recall.Text);
            Assert.IsNull(printer.NextFieldNumber);
            Assert.IsNull(printer.NextElementFieldData);
        }

        [TestMethod]
        public void FieldVariable_BehavesLikeFieldData()
        {
            VirtualPrinter printer = Run(new FieldVariableZplCommandAnalyzer(), "^FVabc");

            Assert.AreEqual("abc", printer.NextElementFieldData.Content);
            Assert.IsTrue(new FieldVariableZplCommandAnalyzer().CanAnalyze("^FVabc"));
            Assert.IsFalse(new FieldVariableZplCommandAnalyzer().CanAnalyze("^FDabc"));
        }

        // ^FB w,n,s,j,i
        [TestMethod]
        public void FieldBlock_Defaults()
        {
            VirtualPrinter printer = Run(new FieldBlockZplCommandAnalyzer(), "^FB");

            Assert.AreEqual(0, printer.NextElementFieldBlock.WidthOfTextBlockLine);
            Assert.AreEqual(1, printer.NextElementFieldBlock.MaximumNumberOfLinesInTextBlock);
            Assert.AreEqual(0, printer.NextElementFieldBlock.AddOrDeleteSpaceBetweenLines);
            Assert.AreEqual(TextJustification.Left, printer.NextElementFieldBlock.TextJustification);
            Assert.AreEqual(0, printer.NextElementFieldBlock.HangingIndentOfTheSecondAndRemainingLines);
        }

        [TestMethod]
        public void FieldBlock_AllParameters()
        {
            VirtualPrinter printer = Run(new FieldBlockZplCommandAnalyzer(), "^FB400,5,-2,C,15");

            Assert.AreEqual(400, printer.NextElementFieldBlock.WidthOfTextBlockLine);
            Assert.AreEqual(5, printer.NextElementFieldBlock.MaximumNumberOfLinesInTextBlock);
            Assert.AreEqual(-2, printer.NextElementFieldBlock.AddOrDeleteSpaceBetweenLines);
            Assert.AreEqual(TextJustification.Center, printer.NextElementFieldBlock.TextJustification);
            Assert.AreEqual(15, printer.NextElementFieldBlock.HangingIndentOfTheSecondAndRemainingLines);
        }

        [TestMethod]
        [DataRow("L", TextJustification.Left)]
        [DataRow("C", TextJustification.Center)]
        [DataRow("R", TextJustification.Right)]
        [DataRow("J", TextJustification.Justified)]
        [DataRow("", TextJustification.Left)]
        [DataRow("X", TextJustification.Left)]
        public void FieldBlock_Justification(string value, TextJustification expected)
        {
            VirtualPrinter printer = Run(new FieldBlockZplCommandAnalyzer(), "^FB100,1,0," + value);

            Assert.AreEqual(expected, printer.NextElementFieldBlock.TextJustification);
        }

        [TestMethod]
        public void FieldBlock_InvalidNumbersUseDefaults()
        {
            VirtualPrinter printer = Run(new FieldBlockZplCommandAnalyzer(), "^FBa,b,c,L,d");

            Assert.AreEqual(0, printer.NextElementFieldBlock.WidthOfTextBlockLine);
            Assert.AreEqual(1, printer.NextElementFieldBlock.MaximumNumberOfLinesInTextBlock);
            Assert.AreEqual(0, printer.NextElementFieldBlock.AddOrDeleteSpaceBetweenLines);
            Assert.AreEqual(0, printer.NextElementFieldBlock.HangingIndentOfTheSecondAndRemainingLines);
        }

        // ^FO x,y,z
        [TestMethod]
        public void FieldOrigin_Parameters()
        {
            VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO10,20");

            Assert.AreEqual(10, printer.NextElementPosition.X);
            Assert.AreEqual(20, printer.NextElementPosition.Y);
            Assert.IsFalse(printer.NextElementPosition.CalculateFromBottom);
            Assert.IsFalse(printer.NextElementPosition.UseDefaultPosition);
            Assert.AreEqual(FieldJustification.None, printer.NextElementFieldJustification);
        }

        [TestMethod]
        public void FieldOrigin_NoParametersMeansOrigin()
        {
            VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO");

            Assert.AreEqual(0, printer.NextElementPosition.X);
            Assert.AreEqual(0, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldOrigin_DecimalsAreTruncated()
        {
            VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO10.7,20.2");

            Assert.AreEqual(10, printer.NextElementPosition.X);
            Assert.AreEqual(20, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldOrigin_DecimalsAreCultureInvariant()
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO10.5,20.5");

                Assert.AreEqual(10, printer.NextElementPosition.X);
                Assert.AreEqual(20, printer.NextElementPosition.Y);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [TestMethod]
        [DataRow("abc", "def")]
        [DataRow("99999999999", "-99999999999")]
        [DataRow("-99999999999", "99999999999")]
        public void FieldOrigin_InvalidOrOutOfRangeCoordinatesAreZero(string x, string y)
        {
            VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO" + x + "," + y);

            Assert.AreEqual(0, printer.NextElementPosition.X);
            Assert.AreEqual(0, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldOrigin_NegativeCoordinates()
        {
            VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO-5,-6");

            Assert.AreEqual(-5, printer.NextElementPosition.X);
            Assert.AreEqual(-6, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldOrigin_AddsLabelHome()
        {
            VirtualPrinter printer = new();
            printer.SetLabelHome(100, 200);

            Run(new FieldOriginZplCommandAnalzer(), "^FO10,20", printer);

            Assert.AreEqual(110, printer.NextElementPosition.X);
            Assert.AreEqual(220, printer.NextElementPosition.Y);
        }

        [TestMethod]
        [DataRow("0", FieldJustification.Left)]
        [DataRow("1", FieldJustification.Right)]
        [DataRow("2", FieldJustification.Auto)]
        public void FieldOrigin_Justification(string value, FieldJustification expected)
        {
            VirtualPrinter printer = Run(new FieldOriginZplCommandAnalzer(), "^FO10,20," + value);

            Assert.AreEqual(expected, printer.NextElementFieldJustification);
        }

        [TestMethod]
        public void FieldOrigin_EmptyJustificationUsesPrinterJustification()
        {
            VirtualPrinter printer = new();
            printer.SetFieldJustification(FieldJustification.Right);

            Run(new FieldOriginZplCommandAnalzer(), "^FO10,20,", printer);

            Assert.AreEqual(FieldJustification.Right, printer.NextElementFieldJustification);
        }

        // ^FT x,y,z
        [TestMethod]
        public void FieldTypeset_Parameters()
        {
            VirtualPrinter printer = Run(new FieldTypesetZplCommandAnalyzer(), "^FT10,20");

            Assert.AreEqual(10, printer.NextElementPosition.X);
            Assert.AreEqual(20, printer.NextElementPosition.Y);
            Assert.IsTrue(printer.NextElementPosition.CalculateFromBottom);
            Assert.IsFalse(printer.NextElementPosition.UseDefaultPosition);
        }

        [TestMethod]
        [DataRow("^FT")]
        [DataRow("^FT,200")]
        [DataRow("^FTabc,5")]
        [DataRow("^FT99999999999,5")]
        [DataRow("^FT-99999999999,5")]
        public void FieldTypeset_MissingOrInvalidXUsesDefaultPosition(string command)
        {
            VirtualPrinter printer = Run(new FieldTypesetZplCommandAnalyzer(), command);

            Assert.IsTrue(printer.NextElementPosition.UseDefaultPosition);
            Assert.AreEqual(0, printer.NextElementPosition.X);
        }

        [TestMethod]
        [DataRow("^FT10,")]
        [DataRow("^FT10,abc")]
        [DataRow("^FTabc,def")]
        [DataRow("^FT10,99999999999")]
        [DataRow("^FT10,-99999999999")]
        public void FieldTypeset_MissingOrInvalidYUsesDefaultPosition(string command)
        {
            VirtualPrinter printer = Run(new FieldTypesetZplCommandAnalyzer(), command);

            Assert.IsTrue(printer.NextElementPosition.UseDefaultPosition);
            Assert.AreEqual(0, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldTypeset_OnlyXKeepsExplicitPosition()
        {
            VirtualPrinter printer = Run(new FieldTypesetZplCommandAnalyzer(), "^FT10");

            Assert.IsFalse(printer.NextElementPosition.UseDefaultPosition);
            Assert.AreEqual(10, printer.NextElementPosition.X);
            Assert.AreEqual(0, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldTypeset_EmptyXStillParsesY()
        {
            VirtualPrinter printer = Run(new FieldTypesetZplCommandAnalyzer(), "^FT,50");

            Assert.AreEqual(50, printer.NextElementPosition.Y);
        }

        [TestMethod]
        public void FieldTypeset_DecimalsAreCultureInvariant()
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                VirtualPrinter printer = Run(new FieldTypesetZplCommandAnalyzer(), "^FT10.5,20.5");

                Assert.AreEqual(10, printer.NextElementPosition.X);
                Assert.AreEqual(20, printer.NextElementPosition.Y);
                Assert.IsFalse(printer.NextElementPosition.UseDefaultPosition);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [TestMethod]
        public void FieldTypeset_AddsLabelHomeAndSetsJustification()
        {
            VirtualPrinter printer = new();
            printer.SetLabelHome(5, 6);

            Run(new FieldTypesetZplCommandAnalyzer(), "^FT10,20,1", printer);

            Assert.AreEqual(15, printer.NextElementPosition.X);
            Assert.AreEqual(26, printer.NextElementPosition.Y);
            Assert.AreEqual(FieldJustification.Right, printer.NextElementFieldJustification);
        }
    }
}
