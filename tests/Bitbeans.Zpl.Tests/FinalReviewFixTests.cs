using Bitbeans.Zpl.Elements;
using System;

namespace Bitbeans.Zpl.Tests
{
    /// <summary>Tests for the name/device validation (ZPL injection), QR code ranges and ^GF parameter order.</summary>
    [TestClass]
    public class FinalReviewFixTests
    {
        private static readonly FakeImageConverter Converter = new FakeImageConverter();

        private static readonly string[] BadNames =
        [
            "A^B", "A~B", "A,B", "A:B", "A B", "A\tB", "A\nB", "A\u0001B", "A\u0085B"
        ];

        [TestMethod]
        public void RecallFormatRejectsInjection()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat("R:A.ZPL^FS^FO1,1^FDX"));
            Assert.AreEqual("formatName", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat("A.ZPL~JA"));
        }

        [TestMethod]
        public void RecallFormatAcceptsPathsAndRejectsInvalidOnes()
        {
            Assert.AreEqual("^XFR:LABEL.ZPL^FS", new ZplRecallFormat("R:LABEL.ZPL").ToZplString());
            Assert.AreEqual("^XFLABEL.ZPL^FS", new ZplRecallFormat("LABEL.ZPL").ToZplString());
            Assert.AreEqual("Z", new ZplRecallFormat("Z:X.ZPL").FormatName.Substring(0, 1));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplRecallFormat(null));
            // An omitted name is valid ZPL; the printer then uses UNKNOWN
            Assert.AreEqual("^XF^FS", new ZplRecallFormat(string.Empty).ToZplString());
            Assert.AreEqual("^XFR:^FS", new ZplRecallFormat("R:").ToZplString());
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat("RR:X.ZPL"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat(":X.ZPL"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat("Q:X.ZPL"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat("R:X:Y.ZPL"));
            foreach (string bad in BadNames)
            {
                if (bad != "A:B") // a valid device prefix
                {
                    Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallFormat(bad), bad);
                }
            }
        }

        [TestMethod]
        public void DownloadGraphicsRejectsInjectionAndBadDevice()
        {
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics('R', "A^FS", [1], Converter));
            Assert.AreEqual("imageName", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics('R', "A.B", [1], Converter));
            Assert.AreEqual("UNKNOWN", new ZplDownloadGraphics('R', string.Empty, [1], Converter).ImageName);
            foreach (string bad in BadNames)
            {
                Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics('R', bad, [1], Converter), bad);
            }

            ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics(':', "A", [1], Converter));
            Assert.AreEqual("storageDevice", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics('r', "A", [1], Converter));
            ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadGraphics('Z', "A", [1], Converter));
            Assert.AreEqual("storageDevice", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadObjects('Z', "A.PNG", [1], Converter));
        }

        [TestMethod]
        public void DownloadObjectsValidatesNameAndDevice()
        {
            Assert.AreEqual("SAMPLE.PNG", new ZplDownloadObjects('E', "SAMPLE.PNG", [1], Converter).ObjectName);
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadObjects('R', "A,P^FS", [1], Converter));
            Assert.AreEqual("imageName", ex.ParamName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadObjects('R', "R:A.PNG", [1], Converter));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplDownloadObjects('X', "A.PNG", [1], Converter));
        }

        [TestMethod]
        public void RecallGraphicValidatesNameAndDevice()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallGraphic(0, 0, 'R', "A^FS"));
            Assert.AreEqual("UNKNOWN", new ZplRecallGraphic(0, 0, 'R', string.Empty).ImageName);
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplRecallGraphic(0, 0, 'R', null));
            var ex = Assert.ThrowsExactly<ArgumentException>(() => new ZplRecallGraphic(0, 0, '^', "A"));
            Assert.AreEqual("storageDevice", ex.ParamName);
        }

        [TestMethod]
        public void ImageMoveValidatesParts()
        {
            Assert.AreEqual("^FO0,0\n^IMR:LOGO.GRF^FS", new ZplImageMove(0, 0, 'R', "LOGO", "GRF").ToZplString());
            Assert.AreEqual(string.Empty, new ZplImageMove(0, 0, 'R', "LOGO.GRF", string.Empty).Extension);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplImageMove(0, 0, 'R', "A^FS", "GRF"));
            Assert.AreEqual("^FO0,0\n^IMR:UNKNOWN.GRF^FS", new ZplImageMove(0, 0, 'R', string.Empty, "GRF").ToZplString());
            Assert.AreEqual("^FO0,0\n^IMR:LOGO.GRF^FS", new ZplImageMove(0, 0, 'R', "LOGO.GRF", string.Empty).ToZplString());
            Assert.AreEqual("LOGO.GRF", new ZplImageMove(0, 0, 'R', "LOGO", "GRF").FileName);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplImageMove(0, 0, 'R', "A", "G^"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplImageMove(0, 0, 'R', "A", ".GRF"));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplImageMove(0, 0, 'R', "A", null));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplImageMove(0, 0, '1', "A", "GRF"));
        }

        [TestMethod]
        public void FontIdentifierValidatesParts()
        {
            Assert.AreEqual("^CWE,R:ARIAL.TTF", new ZplFontIdentifier('E', "R", "ARIAL.TTF").ToZplString());
            Assert.AreEqual('7', new ZplFontIdentifier('7', "E", "A.TTF").FontReplaceLetter);
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier(',', "R", "A.TTF"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier('ä', "R", "A.TTF"));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplFontIdentifier('E', null, "A.TTF"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier('E', "R:", "A.TTF"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier('E', string.Empty, "A.TTF"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier('E', "Q", "A.TTF"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFontIdentifier('E', "R", "A.TTF^FS"));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplFontIdentifier('E', "R", null));
        }

        [TestMethod]
        public void FontValidatesFontName()
        {
            Assert.AreEqual("^AEN,30,30", new ZplFont(30, 30, "E").ToZplString());
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplFont(30, 30, null));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFont(30, 30, string.Empty));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFont(30, 30, "AB"));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplFont(30, 30, "^"));
        }

        [TestMethod]
        public void QrCodeEnforcesModelAndMask()
        {
            Assert.AreEqual(1, new ZplQrCode("X", 0, 0, model: 1, maskValue: 0).Model);
            Assert.AreEqual(7, new ZplQrCode("X", 0, 0, model: 2, maskValue: 7).MaskValue);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplQrCode("X", 0, 0, model: 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplQrCode("X", 0, 0, model: 3));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplQrCode("X", 0, 0, maskValue: -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplQrCode("X", 0, 0, maskValue: 8));
        }

        [TestMethod]
        public void GraphicFieldRendersBinaryByteCountBeforeGraphicFieldCount()
        {
            var field = new ZplGraphicField(0, 0, 4, 8, 2, "FF00FF00");
            Assert.AreEqual(4, field.BinaryByteCount);
            Assert.AreEqual(8, field.GraphicFieldCount);
            Assert.AreEqual("^FO0,0\n^GFA,4,8,2,FF00FF00^FS", field.ToZplString());
        }
    }
}
