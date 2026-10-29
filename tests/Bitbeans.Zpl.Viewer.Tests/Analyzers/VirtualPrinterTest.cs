using System.Globalization;
using Bitbeans.Zpl.Viewer.Models;
using Bitbeans.Zpl.Viewer.Properties;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    [TestClass]
    public class VirtualPrinterTest
    {
        [TestMethod]
        public void InitialState()
        {
            VirtualPrinter printer = new();

            Assert.IsNull(printer.LabelHomePosition);
            Assert.IsNull(printer.NextElementPosition);
            Assert.IsNull(printer.NextElementFieldData);
            Assert.IsNull(printer.NextElementFieldBlock);
            Assert.IsNull(printer.NextFont);
            Assert.IsNull(printer.NextElementFieldHexadecimalIndicator);
            Assert.IsNull(printer.NextDownloadFormatName);
            Assert.IsNull(printer.NextFieldNumber);
            Assert.AreEqual(FieldOrientation.Normal, printer.FieldOrientation);
            Assert.AreEqual(FieldJustification.None, printer.FieldJustification);
            Assert.AreEqual(FieldJustification.None, printer.NextElementFieldJustification);
            Assert.AreEqual(0, printer.FontWidth);
            Assert.AreEqual(0, printer.FontHeight);
            Assert.AreEqual("A", printer.FontName);
            Assert.IsFalse(printer.NextElementFieldReverse);
            Assert.IsFalse(printer.LabelReverse);
            Assert.IsEmpty(printer.Comments);
            Assert.AreEqual(2, printer.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(3.0, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(10, printer.BarcodeInfo.Height);
        }

        [TestMethod]
        public void ElementPosition()
        {
            VirtualPrinter printer = new();

            printer.SetNextElementPosition(1, 2);
            Assert.AreEqual(1, printer.NextElementPosition.X);
            Assert.AreEqual(2, printer.NextElementPosition.Y);
            Assert.IsFalse(printer.NextElementPosition.CalculateFromBottom);
            Assert.IsFalse(printer.NextElementPosition.UseDefaultPosition);

            printer.SetNextElementPosition(3, 4, true, true);
            Assert.IsTrue(printer.NextElementPosition.CalculateFromBottom);
            Assert.IsTrue(printer.NextElementPosition.UseDefaultPosition);

            printer.ClearNextElementPosition();
            Assert.AreEqual(0, printer.NextElementPosition.X);
            Assert.AreEqual(0, printer.NextElementPosition.Y);
            Assert.IsFalse(printer.NextElementPosition.CalculateFromBottom);
            Assert.IsFalse(printer.NextElementPosition.UseDefaultPosition);
        }

        [TestMethod]
        public void FieldDataContentCreatesPlainFieldData()
        {
            VirtualPrinter printer = new();

            printer.SetNextElementFieldDataContent("abc");

            Assert.AreEqual("abc", printer.NextElementFieldData.Content);
            Assert.AreEqual(typeof(FieldDataBase), printer.NextElementFieldData.GetType());
        }

        [TestMethod]
        public void FieldDataContentIsKeptWhenTheTypeOfFieldDataChanges()
        {
            VirtualPrinter printer = new();
            printer.SetNextElementFieldDataContent("abc");
            Code39BarcodeFieldData barcode = new();

            printer.SetNextElementFieldData(barcode);

            Assert.AreSame(barcode, printer.NextElementFieldData);
            Assert.AreEqual("abc", barcode.Content);
        }

        [TestMethod]
        public void FieldDataWithoutPreviousFieldDataHasNoContent()
        {
            VirtualPrinter printer = new();
            Code39BarcodeFieldData barcode = new();

            printer.SetNextElementFieldData(barcode);
            Assert.IsNull(printer.NextElementFieldData.Content);

            printer.SetNextElementFieldDataContent("later");
            Assert.AreEqual("later", barcode.Content);

            printer.ClearNextElementFieldData();
            Assert.IsNull(printer.NextElementFieldData);
        }

        [TestMethod]
        public void FieldBlock()
        {
            VirtualPrinter printer = new();
            FieldBlock block = new();

            printer.SetNextElementFieldBlock(block);
            Assert.AreSame(block, printer.NextElementFieldBlock);

            printer.ClearNextElementFieldBlock();
            Assert.IsNull(printer.NextElementFieldBlock);
        }

        [TestMethod]
        public void NextFont()
        {
            VirtualPrinter printer = new();

            printer.SetNextFont("B", FieldOrientation.Rotated90, 11, 22);
            Assert.AreEqual("B", printer.NextFont.FontName);
            Assert.AreEqual(FieldOrientation.Rotated90, printer.NextFont.FieldOrientation);
            Assert.AreEqual(11, printer.NextFont.FontWidth);
            Assert.AreEqual(22, printer.NextFont.FontHeight);

            printer.ClearNextFont();
            Assert.IsNull(printer.NextFont);
        }

        [TestMethod]
        public void FieldOrientationAlsoChangesAnExistingNextFont()
        {
            VirtualPrinter printer = new();
            printer.SetNextFont("B", FieldOrientation.Normal, 11, 22);

            printer.SetFieldOrientation(FieldOrientation.Rotated270);

            Assert.AreEqual(FieldOrientation.Rotated270, printer.FieldOrientation);
            Assert.AreEqual(FieldOrientation.Rotated270, printer.NextFont.FieldOrientation);
            Assert.AreEqual("B", printer.NextFont.FontName);
            Assert.AreEqual(11, printer.NextFont.FontWidth);
            Assert.AreEqual(22, printer.NextFont.FontHeight);
        }

        [TestMethod]
        public void FieldOrientationWithoutNextFont()
        {
            VirtualPrinter printer = new();

            printer.SetFieldOrientation(FieldOrientation.Rotated180);

            Assert.AreEqual(FieldOrientation.Rotated180, printer.FieldOrientation);
            Assert.IsNull(printer.NextFont);
        }

        [TestMethod]
        public void PerFieldFlags()
        {
            VirtualPrinter printer = new();

            printer.SetNextElementFieldReverse();
            Assert.IsTrue(printer.NextElementFieldReverse);
            printer.ClearNextElementFieldReverse();
            Assert.IsFalse(printer.NextElementFieldReverse);

            printer.SetNextElementFieldHexadecimalIndicator('\\');
            Assert.AreEqual('\\', printer.NextElementFieldHexadecimalIndicator);
            printer.ClearNextElementFieldHexadecimalIndicator();
            Assert.IsNull(printer.NextElementFieldHexadecimalIndicator);

            printer.SetNextElementFieldJustification(FieldJustification.Right);
            Assert.AreEqual(FieldJustification.Right, printer.NextElementFieldJustification);
            printer.ClearNextElementFieldJustification();
            Assert.AreEqual(FieldJustification.None, printer.NextElementFieldJustification);
        }

        [TestMethod]
        public void LabelWideSettings()
        {
            VirtualPrinter printer = new();

            printer.SetLabelReverse(true);
            printer.SetFieldJustification(FieldJustification.Auto);
            printer.SetFontWidth(5);
            printer.SetFontHeight(6);
            printer.SetFontName("0");
            printer.SetBarcodeModuleWidth(4);
            printer.SetBarcodeWideBarToNarrowBarWidthRatio(2.5);
            printer.SetBarcodeHeight(80);
            printer.SetLabelHome(7, 8);

            Assert.IsTrue(printer.LabelReverse);
            Assert.AreEqual(FieldJustification.Auto, printer.FieldJustification);
            Assert.AreEqual(5, printer.FontWidth);
            Assert.AreEqual(6, printer.FontHeight);
            Assert.AreEqual("0", printer.FontName);
            Assert.AreEqual(4, printer.BarcodeInfo.ModuleWidth);
            Assert.AreEqual(2.5, printer.BarcodeInfo.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(80, printer.BarcodeInfo.Height);
            Assert.AreEqual(7, printer.LabelHomePosition.X);
            Assert.AreEqual(8, printer.LabelHomePosition.Y);
        }

        [TestMethod]
        public void Comments()
        {
            VirtualPrinter printer = new();

            printer.AddComment("a");
            printer.AddComment("b");
            CollectionAssert.AreEqual(new[] { "a", "b" }, printer.Comments);

            printer.ClearComments();
            Assert.IsEmpty(printer.Comments);
        }

        [TestMethod]
        public void DownloadFormatNameAndFieldNumber()
        {
            VirtualPrinter printer = new();

            printer.SetNextDownloadFormatName("FMT");
            Assert.AreEqual("FMT", printer.NextDownloadFormatName);
            printer.ClearNextDownloadFormatName();
            Assert.IsNull(printer.NextDownloadFormatName);

            printer.SetNextFieldNumber(12);
            Assert.AreEqual(12, printer.NextFieldNumber);
            printer.ClearNextFieldNumber();
            Assert.IsNull(printer.NextFieldNumber);
        }

        [TestMethod]
        public void ModelClassesKeepTheirValues()
        {
            LabelHome home = new(3, 4);
            Assert.AreEqual(3, home.X);
            Assert.AreEqual(4, home.Y);

            LabelPosition position = new(1, 2, true, true);
            Assert.AreEqual(1, position.X);
            Assert.AreEqual(2, position.Y);
            Assert.IsTrue(position.CalculateFromBottom);
            Assert.IsTrue(position.UseDefaultPosition);

            FontInfo font = new("A", FieldOrientation.Rotated180, 5, 6);
            Assert.AreEqual("A", font.FontName);
            Assert.AreEqual(FieldOrientation.Rotated180, font.FieldOrientation);
            Assert.AreEqual(5, font.FontWidth);
            Assert.AreEqual(6, font.FontHeight);
        }

        [TestMethod]
        public void ResourcesCultureCanBeOverridden()
        {
            Assert.IsNotNull(new Resources());
            Assert.IsNotNull(Resources.ResourceManager);
            Assert.IsNotEmpty(Resources.ZplGS);

            CultureInfo original = Resources.Culture;
            try
            {
                Resources.Culture = CultureInfo.InvariantCulture;
                Assert.AreSame(CultureInfo.InvariantCulture, Resources.Culture);
                Assert.IsNotEmpty(Resources.ZplGS);
            }
            finally
            {
                Resources.Culture = original;
            }
        }
    }
}
