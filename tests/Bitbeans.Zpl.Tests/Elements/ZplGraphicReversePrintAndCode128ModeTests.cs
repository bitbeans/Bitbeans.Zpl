using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Tests.Elements
{
    [TestClass]
    public class ZplGraphicReversePrintAndCode128ModeTests
    {
        [TestMethod]
        public void GraphicBox_ReversePrint_RendersFieldReverse()
        {
            var output = new ZplGraphicBox(10, 20, 100, 50, 3, reversePrint: true).ToZplString();

            Assert.AreEqual("^FO10,20\n^FR^GB100,50,3,B,0^FS", output);
        }

        [TestMethod]
        public void GraphicBox_NoReversePrint_OmitsFieldReverse()
        {
            var output = new ZplGraphicBox(10, 20, 100, 50, 3).ToZplString();

            Assert.AreEqual("^FO10,20\n^GB100,50,3,B,0^FS", output);
        }

        [TestMethod]
        public void GraphicCircle_ReversePrint_RendersFieldReverse()
        {
            var output = new ZplGraphicCircle(10, 20, 80, 2, reversePrint: true).ToZplString();

            Assert.AreEqual("^FO10,20\n^FR^GC80,2,B^FS", output);
        }

        [TestMethod]
        public void GraphicEllipse_ReversePrint_RendersFieldReverse()
        {
            var output = new ZplGraphicEllipse(10, 20, 80, 40, 2, reversePrint: true).ToZplString();

            Assert.AreEqual("^FO10,20\n^FR^GE80,40,2,B^FS", output);
        }

        [TestMethod]
        public void GraphicDiagonalLine_ReversePrint_RendersFieldReverse()
        {
            var output = new ZplGraphicDiagonalLine(10, 20, 80, 40, 2, reversePrint: true).ToZplString();

            Assert.AreEqual("^FO10,20\n^FR^GD80,40,2,B,R^FS", output);
        }

        [TestMethod]
        public void Barcode128_DefaultMode_OmitsOptionalParameters()
        {
            var output = new ZplBarcode128("123456", 10, 20).ToZplString();

            StringAssert.Contains(output, "^BCN,100,Y,N\n");
        }

        [TestMethod]
        [DataRow("U", true, "^BCN,100,Y,N,Y,U")]
        [DataRow("A", false, "^BCN,100,Y,N,N,A")]
        [DataRow("D", false, "^BCN,100,Y,N,N,D")]
        [DataRow("N", true, "^BCN,100,Y,N,Y,N")]
        public void Barcode128_ModeAndUccCheckDigit_AreRendered(string mode, bool uccCheckDigit, string expected)
        {
            var output = new ZplBarcode128("123456", 10, 20, mode: mode, uccCheckDigit: uccCheckDigit).ToZplString();

            StringAssert.Contains(output, expected + "\n");
        }
    }
}
