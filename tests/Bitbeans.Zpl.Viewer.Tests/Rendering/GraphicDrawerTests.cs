using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class GraphicDrawerTests
    {
        private static SKBitmap R(string body, DrawerOptions options = null)
        {
            return RenderHelper.Render("^XA" + body + "^XZ", options);
        }

        // ---------- ^GB box ----------

        [TestMethod]
        public void Box_HasBlackBorderAndWhiteInside()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,60,5^FS");

            Assert.AreEqual(400, bmp.Width);
            Assert.AreEqual(400, bmp.Height);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 50, 80));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 52, 80));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 147, 80));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 52));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 107));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 80));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 45, 80));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 155, 80));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 45));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 115));

            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(50, bounds.Left);
            Assert.AreEqual(50, bounds.Top);
            Assert.AreEqual(150, bounds.Right);
            Assert.AreEqual(110, bounds.Bottom);
        }

        [TestMethod]
        public void Box_ThicknessLargerThanSizeIsFilled()
        {
            // the box grows to the thickness (100) in height
            using SKBitmap bmp = R("^FO50,50^GB100,60,100^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(100, bounds.Width);
            Assert.AreEqual(100, bounds.Height);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 80));
        }

        [TestMethod]
        public void Box_BorderLargerThanWidthExpandsWidth()
        {
            // width 10 < thickness 30: width is enlarged to the thickness
            using SKBitmap bmp = R("^FO50,50^GB10,100,30^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(30, bounds.Width);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 65, 100));
        }

        [TestMethod]
        public void Box_BorderLargerThanHeightExpandsHeight()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,10,30^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(30, bounds.Height);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 65));
        }

        [TestMethod]
        public void Box_BorderLargerThanHalfNarrowWidthIsReduced()
        {
            // width 20, height 100, thickness 15 -> thickness is limited to 10, so the box is filled
            using SKBitmap bmp = R("^FO50,50^GB20,100,15^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 100));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 40, 100));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 75, 100));
        }

        [TestMethod]
        public void Box_BorderLargerThanHalfNarrowHeightIsReduced()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,20,15^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 60));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 40));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 75));
        }

        [TestMethod]
        public void Box_ZeroThicknessIsOnePixel()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,60,0^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 50, 80));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 52, 80));
        }

        [TestMethod]
        public void Box_RoundedCornersAreCut()
        {
            using SKBitmap square = R("^FO50,50^GB100,100,5^FS");
            using SKBitmap rounded = R("^FO50,50^GB100,100,5,B,8^FS");

            Assert.IsTrue(RenderHelper.IsDark(square, 51, 51));
            Assert.IsTrue(RenderHelper.IsLight(rounded, 51, 51));
            Assert.IsTrue(RenderHelper.IsDark(rounded, 100, 52));
            Assert.IsTrue(RenderHelper.IsDark(rounded, 52, 100));
            Assert.IsTrue(RenderHelper.IsLight(rounded, 100, 100));
        }

        [TestMethod]
        public void Box_RoundedThickBorderIsBuiltInIncrements()
        {
            using SKBitmap bmp = R("^FO50,50^GB120,120,24,B,4^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 110, 55));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 55, 110));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 110, 110));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 51, 51));
        }

        [TestMethod]
        public void Box_WhiteLineOverBlackBox()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO70,70^GB60,60,60,W^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 60));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 140, 140));
        }

        [TestMethod]
        public void Box_WhiteRoundedBox()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO70,70^GB60,60,10,W,8^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 72));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 100));
        }

        [TestMethod]
        public void Box_FieldTypesetPositionsBottomLeft()
        {
            using SKBitmap bmp = R("^FT50,150^GB100,60,5^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(50, bounds.Left);
            Assert.AreEqual(90, bounds.Top);
            Assert.AreEqual(150, bounds.Bottom);
        }

        [TestMethod]
        public void Box_FieldTypesetAtTopFallsBackToTopEdge()
        {
            using SKBitmap bmp = R("^FT50,20^GB100,60,5^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.AreEqual(0, bounds.Top);
            Assert.AreEqual(60, bounds.Bottom);
        }

        [TestMethod]
        public void Box_DefaultPositionFollowsPreviousElement()
        {
            // first box ends at (150,110); the second box uses the default position and starts there
            using SKBitmap bmp = RenderHelper.RenderElements(
            [
                new ZplGraphicBox(50, 50, 100, 60, 60),
                new ZplGraphicBox(0, 0, 40, 40, 40, useDefaultPosition: true),
            ]);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 170, 130));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 190, 130));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 170, 150));
        }

        [TestMethod]
        public void Box_FieldReverseInvertsBlack()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO80,80^FR^GB40,40,40^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 60));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
        }

        [TestMethod]
        public void Box_FieldReverseOnWhiteBackgroundIsBlack()
        {
            using SKBitmap bmp = R("^FO80,80^FR^GB40,40,40^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 100));
        }

        [TestMethod]
        public void Box_FieldReverseOfWhiteLineKeepsBackground()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO80,80^FR^GB40,40,40,W^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 60));
            // an inverted white element: the underlying pixels are flipped using the white inversion
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));
        }

        [TestMethod]
        public void Box_LabelReverseInvertsEveryElement()
        {
            using SKBitmap bmp = R("^LRY^FO50,50^GB100,100,100^FS^FO80,80^GB40,40,40^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 60));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
        }

        [TestMethod]
        public void Box_NextDefaultPositionForFieldTypeset()
        {
            using SKBitmap bmp = R("^FT50,150^GB40,40,40^FS^GB40,40,40^FS");
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));
        }

        // ---------- ^GC circle ----------

        [TestMethod]
        public void Circle_RingIsDarkCenterIsLight()
        {
            using SKBitmap bmp = R("^FO50,50^GC100,5^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 52, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 147, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 52));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 147));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 55, 55));
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(Math.Abs(bounds.Left - 50) <= 1);
            Assert.IsTrue(Math.Abs(bounds.Right - 150) <= 1);
        }

        [TestMethod]
        public void Circle_ThicknessLargerThanRadiusIsFilled()
        {
            using SKBitmap bmp = R("^FO50,50^GC40,100^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 70, 70));
        }

        [TestMethod]
        public void Circle_WhiteOnBlack()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO70,70^GC60,30,W^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 55, 55));
        }

        [TestMethod]
        public void Circle_FieldTypesetAndFallback()
        {
            using SKBitmap bmp = R("^FT50,150^GC100,100^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 100));

            using SKBitmap fallback = R("^FT50,20^GC100,100^FS");
            SKRectI bounds = RenderHelper.DarkBounds(fallback).Value;
            Assert.IsTrue(bounds.Top <= 1);
        }

        [TestMethod]
        public void Circle_DefaultPosition()
        {
            using SKBitmap bmp = RenderHelper.RenderElements(
            [
                new ZplGraphicBox(10, 10, 10, 10, 10),
                new ZplGraphicCircle(0, 0, 50, 50, useDefaultPosition: true),
            ]);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 45, 45));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 5, 5));
        }

        [TestMethod]
        public void Circle_ReverseInvertsUnderlyingBlack()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO70,70^FR^GC60,60^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 55, 55));
        }

        [TestMethod]
        public void Circle_ReverseWhite()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO70,70^FR^GC60,60,W^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 55, 55));
            Assert.IsNotNull(RenderHelper.DarkBounds(bmp));
        }

        // ---------- ^GE ellipse ----------

        [TestMethod]
        public void Ellipse_RingIsDarkCenterIsLight()
        {
            using SKBitmap bmp = R("^FO50,50^GE100,60,5^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 80));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 52, 80));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 147, 80));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 52));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 107));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 52, 52));
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(Math.Abs(bounds.Width - 100) <= 1);
            Assert.IsTrue(Math.Abs(bounds.Height - 60) <= 1);
        }

        [TestMethod]
        public void Ellipse_NarrowWidthIsFilled()
        {
            using SKBitmap bmp = R("^FO50,50^GE10,60,20^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 55, 80));
        }

        [TestMethod]
        public void Ellipse_LowHeightIsFilled()
        {
            using SKBitmap bmp = R("^FO50,50^GE100,10,20^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 55));
        }

        [TestMethod]
        public void Ellipse_FieldTypesetAndFallback()
        {
            using SKBitmap bmp = R("^FT50,150^GE100,60,60^FS");
            SKRectI bounds = RenderHelper.DarkBounds(bmp).Value;
            Assert.IsTrue(Math.Abs(bounds.Top - 90) <= 1);

            using SKBitmap fallback = R("^FT50,20^GE100,60,60^FS");
            Assert.IsTrue(RenderHelper.DarkBounds(fallback).Value.Top <= 1);
        }

        [TestMethod]
        public void Ellipse_DefaultPosition()
        {
            using SKBitmap bmp = RenderHelper.RenderElements(
            [
                new ZplGraphicBox(10, 10, 10, 10, 10),
                new ZplGraphicEllipse(0, 0, 50, 50, 50, useDefaultPosition: true),
            ]);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 45, 45));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 5, 5));
        }

        [TestMethod]
        public void Ellipse_ReverseAndWhite()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO70,70^FR^GE60,60,60^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));

            using SKBitmap white = R("^FO50,50^GB100,100,100^FS^FO70,70^FR^GE60,60,60,W^FS");
            Assert.IsTrue(RenderHelper.IsDark(white, 55, 55));
        }

        // ---------- ^GD diagonal line ----------

        [TestMethod]
        public void Diagonal_RightLeaningGoesBottomLeftToTopRight()
        {
            using SKBitmap bmp = R("^FO50,50^GD100,100,5,B,R^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 62, 138));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 62, 62));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 138, 138));
        }

        [TestMethod]
        public void Diagonal_LeftLeaningGoesTopLeftToBottomRight()
        {
            using SKBitmap bmp = R("^FO50,50^GD100,100,5,B,L^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 62, 62));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 62, 138));
        }

        [TestMethod]
        public void Diagonal_BackslashOrientationIsLeftLeaning()
        {
            using SKBitmap bmp = R("^FO50,50^GD100,100,5,B,\\^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 62, 62));
        }

        [TestMethod]
        public void Diagonal_WhiteOnBlack()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO50,50^GD100,100,10,W,L^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 140));
        }

        [TestMethod]
        public void Diagonal_FieldTypesetAndFallback()
        {
            using SKBitmap bmp = R("^FT50,150^GD100,100,5,B,L^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 100, 100));

            using SKBitmap fallback = R("^FT50,50^GD100,100,5,B,L^FS");
            Assert.IsTrue(RenderHelper.IsDark(fallback, 100, 50));
        }

        [TestMethod]
        public void Diagonal_DefaultPosition()
        {
            using SKBitmap bmp = RenderHelper.RenderElements(
            [
                new ZplGraphicBox(10, 10, 10, 10, 10),
                new ZplGraphicDiagonalLine(0, 0, 50, 50, 5, rightLeaningDiagonal: false, useDefaultPosition: true),
            ]);
            Assert.IsTrue(RenderHelper.IsDark(bmp, 45, 45));
            Assert.IsTrue(RenderHelper.IsLight(bmp, 5, 5));
        }

        [TestMethod]
        public void Diagonal_ReverseInvertsUnderlyingBlack()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO50,50^FR^GD100,100,10,B,L^FS");
            Assert.IsTrue(RenderHelper.IsLight(bmp, 100, 100));
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 140));
        }

        [TestMethod]
        public void Diagonal_ReverseWhite()
        {
            using SKBitmap bmp = R("^FO50,50^GB100,100,100^FS^FO50,50^FR^GD100,100,10,W,L^FS");
            Assert.IsTrue(RenderHelper.IsDark(bmp, 60, 140));
        }

        // ---------- direct drawer calls with other elements ----------

        [TestMethod]
        public void IsReverseAndWhiteDrawFlagsOnDifferentElements()
        {
            ZplElementBase[] elements = RenderHelper.Parse("^XA^FO1,1^GB10,10,1^FS^FO1,1^GC10,1^FS^FO1,1^GD10,10,1^FS^FO1,1^GE10,10,1^FS^XZ");
            Assert.AreEqual(4, elements.Length);
            IElementDrawer[] drawers =
            [
                new GraphicBoxElementDrawer(),
                new GraphicCircleElementDrawer(),
                new GraphicDiagonalLineElementDrawer(),
                new GraphicEllipseElementDrawer(),
            ];

            for (int i = 0; i < drawers.Length; i++)
            {
                for (int j = 0; j < elements.Length; j++)
                {
                    Assert.AreEqual(i == j, drawers[i].CanDraw(elements[j]));
                    Assert.IsFalse(drawers[i].IsReverseDraw(elements[j]));
                    Assert.IsFalse(drawers[i].IsWhiteDraw(elements[j]));
                    Assert.IsFalse(drawers[i].ForceBitmapDraw(elements[j]));
                }
            }
        }
    }
}
