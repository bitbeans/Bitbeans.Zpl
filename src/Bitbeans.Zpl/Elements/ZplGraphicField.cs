using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Graphic field (<c>^GF</c>): image data sent inline with the position. The values and the graphic data are rendered as given: only the position is scaled by the DPI, the image itself cannot be resampled without an imaging library, so it is not DPI scaled (use <see cref="ZplDownloadGraphics"/> or <see cref="ZplDownloadObjects"/> with an <see cref="ImageConverters.IImageConverter"/> for that).</summary>
    public class ZplGraphicField : ZplPositionedElementBase
    {
        /// <summary>Compression type (<c>a</c> parameter): <c>A</c> ASCII hexadecimal, <c>B</c> binary, <c>C</c> compressed binary.</summary>
        public char CompressionType { get; private set; }
        /// <summary>Binary byte count (<c>b</c> parameter): total number of bytes to be transmitted for the graphic, i.e. the number of bytes that follow in the data (<see cref="Data"/>); for ASCII hexadecimal data it equals <see cref="GraphicFieldCount"/>.</summary>
        public int BinaryByteCount { get; private set; }
        /// <summary>Graphic field count (<c>c</c> parameter): total number of bytes comprising the graphic format (bytes per row times the number of rows, the size of the uncompressed image); divided by <see cref="BytesPerRow"/> it gives the number of image rows. For ASCII hexadecimal data it equals <see cref="BinaryByteCount"/>.</summary>
        public int GraphicFieldCount { get; private set; }
        /// <summary>Number of bytes per image row (<c>d</c> parameter).</summary>
        public int BytesPerRow { get; private set; }
        /// <summary>The graphic data as sent with <c>^GF</c>, rendered unchanged after the parameters: ASCII hexadecimal (for example <c>FF00</c>, one row is <see cref="BytesPerRow"/> bytes), optionally compressed with the ZPL run-length scheme, or <c>:B64:</c> / <c>:Z64:</c> encoded data. The viewer converts it to an image when drawing.</summary>
        public string Data { get; private set; }

        /// <summary>Creates a graphic field.</summary>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="binaryByteCount">Binary byte count (<c>b</c> parameter), see <see cref="BinaryByteCount"/>.</param>
        /// <param name="graphicFieldCount">Graphic field count (<c>c</c> parameter), see <see cref="GraphicFieldCount"/>.</param>
        /// <param name="bytesPerRow">Number of bytes per image row.</param>
        /// <param name="data">The image data in the format given by <paramref name="compressionType"/>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="compressionType"><c>A</c> (ASCII hexadecimal, default), <c>B</c> or <c>C</c>.</param>
        /// <param name="fieldJustification">Field justification passed to the position command (<c>^FO</c>/<c>^FT</c>, third parameter); <see cref="FieldJustification.None"/> omits it. <see cref="FieldJustification.Right"/> makes the position the right edge of the element.</param>
        public ZplGraphicField(
            int positionX,
            int positionY,
            int binaryByteCount,
            int graphicFieldCount,
            int bytesPerRow,
            string data,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            char compressionType = 'A',
            FieldJustification fieldJustification = FieldJustification.None)
            : base(positionX, positionY, bottomToTop, useDefaultPosition, fieldJustification)
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
