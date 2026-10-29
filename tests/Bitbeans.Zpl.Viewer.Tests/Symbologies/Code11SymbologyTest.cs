using System;
using System.Text;
using Bitbeans.Zpl.Viewer.Symologies;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    [TestClass]
    public class Code11SymbologyTest
    {
        // Code 11 symbol widths: 1 = wide element. Five elements (bar/space alternating, starting with a bar)
        // per character, '0'-'9' and '-'; start/stop is "nnwwn" (the character is the same as '*').
        private static readonly string[] Widths =
        [
            "00001", "10001", "01001", "11000", "00101",
            "10100", "01100", "00011", "10010", "10000", "00100"
        ];

        private const string Charset = "0123456789-";

        private static string Symbol(string widths)
        {
            StringBuilder builder = new();
            for (int i = 0; i < widths.Length; i++)
            {
                builder.Append(i % 2 == 0 ? '1' : '0', widths[i] == '1' ? 2 : 1);
            }

            return builder.ToString();
        }

        private static string Expected(string encodedContent)
        {
            StringBuilder builder = new(Symbol("00110"));
            foreach (char character in encodedContent)
            {
                builder.Append('0');
                builder.Append(Symbol(Widths[Charset.IndexOf(character)]));
            }

            builder.Append('0');
            builder.Append(Symbol("00110"));
            return builder.ToString();
        }

        private static char CheckDigit(string content, int maximumWeight)
        {
            int sum = 0;
            int weight = 1;
            for (int i = content.Length - 1; i >= 0; i--)
            {
                sum += Charset.IndexOf(content[i]) * weight;
                weight = weight == maximumWeight ? 1 : weight + 1;
            }

            return Charset[sum % 11];
        }

        [TestMethod]
        public void EncodeWithOneCheckDigitMatchesSpecification()
        {
            (bool[] data, string interpretation) = Code11Symbology.Encode("123-45", Code11CheckDigitCount.One);

            // Weights right to left: 5*1 + 4*2 + 10*3 + 3*4 + 2*5 + 1*6 = 71, 71 mod 11 = 5
            Assert.AreEqual("123-455", "123-45" + CheckDigit("123-45", 10));
            Assert.AreEqual(Expected("123-455"), BarPattern.ToText(data));
            Assert.AreEqual("▵123-455▵", interpretation);
        }

        [TestMethod]
        public void EncodeWithTwoCheckDigitsMatchesSpecification()
        {
            (bool[] data, string interpretation) = Code11Symbology.Encode("123-45", Code11CheckDigitCount.Two);

            // C = 5; K: 5*1 + 5*2 + 4*3 + 10*4 + 3*5 + 2*6 + 1*7 = 101, 101 mod 11 = 2
            Assert.AreEqual(Expected("123-4552"), BarPattern.ToText(data));
            Assert.AreEqual("▵123-4552△", interpretation);
        }

        [TestMethod]
        [DataRow("0", Code11CheckDigitCount.Two)]
        [DataRow("0", Code11CheckDigitCount.One)]
        [DataRow("-", Code11CheckDigitCount.One)]
        [DataRow("1234567890", Code11CheckDigitCount.One)]
        [DataRow("12345678901", Code11CheckDigitCount.One)]
        [DataRow("12345678901", Code11CheckDigitCount.Two)]
        [DataRow("123456789-0123456789-", Code11CheckDigitCount.Two)]
        [DataRow("99999999999999999999", Code11CheckDigitCount.Two)]
        public void EncodeAppendsCalculatedCheckDigits(string content, Code11CheckDigitCount count)
        {
            string expectedContent = content;
            char c = CheckDigit(content, 10);
            expectedContent += c;
            if (count == Code11CheckDigitCount.Two)
            {
                expectedContent += CheckDigit(content + c, 9);
            }

            (bool[] data, string interpretation) = Code11Symbology.Encode(content, count);

            Assert.AreEqual(Expected(expectedContent), BarPattern.ToText(data));
            Assert.AreEqual(
                Code11Symbology.StartStopInterpretationCharacter + expectedContent + (count == Code11CheckDigitCount.Two ? Code11Symbology.LargeStopInterpretationCharacter : Code11Symbology.StartStopInterpretationCharacter),
                interpretation);
        }

        [TestMethod]
        [DataRow("0123456789-")]
        public void EncodeEveryCharacterOfTheCharset(string content)
        {
            (bool[] data, _) = Code11Symbology.Encode(content, Code11CheckDigitCount.One);
            Assert.AreEqual(Expected(content + CheckDigit(content, 10)), BarPattern.ToText(data));
        }

        [TestMethod]
        public void EncodeNullThrows()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => Code11Symbology.Encode(null, Code11CheckDigitCount.One));
        }

        [TestMethod]
        public void EncodeEmptyThrows()
        {
            Assert.ThrowsExactly<ArgumentException>(() => Code11Symbology.Encode(string.Empty, Code11CheckDigitCount.One));
        }

        [TestMethod]
        [DataRow("12A")]
        [DataRow("1 2")]
        [DataRow("*")]
        public void EncodeNonEncodableCharacterThrows(string content)
        {
            Assert.ThrowsExactly<ArgumentException>(() => Code11Symbology.Encode(content, Code11CheckDigitCount.Two));
        }
    }
}
