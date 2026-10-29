using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Image move (<c>^IM</c>): moves an image from the storage area directly into the bitmap. Identical to recalling a graphic (<c>^XG</c>) without sizing parameters. Rendered as <c>^IMd:name.ext^FS</c> (see <see cref="FileName"/>).</summary>
    /// <remarks>
    /// Format: <c>^IMd:o.x</c>
    /// </remarks>
    public class ZplImageMove : ZplPositionedElementBase
    {
        /// <summary>Storage device of the object (<c>R</c>, <c>E</c>, <c>B</c>, <c>A</c> or <c>Z</c>).</summary>
        public char StorageDevice { get; private set; }
        /// <summary>Name of the stored object, normally without extension; <c>UNKNOWN</c> if an empty name was given.</summary>
        public string ObjectName { get; private set; }
        /// <summary>Extension of the stored object, e.g. <c>GRF</c> or <c>PNG</c>.</summary>
        public string Extension { get; private set; }
        /// <summary>Object name and extension as stored on the printer, e.g. <c>LOGO.PNG</c>; just <see cref="ObjectName"/> if <see cref="Extension"/> is empty.</summary>
        public string FileName => Extension.Length == 0 ? ObjectName : $"{ObjectName}.{Extension}";

        /// <summary>Creates an image move.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="storageDevice">Storage device of the object (<c>R</c>, <c>E</c>, <c>B</c> or <c>A</c>).</param>
        /// <param name="objectName">Object name; normally without extension. An empty name is replaced by <c>UNKNOWN</c>, the name the printer uses when it is omitted.</param>
        /// <param name="extension">Object extension without dot, e.g. <c>GRF</c>; may be empty if <paramref name="objectName"/> already contains it.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="objectName"/> or <paramref name="extension"/> is <see langword="null"/>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="storageDevice"/> is not one of <c>R</c>, <c>E</c>, <c>B</c>, <c>A</c>, <c>Z</c>; <paramref name="objectName"/>/<paramref name="extension"/> contains <c>^</c>, <c>~</c>, comma, colon, whitespace or control characters (the extension also no dot, but it may be empty).</exception>
        public ZplImageMove(
            int positionX, 
            int positionY, 
            char storageDevice, 
            string objectName, 
            string extension, 
            bool bottomToTop = false, 
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification)
        {
            StorageDevice = ZplIdentifier.Device(storageDevice, nameof(storageDevice));
            ObjectName = ZplIdentifier.ObjectName(objectName, nameof(objectName), allowDot: true);
            Extension = ZplIdentifier.Name(extension, nameof(extension), allowDot: false, allowEmpty: true);
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^IM{StorageDevice}:{FileName}^FS");

            return result;
        }
    }
}
