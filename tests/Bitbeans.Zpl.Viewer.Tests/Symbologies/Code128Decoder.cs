using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Bitbeans.Zpl.Viewer.Symologies;
using ZXing;
using ZXing.Common;
using ZXing.OneD;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    /// <summary>
    /// Independent verification helpers for Code 128 module patterns.
    /// </summary>
    internal static class Code128Decoder
    {
        private const string StopPattern = "1100011101011";

        private static readonly Dictionary<string, int> SymbolValues = LoadSymbolValues();

        private static Dictionary<string, int> LoadSymbolValues()
        {
            FieldInfo field = typeof(ZplCode128Symbology).GetField("patterns", BindingFlags.NonPublic | BindingFlags.Static);
            int[] patterns = (int[])field.GetValue(null);
            Dictionary<string, int> result = new();
            for (int value = 0; value < 106; value++)
            {
                result.Add(Convert.ToString(patterns[value], 2), value);
            }

            return result;
        }

        /// <summary>
        /// Splits modules into 11 module symbols and returns the symbol values (start, data, check),
        /// verifying the stop pattern and the checksum on the way.
        /// </summary>
        public static int[] Symbols(bool[] modules)
        {
            string text = BarPattern.ToText(modules);
            Assert.IsTrue(text.EndsWith(StopPattern, StringComparison.Ordinal), "stop pattern missing");
            string body = text.Substring(0, text.Length - StopPattern.Length);
            Assert.AreEqual(0, body.Length % 11, "body must consist of 11 module symbols");

            List<int> values = [];
            for (int i = 0; i < body.Length; i += 11)
            {
                Assert.IsTrue(SymbolValues.TryGetValue(body.Substring(i, 11), out int value), "unknown symbol at " + i);
                values.Add(value);
            }

            int[] all = values.ToArray();
            int checksum = all[0];
            for (int i = 1; i < all.Length - 1; i++)
            {
                checksum += i * all[i];
            }

            Assert.AreEqual(checksum % 103, all[^1], "checksum");
            return all;
        }

        /// <summary>
        /// Returns start, data (without checksum) values.
        /// </summary>
        public static int[] Data(bool[] modules)
        {
            int[] all = Symbols(modules);
            return all.Take(all.Length - 1).ToArray();
        }

        /// <summary>
        /// Decodes the pattern with the ZXing Code 128 reader (as an independent implementation).
        /// </summary>
        public static string ZXingText(bool[] modules, bool gs1 = false)
        {
            BitArray row = new(modules.Length + 20);
            for (int i = 0; i < modules.Length; i++)
            {
                row[i + 10] = modules[i];
            }

            Dictionary<DecodeHintType, object> hints = new();
            if (gs1)
            {
                hints[DecodeHintType.ASSUME_GS1] = true;
            }

            Result result = new Code128Reader().decodeRow(0, row, hints);
            Assert.IsNotNull(result, "ZXing could not decode the barcode");
            return result.Text;
        }
    }
}
