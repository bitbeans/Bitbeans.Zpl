using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of elements that are placed on the label with a field position command: <c>^FO</c> (<see cref="ZplFieldOrigin"/>) or, for bottom-to-top positioning, <c>^FT</c> (<see cref="ZplFieldTypeset"/>).</summary>
    public abstract class ZplPositionedElementBase : ZplElementBase
    {
        /// <summary>X position in dots, relative to the label home (<c>^LH</c>).</summary>
        public int PositionX { get; private set; }
        /// <summary>Y position in dots, relative to the label home (<c>^LH</c>).</summary>
        public int PositionY { get; private set; }
        /// <summary>When <see langword="true"/> and <c>^FT</c> is used, the coordinates are omitted. Has no effect for <c>^FO</c>.</summary>
        public bool UseDefaultPosition { get; private set; }
        /// <summary>The <c>^FO</c> position, or <see langword="null"/> if the element is positioned with <c>^FT</c>.</summary>
        public ZplFieldOrigin FieldOrigin { get; private set; }
        /// <summary>The <c>^FT</c> position, or <see langword="null"/> if the element is positioned with <c>^FO</c>.</summary>
        public ZplFieldTypeset FieldTypeset { get; private set; }

        /// <summary>Initializes the element and creates its position command.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="bottomToTop"><see langword="true"/> to use <c>^FT</c> (<see cref="ZplFieldTypeset"/>), <see langword="false"/> to use <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification passed to the position command.</param>
        public ZplPositionedElementBase(int positionX, int positionY, bool bottomToTop, bool useDefaultPosition, FieldJustification fieldJustification = FieldJustification.None) : base()
        {
            this.UseDefaultPosition = useDefaultPosition;
            
            if (bottomToTop)
            {
                FieldTypeset = new ZplFieldTypeset(positionX, positionY, fieldJustification, useDefaultPosition);
                PositionX = positionX;
                PositionY = positionY;
                return;
            }

            FieldOrigin = new ZplFieldOrigin(positionX, positionY, fieldJustification);
            PositionX = positionX;
            PositionY = positionY;
        }

        /// <summary>Renders the position command (<c>^FO</c> or <c>^FT</c>).</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL line of the position command.</returns>
        public IEnumerable<string> RenderPosition(ZplRenderOptions context)
        {
            if (FieldOrigin != null)
            {
                return FieldOrigin.Render(context);
            }

            return FieldTypeset.Render(context);
        }
    }
}
