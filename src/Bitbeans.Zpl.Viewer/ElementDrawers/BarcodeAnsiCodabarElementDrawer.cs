using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Helpers;

using SkiaSharp;

using System;

using ZXing.OneD;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    /// <summary>
    /// Drawer for Code 39 Barcode elements
    /// </summary>
    public class BarcodeAnsiCodabarElementDrawer : BarcodeDrawerBase
    {
        ///<inheritdoc/>
        public override bool CanDraw(ZplElementBase element)
        {
            return element is ZplBarcodeAnsiCodabar;
        }

        ///<inheritdoc/>
        public override SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            if (element is ZplBarcodeAnsiCodabar barcode)
            {
                float x = barcode.PositionX;
                float y = barcode.PositionY;
                char start = barcode.StartCharacter;
                char end = barcode.StopCharacter;
                string content = barcode.Content.Trim('*');

                if (string.IsNullOrEmpty(content))
                {
                    return currentPosition;
                }

                if (barcode.UseDefaultPosition)
                {
                    x = currentPosition.X;
                    y = currentPosition.Y;
                }

                if (barcode.HexadecimalIndicator is char hexIndicator)
                {
                    content = content.ReplaceHexEscapes(hexIndicator, internationalFont);
                }

                string interpretation = $"{start}{content}{end}";

                CodaBarWriter writer = new();
                bool[] result = writer.encode(content);
                int narrow = barcode.ModuleWidth;
                int wide = (int)Math.Floor(barcode.WideBarToNarrowBarWidthRatio * narrow);
                result = AdjustWidths(result, wide, narrow);
                using SKBitmap resizedImage = BoolArrayToSKBitmap(result, barcode.Height);
                this.JustifyOrigin(ref x, ref y, resizedImage.Width, barcode.FieldOrigin != null, barcode.FieldOrientation, barcode.FieldJustification);
                this.DrawBarcode(resizedImage, x, y, resizedImage.Width, resizedImage.Height, barcode.FieldOrigin != null, barcode.FieldOrientation);

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
