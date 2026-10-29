using Bitbeans.Zpl;
using Bitbeans.Zpl.Elements;

using SkiaSharp;

namespace Bitbeans.Zpl.Viewer.ElementDrawers
{
    public abstract class ElementDrawerBase : IElementDrawer
    {
        internal IPrinterStorage printerStorage;
        internal SKCanvas skCanvas;

        ///<inheritdoc/>
        public void Prepare(
            IPrinterStorage printerStorage,
            SKCanvas skCanvas)
        {
            this.printerStorage = printerStorage;
            this.skCanvas = skCanvas;
        }

        ///<inheritdoc/>
        public abstract bool CanDraw(ZplElementBase element);

        ///<inheritdoc/>
        public virtual bool IsReverseDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual bool IsWhiteDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual bool ForceBitmapDraw(ZplElementBase element)
        {
            return false;
        }

        ///<inheritdoc/>
        public virtual SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition)
        {
            return currentPosition;
        }

        ///<inheritdoc/>
        public virtual SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont)
        {
            return this.Draw(element, options, currentPosition);
        }

        ///<inheritdoc/>
        public virtual SKPoint Draw(ZplElementBase element, DrawerOptions options, SKPoint currentPosition, InternationalFont internationalFont, int printDensityDpmm)
        {
            return this.Draw(element, options, currentPosition, internationalFont);
        }

        protected virtual SKPoint CalculateNextDefaultPosition(float x, float y, float elementWidth, float elementHeight, bool useFieldOrigin, Zpl.FieldOrientation fieldOrientation, SKPoint currentPosition)
        {
            if (useFieldOrigin)
            {
                switch (fieldOrientation)
                {
                    case Zpl.FieldOrientation.Normal:
                        return new SKPoint(x + elementWidth, y + elementHeight);
                    case Zpl.FieldOrientation.Rotated90:
                        return new SKPoint(x, y + elementHeight);
                    case Zpl.FieldOrientation.Rotated180:
                        return new SKPoint(x - elementWidth, y);
                    case Zpl.FieldOrientation.Rotated270:
                        return new SKPoint(x, y - elementHeight);
                }
            }
            else
            {
                switch (fieldOrientation)
                {
                    case Zpl.FieldOrientation.Normal:
                        return new SKPoint(x + elementWidth, y);
                    case Zpl.FieldOrientation.Rotated90:
                        return new SKPoint(x, y + elementWidth);
                    case Zpl.FieldOrientation.Rotated180:
                        return new SKPoint(x - elementWidth, y);
                    case Zpl.FieldOrientation.Rotated270:
                        return new SKPoint(x, y - elementWidth);
                }
            }

            return currentPosition;
        }

    }
}
