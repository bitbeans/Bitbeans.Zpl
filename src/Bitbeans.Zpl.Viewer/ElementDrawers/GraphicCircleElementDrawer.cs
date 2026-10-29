using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using SkiaSharp;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>Draws graphic circle (<c>^GC</c>) elements.</summary>
    public class GraphicCircleElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplGraphicCircle;
        }

        ///<inheritdoc/>
        public override bool IsReverseDraw(ZplElementBase element)
        {
            if (element is ZplGraphicCircle graphicCircle)
            {
                return graphicCircle.ReversePrint;
            }

            return false;
        }

        ///<inheritdoc/>
        public override bool IsWhiteDraw(ZplElementBase element)
        {
            if (element is ZplGraphicCircle graphicCircle)
            {
                return graphicCircle.LineColor == LineColor.White;
            }

            return false;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplGraphicCircle graphicCircle)
            {
                float radius = graphicCircle.Diameter / 2.0f;
                float border = graphicCircle.BorderThickness;

                if (border > radius)
                {
                    border = radius;
                }

                using SKPaint skPaint = new()
                {
                    IsAntialias = options.Antialias,
                    Style = SKPaintStyle.Stroke,
                    Color = SKColors.Black,
                    StrokeWidth = border
                };
                if (graphicCircle.LineColor == LineColor.White)
                {
                    skPaint.Color = SKColors.White;
                }

                float halfBorderThickness = border / 2.0f;

                float radiusMinusBorder = radius - halfBorderThickness;
                float offset = halfBorderThickness + radiusMinusBorder;

                SKPoint position = ResolvePosition(graphicCircle, currentPosition);
                float baseX = JustifyX(graphicCircle, position.X, graphicCircle.Diameter);
                float baseY = position.Y;

                float x = baseX + offset;
                float y = baseY + offset;

                if (graphicCircle.FieldTypeset != null)
                {
                    // the position is the bottom left corner; a circle above the label is clipped, not moved
                    y -= graphicCircle.Diameter;
                }

                this.skCanvas.DrawCircle(x, y, radiusMinusBorder, skPaint);
                return this.CalculateNextDefaultPosition(baseX, position.Y, graphicCircle.Diameter, graphicCircle.Diameter, graphicCircle.FieldOrigin != null, FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
