using System;
using System.Reflection;
using ZXing.Common;
using ZXing.Maxicode.Internal;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    /// <summary>
    /// Decodes MaxiCode module arrays with the ZXing decoder (independent reference implementation).
    /// </summary>
    internal static class MaxiCodeDecoder
    {
        public const int Columns = 30;
        public const int Rows = 33;

        /// <summary>
        /// Converts the 974 module array (30 modules in even rows, 29 modules in odd rows) to a bit matrix.
        /// </summary>
        public static BitMatrix ToBitMatrix(bool[] modules)
        {
            BitMatrix matrix = new(Columns, Rows);
            int index = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns - row % 2; column++)
                {
                    matrix[column, row] = modules[index++];
                }
            }

            Assert.AreEqual(modules.Length, index);
            return matrix;
        }

        /// <summary>
        /// Decodes the symbol (including error correction), returning null on failure.
        /// </summary>
        public static DecoderResult Decode(bool[] modules)
        {
            return new Decoder().decode(ToBitMatrix(modules));
        }

        /// <summary>
        /// Reads the 144 raw codewords (without error correction).
        /// </summary>
        public static byte[] ReadCodewords(bool[] modules)
        {
            // BitMatrixParser is internal to ZXing.
            Type parserType = typeof(Decoder).Assembly.GetType("ZXing.Maxicode.Internal.BitMatrixParser", throwOnError: true);
            object parser = Activator.CreateInstance(parserType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, [ToBitMatrix(modules)], null);
            return (byte[])parserType.GetMethod("readCodewords", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(parser, null);
        }

        /// <summary>
        /// Decodes the text and verifies mode, error correction level and success.
        /// </summary>
        public static string DecodeText(bool[] modules, int expectedMode)
        {
            DecoderResult result = Decode(modules);
            Assert.IsNotNull(result, "ZXing could not decode the MaxiCode symbol");
            Assert.AreEqual(expectedMode.ToString(), result.ECLevel);
            return result.Text;
        }
    }
}
