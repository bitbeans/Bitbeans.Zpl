using Bitbeans.Zpl;
using Bitbeans.Zpl.Viewer.BitmapFonts;

using SkiaSharp;

using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class BitmapFontTests
    {
        private static ZplBitmapFontMetrics Metrics(int expansionX = 1, int expansionY = 1, int lineSpacing = 0)
        {
            return new ZplBitmapFontMetrics("T", 5, 9, 1, 7, expansionX, expansionY, lineSpacing);
        }

        private static ZplBitmapGlyph Block(int codePoint, int width, int height)
        {
            bool[] pixels = new bool[width * height];
            Array.Fill(pixels, true);
            return new ZplBitmapGlyph(codePoint, width, height, pixels);
        }

        // ---------- ZplBitmapFontMetrics ----------

        [TestMethod]
        public void Metrics_ExposesTheGivenValuesAndDerivedSizes()
        {
            ZplBitmapFontMetrics metrics = new("T", 5, 9, 1, 7, 2, 3, 4);

            Assert.AreEqual("T", metrics.Name);
            Assert.AreEqual(5, metrics.MatrixWidth);
            Assert.AreEqual(9, metrics.MatrixHeight);
            Assert.AreEqual(1, metrics.IntercharacterGap);
            Assert.AreEqual(7, metrics.Baseline);
            Assert.AreEqual(2, metrics.ExpansionX);
            Assert.AreEqual(3, metrics.ExpansionY);
            Assert.AreEqual(4, metrics.LineSpacing);
            Assert.AreEqual(12, metrics.Advance);
            Assert.AreEqual(10, metrics.RenderedGlyphWidth);
            Assert.AreEqual(27, metrics.RenderedGlyphHeight);
            Assert.AreEqual(31, metrics.LineHeight);
        }

        [TestMethod]
        public void Metrics_DefaultsToNoExpansion()
        {
            ZplBitmapFontMetrics metrics = new("T", 5, 9, 0, 0);

            Assert.AreEqual(1, metrics.ExpansionX);
            Assert.AreEqual(1, metrics.ExpansionY);
            Assert.AreEqual(0, metrics.LineSpacing);
            Assert.AreEqual(5, metrics.Advance);
            Assert.AreEqual(9, metrics.LineHeight);
        }

        [TestMethod]
        public void Metrics_WithExpansionKeepsTheFontShape()
        {
            ZplBitmapFontMetrics expanded = Metrics().WithExpansion(4, 2, 3);

            Assert.AreEqual("T", expanded.Name);
            Assert.AreEqual(5, expanded.MatrixWidth);
            Assert.AreEqual(9, expanded.MatrixHeight);
            Assert.AreEqual(1, expanded.IntercharacterGap);
            Assert.AreEqual(7, expanded.Baseline);
            Assert.AreEqual(4, expanded.ExpansionX);
            Assert.AreEqual(2, expanded.ExpansionY);
            Assert.AreEqual(3, expanded.LineSpacing);
            Assert.AreEqual(1, Metrics().WithExpansion(1, 1).ExpansionX);
            Assert.AreEqual(0, Metrics().WithExpansion(1, 1).LineSpacing);
        }

        [TestMethod]
        public void Metrics_RejectsInvalidValues()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new ZplBitmapFontMetrics(null, 5, 9, 1, 7));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplBitmapFontMetrics("  ", 5, 9, 1, 7));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", 0, 9, 1, 7));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", 5, 0, 1, 7));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", 5, 9, -1, 7));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", 5, 9, 1, -1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", 5, 9, 1, 7, 0, 1));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", 5, 9, 1, 7, 1, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Metrics().WithExpansion(0, 1));
        }

        [TestMethod]
        public void Metrics_InvalidValuesNameTheParameter()
        {
            ArgumentOutOfRangeException width = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapFontMetrics("T", -2, 9, 1, 7));
            ArgumentException name = Assert.ThrowsExactly<ArgumentException>(() => new ZplBitmapFontMetrics("", 5, 9, 1, 7));

            Assert.AreEqual("matrixWidth", width.ParamName);
            Assert.AreEqual(-2, width.ActualValue);
            Assert.AreEqual("name", name.ParamName);
        }

        // ---------- ZplBitmapGlyph ----------

        [TestMethod]
        public void Glyph_ExposesPixelsRowByRow()
        {
            ZplBitmapGlyph glyph = new('x', 3, 2, [true, false, false, false, true, true]);

            Assert.AreEqual('x', glyph.CodePoint);
            Assert.AreEqual(3, glyph.Width);
            Assert.AreEqual(2, glyph.Height);
            Assert.IsTrue(glyph.IsSet(0, 0));
            Assert.IsFalse(glyph.IsSet(1, 0));
            Assert.IsFalse(glyph.IsSet(2, 0));
            Assert.IsFalse(glyph.IsSet(0, 1));
            Assert.IsTrue(glyph.IsSet(1, 1));
            Assert.IsTrue(glyph.IsSet(2, 1));
        }

        [TestMethod]
        public void Glyph_CopiesItsPixels()
        {
            bool[] source = [true, false];
            ZplBitmapGlyph glyph = new('x', 2, 1, source);

            source[0] = false;
            bool[] copy = glyph.GetPixels();
            copy[1] = true;

            Assert.IsTrue(glyph.IsSet(0, 0));
            Assert.IsFalse(glyph.IsSet(1, 0));
            CollectionAssert.AreEqual(new[] { true, false }, glyph.GetPixels());
        }

        [TestMethod]
        public void Glyph_RejectsInvalidArguments()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapGlyph('x', 0, 1, []));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ZplBitmapGlyph('x', 1, 0, []));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplBitmapGlyph('x', 1, 1, null));
            Assert.ThrowsExactly<ArgumentException>(() => new ZplBitmapGlyph('x', 2, 2, [true]));
        }

        [TestMethod]
        public void Glyph_RejectsCoordinatesOutsideTheGlyph()
        {
            ZplBitmapGlyph glyph = Block('x', 2, 3);

            Assert.IsTrue(glyph.IsSet(1, 2));
            Assert.AreEqual("x", Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => glyph.IsSet(-1, 0)).ParamName);
            Assert.AreEqual("x", Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => glyph.IsSet(2, 0)).ParamName);
            Assert.AreEqual("y", Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => glyph.IsSet(0, -1)).ParamName);
            Assert.AreEqual("y", Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => glyph.IsSet(0, 3)).ParamName);
        }

        // ---------- ZplBitmapFontData ----------

        [TestMethod]
        public void FontData_LooksUpGlyphsByCodePoint()
        {
            ZplBitmapGlyph a = Block('A', 2, 2);
            Dictionary<int, ZplBitmapGlyph> source = new() { ['A'] = a };
            ZplBitmapFontData data = new(Metrics(), source);

            source['B'] = Block('B', 2, 2);

            Assert.AreEqual("T", data.Metrics.Name);
            Assert.AreEqual(1, data.Glyphs.Count);
            Assert.IsTrue(data.TryGetGlyph('A', out ZplBitmapGlyph found));
            Assert.AreSame(a, found);
            Assert.IsFalse(data.TryGetGlyph('B', out ZplBitmapGlyph missing));
            Assert.IsNull(missing);
        }

        [TestMethod]
        public void FontData_RejectsNullArguments()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplBitmapFontData(null, new Dictionary<int, ZplBitmapGlyph>()));
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplBitmapFontData(Metrics(), null));
        }

        [TestMethod]
        public void EmptyProvider_NeverHasAFont()
        {
            IZplBitmapFontProvider provider = EmptyZplBitmapFontProvider.Instance;

            Assert.IsFalse(provider.TryGet("0", 8, InternationalFont.ZCP850, out ZplBitmapFontData font));
            Assert.IsNull(font);
        }

        // ---------- ZplBitmapFontRenderer ----------

        [TestMethod]
        public void Renderer_RejectsNullArguments()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ZplBitmapFontRenderer(null));
            Assert.ThrowsExactly<ArgumentNullException>(() => ZplBitmapFontRenderer.MeasureTextWidth("a", null));

            using SKBitmap bitmap = new(10, 10);
            using SKCanvas canvas = new(bitmap);
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            ZplBitmapFontRenderer renderer = new(canvas);
            ZplBitmapFontData data = TestBitmapFontProvider.CreateFont("T");
            ZplBitmapGlyph glyph = Block('a', 1, 1);

            Assert.ThrowsExactly<ArgumentNullException>(() => renderer.DrawText("a", null, Metrics(), 0, 0, paint));
            Assert.ThrowsExactly<ArgumentNullException>(() => renderer.DrawText("a", data, null, 0, 0, paint));
            Assert.ThrowsExactly<ArgumentNullException>(() => renderer.DrawText("a", data, Metrics(), 0, 0, null));
            Assert.ThrowsExactly<ArgumentNullException>(() => renderer.DrawGlyph(null, Metrics(), 0, 0, paint));
            Assert.ThrowsExactly<ArgumentNullException>(() => renderer.DrawGlyph(glyph, null, 0, 0, paint));
            Assert.ThrowsExactly<ArgumentNullException>(() => renderer.DrawGlyph(glyph, Metrics(), 0, 0, null));
        }

        [TestMethod]
        public void Renderer_MeasuresTheAdvancePerCharacter()
        {
            ZplBitmapFontMetrics metrics = Metrics(2, 1);

            Assert.AreEqual(0, ZplBitmapFontRenderer.MeasureTextWidth(null, metrics));
            Assert.AreEqual(0, ZplBitmapFontRenderer.MeasureTextWidth(string.Empty, metrics));
            Assert.AreEqual(12, ZplBitmapFontRenderer.MeasureTextWidth("A", metrics));
            Assert.AreEqual(36, ZplBitmapFontRenderer.MeasureTextWidth("ABC", metrics));
            // a surrogate pair is a single character
            Assert.AreEqual(12, ZplBitmapFontRenderer.MeasureTextWidth("\U0001F600", metrics));
        }

        [TestMethod]
        public void Renderer_CreatePaintIsAliasFreeAndFilled()
        {
            using SKPaint normal = ZplBitmapFontRenderer.CreatePaint(false);
            using SKPaint reverse = ZplBitmapFontRenderer.CreatePaint(true);

            Assert.IsFalse(normal.IsAntialias);
            Assert.AreEqual(SKPaintStyle.Fill, normal.Style);
            Assert.AreEqual(SKBlendMode.SrcOver, normal.BlendMode);
            Assert.AreEqual(SKBlendMode.Xor, reverse.BlendMode);
        }

        [TestMethod]
        public void Renderer_DrawTextPlacesGlyphsOnTheAdvanceGrid()
        {
            using SKBitmap bitmap = new(60, 30);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            ZplBitmapFontData data = TestBitmapFontProvider.CreateFont("T");
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            paint.Color = SKColors.Black;

            int width = new ZplBitmapFontRenderer(canvas).DrawText("AB A", data, Metrics(), 4, 3, paint);

            Assert.AreEqual(24, width);
            // each glyph is a 5x9 frame; advance is 6, the space draws nothing
            Assert.AreEqual(new SKRectI(4, 3, 27, 12), RenderHelper.DarkBounds(bitmap).Value);
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 4, 3));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 8, 11));
            Assert.IsTrue(RenderHelper.IsLight(bitmap, 6, 6));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 10, 3));
            Assert.AreEqual(0, RenderHelper.CountDark(bitmap, 16, 0, 22, 30));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 22, 3));
        }

        [TestMethod]
        public void Renderer_DrawTextSkipsMissingGlyphsButAdvances()
        {
            using SKBitmap bitmap = new(60, 30);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            ZplBitmapFontData data = TestBitmapFontProvider.CreateFont("T");
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            paint.Color = SKColors.Black;

            // U+00E4 has no glyph in the test font
            int width = new ZplBitmapFontRenderer(canvas).DrawText("äA", data, Metrics(), 0, 0, paint);

            Assert.AreEqual(12, width);
            Assert.AreEqual(new SKRectI(6, 0, 11, 9), RenderHelper.DarkBounds(bitmap).Value);
        }

        [TestMethod]
        public void Renderer_DrawTextOfNothingDrawsNothing()
        {
            using SKBitmap bitmap = new(10, 10);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            ZplBitmapFontData data = TestBitmapFontProvider.CreateFont("T");
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            ZplBitmapFontRenderer renderer = new(canvas);

            Assert.AreEqual(0, renderer.DrawText(null, data, Metrics(), 0, 0, paint));
            Assert.AreEqual(0, renderer.DrawText(string.Empty, data, Metrics(), 0, 0, paint));
            Assert.IsNull(RenderHelper.DarkBounds(bitmap));
        }

        [TestMethod]
        public void Renderer_DrawGlyphExpandsEveryPixel()
        {
            using SKBitmap bitmap = new(40, 40);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            paint.Color = SKColors.Black;
            // 2x2 glyph with only the diagonal set, in a 2x2 matrix, expanded 3x2
            ZplBitmapGlyph glyph = new('d', 2, 2, [true, false, false, true]);
            ZplBitmapFontMetrics metrics = new("D", 2, 2, 0, 0, 3, 2);

            new ZplBitmapFontRenderer(canvas).DrawGlyph(glyph, metrics, 10, 20, paint);

            Assert.AreEqual(new SKRectI(10, 20, 16, 24), RenderHelper.DarkBounds(bitmap).Value);
            Assert.AreEqual(3 * 2 * 2, RenderHelper.CountDark(bitmap));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 12, 21));
            Assert.IsTrue(RenderHelper.IsLight(bitmap, 13, 21));
            Assert.IsTrue(RenderHelper.IsLight(bitmap, 12, 22));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 13, 22));
            Assert.IsTrue(RenderHelper.IsDark(bitmap, 15, 23));
        }

        [TestMethod]
        public void Renderer_DrawGlyphClipsToTheFontMatrix()
        {
            using SKBitmap bitmap = new(40, 40);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            paint.Color = SKColors.Black;
            // a 6x8 glyph in a 3x4 matrix: only the top-left 3x4 pixels are drawn
            ZplBitmapGlyph glyph = Block('b', 6, 8);
            ZplBitmapFontMetrics metrics = new("D", 3, 4, 0, 0);

            new ZplBitmapFontRenderer(canvas).DrawGlyph(glyph, metrics, 5, 5, paint);

            Assert.AreEqual(new SKRectI(5, 5, 8, 9), RenderHelper.DarkBounds(bitmap).Value);
        }

        [TestMethod]
        public void Renderer_DrawGlyphSmallerThanTheMatrixDrawsOnlyItsPixels()
        {
            using SKBitmap bitmap = new(40, 40);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            paint.Color = SKColors.Black;
            ZplBitmapGlyph glyph = Block('s', 2, 2);
            ZplBitmapFontMetrics metrics = new("D", 6, 8, 0, 0);

            new ZplBitmapFontRenderer(canvas).DrawGlyph(glyph, metrics, 5, 5, paint);

            Assert.AreEqual(new SKRectI(5, 5, 7, 7), RenderHelper.DarkBounds(bitmap).Value);
        }

        [TestMethod]
        public void Renderer_SnapsPositionsToWholePixels()
        {
            using SKBitmap bitmap = new(40, 40);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(false);
            paint.Color = SKColors.Black;

            new ZplBitmapFontRenderer(canvas).DrawGlyph(Block('s', 2, 2), new ZplBitmapFontMetrics("D", 2, 2, 0, 0), 10.6f, 20.4f, paint);

            Assert.AreEqual(new SKRectI(11, 20, 13, 22), RenderHelper.DarkBounds(bitmap).Value);
        }

        [TestMethod]
        public void Renderer_ReversePaintErasesWhatIsUnderneath()
        {
            using SKBitmap bitmap = new(20, 20);
            using SKCanvas canvas = new(bitmap);
            canvas.Clear(SKColors.White);
            using SKPaint paint = ZplBitmapFontRenderer.CreatePaint(true);
            paint.Color = SKColors.Black;

            new ZplBitmapFontRenderer(canvas).DrawGlyph(Block('s', 4, 4), new ZplBitmapFontMetrics("D", 4, 4, 0, 0), 2, 2, paint);

            Assert.AreEqual(0, bitmap.GetPixel(3, 3).Alpha);
            Assert.AreEqual(255, bitmap.GetPixel(10, 10).Alpha);
            Assert.AreEqual(255, bitmap.GetPixel(6, 6).Alpha);
        }
    }
}
