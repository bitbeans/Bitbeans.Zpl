using System;
using System.Collections.Generic;
using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class FieldNumberTest
    {
        [TestMethod]
        public void SingleElement()
        {
            var textField = new ZplTextField(null, 50, 100, ZplConstants.Font.Default);
            var output = new ZplFieldNumber(100, textField).ToZplString();

            Assert.IsNotNull(output);
            Assert.AreEqual("^A0N,30,30\n^FO50,100\n^FN100^FS", output);
        }

        [TestMethod]
        public void FieldNumberReplacesFieldDataInsideTheField()
        {
            var textField = new ZplTextField("", 50, 50, ZplConstants.Font.Default);

            Assert.AreEqual("^A0N,30,30\n^FO50,50\n^FN1^FS", new ZplFieldNumber(1, textField).ToZplString());
        }

        [TestMethod]
        public void FieldNumberKeepsHexadecimalIndicatorAndReversePrint()
        {
            var textField = new ZplTextField("x", 50, 50, ZplConstants.Font.Default, hexadecimalIndicator: '_', reversePrint: true);

            Assert.AreEqual("^A0N,30,30\n^FO50,50\n^FH^FR^FN2^FS", new ZplFieldNumber(2, textField).ToZplString());
        }

        [TestMethod]
        public void FieldNumberWithFieldBlockAndBarcode()
        {
            var block = new ZplFieldBlock("x", 10, 20, 300, ZplConstants.Font.Default);
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FB300,1,0,L,0\n^FN3^FS", new ZplFieldNumber(3, block).ToZplString());

            var barcode = new ZplBarcode128("123", 10, 20);
            Assert.AreEqual("^FO10,20\n^BY2,3\n^BCN,100,Y,N\n^FN4^FS", new ZplFieldNumber(4, barcode).ToZplString());
        }

        [TestMethod]
        public void FieldNumberReplacesFieldDataOfGenericFieldDataElement()
        {
            var element = new ZplFieldDataElementBase("x", 1, 2, FieldOrientation.Normal, null, false, false);
            Assert.AreEqual("^FO1,2\n^FN5^FS", new ZplFieldNumber(5, element).ToZplString());
        }

        [TestMethod]
        public void FieldNumberAppendsWhenLastLineHasNoFieldData()
        {
            Assert.AreEqual("^A0N,30,30^FN6^FS", new ZplFieldNumber(6, new NoFieldDataElement()).ToZplString());
        }

        [TestMethod]
        public void WrongTypeElement()
        {
            var data = new ZplReferenceGrid();
            Assert.Throws<ArgumentException>(() => new ZplFieldNumber(100, data).ToZplString());
        }

        private sealed class NoFieldDataElement : ZplElementBase, IFormatElement
        {
            public override IEnumerable<string> Render(ZplRenderOptions context)
            {
                return new[] { "^A0N,30,30" };
            }

            public void SetTemplateContent(string content)
            {
            }
        }
    }
}
