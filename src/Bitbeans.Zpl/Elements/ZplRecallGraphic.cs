using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Recall graphic (<c>^XG</c>): prints a graphic (<c>.GRF</c>) stored in printer memory, with the field position.</summary>
    /// <remarks>
    /// Format: <c>^XGd:o.x,mx,my</c>
    /// </remarks>
    public class ZplRecallGraphic : ZplPositionedElementBase
    {
        /// <summary>Storage device of the graphic (<c>R</c>, <c>E</c>, <c>B</c> or <c>A</c>).</summary>
        public char StorageDevice { get; private set; }
        /// <summary>Name of the stored graphic (without extension, at most 8 characters); the extension is always <c>.GRF</c>.</summary>
        public string ImageName { get; private set; }
        private string _extension { get; set; }
        /// <summary>Magnification in x direction (<c>mx</c> parameter, 1 to 10).</summary>
        public int MagnificationFactorX { get; private set; }
        /// <summary>Magnification in y direction (<c>my</c> parameter, 1 to 10).</summary>
        public int MagnificationFactorY { get; private set; }

        /// <summary>Creates a recall graphic.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="storageDevice">Storage device letter: <c>R</c> (DRAM), <c>E</c> (flash), <c>B</c> (memory card) or <c>A</c> (auto).</param>
        /// <param name="imageName">Name of the stored graphic, maximum 8 characters.</param>
        /// <param name="magnificationFactorX">Magnification in x direction, 1-10; default 1.</param>
        /// <param name="magnificationFactorY">Magnification in y direction, 1-10; default 1.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/> (see <see cref="ZplFieldTypeset"/>).</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        /// <exception cref="ArgumentException"><paramref name="imageName"/> is longer than 8 characters.</exception>
        public ZplRecallGraphic(
            int positionX,
            int positionY,
            char storageDevice,
            string imageName,
            int magnificationFactorX = 1,
            int magnificationFactorY = 1,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification)
        {
            if (imageName.Length > 8)
            {
                throw new ArgumentException("Maximum length of 8 characters exceeded", nameof(imageName));
            }

            _extension = "GRF";

            StorageDevice = storageDevice;
            ImageName = imageName;
            MagnificationFactorX = magnificationFactorX;
            MagnificationFactorY = magnificationFactorY;
        }

        ///<inheritdoc/>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^XG{StorageDevice}:{ImageName}.{_extension},{MagnificationFactorX},{MagnificationFactorY}^FS");

            return result;
        }
    }
}
