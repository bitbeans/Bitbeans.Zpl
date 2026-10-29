using System;
using System.Text.RegularExpressions;

namespace Bitbeans.Zpl.Helpers
{
    /// <summary>Parsing of the <c>:B64:data:crc</c> / <c>:Z64:data:crc</c> container format shared by <see cref="ZebraB64CompressionHelper"/> and <see cref="ZebraZ64CompressionHelper"/>.</summary>
    internal static class ZebraBase64Format
    {
        private static readonly Regex _regex = new Regex(
            "^:(B64|Z64):([A-Za-z0-9+/]*={0,2}):([0-9a-fA-F]{4})$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>Extracts the Base64 text of the container and verifies its CRC.</summary>
        /// <param name="data">The complete data, e.g. <c>:B64:AAA=:1234</c>. Leading/trailing white space and line breaks are ignored.</param>
        /// <param name="format">The expected format tag, <c>B64</c> or <c>Z64</c>.</param>
        /// <returns>The Base64 text.</returns>
        /// <exception cref="FormatException">The data does not match the format completely, or the CRC (CRC-16 over the Base64 text) does not match.</exception>
        internal static string ReadVerifiedBase64(string data, string format)
        {
            var match = _regex.Match(data.Trim().Replace("\r", string.Empty).Replace("\n", string.Empty));
            if (!match.Success || match.Groups[1].Value != format)
            {
                throw new FormatException($"Data not in {format} format");
            }

            string base64 = match.Groups[2].Value;
            string expectedCrc = Crc16.ComputeHex(base64.EncodeBytes());
            if (!string.Equals(expectedCrc, match.Groups[3].Value, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"CRC of {format} data does not match");
            }

            return base64;
        }
    }
}
