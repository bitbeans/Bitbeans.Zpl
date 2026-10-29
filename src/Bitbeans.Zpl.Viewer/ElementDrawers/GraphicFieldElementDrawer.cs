using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.SkiaSharp;
using Bitbeans.Zpl.Viewer.Helpers;

using SkiaSharp;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Graphic Field elements
    /// </summary>
    public class GraphicFieldElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplGraphicField;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplGraphicField graphicField)
            {
                // the graphic data is converted when drawing: ASCII hexadecimal or compressed (ACS, :B64:, :Z64:)
                byte[] grfData = ImageHelper.GetImageBytes(graphicField.Data, graphicField.BytesPerRow);
                byte[] imageData = new SkiaImageConverter().ConvertGrfToPng(grfData, graphicField.BytesPerRow);
                using SKBitmap image = SKBitmap.Decode(imageData);

                SKPoint position = ResolvePosition(graphicField, currentPosition);
                float x = JustifyX(graphicField, position.X, image.Width);
                float y = position.Y;

                bool useFieldTypeset = graphicField.FieldTypeset != null;
                if (useFieldTypeset)
                {
                    // the position is the bottom left corner; an image above the label is clipped, not moved
                    y -= image.Height;
                }

                this.skCanvas.DrawBitmap(image, x, y, SKSamplingOptions.Default);
                return this.CalculateNextDefaultPosition(x, y, image.Width, image.Height, graphicField.FieldOrigin != null, Zpl.FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
