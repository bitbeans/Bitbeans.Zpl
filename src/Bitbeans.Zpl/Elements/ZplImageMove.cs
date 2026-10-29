using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Image move (<c>^IM</c>): moves an image from the storage area directly into the bitmap. Identical to recalling a graphic (<c>^XG</c>) without sizing parameters.</summary>
    /// <remarks>
    /// Format: <c>^IMd:o.x</c>
    /// </remarks>
    public class ZplImageMove : ZplPositionedElementBase
    {
        /// <summary>Storage device of the object (<c>R</c>, <c>E</c>, <c>B</c> or <c>A</c>).</summary>
        public char StorageDevice { get; private set; }
        /// <summary>Name of the stored object (without extension).</summary>
        public string ObjectName { get; private set; }
        /// <summary>Extension of the stored object, e.g. <c>GRF</c> or <c>PNG</c>.</summary>
        public string Extension { get; private set; }

        /// <summary>Creates an image move.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="storageDevice">Storage device of the object (<c>R</c>, <c>E</c>, <c>B</c> or <c>A</c>).</param>
        /// <param name="objectName">Object name without extension.</param>
        /// <param name="extension">Object extension, e.g. <c>GRF</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        public ZplImageMove(
            int positionX, 
            int positionY, 
            char storageDevice, 
            string objectName, 
            string extension, 
            bool bottomToTop = false, 
            bool useDefaultPosition = false)
            : base(positionX, positionY, bottomToTop, useDefaultPosition)
        {
            StorageDevice = storageDevice;
            ObjectName = objectName;
            Extension = extension;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^IM{StorageDevice}:{ObjectName}.{Extension}");

            return result;
        }
    }
}
