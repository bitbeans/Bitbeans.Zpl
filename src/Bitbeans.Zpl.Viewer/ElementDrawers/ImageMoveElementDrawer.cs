using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using SkiaSharp;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Image Move elements
    /// </summary>
    public class ImageMoveElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplImageMove;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplImageMove imageMove)
            {
                byte[] imageData = this.printerStorage.GetFile(imageMove.StorageDevice, imageMove.ObjectName);
                if (imageData.Length == 0)
                {
                    return currentPosition;
                }

                using SKBitmap image = SKBitmap.Decode(imageData);

                SKPoint position = ResolvePosition(imageMove, currentPosition);
                float x = JustifyX(imageMove, position.X, image.Width);
                float y = position.Y;

                bool useFieldTypeset = imageMove.FieldTypeset != null;
                if (useFieldTypeset)
                {
                    // the position is the bottom left corner; an image above the label is clipped, not moved
                    y -= image.Height;
                }

                this.skCanvas.DrawBitmap(image, x, y, SKSamplingOptions.Default);

                return this.CalculateNextDefaultPosition(x, y, image.Width, image.Height, imageMove.FieldOrigin != null, Zpl.FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
