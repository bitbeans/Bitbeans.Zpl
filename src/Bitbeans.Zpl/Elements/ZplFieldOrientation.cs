using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Field orientation (<c>^FW</c>): sets the default orientation and justification of all following fields that do not specify their own.</summary>
    public class ZplFieldOrientation : ZplElementBase
    {
        /// <summary>Default orientation of the following fields.</summary>
        public FieldOrientation FieldOrientation { get; private set; }
        /// <summary>Default justification of the following fields.</summary>
        public FieldJustification FieldJustification { get; private set; }

        /// <summary>Creates a <c>^FW</c> command.</summary>
        /// <param name="fieldOrientation">Default field orientation (<c>^FW</c> parameter <c>r</c>).</param>
        /// <param name="fieldJustification">Default field justification (<c>^FW</c> parameter <c>z</c>); <see cref="FieldJustification.None"/> omits it.</param>
        public ZplFieldOrientation(FieldOrientation fieldOrientation, FieldJustification fieldJustification = FieldJustification.None)
        {
            this.FieldOrientation = fieldOrientation;
            this.FieldJustification = fieldJustification;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            return new[] { $"^FW{RenderFieldOrientation(this.FieldOrientation)},{RenderFieldJustification(this.FieldJustification)}".TrimEnd(',') };
        }

    }
}
