using System;
using System.Linq;
using Bitbeans.Zpl.Viewer.Symologies;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    [TestClass]
    public class MaxiCodeSymbologyTest
    {
        private const string Gs = "\u001d";
        private const string Rs = "\u001e";

        private static readonly int[] OrientationModules = [28, 29, 276, 277, 306, 450, 480, 492, 522, 659, 689, 666, 696];

        private static string RoundTrip(string content, int mode)
        {
            return MaxiCodeDecoder.DecodeText(MaxiCodeSymbology.Encode(content, mode), mode);
        }

        [TestMethod]
        public void SymbolHasFixedSizeAndOrientationModules()
        {
            bool[] modules = MaxiCodeSymbology.Encode("Hello", 4);

            Assert.AreEqual(974, modules.Length);
            foreach (int index in OrientationModules)
            {
                Assert.IsTrue(modules[index], "orientation module " + index);
            }
        }

        [TestMethod]
        public void EncodeIsDeterministic()
        {
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode("Hello", 4), MaxiCodeSymbology.Encode("Hello", 4));
        }

        // ---- modes 4 and 5 (standard and enhanced error correction) ----

        [TestMethod]
        [DataRow("Hello", 4)]
        [DataRow("HELLO WORLD", 4)]
        [DataRow("hello world", 4)]
        [DataRow("Hello World 0123456789", 4)]
        [DataRow("A", 4)]
        [DataRow("Hello", 5)]
        [DataRow("HELLO WORLD", 5)]
        [DataRow("hello world", 5)]
        [DataRow("1Z999AA10123456784", 5)]
        public void RoundTripsThroughZXing(string content, int mode)
        {
            Assert.AreEqual(content, RoundTrip(content, mode));
        }

        [TestMethod]
        public void EmptyContent()
        {
            Assert.AreEqual(string.Empty, RoundTrip(string.Empty, 4));
            Assert.AreEqual(string.Empty, RoundTrip(string.Empty, 5));
        }

        [TestMethod]
        [DataRow("ABCDEFGH")]
        [DataRow("ABCDEFGHI")]
        [DataRow("ABCDEFGHIJ")]
        [DataRow("ABCDEFGHéxyz")]
        [DataRow("ABCDEFGé")]
        [DataRow("ABCD123456789")]
        [DataRow("ABCDEFGH123456789")]
        [DataRow("ABCDEFGHabcdefghijklmn")]
        public void PrimaryMessageIsFollowedByItsErrorCorrection(string content)
        {
            // Codewords may be appended in groups (shifts, numeric shift) that cross the primary message boundary.
            Assert.AreEqual(content, RoundTrip(content, 4));
            Assert.AreEqual(content, RoundTrip(content, 5));
        }

        [TestMethod]
        [DataRow("123456789")]
        [DataRow("12345678")]
        [DataRow("1234567890123456789")]
        [DataRow("00000000012345678")]
        [DataRow("999999999")]
        [DataRow("ABC123456789DEF")]
        [DataRow("abc123456789def")]
        public void NumericRuns(string content)
        {
            Assert.AreEqual(content, RoundTrip(content, 4));
        }

        [TestMethod]
        [DataRow("abc")]
        [DataRow("a")]
        [DataRow("a :")]
        [DataRow("aB")]
        [DataRow("abcDEFG")]
        [DataRow("abcDEFb")]
        [DataRow("abcDEb")]
        [DataRow("abcDb")]
        [DataRow("abcD")]
        [DataRow("abcDE")]
        [DataRow("abcDEF")]
        [DataRow("abcDEFGHIJ klm")]
        [DataRow("Ab")]
        [DataRow("Abc")]
        [DataRow("Abc def GHI jkl MNO")]
        [DataRow("aB cD eF")]
        [DataRow("`{}~\u007f;<=>?[\\]^_,./:@!|")]
        [DataRow("abc\r")]
        [DataRow("abc\r\u001c\u001d\u001e")]
        [DataRow("abc12345 678")]
        public void CodeSetSwitching(string content)
        {
            Assert.AreEqual(content, RoundTrip(content, 4));
        }

        [TestMethod]
        [DataRow("Hello\r\nWorld")]
        [DataRow("Line1\nLine2")]
        [DataRow("\r\n")]
        public void LineBreaksAreEncoded(string content)
        {
            Assert.AreEqual(content, RoundTrip(content, 4));
        }

        [TestMethod]
        [DataRow("À")]
        [DataRow("à")]
        [DataRow("\u0001")]
        [DataRow("©")]
        [DataRow("éüßÄ °£µ")]
        [DataRow("abcÀdef")]
        [DataRow("ABCàDEF")]
        [DataRow("ABC\u0001DEF")]
        public void Iso88591Characters(string content)
        {
            Assert.AreEqual(content, RoundTrip(content, 4));
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode(content, 4), MaxiCodeSymbology.EncodeExact(content, 4));
        }

        [TestMethod]
        public void StructuredAppendStyleMessage()
        {
            string content = "[)>" + Rs + "01" + Gs + "96" + "1Z12345" + Gs + "UPSN" + Gs + "12345E" + Gs + "089" + Rs + "\u0004";
            Assert.AreEqual(content, RoundTrip(content, 4));
        }

        [TestMethod]
        public void UnencodableCharactersAreDropped()
        {
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode("ABCD", 4), MaxiCodeSymbology.Encode("AB€\U0001F600CD", 4));
        }

        [TestMethod]
        public void ExactEncodingRejectsCharactersOutsideIso88591()
        {
            ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => MaxiCodeSymbology.EncodeExact("AB€CD", 4));
            Assert.AreEqual("content", exception.ParamName);
        }

        // ---- capacity ----

        [TestMethod]
        public void Mode4CapacityIs93Characters()
        {
            string full = new('A', 93);
            Assert.AreEqual(full, RoundTrip(full, 4));
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode(full, 4), MaxiCodeSymbology.EncodeExact(full, 4));
        }

        [TestMethod]
        public void Mode4OverflowIsTruncated()
        {
            string tooLong = new string('A', 93) + "BCDEF";
            Assert.AreEqual(new string('A', 93), RoundTrip(tooLong, 4));
        }

        [TestMethod]
        public void Mode4OverflowThrowsForExactEncoding()
        {
            ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => MaxiCodeSymbology.EncodeExact(new string('A', 94), 4));
            Assert.AreEqual("content", exception.ParamName);
            StringAssert.Contains(exception.Message, "capacity");
        }

        [TestMethod]
        public void Mode5CapacityIs77Characters()
        {
            string full = new('A', 77);
            Assert.AreEqual(full, RoundTrip(full, 5));
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode(full, 5), MaxiCodeSymbology.EncodeExact(full, 5));
        }

        [TestMethod]
        public void Mode5OverflowFallsBackToMode4()
        {
            string content = new('A', 80);
            Assert.AreEqual(content, RoundTrip(content, 4 /* result mode */));
            Assert.AreEqual(content, MaxiCodeDecoder.DecodeText(MaxiCodeSymbology.Encode(content, 5), 4));
        }

        [TestMethod]
        public void Mode5OverflowBeyondMode4CapacityIsTruncated()
        {
            string content = new string('A', 93) + "BCDEFGH";
            Assert.AreEqual(new string('A', 93), MaxiCodeDecoder.DecodeText(MaxiCodeSymbology.Encode(content, 5), 4));
        }

        [TestMethod]
        public void Mode5OverflowThrowsForExactEncoding()
        {
            ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => MaxiCodeSymbology.EncodeExact(new string('A', 78), 5));
            Assert.AreEqual("content", exception.ParamName);
            StringAssert.Contains(exception.Message, "mode 5");
        }

        [TestMethod]
        public void Mode6IsEncodedLikeMode4ExceptForTheModeCodeword()
        {
            byte[] mode4 = MaxiCodeDecoder.ReadCodewords(MaxiCodeSymbology.Encode("Hello", 4));
            byte[] mode6 = MaxiCodeDecoder.ReadCodewords(MaxiCodeSymbology.Encode("Hello", 6));

            Assert.AreEqual(4, mode4[0] & 0x0F);
            Assert.AreEqual(6, mode6[0] & 0x0F);
            CollectionAssert.AreEqual(mode4.Skip(1).Take(9).ToArray(), mode6.Skip(1).Take(9).ToArray());
            CollectionAssert.AreEqual(mode4.Skip(20).ToArray(), mode6.Skip(20).ToArray());
        }

        // ---- modes 2 and 3 (structured carrier message) ----

        [TestMethod]
        public void Mode2WithUsCountryPadsPostalCode()
        {
            Assert.AreEqual("123000000" + Gs + "840" + Gs + "001" + Gs + "Hello", RoundTrip("001840123Hello", 2));
        }

        [TestMethod]
        public void Mode2WithNineDigitPostalCode()
        {
            Assert.AreEqual("123456789" + Gs + "840" + Gs + "001" + Gs + "Hello World", RoundTrip("001840123456789Hello World", 2));
        }

        [TestMethod]
        public void Mode2WithOtherCountryKeepsPostalCodeLength()
        {
            Assert.AreEqual("1234" + Gs + "124" + Gs + "066" + Gs + "Hello", RoundTrip("0661241234Hello", 2));
        }

        [TestMethod]
        public void Mode2WithoutMessage()
        {
            Assert.AreEqual("123456789" + Gs + "840" + Gs + "001" + Gs, RoundTrip("001840123456789", 2));
        }

        [TestMethod]
        public void Mode2WithLongMessage()
        {
            string message = new string('X', 60) + "123456789" + "abc";
            Assert.AreEqual("12345" + Gs + "276" + Gs + "999" + Gs + message, RoundTrip("99927612345" + message, 2));
        }

        [TestMethod]
        public void Mode2WithGsDelimitedPrimaryMessage()
        {
            string content = "[)>" + Rs + "01" + Gs + "96" + "123456789" + Gs + "840" + Gs + "001" + Gs + "1Z12345E" + Rs + "\u0004";
            Assert.AreEqual(content, RoundTrip(content, 2));
        }

        [TestMethod]
        public void Mode3WithAlphanumericPostalCode()
        {
            Assert.AreEqual("AB12CD" + Gs + "826" + Gs + "003" + Gs + "Hello", RoundTrip("003826AB12CDHello", 3));
        }

        [TestMethod]
        public void Mode3WithShortPostalCodeIsPadded()
        {
            string text = RoundTrip("003826AB1 hello", 3);
            StringAssert.StartsWith(text, "AB1");
            StringAssert.EndsWith(text, Gs + "826" + Gs + "003" + Gs + "hello");
        }

        [TestMethod]
        public void Mode3WithNumericPostalCode()
        {
            Assert.AreEqual("123456" + Gs + "840" + Gs + "001" + Gs + "789Hello World", RoundTrip("001840123456789Hello World", 3));
        }

        [TestMethod]
        public void Mode3WithoutMessage()
        {
            Assert.AreEqual("AB12CD" + Gs + "826" + Gs + "003" + Gs, RoundTrip("003826AB12CD", 3));
        }

        [TestMethod]
        public void Mode3WithGsDelimitedPrimaryMessage()
        {
            string content = "[)>" + Rs + "01" + Gs + "96" + "AB12CD" + Gs + "826" + Gs + "003" + Gs + "1Z12345E" + Rs + "\u0004";
            Assert.AreEqual(content, RoundTrip(content, 3));
        }

        [TestMethod]
        public void Mode3WithGsDelimitedShortPostalCode()
        {
            string content = "[)>" + Rs + "01" + Gs + "96" + "AB1" + Gs + "826" + Gs + "003" + Gs + "Hi";
            string text = RoundTrip(content, 3);
            StringAssert.Contains(text, "AB1");
            StringAssert.EndsWith(text, Gs + "826" + Gs + "003" + Gs + "Hi");
        }

        [TestMethod]
        [DataRow("ABC", 2)]
        [DataRow("001840", 2)]
        [DataRow("00184x123", 2)]
        [DataRow("ABC", 3)]
        [DataRow("001826ab12Hello", 3)]
        [DataRow("001826", 3)]
        public void InvalidPrimaryMessageFallsBackToMode4(string content, int mode)
        {
            Assert.AreEqual(content, MaxiCodeDecoder.DecodeText(MaxiCodeSymbology.Encode(content, mode), 4));
        }

        [TestMethod]
        [DataRow("ABC", 2)]
        [DataRow("001840", 2)]
        [DataRow("ABC", 3)]
        [DataRow("001826ab12Hello", 3)]
        public void InvalidPrimaryMessageThrowsForExactEncoding(string content, int mode)
        {
            ArgumentException exception = Assert.ThrowsExactly<ArgumentException>(() => MaxiCodeSymbology.EncodeExact(content, mode));
            Assert.AreEqual("content", exception.ParamName);
            StringAssert.Contains(exception.Message, "mode " + mode);
        }

        [TestMethod]
        public void ExactEncodingMatchesEncodeForValidContent()
        {
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode("001840123456789Hello", 2), MaxiCodeSymbology.EncodeExact("001840123456789Hello", 2));
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode("003826AB12CDHello", 3), MaxiCodeSymbology.EncodeExact("003826AB12CDHello", 3));
            CollectionAssert.AreEqual(MaxiCodeSymbology.Encode("Hello", 5), MaxiCodeSymbology.EncodeExact("Hello", 5));
        }
    }
}
