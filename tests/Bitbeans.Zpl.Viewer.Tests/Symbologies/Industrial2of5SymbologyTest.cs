using System;
using System.Text;
using Bitbeans.Zpl.Viewer.Symologies;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    [TestClass]
    public class Industrial2of5SymbologyTest
    {
        // Standard 2 of 5 digit encoding (weights 1-2-4-7-parity): 1 = wide bar, 0 = narrow bar.
        private static readonly string[] WideBars =
        [
            "00110", "10001", "01001", "11000", "00101",
            "10100", "01100", "00011", "10010", "01010"
        ];

        private const string Start = "11011010";
        private const string Stop = "11010110";

        private static string Expected(string digits)
        {
            StringBuilder builder = new(Start);
            foreach (char digit in digits)
            {
                foreach (char wide in WideBars[digit - '0'])
                {
                    builder.Append(wide == '1' ? "111" : "1");
                    builder.Append('0');
                }
            }

            builder.Append(Stop);
            return builder.ToString();
        }

        [TestMethod]
        public void EncodeNullThrows()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Industrial2of5Symbology.Encode(null));
        }

        [TestMethod]
        public void EncodeEmptyContentProducesOnlyStartAndStop()
        {
            Assert.AreEqual(Start + Stop, BarPattern.ToText(Industrial2of5Symbology.Encode(string.Empty)));
        }

        [TestMethod]
        [DataRow("0")]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("3")]
        [DataRow("4")]
        [DataRow("5")]
        [DataRow("6")]
        [DataRow("7")]
        [DataRow("8")]
        [DataRow("9")]
        [DataRow("0123456789")]
        public void EncodeMatchesSpecification(string digits)
        {
            Assert.AreEqual(Expected(digits), BarPattern.ToText(Industrial2of5Symbology.Encode(digits)));
        }

        [TestMethod]
        public void EncodeDigitZeroLiteralPattern()
        {
            Assert.AreEqual("11011010" + "10101110111010" + "11010110", BarPattern.ToText(Industrial2of5Symbology.Encode("0")));
        }

        [TestMethod]
        [DataRow("12A4")]
        [DataRow("12 4")]
        [DataRow("-1")]
        public void EncodeNonDigitThrows(string content)
        {
            Assert.ThrowsExactly<ArgumentException>(() => Industrial2of5Symbology.Encode(content));
        }

        [TestMethod]
        public void EncodeNonAsciiDigitThrowsArgumentException()
        {
            // ARABIC-INDIC DIGIT THREE is a char.IsDigit() character but not encodable.
            Assert.ThrowsExactly<ArgumentException>(() => Industrial2of5Symbology.Encode("1٣"));
        }
    }
}
