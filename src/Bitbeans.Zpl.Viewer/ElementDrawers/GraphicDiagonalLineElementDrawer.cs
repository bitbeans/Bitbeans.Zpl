using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>Draws graphic diagonal line (<c>^GD</c>) elements.</summary>
    public class GraphicDiagonalLineElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplGraphicDiagonalLine;
        }

        ///<inheritdoc/>
        public override bool IsReverseDraw(ZplElementBase element)
        {
            if (element is ZplGraphicDiagonalLine graphicLine)
            {
                return graphicLine.ReversePrint;
            }

            return false;
        }

        ///<inheritdoc/>
        public override bool IsWhiteDraw(ZplElementBase element)
        {
            if (element is ZplGraphicDiagonalLine graphicLine)
            {
                return graphicLine.LineColor == LineColor.White;
            }

            return false;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplGraphicDiagonalLine graphicLine)
            {
                int border = graphicLine.BorderThickness;
                int width = graphicLine.Width;
                int height = graphicLine.Height;

                SKPoint position = ResolvePosition(graphicLine, currentPosition);
                float x = JustifyX(graphicLine, position.X, width);
                float y = position.Y;

                if (graphicLine.FieldTypeset != null)
                {
                    // the position is the bottom left corner; a line above the label is clipped, not moved
                    y -= height;
                }

                // the line must stay within width x height: the thickness is part of the width
                float thickness = Math.Min(border, width);
                SKPoint skPointLL = new(x, y + height);
                SKPoint skPointUL = new(x, y);

                using SKPaint skPaint = new()
                {
                    IsAntialias = options.Antialias,
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.Black
                };

                if (graphicLine.LineColor == LineColor.White)
                {
                    skPaint.Color = SKColors.White;
                }

                using SKPathBuilder pathBuilder = new();
                if (graphicLine.RightLeaningDiagonal)
                {
                    pathBuilder.MoveTo(skPointLL);
                    pathBuilder.RLineTo(thickness, 0);
                    pathBuilder.RLineTo(width - thickness, -height);
                    pathBuilder.RLineTo(-thickness, 0);
                    pathBuilder.Close();
                }
                else
                {
                    pathBuilder.MoveTo(skPointUL);
                    pathBuilder.RLineTo(thickness, 0);
                    pathBuilder.RLineTo(width - thickness, height);
                    pathBuilder.RLineTo(-thickness, 0);
                    pathBuilder.Close();
                }

                using SKPath path = pathBuilder.Detach();
                this.skCanvas.DrawPath(path, skPaint);

                // Calculate next position based on box dimensions
                return this.CalculateNextDefaultPosition(x, y, width, height, graphicLine.FieldOrigin != null, FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
