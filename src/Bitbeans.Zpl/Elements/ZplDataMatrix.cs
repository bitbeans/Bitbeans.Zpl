using Bitbeans.Zpl.Helpers;
using System.Collections.Generic;
using System.Text;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Data Matrix bar code (<c>^BX</c>) with the field position and field data.</summary>
    /// <remarks>
    /// Format: <c>^BXo,h,s,c,r,f,g,a</c>. The extended parameters (columns, rows, format, escape character, aspect ratio) are only rendered when they differ from their defaults.
    /// </remarks>
    public class ZplDataMatrix : ZplFieldDataElementBase
    {
        private const int DefaultFormat = 6;
        private const char DefaultEscapeSequence = '~';

        /// <summary>Height of the individual symbol elements (modules) in dots (<c>h</c> parameter); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</summary>
        public int Height { get; protected set; }

        /// <summary>Quality level / error correction (<c>s</c> parameter).</summary>
        public QualityLevel QualityLevel { get; protected set; }

        /// <summary>Number of columns to encode (<c>c</c> parameter), or <see langword="null"/> for automatic size; only used with quality levels below ECC 200.</summary>
        public int? Columns { get; protected set; }

        /// <summary>Number of rows to encode (<c>r</c> parameter), or <see langword="null"/> for automatic size; only used with quality levels below ECC 200.</summary>
        public int? Rows { get; protected set; }

        /// <summary>Format ID (<c>f</c> parameter, 0 to 6); default 6. Only used with quality levels below ECC 200.</summary>
        public int Format { get; protected set; }

        /// <summary>Escape (control sequence) character (<c>g</c> parameter); default <c>~</c>.</summary>
        public char EscapeSequence { get; protected set; }

        /// <summary>Aspect ratio (<c>a</c> parameter): 1 for square, 2 for rectangular; <see langword="null"/> keeps the printer default. ECC 200 only.</summary>
        public int? AspectRatio { get; protected set; }

        /// <summary>Creates a Data Matrix bar code.</summary>
        /// <param name="content">The data to encode, written to the <c>^FD</c> field data command.</param>
        /// <param name="positionX">X position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="positionY">Y position of the field in dots, relative to the label home (<c>^LH</c>); scaled by <see cref="ZplRenderOptions.ScaleFactor"/> when rendered.</param>
        /// <param name="height">Height of the symbol modules in dots; default 100.</param>
        /// <param name="qualityLevel">Quality level; default <see cref="QualityLevel.ECC0"/> (note that ECC 200 is the recommended level).</param>
        /// <param name="fieldOrientation">Field orientation: normal (<c>N</c>), rotated 90 degrees (<c>R</c>), inverted 180 degrees (<c>I</c>) or read from bottom up (<c>B</c>).</param>
        /// <param name="hexadecimalIndicator">Hexadecimal escape character for <c>^FH</c>, or <see langword="null"/> to not emit <c>^FH</c>. The default indicator <c>_</c> is rendered as a plain <c>^FH</c>.</param>
        /// <param name="bottomToTop"><see langword="true"/> to position the field with <c>^FT</c> (<see cref="ZplFieldTypeset"/>) instead of <c>^FO</c> (<see cref="ZplFieldOrigin"/>).</param>
        /// <param name="useDefaultPosition">Only evaluated together with <paramref name="bottomToTop"/>: <see langword="true"/> renders <c>^FT</c> without coordinates so the printer continues at its default position. Ignored for <c>^FO</c>.</param>
        /// <param name="columns">Number of columns to encode, <see langword="null"/> for automatic.</param>
        /// <param name="rows">Number of rows to encode, <see langword="null"/> for automatic.</param>
        /// <param name="format">Format ID (0-6); default 6.</param>
        /// <param name="escapeSequence">Escape character; default <c>~</c>.</param>
        /// <param name="aspectRatio">1 for square, 2 for rectangular, <see langword="null"/> for the printer default.</param>
        public ZplDataMatrix(
            string content,
            int positionX,
            int positionY,
            int height = 100,
            QualityLevel qualityLevel = QualityLevel.ECC0,
            FieldOrientation fieldOrientation = FieldOrientation.Normal,
            char? hexadecimalIndicator = null,
            bool bottomToTop = false,
            bool useDefaultPosition = false,
            int? columns = null,
            int? rows = null,
            int format = DefaultFormat,
            char escapeSequence = DefaultEscapeSequence,
            int? aspectRatio = null)
            : base(content, positionX, positionY, fieldOrientation, hexadecimalIndicator, bottomToTop, useDefaultPosition)
        {
            Height = height;
            QualityLevel = qualityLevel;
            Columns = columns;
            Rows = rows;
            Format = format;
            EscapeSequence = escapeSequence;
            AspectRatio = aspectRatio;
        }

        /// <summary>Renders <see cref="QualityLevel"/> as ZPL value.</summary>
        /// <returns><c>0</c>, <c>50</c>, <c>80</c>, <c>100</c>, <c>140</c> or <c>200</c>.</returns>
        protected string RenderQualityLevel()
        {
            return RenderQualityLevel(QualityLevel);
        }

        /// <summary>Renders the position, <c>^BXo,h,s[,c,r,f,g,a]</c> and the field data.</summary>
        /// <param name="context">Render options (scaling).</param>
        /// <returns>The ZPL lines of the bar code.</returns>
        public override IEnumerable<string> Render(ZplRenderOptions context)
        {
            //^FO100,100
            //^BXN,10,200
            //^FDZEBRA TECHNOLOGIES CORPORATION ^ FS
            List<string> result = new();
            result.AddRange(RenderPosition(context));
            List<string> parameters = new()
            {
                context.Scale(Height).ToString(),
                RenderQualityLevel()
            };

            bool hasExtendedParameters = Columns.HasValue || Rows.HasValue || Format != DefaultFormat || EscapeSequence != DefaultEscapeSequence || AspectRatio.HasValue;

            if (hasExtendedParameters)
            {
                parameters.Add(Columns?.ToString() ?? string.Empty);
                parameters.Add(Rows?.ToString() ?? string.Empty);
            }

            if (Format != DefaultFormat || EscapeSequence != DefaultEscapeSequence || AspectRatio.HasValue)
            {
                parameters.Add(Format != DefaultFormat ? Format.ToString() : string.Empty);
            }

            if (EscapeSequence != DefaultEscapeSequence || AspectRatio.HasValue)
            {
                parameters.Add(EscapeSequence.ToString());
            }

            if (AspectRatio.HasValue)
            {
                parameters.Add(AspectRatio.Value.ToString());
            }

            result.Add($"^BX{RenderFieldOrientation()},{string.Join(",", parameters)}");
            result.Add(RenderFieldDataSection());

            return result;
        }

    }
}
