using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Helpers;
using System;

namespace Bitbeans.Zpl.Tests
{
    /// <summary>Regression tests for scaling, validation and compression format checks.</summary>
    [TestClass]
    public class ReviewFixTests
    {
        // ---- ZplRenderOptions ----

        [TestMethod]
        public void ScaleRoundsAwayFromZeroAndKeepsNonZeroValuesNonZero()
        {
            var down = new ZplRenderOptions { SourcePrintDpi = 300, TargetPrintDpi = 203 };
            Assert.AreEqual(1, down.Scale(1)); // 0.68 -> 1
            Assert.AreEqual(2, down.Scale(3)); // 2.03 -> 2
            Assert.AreEqual(0, down.Scale(0));
            Assert.AreEqual(-1, down.Scale(-1));
            Assert.AreEqual(-2, down.Scale(-3));

            var tiny = new ZplRenderOptions { SourcePrintDpi = 600, TargetPrintDpi = 100 };
            Assert.AreEqual(1, tiny.Scale(1)); // 0.17 -> 0 -> 1
            Assert.AreEqual(-1, tiny.Scale(-1));
            Assert.AreEqual(0, tiny.Scale(0));
            Assert.AreEqual(1, tiny.Scale(3)); // 0.5 -> 1 (away from zero)
            Assert.AreEqual(-1, tiny.Scale(-3));
        }

        [TestMethod]
        public void OneDotBorderStaysVisibleWhenScalingDown()
        {
            var options = new ZplRenderOptions { SourcePrintDpi = 300, TargetPrintDpi = 203 };
            Assert.AreEqual("^FO0,0\n^GB68,68,1,B,0^FS", new ZplGraphicBox(0, 0, 100, 100, 1).ToZplString(options));
        }

        [TestMethod]
        public void ScaleDoubleIsNotRounded()
        {
            var options = new ZplRenderOptions { SourcePrintDpi = 200, TargetPrintDpi = 300 };
            Assert.AreEqual(0.75, options.Scale(0.5));
        }

        [TestMethod]
        public void ScaleModuleWidthRoundsAndClampsToOneToTen()
        {
            var options = new ZplRenderOptions { SourcePrintDpi = 100, TargetPrintDpi = 250 };
            Assert.AreEqual(1, options.ScaleModuleWidth(0));
            Assert.AreEqual(1, options.ScaleModuleWidth(-4));
            Assert.AreEqual(3, options.ScaleModuleWidth(1));
            Assert.AreEqual(5, options.ScaleModuleWidth(2));
            Assert.AreEqual(10, options.ScaleModuleWidth(4));
            Assert.AreEqual(10, options.ScaleModuleWidth(40));
        }

