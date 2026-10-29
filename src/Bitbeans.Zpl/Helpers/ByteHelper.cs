using System;
using System.Text;

namespace Bitbeans.Zpl.Helpers
{
    /// <summary>
    /// Conversion helpers for binary data
    /// </summary>
    public static class ByteHelper
    {
        /// <summary>
        /// Encode bytes as Base64
        /// </summary>
        public static string ToBase64(this byte[] bytes) { return Convert.ToBase64String(bytes); }

        /// <summary>
        /// Decode Base64 to bytes
        /// </summary>
        public static byte[] FromBase64(this string base64) { return Convert.FromBase64String(base64); }

        /// <summary>
        /// Encode bytes as upper case hexadecimal string
        /// </summary>
        public static string ToHexFromBytes(this byte[] bytes) { return BytesToHex(bytes); }

        /// <summary>
        /// Decode a hexadecimal string to bytes
        /// </summary>
        public static byte[] ToBytesFromHex(this string hex) { return HexToBytes(hex); }

        /// <summary>
        /// Encode a string as ASCII bytes
        /// </summary>
        public static byte[] EncodeBytes(this string hex) { return Encoding.ASCII.GetBytes(hex); }

        /// <summary>
        /// Decode a hexadecimal string to bytes, line breaks are ignored
        /// </summary>
        /// <param name="hex">Hexadecimal string</param>
        /// <returns>Decoded bytes</returns>
        /// <exception cref="FormatException">The string is not valid hexadecimal data</exception>
        public static byte[] HexToBytes(string hex)
        {
            if (hex.AsSpan().IndexOfAny('\r', '\n') != -1)
            {
                hex = hex.Replace("\n", string.Empty).Replace("\r", string.Empty);
            }

            return Convert.FromHexString(hex);
        }

        /// <summary>
        /// Encode bytes as upper case hexadecimal string
        /// </summary>
        /// <param name="bytes">Bytes</param>
        /// <returns>Hexadecimal string</returns>
        public static string BytesToHex(byte[] bytes)
        {
            return Convert.ToHexString(bytes);
        }
    }
}
