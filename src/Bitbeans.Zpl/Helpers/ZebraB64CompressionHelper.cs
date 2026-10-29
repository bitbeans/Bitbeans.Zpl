using System;

namespace Bitbeans.Zpl.Helpers
{
    /// <summary>
    /// B64 Data Compression Scheme for ~DG and ~DB Commands Raw data is encoded using Base64 A CRC is calculated across
    /// the Base64-encoded data. If the CRC-check fails or the download is aborted, the object can be invalidated by the
    /// printer. This scheme is not very efficient and only added for compatibility reasons.
    /// </summary>
    public static class ZebraB64CompressionHelper
    {
        /// <summary>Encodes hexadecimal data in the B64 format (Base64 plus CRC).</summary>
        /// <param name="hexData">Hexadecimal data; line breaks are ignored.</param>
        /// <returns>B64 encoded data (<c>:B64:data:crc</c>).</returns>
        public static string Compress(string hexData)
        {
            var cleanedHexData = hexData.Replace("\n", string.Empty).Replace("\r", string.Empty);
            return Compress(cleanedHexData.ToBytesFromHex());
        }

        /// <summary>Encodes binary data in the B64 format (Base64 plus CRC, no compression).</summary>
        /// <param name="bytes">Binary data.</param>
        /// <returns>B64 encoded data (<c>:B64:data:crc</c>).</returns>
        public static string Compress(byte[] bytes)
        {
            var base64 = Convert.ToBase64String(bytes);
            var crc16 = Crc16.ComputeHex(base64.EncodeBytes());
            return ":B64:" + base64 + ":" + crc16;
        }

        /// <summary>Decodes B64 encoded data and verifies the CRC (CRC-16 over the Base64 text).</summary>
        /// <param name="hexData">B64 encoded data (<c>:B64:data:crc</c>); the whole input must match the format.</param>
        /// <returns>Binary data.</returns>
        /// <exception cref="FormatException">The data is not completely in the B64 format, is not valid Base64, or the CRC does not match.</exception>
        public static byte[] Uncompress(string hexData)
        {
            return ZebraBase64Format.ReadVerifiedBase64(hexData, "B64").FromBase64();
        }
    }
}