        [TestMethod]
        public void DpiMustBePositive()
        {
            var options = new ZplRenderOptions();
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.SourcePrintDpi = 0);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.SourcePrintDpi = -203);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.TargetPrintDpi = 0);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => options.TargetPrintDpi = -1);
            Assert.AreEqual(203, options.SourcePrintDpi);
            Assert.AreEqual(203, options.TargetPrintDpi);

            options.SourcePrintDpi = 1;
            options.TargetPrintDpi = 600;
            Assert.AreEqual(600.0, options.ScaleFactor);
        }

        // ---- Code 128 mode, graphic symbol ----

        [TestMethod]
        [DataRow("N")]
        [DataRow("U")]
        [DataRow("A")]
        [DataRow("D")]
        public void Barcode128AcceptsDefinedModes(string mode)
        {
            Assert.AreEqual(mode, new ZplBarcode128("1", 0, 0, mode: mode).Mode);
            var element = new ZplBarcode128("1", 0, 0);
            element.Mode = mode;
            Assert.AreEqual(mode, element.Mode);
        }

        [TestMethod]
        [DataRow("X")]
        [DataRow("n")]
        [DataRow("")]
        [DataRow("NN")]
        [DataRow(null)]
        public void Barcode128RejectsUndefinedModes(string mode)
        {
            var ctor = Assert.ThrowsExactly<ArgumentException>(() => new ZplBarcode128("1", 0, 0, mode: mode));
            Assert.AreEqual("mode", ctor.ParamName);

            var element = new ZplBarcode128("1", 0, 0);
            Assert.ThrowsExactly<ArgumentException>(() => element.Mode = mode);
            Assert.AreEqual("N", element.Mode);
        }

        [TestMethod]
        public void GraphicSymbolRejectsUndefinedCharacters()
        {
            var text = Assert.ThrowsExactly<ArgumentException>(() => new ZplGraphicSymbol("Z", 0, 0, 10, 10));
            Assert.AreEqual("text", text.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplGraphicSymbol("a", 0, 0, 10, 10));
            var character = Assert.ThrowsExactly<ArgumentException>(() => new ZplGraphicSymbol((GraphicSymbolCharacter)'F', 0, 0, 10, 10));
            Assert.AreEqual("character", character.ParamName);
            Assert.AreEqual("^FO0,0\n^GSN,10,10^FDE^FS", new ZplGraphicSymbol("E", 0, 0, 10, 10).ToZplString());
        }

        // ---- ACS ----

        [TestMethod]
        public void AcsCompressRejectsIncompleteLastRow()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => ZebraACSCompressionHelper.Compress("AA", 2));
            Assert.AreEqual("hexData", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => ZebraACSCompressionHelper.Compress("AAAAAA", 2));
            Assert.ThrowsExactly<ArgumentException>(() => ZebraACSCompressionHelper.Compress("AAAA\nAA\n", 2));
            Assert.AreEqual("JA", ZebraACSCompressionHelper.Compress("AAAA", 2));
        }

        [TestMethod]
        public void AcsCompressRejectsNonPositiveBytesPerRow()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => ZebraACSCompressionHelper.Compress("AA", 0));
            Assert.AreEqual("bytesPerRow", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => ZebraACSCompressionHelper.Compress("AA", -1));
            Assert.ThrowsExactly<ArgumentException>(() => ZebraACSCompressionHelper.Compress(string.Empty, 0));
        }

        [TestMethod]
        public void AcsCompressAcceptsEmptyData()
        {
            Assert.AreEqual(string.Empty, ZebraACSCompressionHelper.Compress(string.Empty, 2));
        }

        // ---- B64 / Z64 ----

        private static string WithCrc(string tag, string base64)
        {
            return $":{tag}:{base64}:{Crc16.ComputeHex(base64.EncodeBytes())}";
        }

        [TestMethod]
        public void B64UncompressVerifiesCrc()
        {
            byte[] data = [1, 2, 3, 4, 5];
            string valid = ZebraB64CompressionHelper.Compress(data);
            CollectionAssert.AreEqual(data, ZebraB64CompressionHelper.Uncompress(valid));

            string crc = valid.Substring(valid.LastIndexOf(':') + 1);
            string wrong = valid.Substring(0, valid.LastIndexOf(':') + 1) + (crc == "0000" ? "0001" : "0000");
            var ex = Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(wrong));
            Assert.Contains("CRC", ex.Message);
        }

        [TestMethod]
        public void B64UncompressIgnoresCrcCaseAndSurroundingWhitespace()
        {
            byte[] data = [250, 251, 252, 253, 254, 255, 0, 1];
            string valid = ZebraB64CompressionHelper.Compress(data);
            string crc = valid.Substring(valid.LastIndexOf(':') + 1);
            string upper = valid.Substring(0, valid.LastIndexOf(':') + 1) + crc.ToUpperInvariant();
            CollectionAssert.AreEqual(data, ZebraB64CompressionHelper.Uncompress(upper));
            CollectionAssert.AreEqual(data, ZebraB64CompressionHelper.Uncompress("\r\n  " + valid + "\r\n"));
            CollectionAssert.AreEqual(data, ZebraB64CompressionHelper.Uncompress(valid.Insert(8, "\r\n")));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("QUJD")]
        [DataRow(":B64:QUJD")]
        [DataRow("x:B64:QUJD:0000")]
        [DataRow(":B64:QUJD:0000x")]
        [DataRow(":B64:QUJD:000")]
        [DataRow(":B64:QUJD:00000")]
        [DataRow(":B64:QU JD:0000")]
        [DataRow(":B64:QUJD:0000 :B64:QUJD:0000")]
        [DataRow(":Z64:QUJD:0000")]
        [DataRow(":b64:QUJD:0000")]
        public void B64UncompressRejectsMalformedInput(string input)
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(input));
        }

        [TestMethod]
        public void B64UncompressRejectsTrailingAndLeadingGarbageEvenWithValidCrc()
        {
            string valid = WithCrc("B64", "QUJD");
            Assert.AreEqual(3, ZebraB64CompressionHelper.Uncompress(valid).Length);
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(valid + "junk"));
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress("junk" + valid));
        }

        [TestMethod]
        public void B64UncompressRejectsInvalidBase64WithValidCrc()
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(WithCrc("B64", "QUJ")));
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(WithCrc("B64", "Q===")));
        }

        [TestMethod]
        public void Z64UncompressVerifiesCrc()
        {
            byte[] data = [1, 2, 3, 4, 5, 5, 5, 5, 5, 5];
            string valid = ZebraZ64CompressionHelper.Compress(data);
            CollectionAssert.AreEqual(data, ZebraZ64CompressionHelper.Uncompress(valid));

            string crc = valid.Substring(valid.LastIndexOf(':') + 1);
            string wrong = valid.Substring(0, valid.LastIndexOf(':') + 1) + (crc == "0000" ? "0001" : "0000");
            var ex = Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress(wrong));
            Assert.Contains("CRC", ex.Message);
        }

        [TestMethod]
        [DataRow("")]
        [DataRow(":Z64:eJzLTAYABYgCHg==")]
        [DataRow(":Z64:eJzLTAYABYgCHg==:")]
        [DataRow("x:Z64:eJzLTAYABYgCHg==:0000")]
        [DataRow(":Z64:eJzLTAYABYgCHg==:0000x")]
        [DataRow(":B64:QUJD:0000")]
        public void Z64UncompressRejectsMalformedInput(string input)
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress(input));
        }

        [TestMethod]
        public void Z64UncompressRejectsInvalidBase64AndInvalidZlibData()
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress(WithCrc("Z64", "QUJ")));

            //valid base64 and valid CRC, but not zlib data
            var ex = Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress(WithCrc("Z64", "QUJDREVGR0g=")));
            Assert.IsInstanceOfType<System.IO.InvalidDataException>(ex.InnerException);
        }

        [TestMethod]
        public void UncompressIsStrictAboutFormatTag()
        {
            string b64 = ZebraB64CompressionHelper.Compress([1, 2, 3]);
            Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress(b64));
            string z64 = ZebraZ64CompressionHelper.Compress([1, 2, 3]);
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(z64));
        }
    }
}
