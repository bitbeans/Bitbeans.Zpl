using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.ElementDrawers;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.Tests.Rendering
{
    [TestClass]
    public class GraphicDrawerEdgeTests
    {
        [TestMethod]
        public void ForceBitmapDraw_OnlyForRoundedBoxes()
        {
            GraphicBoxElementDrawer drawer = new();

            Assert.IsTrue(drawer.ForceBitmapDraw(new ZplGraphicBox(10, 10, 50, 50, 3, cornerRounding: 4)));
            Assert.IsFalse(drawer.ForceBitmapDraw(new ZplGraphicBox(10, 10, 50, 50, 3)));
            Assert.IsFalse(drawer.ForceBitmapDraw(new ZplGraphicCircle(10, 10, 50, 3)));
        }

        [TestMethod]
        public void GraphicDrawers_IgnoreOtherElements()
        {
            ElementDrawerBase[] drawers =
            [
                new GraphicBoxElementDrawer(),
                new GraphicCircleElementDrawer(),
                new GraphicDiagonalLineElementDrawer(),
                new GraphicEllipseElementDrawer(),
            ];
            ZplElementBase other = new ZplImageMove(1, 2, 'R', "LOGO.GRF", string.Empty);
            SKPoint current = new(11, 12);

            (SKSurface surface, SKCanvas canvas) = RenderHelper.CreateSurface();
            using (surface)
            {
                foreach (ElementDrawerBase drawer in drawers)
                {
                    drawer.Prepare(new PrinterStorage(), canvas);

                    Assert.IsFalse(drawer.CanDraw(other), drawer.GetType().Name);
                    Assert.IsFalse(drawer.IsReverseDraw(other), drawer.GetType().Name);
                    Assert.IsFalse(drawer.IsWhiteDraw(other), drawer.GetType().Name);
                    Assert.AreEqual(current, drawer.Draw(other, new DrawerOptions(), current, InternationalFont.ZCP850, 8), drawer.GetType().Name);
                }

                using SKBitmap bitmap = SnapshotOf(surface);
                Assert.AreEqual(0, RenderHelper.CountDark(bitmap));
            }
        }

        private static SKBitmap SnapshotOf(SKSurface surface)
        {
            using SKImage image = surface.Snapshot();
            return SKBitmap.FromImage(image);
        }
    }
}
