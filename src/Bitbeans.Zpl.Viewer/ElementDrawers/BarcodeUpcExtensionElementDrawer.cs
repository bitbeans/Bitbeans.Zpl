using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Helpers;
using Bitbeans.Zpl.Viewer.Symologies;

using SkiaSharp;

using System;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>Draws UPC/EAN extension (<c>^BS</c>) barcode elements.</summary>
    public class BarcodeUpcExtensionElementDrawer : BarcodeDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplBarcodeUpcExtension;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplBarcodeUpcExtension barcode)
            {
                float x = barcode.PositionX;
                float y = barcode.PositionY;

                if (barcode.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                string content = barcode.Content;
                if (barcode.HexadecimalIndicator is char hexIndicator)
                {
                    content = content.ReplaceHexEscapes(hexIndicator, internationalFont);
                }

                if (content.Length <= 2)
                {
                    // EAN-2
                    content = content.PadLeft(2, '0');
                }
                else
                {
                    // EAN-5
                    content = content.PadLeft(5, '0').Substring(0, 5);
                }

                string interpretation = content;

                bool[] data = UpcExtensionSymbology.Encode(content);
                using SKBitmap resizedImage = BoolArrayToSKBitmap(data, barcode.Height, barcode.ModuleWidth);
                byte[] png = resizedImage.Encode(SKEncodedImageFormat.Png, 100).ToArray();
                this.DrawBarcode(png, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation);

                if (barcode.PrintInterpretationLine)
                {
                    this.DrawBitmapInterpretationLine(interpretation, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, barcode.PrintInterpretationLineAboveCode, options, internationalFont, printDensityDpmm, barcode.ModuleWidth);
                }

                return this.CalculateNextDefaultPosition(x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation, currentPosition);
            }

            return currentPosition;
        }

    }
}
