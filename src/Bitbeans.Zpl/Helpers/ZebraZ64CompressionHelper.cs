using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Bitbeans.Zpl.Helpers
{
    /// <summary>
    /// Z64 Data Compression Scheme for ~DG and ~DB Commands First compresses the data using the LZ77 algorithm to
    /// reduce its size, then compressed data is then encoded using Base64 A CRC is calculated across the Base64-encoded
    /// data. If the CRC-check fails or the download is aborted, the object can be invalidated by the printer. reduces
    /// the actual number of data bytes and the amount of time required to download graphic images and bitmapped fonts
    /// with the ~DG and ~DB commands
    /// </summary>
    public static class ZebraZ64CompressionHelper
    {
        private static readonly Regex _z64Regex = new Regex(":(Z64):(\\S+):([0-9a-fA-F]+)", RegexOptions.Compiled);

        /// <summary>
        /// Compress hexadecimal data to the Z64 format
        /// </summary>
        /// <param name="hexData">Hexadecimal data, line breaks are ignored</param>
        /// <returns>Z64 encoded data including the CRC</returns>
        public static string Compress(string hexData)
        {
            var cleanedHexData = hexData.Replace("\n", string.Empty).Replace("\r", string.Empty);
            return Compress(cleanedHexData.ToBytesFromHex());
        }

        /// <summary>
        /// Compress binary data to the Z64 format
        /// </summary>
        /// <param name="bytes">Binary data</param>
        /// <returns>Z64 encoded data including the CRC</returns>
        public static string Compress(byte[] bytes)
        {
            var data = Deflate(bytes);
            var base64 = data.ToBase64();
            return ":Z64:" + base64 + ":" + Crc16.ComputeHex(base64.EncodeBytes());
        }

        /// <summary>
        /// Uncompress Z64 encoded data
        /// </summary>
        /// <param name="hexData">Z64 encoded data (<c>:Z64:data:crc</c>)</param>
        /// <returns>Binary data</returns>
        /// <exception cref="FormatException">The data is not in the Z64 format</exception>
        public static byte[] Uncompress(string hexData)
        {
            var match = _z64Regex.Match(hexData);
            if (!match.Success)
            {
                throw new FormatException("Hex string not in Z64 format");
            }

            var imageBase64 = match.Groups[2].Value;
            return Inflate(imageBase64.FromBase64());
        }

        /// <summary>
        /// Compress data with ZLib headers
        /// </summary>
        /// <param name="data">Binary data</param>
        /// <param name="compressionLevel">Compression level</param>
        /// <returns>ZLib compressed data</returns>
        internal static byte[] Deflate(byte[] data, CompressionLevel compressionLevel = CompressionLevel.SmallestSize)
        {
            using var ms = new MemoryStream();
            using (var compressor = new ZLibStream(ms, compressionLevel))
            {
                compressor.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Decompress data with ZLib headers
        /// </summary>
        /// <param name="data">ZLib compressed data</param>
        /// <returns>Binary data</returns>
        internal static byte[] Inflate(byte[] data)
        {
            using var outputStream = new MemoryStream();
            using var inputStream = new MemoryStream(data);
            using var decompressor = new ZLibStream(inputStream, CompressionMode.Decompress);
            decompressor.CopyTo(outputStream);
            return outputStream.ToArray();
        }
    }
}
