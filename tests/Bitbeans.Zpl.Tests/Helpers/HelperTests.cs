using Bitbeans.Zpl.Helpers;
using System;
using System.Text;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class HelperTests
    {
        // ---- ByteHelper ----

        [TestMethod]
        public void HexRoundTrip()
        {
            var bytes = new byte[] { 0x00, 0x0F, 0xA5, 0xFF };

            Assert.AreEqual("000FA5FF", bytes.ToHexFromBytes());
            Assert.AreEqual("000FA5FF", ByteHelper.BytesToHex(bytes));
            CollectionAssert.AreEqual(bytes, "000fa5ff".ToBytesFromHex());
            CollectionAssert.AreEqual(bytes, ByteHelper.HexToBytes("000FA5FF"));
        }

        [TestMethod]
        public void HexToBytesIgnoresLineBreaks()
        {
            CollectionAssert.AreEqual(new byte[] { 0x01, 0x23, 0x45, 0x67 }, ByteHelper.HexToBytes("0123\n45\r\n67\r"));
        }

        [TestMethod]
        public void HexToBytesRejectsInvalidHex()
        {
            Assert.ThrowsExactly<FormatException>(() => ByteHelper.HexToBytes("ZZ"));
            Assert.ThrowsExactly<FormatException>(() => ByteHelper.HexToBytes("ABC"));
        }

        [TestMethod]
        public void Base64RoundTrip()
        {
            var bytes = new byte[] { 0xFF, 0x00, 0xFE };

            Assert.AreEqual("/wD+", bytes.ToBase64());
            CollectionAssert.AreEqual(bytes, "/wD+".FromBase64());
            Assert.ThrowsExactly<FormatException>(() => "***".FromBase64());
        }

        [TestMethod]
        public void EncodeBytesUsesAscii()
        {
            CollectionAssert.AreEqual(new byte[] { 0x41, 0x31 }, "A1".EncodeBytes());
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("abc"), "abc".EncodeBytes());
        }

        // ---- Crc16 ----

        [TestMethod]
        public void Crc16MatchesXmodemCheckValue()
        {
            var bytes = Encoding.ASCII.GetBytes("123456789");

            Assert.AreEqual((ushort)0x31C3, Crc16.Compute(bytes));
            Assert.AreEqual("31c3", Crc16.ComputeHex(bytes));
            Assert.AreEqual("0000", Crc16.ComputeHex([]));
        }

        // ---- B64 / Z64 ----

        [TestMethod]
        public void B64CompressFormat()
        {
            var compressed = ZebraB64CompressionHelper.Compress(new byte[] { 0xFF, 0x00, 0xFE });

            Assert.AreEqual(":B64:/wD+:" + Crc16.ComputeHex(Encoding.ASCII.GetBytes("/wD+")), compressed);
            Assert.AreEqual(compressed, ZebraB64CompressionHelper.Compress("FF00\r\nFE"));
            CollectionAssert.AreEqual(new byte[] { 0xFF, 0x00, 0xFE }, ZebraB64CompressionHelper.Uncompress(compressed));
        }

        [TestMethod]
        public void B64UncompressRejectsOtherFormats()
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress(":Z64:/wD+:0000"));
            Assert.ThrowsExactly<FormatException>(() => ZebraB64CompressionHelper.Uncompress("FF00"));
        }

        [TestMethod]
        public void Z64CompressFormat()
        {
            var data = new byte[200];
            var compressed = ZebraZ64CompressionHelper.Compress(data);

            StringAssert.StartsWith(compressed, ":Z64:");
            Assert.IsLessThan(data.Length, compressed.Length);
            CollectionAssert.AreEqual(data, ZebraZ64CompressionHelper.Uncompress(compressed));
            Assert.AreEqual(
                ZebraZ64CompressionHelper.Compress(new byte[] { 0x01, 0x02 }),
                ZebraZ64CompressionHelper.Compress("01\n02"));
        }

        [TestMethod]
        public void Z64UncompressRejectsOtherFormats()
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress(":B64:/wD+:0000"));
        }

        // ---- ACS ----

        [TestMethod]
        public void AcsCompressRunsOfZerosAndRepeatedLines()
        {
            // rows: FF00 / 0000 / FF00 / FF00
            Assert.AreEqual("HF,,HF,:", ZebraACSCompressionHelper.Compress("FF000000FF00FF00", 2));
        }

        [TestMethod]
        public void AcsCompressRunsOfOnes()
        {
            // "!" fills the rest of the line with one bits (hex F)
            Assert.AreEqual("!H0!", ZebraACSCompressionHelper.Compress("FFFF" + "00FF", 2));
            Assert.AreEqual("!", ZebraACSCompressionHelper.Compress("FFFF", 2));
            Assert.AreEqual("J1", ZebraACSCompressionHelper.Compress("1111", 2));
        }

        [TestMethod]
        public void AcsCompressIgnoresLineBreaks()
        {
            Assert.AreEqual("HF,,HF,:", ZebraACSCompressionHelper.Compress("FF00\r\n0000\nFF00\nFF00\n", 2));
        }

        [TestMethod]
        public void AcsUncompressExpandsFillRepeatAndRuns()
        {
            Assert.AreEqual("FF00\n0000\nFF00\nFF00\n", ZebraACSCompressionHelper.Uncompress("HF,,HF,:", 2));
            Assert.AreEqual("FFFF\n00FF\n", ZebraACSCompressionHelper.Uncompress("!H0!", 2));
            Assert.AreEqual("1FFF\n", ZebraACSCompressionHelper.Uncompress("1!", 2));
            Assert.AreEqual("AB00\n", ZebraACSCompressionHelper.Uncompress("AB,", 2));
            Assert.AreEqual("A1B1\n", ZebraACSCompressionHelper.Uncompress("A1B1", 2));
        }

        [TestMethod]
        public void AcsRoundTripOfLongRuns()
        {
            // a 300 byte wide row of FF, 0x00 and a mixed value exercises runs above the 400 character limit
            var row = new string('F', 600) + new string('0', 300) + "A1" + new string('F', 98);
            var hex = row + "\n" + row + "\n" + new string('0', 1000);

            var compressed = ZebraACSCompressionHelper.Compress(hex, 500);
            Assert.AreEqual(hex.Replace("\n", string.Empty), ZebraACSCompressionHelper.Uncompress(compressed, 500).Replace("\n", string.Empty));
        }

        [TestMethod]
        [DataRow(1, "")]
        [DataRow(21, "gG")]
        [DataRow(41, "hG")]
        [DataRow(419, "zY")]
        [DataRow(420, "zg")]
        [DataRow(421, "zgG")]
        [DataRow(440, "zh")]
        [DataRow(441, "zhG")]
        [DataRow(800, "zz")]
        [DataRow(801, "zzG")]
        [DataRow(1000, "zzp")]
        [DataRow(1019, "zzpY")]
        public void AcsCharCount(int count, string expected)
        {
            Assert.AreEqual(expected, ZebraACSCompressionHelper.GetZebraCharCount(count));
        }
    }
}
