using Bitbeans.Zpl;
using Bitbeans.Zpl.Viewer.Helpers;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class HexEscapeEncodingTests
    {
        [TestMethod]
        public void HexEscapesAreDecodedWithTheCodePageOfTheInternationalFont()
        {
            // the same byte (or bytes) decodes to a different character per code page
            (InternationalFont font, string text, string expected)[] cases =
            [
                (InternationalFont.ZCP850, "_84", "ä"),
                (InternationalFont.ZCP1250, "_E8", "č"),
                (InternationalFont.ZCP1251, "_E4", "д"),
                (InternationalFont.ZCP1252, "_E4", "ä"),
                (InternationalFont.ZCP1253, "_E4", "δ"),
                (InternationalFont.ZCP1254, "_FE", "ş"),
                (InternationalFont.ZCP1255, "_E0", "א"),
                (InternationalFont.UTF8, "_C3_A4", "ä"),
                (InternationalFont.UTF16_BE, "_00_E4", "ä"),
                (InternationalFont.UTF16_LE, "_E4_00", "ä"),
            ];

            foreach ((InternationalFont font, string text, string expected) in cases)
            {
                Assert.AreEqual(expected, text.ReplaceHexEscapes('_', font), font.ToString());
            }
        }

        [TestMethod]
        public void PlainCharactersAndInvalidEscapesAreKept()
        {
            Assert.AreEqual("AB", "AB".ReplaceHexEscapes('_', InternationalFont.ZCP1252));
            Assert.AreEqual("a_zz", "a_zz".ReplaceHexEscapes('_', InternationalFont.ZCP1252));
            Assert.AreEqual("a_4", "a_4".ReplaceHexEscapes('_', InternationalFont.ZCP1252));
            Assert.AreEqual("AäB", "A_E4B".ReplaceHexEscapes('_', InternationalFont.ZCP1252));
            Assert.AreEqual("AäB", "A#E4B".ReplaceHexEscapes('#', InternationalFont.ZCP1252));
        }
    }
}
