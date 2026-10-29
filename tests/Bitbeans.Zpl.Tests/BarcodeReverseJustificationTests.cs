using Bitbeans.Zpl.Elements;

using System.Linq;

namespace Bitbeans.Zpl.Tests
{
    /// <summary>Regression tests for reverse print (<c>^FR</c>) and field justification on bar codes.</summary>
    [TestClass]
    public class BarcodeReverseJustificationTests
    {
        private static ZplFieldDataElementBase[] Elements(bool reverse, FieldJustification justification, bool bottomToTop = false)
        {
            return
            [
                new ZplBarcode11("123", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcode39("ABC", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcode93("ABC", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcode128("ABC", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeAnsiCodabar("123", 'A', 'B', 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeEan13("123456789012", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeEan8("1234567", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeIndustrial2of5("123", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeInterleaved2of5("1234", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeLogmars("123", 10, 20, 100, 2, 3, FieldOrientation.Normal, false, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeMsi("123", 10, 20, 100, 2, 3, FieldOrientation.Normal, MsiBarcodeCheckDigitMode.Mod1_10, null, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeUpcA("12345678901", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeUpcE("1234567", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplBarcodeUpcExtension("12", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplQrCode("abc", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplDataMatrix("abc", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplPDF417("abc", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplAztecBarcode("abc", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
                new ZplMaxiCode("abc", 10, 20, reversePrint: reverse, fieldJustification: justification, bottomToTop: bottomToTop),
            ];
        }

        [TestMethod]
        public void ReversePrintIsRenderedAsFieldReverseCommand()
        {
            foreach (ZplFieldDataElementBase element in Elements(true, FieldJustification.None))
            {
                Assert.IsTrue(element.ReversePrint, element.GetType().Name);
                string zpl = element.ToZplString(new ZplRenderOptions());
                Assert.Contains("^FR^FD", zpl, element.GetType().Name);
                Assert.IsLessThan(zpl.IndexOf("^FS", System.StringComparison.Ordinal), zpl.IndexOf("^FR", System.StringComparison.Ordinal), element.GetType().Name);
            }
        }

        [TestMethod]
        public void ReversePrintIsOmittedByDefault()
        {
            foreach (ZplFieldDataElementBase element in Elements(false, FieldJustification.None))
            {
                Assert.IsFalse(element.ReversePrint, element.GetType().Name);
                Assert.DoesNotContain("^FR", element.ToZplString(new ZplRenderOptions()), element.GetType().Name);
            }
        }

        [TestMethod]
        public void ReversePrintComesAfterTheHexadecimalCommand()
        {
            ZplBarcode128 barcode = new("ABC", 10, 20, hexadecimalIndicator: '_', reversePrint: true);
            string[] lines = barcode.Render(new ZplRenderOptions()).ToArray();
            Assert.AreEqual("^FH^FR^FDABC^FS", lines.Last());
        }

        [TestMethod]
        public void QrCodeKeepsPrefixAfterReversePrint()
        {
            ZplQrCode qr = new("abc", 10, 20, reversePrint: true);
            Assert.AreEqual("^FR^FDQA,abc^FS", qr.Render(new ZplRenderOptions()).Last());
        }

        [TestMethod]
        public void FieldJustificationIsRenderedInThePositionCommand()
        {
            foreach (ZplFieldDataElementBase element in Elements(false, FieldJustification.Right))
            {
                Assert.AreEqual(FieldJustification.Right, element.FieldJustification, element.GetType().Name);
                Assert.Contains("^FO10,20,1", element.ToZplString(new ZplRenderOptions()), element.GetType().Name);
            }

            foreach (ZplFieldDataElementBase element in Elements(false, FieldJustification.Auto, bottomToTop: true))
            {
                Assert.AreEqual(FieldJustification.Auto, element.FieldJustification, element.GetType().Name);
                Assert.Contains("^FT10,20,2", element.ToZplString(new ZplRenderOptions()), element.GetType().Name);
            }
        }

        [TestMethod]
        public void FieldJustificationDefaultsToNone()
        {
            ZplBarcodeEan8 ean8 = new("1234567", 1, 2);
            Assert.AreEqual(FieldJustification.None, ean8.FieldJustification);
            Assert.Contains("^FO1,2\n", ean8.ToZplString(new ZplRenderOptions()));
        }
    }
}
