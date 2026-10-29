using Bitbeans.Zpl.Elements;
using System;
using System.Globalization;
using System.Threading;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class ZplBarcodeElementTests
    {
        [TestMethod]
        public void Code11()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^B1N,N,100,Y,N\n^FD123^FS",
                new ZplBarcode11("123", 10, 20).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BY3,2.5\n^B1R,Y,50,N,Y\n^FD123^FS",
                new ZplBarcode11("123", 10, 20, 50, 3, 2.5, FieldOrientation.Rotated90, null, false, true, Code11CheckDigitCount.One).ToZplString());
        }

        [TestMethod]
        public void Code128()
        {
            var element = new ZplBarcode128("123ABC", 100, 300);
            Assert.AreEqual("^FO100,300\n^BY2,3\n^BCN,100,Y,N\n^FD123ABC^FS", element.ToZplString());
            Assert.AreEqual("N", element.Mode);
            Assert.IsFalse(element.UccCheckDigit);

            var custom = new ZplBarcode128("1", 1, 2, 80, 4, 2, FieldOrientation.Rotated180, null, false, true, mode: "U", uccCheckDigit: true);
            Assert.AreEqual("^FO1,2\n^BY4,2\n^BCI,80,N,Y,Y,U\n^FD1^FS", custom.ToZplString());
            Assert.AreEqual("U", custom.Mode);
            Assert.IsTrue(custom.UccCheckDigit);
        }

        [TestMethod]
        public void Code39()
        {
            Assert.AreEqual(
                "^FO100,100\n^BY2,3\n^B3N,N,100,Y,N\n^FD123ABC^FS",
                new ZplBarcode39("123ABC", 100, 100).ToZplString());
            Assert.AreEqual(
                "^FO100,100\n^BY2,3\n^B3B,Y,100,Y,N\n^FD123ABC^FS",
                new ZplBarcode39("123ABC", 100, 100, fieldOrientation: FieldOrientation.Rotated270, mod43CheckDigit: true).ToZplString());
        }

        [TestMethod]
        public void Code93()
        {
            Assert.AreEqual(
                "^FO100,300\n^BY2,3\n^BAN,100,Y,N,N\n^FD123ABC^FS",
                new ZplBarcode93("123ABC", 100, 300).ToZplString());
            Assert.AreEqual(
                "^FO100,300\n^BY2,3\n^BAR,100,N,Y,Y\n^FD123ABC^FS",
                new ZplBarcode93("123ABC", 100, 300, fieldOrientation: FieldOrientation.Rotated90, printInterpretationLine: false, printInterpretationLineAboveCode: true, checkDigit: true).ToZplString());
        }

        [TestMethod]
        public void AnsiCodabar()
        {
            var element = new ZplBarcodeAnsiCodabar("123", 'A', 'B', 10, 20);
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BKN,N,100,Y,N,A,B\n^FD123^FS", element.ToZplString());
            Assert.AreEqual('A', element.StartCharacter);
            Assert.AreEqual('B', element.StopCharacter);
            Assert.IsFalse(element.CheckDigit);

            // lower case start/stop characters are accepted and normalized
            var lower = new ZplBarcodeAnsiCodabar("123", 'c', 'd', 10, 20, checkDigit: true);
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BKN,Y,100,Y,N,C,D\n^FD123^FS", lower.ToZplString());
        }

        [TestMethod]
        public void AnsiCodabarRejectsInvalidStartAndStopCharacter()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() => new ZplBarcodeAnsiCodabar("1", 'E', 'A', 0, 0));
            Assert.ThrowsExactly<InvalidOperationException>(() => new ZplBarcodeAnsiCodabar("1", 'A', 'Z', 0, 0));
        }

        [TestMethod]
        public void Ean13()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BEN,100,Y,N\n^FD123456789012^FS",
                new ZplBarcodeEan13("123456789012", 10, 20).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BER,100,N,Y\n^FD123456789012^FS",
                new ZplBarcodeEan13("123456789012", 10, 20, fieldOrientation: FieldOrientation.Rotated90, printInterpretationLine: false, printInterpretationLineAboveCode: true).ToZplString());
        }

        [TestMethod]
        public void Ean8()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^B8N,100,Y,N\n^FD1234567^FS",
                new ZplBarcodeEan8("1234567", 10, 20).ToZplString());
        }

        [TestMethod]
        public void Industrial2of5()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BIN,100,Y,N\n^FD1234^FS",
                new ZplBarcodeIndustrial2of5("1234", 10, 20).ToZplString());
        }

        [TestMethod]
        public void Interleaved2of5()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^B2N,100,Y,N,N\n^FD1234^FS",
                new ZplBarcodeInterleaved2of5("1234", 10, 20).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^B2N,100,Y,N,Y\n^FD1234^FS",
                new ZplBarcodeInterleaved2of5("1234", 10, 20, mod10CheckDigit: true).ToZplString());
        }

        [TestMethod]
        public void Logmars()
        {
            var element = new ZplBarcodeLogmars("ABC", 10, 20, 100, 2, 3, FieldOrientation.Normal, false);
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BLN,100,N\n^FDABC^FS", element.ToZplString());
            Assert.IsTrue(element.PrintInterpretationLine);

            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BLR,100,Y\n^FDABC^FS",
                new ZplBarcodeLogmars("ABC", 10, 20, 100, 2, 3, FieldOrientation.Rotated90, true).ToZplString());
        }

        [TestMethod]
        public void LogmarsHonorsHexadecimalIndicator()
        {
            var element = new ZplBarcodeLogmars("A_B", 10, 20, 100, 2, 3, FieldOrientation.Normal, false, '_');
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BLN,100,N\n^FH^FDA_B^FS", element.ToZplString());
        }

        [TestMethod]
        [DataRow(MsiBarcodeCheckDigitMode.None, "A")]
        [DataRow(MsiBarcodeCheckDigitMode.Mod1_10, "B")]
        [DataRow(MsiBarcodeCheckDigitMode.Mod2_10, "C")]
        [DataRow(MsiBarcodeCheckDigitMode.Mod_1_11_1_10, "D")]
        public void Msi(MsiBarcodeCheckDigitMode mode, string expectedCode)
        {
            var element = new ZplBarcodeMsi("1234", 10, 20, 100, 2, 3, FieldOrientation.Normal, mode, null);
            Assert.AreEqual($"^FO10,20\n^BY2,3\n^BMN,{expectedCode},100,Y,N,N\n^FD1234^FS", element.ToZplString());
            Assert.AreEqual(mode, element.CheckDigitMode);
            Assert.IsFalse(element.PrintCheckDigit);
        }

        [TestMethod]
        public void MsiWithOptions()
        {
            var element = new ZplBarcodeMsi("12_4", 10, 20, 100, 2, 3, FieldOrientation.Rotated180, MsiBarcodeCheckDigitMode.Mod1_10, '_', false, true, true);
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BMI,B,100,N,Y,Y\n^FH^FD12_4^FS", element.ToZplString());
            Assert.IsTrue(element.PrintCheckDigit);
        }

        [TestMethod]
        public void UpcA()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BUN,100,Y,N,Y\n^FD12345678901^FS",
                new ZplBarcodeUpcA("12345678901", 10, 20).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BUN,100,Y,N,N\n^FD12345678901^FS",
                new ZplBarcodeUpcA("12345678901", 10, 20, printCheckDigit: false).ToZplString());
        }

        [TestMethod]
        public void UpcE()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^B9N,100,Y,N,Y\n^FD1234567^FS",
                new ZplBarcodeUpcE("1234567", 10, 20).ToZplString());
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^B9N,100,Y,N,N\n^FD1234567^FS",
                new ZplBarcodeUpcE("1234567", 10, 20, printCheckDigit: false).ToZplString());
        }

        [TestMethod]
        public void UpcExtension()
        {
            Assert.AreEqual(
                "^FO10,20\n^BY2,3\n^BSN,100,Y,N\n^FD12^FS",
                new ZplBarcodeUpcExtension("12", 10, 20).ToZplString());
        }

        [TestMethod]
        public void PropertiesAreExposed()
        {
            var element = new ZplBarcode39("X", 10, 20, 50, 4, 2.5, FieldOrientation.Rotated90, '_', false, true, true);

            Assert.AreEqual(50, element.Height);
            Assert.AreEqual(4, element.ModuleWidth);
            Assert.AreEqual(2.5, element.WideBarToNarrowBarWidthRatio);
            Assert.AreEqual(FieldOrientation.Rotated90, element.FieldOrientation);
            Assert.AreEqual('_', element.HexadecimalIndicator);
            Assert.IsFalse(element.PrintInterpretationLine);
            Assert.IsTrue(element.PrintInterpretationLineAboveCode);
            Assert.IsTrue(element.Mod43CheckDigit);
            Assert.AreEqual("X", element.Content);
        }

        [TestMethod]
        public void HeightIsScaledButModuleWidthIsNot()
        {
            var options = new ZplRenderOptions { SourcePrintDpi = 200, TargetPrintDpi = 300 };

            Assert.AreEqual(
                "^FO15,30\n^BY2,3\n^BCN,150,Y,N\n^FD1^FS",
                new ZplBarcode128("1", 10, 20).ToZplString(options));
        }

        [TestMethod]
        public void ModuleWidthRatioIsRenderedCultureInvariant()
        {
            var previous = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                Assert.AreEqual(
                    "^FO10,20\n^BY2,2.5\n^BCN,100,Y,N\n^FD1^FS",
                    new ZplBarcode128("1", 10, 20, wideBarToNarrowBarWidthRatio: 2.5).ToZplString());
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = previous;
            }
        }

        [TestMethod]
        public void FieldTypesetAndHexadecimalIndicator()
        {
            var element = new ZplBarcode128("A_B", 10, 20, hexadecimalIndicator: '_', bottomToTop: true);
            Assert.AreEqual("^FT10,20\n^BY2,3\n^BCN,100,Y,N\n^FH^FDA_B^FS", element.ToZplString());

            var custom = new ZplBarcode128("AB", 10, 20, hexadecimalIndicator: '\\');
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BCN,100,Y,N\n^FH\\^FDAB^FS", custom.ToZplString());

            var defaultPosition = new ZplBarcode128("AB", 10, 20, bottomToTop: true, useDefaultPosition: true);
            Assert.AreEqual("^FT\n^BY2,3\n^BCN,100,Y,N\n^FDAB^FS", defaultPosition.ToZplString());
        }
    }
}
