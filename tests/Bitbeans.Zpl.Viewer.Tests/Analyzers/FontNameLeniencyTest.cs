using System.Linq;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// The core validates font names (A-Z, 0-9); the viewer keeps parsing printer input leniently.
    /// </summary>
    [TestClass]
    public class FontNameLeniencyTest
    {
        private static ZplTextField SingleText(string zpl)
        {
            AnalyzeInfo info = AnalyzerTestHelper.Analyze(zpl);
            Assert.IsEmpty(info.Errors, string.Join("\n", info.Errors));
            return info.LabelInfos[0].ZplElements.OfType<ZplTextField>().Single();
        }

        [TestMethod]
        public void FontCallByNameKeepsTheCurrentFont()
        {
            ZplTextField text = SingleText("^XA^CFD^FO10,10^A@N,30,30,E:ARIAL.TTF^FDHello^FS^XZ");

            Assert.AreEqual("D", text.Font.FontName);
            Assert.AreEqual(30, text.Font.FontHeight);
            Assert.AreEqual("Hello", text.Text);
        }

        [TestMethod]
        public void FontCallByNameAfterAnotherFontKeepsThatFont()
        {
            ZplTextField text = SingleText("^XA^FO10,10^AF^A@N,30,30^FDHello^FS^XZ");

            Assert.AreEqual("F", text.Font.FontName);
        }

        [TestMethod]
        public void FontCommandWithoutNameKeepsTheCurrentFont()
        {
            Assert.AreEqual("C", SingleText("^XA^CFC^FO10,10^A^FDx^FS^XZ").Font.FontName);
        }

        [TestMethod]
        public void LowercaseFontNamesAreNormalized()
        {
            Assert.AreEqual("B", SingleText("^XA^FO10,10^Ab,20,20^FDx^FS^XZ").Font.FontName);
            Assert.AreEqual("E", SingleText("^XA^CFe,20^FO10,10^FDx^FS^XZ").Font.FontName);
        }

        [TestMethod]
        public void InvalidDefaultFontNamesKeepTheCurrentFont()
        {
            Assert.AreEqual("A", SingleText("^XA^CF@,20^FO10,10^FDx^FS^XZ").Font.FontName);
            Assert.AreEqual("A", SingleText("^XA^CFAB,20^FO10,10^FDx^FS^XZ").Font.FontName);
        }
    }
}
