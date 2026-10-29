using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using SkiaSharp;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Recall Graphic elements
    /// </summary>
    public class RecallGraphicElementDrawer : ElementDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplRecallGraphic;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            if (element is ZplRecallGraphic recallGraphic)
            {
                byte[] imageData = this.printerStorage.GetFile(recallGraphic.StorageDevice, recallGraphic.ImageName);

                if (imageData.Length == 0)
                {
                    return currentPosition;
                }

                SKPoint position = ResolvePosition(recallGraphic, currentPosition);

                using SKBitmap bitmap = SKBitmap.Decode(imageData);
                float width = bitmap.Width * recallGraphic.MagnificationFactorX;
                float height = bitmap.Height * recallGraphic.MagnificationFactorY;

                float x = JustifyX(recallGraphic, position.X, width);
                float y = position.Y;

                if (recallGraphic.FieldTypeset != null)
                {
                    // the position is the bottom left corner; an image above the label is clipped, not moved
                    y -= height;
                }

                this.skCanvas.DrawBitmap(bitmap, new SKRect(x, y, x + width, y + height), SKSamplingOptions.Default);

                return this.CalculateNextDefaultPosition(x, y, width, height, recallGraphic.FieldOrigin != null, FieldOrientation.Normal, currentPosition);
            }

            return currentPosition;
        }
    }
}
