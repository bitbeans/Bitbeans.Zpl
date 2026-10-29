using System;
using System.Text;
using Bitbeans.Zpl.Viewer.Symologies;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    [TestClass]
    public class UpcExtensionSymbologyTest
    {
        // EAN/UPC digit encodings (GS1 General Specifications): L (odd parity) and G (even parity).
        private static readonly string[] LCodes =
        [
            "0001101", "0011001", "0010011", "0111101", "0100011",
            "0110001", "0101111", "0111011", "0110111", "0001011"
        ];

        private static readonly string[] GCodes =
        [
            "0100111", "0110011", "0011011", "0100001", "0011101",
            "0111001", "0000101", "0010001", "0001001", "0010111"
        ];

        // Parity pattern of the 5 digit supplement indexed by the weighted (3,9,3,9,3) checksum.
        private static readonly string[] Parity5 =
        [
            "GGLLL", "GLGLL", "GLLGL", "GLLLG", "LGGLL",
            "LLGGL", "LLLGG", "LGLGL", "LGLLG", "LLGLG"
        ];

        // Parity pattern of the 2 digit supplement indexed by value modulo 4.
        private static readonly string[] Parity2 = ["LL", "LG", "GL", "GG"];

        private static string Expected(string content)
        {
            string parity;
            if (content.Length == 2)
            {
                parity = Parity2[int.Parse(content) % 4];
            }
            else
            {
                int sum = 3 * Digit(content, 0) + 9 * Digit(content, 1) + 3 * Digit(content, 2) + 9 * Digit(content, 3) + 3 * Digit(content, 4);
                parity = Parity5[sum % 10];
            }

            StringBuilder builder = new("1011");
            for (int i = 0; i < content.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append("01");
                }

                builder.Append(parity[i] == 'L' ? LCodes[Digit(content, i)] : GCodes[Digit(content, i)]);
            }

            return builder.ToString();
        }

        private static int Digit(string content, int index)
        {
            return content[index] - '0';
        }

        [TestMethod]
        [DataRow("00")]
        [DataRow("01")]
        [DataRow("02")]
        [DataRow("03")]
        [DataRow("57")]
        [DataRow("99")]
        [DataRow("00000")]
        [DataRow("12345")]
        [DataRow("52495")]
        [DataRow("90000")]
        [DataRow("55999")]
        [DataRow("01234")]
        [DataRow("98765")]
        [DataRow("11111")]
        [DataRow("29301")]
        [DataRow("84756")]
        [DataRow("30189")]
        [DataRow("77777")]
        [DataRow("19088")]
        public void EncodeMatchesSpecification(string content)
        {
            Assert.AreEqual(Expected(content), BarPattern.ToText(UpcExtensionSymbology.Encode(content)));
        }

        [TestMethod]
        public void EncodeFiveDigitCoversEveryParityPattern()
        {
            // Checksum (3,9,3,9,3 weights) of "00000", "00010"... gives each of 0..9 once.
            for (int check = 0; check < 10; check++)
            {
                // last digit has weight 3: sum = 3 * d, so pick d such that (3 * d) % 10 == check.
                int digit = (check * 7) % 10;
                string content = "0000" + digit;
                Assert.AreEqual(Expected(content), BarPattern.ToText(UpcExtensionSymbology.Encode(content)));
            }
        }

        [TestMethod]
        public void EncodeNullThrows()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => UpcExtensionSymbology.Encode(null));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("1")]
        [DataRow("123")]
        [DataRow("1234")]
        [DataRow("123456")]
        public void EncodeWrongLengthThrows(string content)
        {
            Assert.ThrowsExactly<ArgumentException>(() => UpcExtensionSymbology.Encode(content));
        }

        [TestMethod]
        [DataRow("1A")]
        [DataRow("12A45")]
        [DataRow("-1")]
        [DataRow("+1")]
        [DataRow(" 1")]
        [DataRow("1.5  ")]
        [DataRow("1٣")]
        public void EncodeNonDigitThrows(string content)
        {
            Assert.ThrowsExactly<ArgumentException>(() => UpcExtensionSymbology.Encode(content));
        }
    }
}
