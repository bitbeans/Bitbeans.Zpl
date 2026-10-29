using Bitbeans.Zpl.Helpers;
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
        /// <summary>Storage device of the graphic (<c>R</c>, <c>E</c>, <c>B</c>, <c>A</c> or <c>Z</c>).</summary>
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
        /// <param name="storageDevice">Storage device letter: <c>R</c> (DRAM), <c>E</c> (flash), <c>B</c> (memory card), <c>A</c> (auto) or <c>Z</c> (firmware).</param>
        /// <param name="imageName">Name of the stored graphic, 1 to 8 characters (normally without extension, which is always <c>.GRF</c>; a dot is tolerated because the viewer passes parsed file names); no <c>^ ~ , :</c>, whitespace or control characters.</param>
        /// <param name="magnificationFactorX">Magnification in x direction, 1-10; default 1.</param>
        /// <param name="magnificationFactorY">Magnification in y direction, 1-10; default 1.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/> (see <see cref="ZplFieldTypeset"/>).</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        /// <exception cref="ArgumentNullException"><paramref name="imageName"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="storageDevice"/> is not one of <c>R</c>, <c>E</c>, <c>B</c>, <c>A</c>, <c>Z</c>, or <paramref name="imageName"/> is empty, longer than 8 characters or contains <c>^</c>, <c>~</c>, comma, colon, whitespace or control characters.</exception>
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
            ZplIdentifier.Name(imageName, nameof(imageName), allowDot: true, maxLength: 8);

            _extension = "GRF";

            StorageDevice = ZplIdentifier.Device(storageDevice, nameof(storageDevice));
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
