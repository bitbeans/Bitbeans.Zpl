using Bitbeans.Zpl.Helpers;
using System;
using System.IO.Compression;

namespace Bitbeans.Zpl.Tests
{
    [TestClass]
    public class ZebraZ64CompressionHelperTest
    {
        [TestMethod]
        public void CompressUncompress_Flow_Successful()
        {
            var originalData = "FFFFFFFFFFFFFFFFFFFF8000FFFF0000FFFF00018000FFFF0000FFFF00018000FFFF0000FFFF0001FFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFFFFFFFFFFFFFFFFFF";

            string compressed = ZebraZ64CompressionHelper.Compress(originalData);
            string uncompressed = ZebraZ64CompressionHelper.Uncompress(compressed).ToHexFromBytes();
            Assert.AreEqual(originalData, uncompressed);
        }
        [TestMethod]
        public void Compress_ValidData1_Successful()
        {
            var compressed = ZebraZ64CompressionHelper.Compress("FFFFFFFFFFFFFFFFFFFF8000FFFF0000FFFF00018000FFFF0000FFFF00018000FFFF0000FFFF0001FFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFFFFFFFFFFFFFFFFFF");
            Assert.AreEqual(":Z64:eNr7/x8GGhj+/2cAYUZsLCjNAFKJjQUDAOLDM1I=:8f39", compressed);
            //^XA
            //~DGR:SAMPLE.GRF,00006,02,JFGFH0GFJF
            //^FO20,20
            //^XGR:SAMPLE.GRF,1,1^FS
            //^XZ
        }
        [TestMethod]
        [DataRow(CompressionLevel.Optimal)]
        [DataRow(CompressionLevel.Fastest)]
        [DataRow(CompressionLevel.SmallestSize)]
        [DataRow(CompressionLevel.NoCompression)]
        public void DeflateInflate_RoundTrip_Successful(CompressionLevel compressionLevel)
        {
            var originalData = "FFFFFFFFFFFFFFFFFFFF8000FFFF0000FFFF00018000FFFF0000FFFF00018000FFFF0000FFFF0001FFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFFFFFFFFFFFFFFFFFF";

            byte[] compressed = ZebraZ64CompressionHelper.Deflate(originalData.ToBytesFromHex(), compressionLevel);

            Assert.AreEqual(0x78, compressed[0], "ZLib header expected");
            Assert.AreEqual(originalData, ZebraZ64CompressionHelper.Inflate(compressed).ToHexFromBytes());
        }

        [TestMethod]
        public void Inflate_KnownData_Successful()
        {
            var compressed = "eJz7/x8GGhj+/2cAYUZsLCgNxNhZMAAA4sMzUg==";

            string uncompressed = ZebraZ64CompressionHelper.Inflate(compressed.FromBase64()).ToHexFromBytes();

            Assert.AreEqual("FFFFFFFFFFFFFFFFFFFF8000FFFF0000FFFF00018000FFFF0000FFFF00018000FFFF0000FFFF0001FFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFF0000FFFF0000FFFFFFFFFFFFFFFFFFFFFFFF", uncompressed);
        }

        [TestMethod]
        public void Uncompress_InvalidFormat_Throws()
        {
            Assert.ThrowsExactly<FormatException>(() => ZebraZ64CompressionHelper.Uncompress("FFFF"));
        }
    }
}
