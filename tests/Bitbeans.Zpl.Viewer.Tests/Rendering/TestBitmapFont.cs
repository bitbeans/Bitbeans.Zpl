using Bitbeans.Zpl.Viewer.BitmapFonts;

using System.Collections.Generic;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    /// <summary>
    /// A bitmap font provider for tests: every printable ASCII glyph is an outlined box (the space is empty).
    /// </summary>
    internal sealed class TestBitmapFontProvider : IZplBitmapFontProvider
    {
        public const int MatrixWidth = 5;
        public const int MatrixHeight = 9;
        public const int Gap = 1;
        public const int Baseline = 7;

        private readonly Dictionary<string, ZplBitmapFontData> fonts = new();

        public List<string> Requests { get; } = [];

        public TestBitmapFontProvider(params string[] fontNames)
        {
            foreach (string name in fontNames)
            {
                this.fonts[name] = CreateFont(name);
            }
        }

        public static ZplBitmapFontData CreateFont(string name)
        {
            ZplBitmapFontMetrics metrics = new(name, MatrixWidth, MatrixHeight, Gap, Baseline);
            Dictionary<int, ZplBitmapGlyph> glyphs = new();
            for (int codePoint = 32; codePoint < 127; codePoint++)
            {
                bool[] pixels = new bool[MatrixWidth * MatrixHeight];
                if (codePoint != ' ')
                {
                    for (int row = 0; row < MatrixHeight; row++)
                    {
                        for (int col = 0; col < MatrixWidth; col++)
                        {
                            pixels[row * MatrixWidth + col] = col == 0 || col == MatrixWidth - 1 || row == 0 || row == MatrixHeight - 1;
                        }
                    }
                }

                glyphs[codePoint] = new ZplBitmapGlyph(codePoint, MatrixWidth, MatrixHeight, pixels);
            }

            return new ZplBitmapFontData(metrics, glyphs);
        }

        public bool TryGet(string fontName, int printDensityDpmm, InternationalFont internationalFont, out ZplBitmapFontData font)
        {
            this.Requests.Add(fontName);
            return this.fonts.TryGetValue(fontName, out font);
        }
    }
}
