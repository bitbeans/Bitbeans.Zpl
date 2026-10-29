using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using SkiaSharp;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>Draws graphic ellipse (<c>^GE</c>) elements.</summary>
    public class GraphicEllipseElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplGraphicEllipse;
        }

        ///<inheritdoc/>
        public override bool IsReverseDraw(ZplElementBase element)
        {
            if (element is ZplGraphicEllipse graphicEllipse)
            {
                return graphicEllipse.ReversePrint;
            }

            return false;
        }

        ///<inheritdoc/>
        public override bool IsWhiteDraw(ZplElementBase element)
        {
            if (element is ZplGraphicEllipse graphicEllipse)
            {
                return graphicEllipse.LineColor == LineColor.White;
            }

            return false;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplGraphicEllipse graphicEllipse)
            {
                float border = graphicEllipse.BorderThickness;
                float width = graphicEllipse.Width;
                float height = graphicEllipse.Height;

                // if either width or height is less than half border, ellipse will be completely filled
                if (width < border * 2)
                {
                    border = width / 2;
                }

                if (height < border * 2)
                {
                    border = height / 2;
                }

                SKPoint position = ResolvePosition(graphicEllipse, currentPosition);
                float x = JustifyX(graphicEllipse, position.X, width);
                float y = position.Y;

                if (graphicEllipse.FieldTypeset != null)
                {
                    // the position is the bottom left corner; an ellipse above the label is clipped, not moved
                    y -= height;
                }

                float halfBorderThickness = border / 2.0f;

                x += halfBorderThickness;
                y += halfBorderThickness;

                SKRect bounds = new(x, y, x + width - border, y + height - border);

                using SKPaint skPaint = new()
                {
                    IsAntialias = options.Antialias,
                    Style = SKPaintStyle.Stroke,
                    Color = graphicEllipse.LineColor == LineColor.White ? SKColors.White : SKColors.Black,
                    StrokeWidth = border
                };

                this.skCanvas.DrawOval(bounds, skPaint);
                return this.CalculateNextDefaultPosition(x, y, width, height, graphicEllipse.FieldOrigin != null, FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
