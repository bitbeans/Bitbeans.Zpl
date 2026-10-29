using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Graphic field (<c>^GF</c>): image data sent inline with the position. The values are rendered as given and are not scaled.</summary>
    public class ZplGraphicField : ZplPositionedElementBase
    {
        /// <summary>Compression type (<c>a</c> parameter): <c>A</c> ASCII hexadecimal, <c>B</c> binary, <c>C</c> compressed binary.</summary>
        public char CompressionType { get; private set; }
        /// <summary>Total number of bytes of the graphic (<c>b</c> parameter).</summary>
        public int BinaryByteCount { get; private set; }
        /// <summary>Total number of bytes sent in the data (<c>c</c> parameter); equal to <see cref="BinaryByteCount"/> for ASCII hexadecimal data.</summary>
        public int GraphicFieldCount { get; private set; }
        /// <summary>Number of bytes per image row (<c>d</c> parameter).</summary>
        public int BytesPerRow { get; private set; }
        /// <summary>The image data, rendered unchanged after the parameters.</summary>
        public string Data { get; private set; }

        /// <summary>Creates a graphic field.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="binaryByteCount">Total number of bytes of the graphic.</param>
        /// <param name="graphicFieldCount">Total number of bytes sent in the data.</param>
        /// <param name="bytesPerRow">Number of bytes per image row.</param>
        /// <param name="data">The image data in the format given by <paramref name="compressionType"/>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="compressionType"><c>A</c> (ASCII hexadecimal, default), <c>B</c> or <c>C</c>.</param>
        public ZplGraphicField(
            int positionX,
            int positionY,
            int binaryByteCount,
            int graphicFieldCount,
            int bytesPerRow,
            string data,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            char compressionType = 'A')
            : base(positionX, positionY, bottomToTop, useDefaultPosition)
        {
            CompressionType = compressionType;
            BinaryByteCount = binaryByteCount;
            GraphicFieldCount = graphicFieldCount;
            BytesPerRow = bytesPerRow;
            Data = data;
        }

        /// <summary>Renders the position and <c>^GFa,b,c,d,data^FS</c>.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the graphic field.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();
            result.AddRange(RenderPosition(context));
            result.Add($"^GF{CompressionType},{BinaryByteCount},{GraphicFieldCount},{BytesPerRow},{Data}^FS");

            return result;
        }
    }
}
