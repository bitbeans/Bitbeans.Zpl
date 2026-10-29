using Bitbeans.Zpl.Elements;
using System;
using System.Linq;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class ZplElementBaseTests
    {
        private readonly ZplElementBase element = new ZplRaw("^XA");

        [TestMethod]
        public void Defaults()
        {
            Assert.IsTrue(element.IsEnabled);
            Assert.IsNull(element.Id);
            Assert.IsNotNull(element.Comments);
            Assert.IsEmpty(element.Comments);
        }

        [TestMethod]
        public void RenderUsesDefaultOptionsAndJoinsWithSpaceOrNewLine()
        {
            var text = new ZplTextField("A", 10, 20, new ZplFont());

            Assert.AreEqual("^A0N,30,30", text.Render().First());
            Assert.AreEqual("^A0N,30,30 ^FO10,20 ^FDA^FS", text.RenderToString());
            Assert.AreEqual("^A0N,30,30\n^FO10,20\n^FDA^FS", text.ToZplString());
        }

        [TestMethod]
        public void ToZplStringUsesGivenOptions()
        {
            var box = new ZplGraphicBox(100, 100, 100, 100);
            var options = new ZplRenderOptions { SourcePrintDpi = 100, TargetPrintDpi = 200 };

            Assert.AreEqual("^FO200,200\n^GB200,200,2,B,0^FS", box.ToZplString(options));
        }

        [TestMethod]
        [DataRow(FieldOrientation.Normal, "N")]
        [DataRow(FieldOrientation.Rotated90, "R")]
        [DataRow(FieldOrientation.Rotated180, "I")]
        [DataRow(FieldOrientation.Rotated270, "B")]
        public void RenderFieldOrientation(FieldOrientation value, string expected)
        {
            Assert.AreEqual(expected, element.RenderFieldOrientation(value));
        }

        [TestMethod]
        [DataRow(QualityLevel.ECC0, "0")]
        [DataRow(QualityLevel.ECC50, "50")]
        [DataRow(QualityLevel.ECC80, "80")]
        [DataRow(QualityLevel.ECC100, "100")]
        [DataRow(QualityLevel.ECC140, "140")]
        [DataRow(QualityLevel.ECC200, "200")]
        public void RenderQualityLevel(QualityLevel value, string expected)
        {
            Assert.AreEqual(expected, element.RenderQualityLevel(value));
        }

        [TestMethod]
        [DataRow(FieldJustification.None, "")]
        [DataRow(FieldJustification.Left, "0")]
        [DataRow(FieldJustification.Right, "1")]
        [DataRow(FieldJustification.Auto, "2")]
        public void RenderFieldJustification(FieldJustification value, string expected)
        {
            Assert.AreEqual(expected, element.RenderFieldJustification(value));
        }

        [TestMethod]
        [DataRow(LineColor.Black, "B")]
        [DataRow(LineColor.White, "W")]
        public void RenderLineColor(LineColor value, string expected)
        {
            Assert.AreEqual(expected, element.RenderLineColor(value));
        }

        [TestMethod]
        [DataRow(ErrorCorrectionLevel.UltraHighReliability, "H")]
        [DataRow(ErrorCorrectionLevel.HighReliability, "Q")]
        [DataRow(ErrorCorrectionLevel.Standard, "M")]
        [DataRow(ErrorCorrectionLevel.HighDensity, "L")]
        public void RenderErrorCorrectionLevel(ErrorCorrectionLevel value, string expected)
        {
            Assert.AreEqual(expected, element.RenderErrorCorrectionLevel(value));
        }

        [TestMethod]
        public void RenderBoolean()
        {
            Assert.AreEqual("Y", element.RenderBoolean(true));
            Assert.AreEqual("N", element.RenderBoolean(false));
        }

        [TestMethod]
        public void UnknownEnumValuesAreRejected()
        {
            Assert.ThrowsExactly<NotImplementedException>(() => element.RenderFieldOrientation((FieldOrientation)99));
            Assert.ThrowsExactly<NotImplementedException>(() => element.RenderQualityLevel((QualityLevel)99));
            Assert.ThrowsExactly<NotImplementedException>(() => element.RenderFieldJustification((FieldJustification)99));
            Assert.ThrowsExactly<NotImplementedException>(() => element.RenderLineColor((LineColor)99));
            Assert.ThrowsExactly<NotImplementedException>(() => element.RenderErrorCorrectionLevel((ErrorCorrectionLevel)99));
        }
    }
}
