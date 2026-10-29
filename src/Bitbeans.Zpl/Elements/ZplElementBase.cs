using System;
using System.Collections.Generic;

namespace Bitbeans.Zpl.Elements
{
    /// <summary>Base class of all ZPL elements. An element renders itself to one or more lines of ZPL II code.</summary>
    public abstract class ZplElementBase
    {
        /// <summary>Comments describing the element. They are rendered as <c>^FX</c> comment by <see cref="ZplEngine"/> when <see cref="ZplRenderOptions.DisplayComments"/> is set.</summary>
        public List<string> Comments { get; protected set; }

        /// <summary>Indicates to the rendering process whether this element is rendered. Disabled elements are skipped by <see cref="ZplEngine"/>. Default: <see langword="true"/>.</summary>
        public bool IsEnabled { get; set; }

        /// <summary>Optionally identifies the element for later lookup or manipulation. Not rendered.</summary>
        public string Id { get; set; }

        /// <summary>Initializes the element: no comments, <see cref="IsEnabled"/> is <see langword="true"/>.</summary>
        public ZplElementBase()
        {
            Comments = new List<string>();
            IsEnabled = true;
        }

        /// <summary>Renders the element with default <see cref="ZplRenderOptions"/>.</summary>
        /// <returns>The ZPL lines of the element.</returns>
        public IEnumerable<string> Render()
        {
            return Render(new ZplRenderOptions());
        }

        /// <summary>Renders the element with default <see cref="ZplRenderOptions"/> and joins the lines with a space.</summary>
        /// <returns>The ZPL code on a single line.</returns>
        public string RenderToString()
        {
            return string.Join(" ", Render());
        }

        /// <summary>Renders the element to ZPL lines.</summary>
        /// <param name="context">Render options; positions and sizes are scaled with <see cref="ZplRenderOptions.ScaleFactor"/>.</param>
        /// <returns>The ZPL lines of the element.</returns>
        public abstract IEnumerable<string> Render(ZplRenderOptions context);

        /// <summary>Converts a field orientation to its ZPL letter.</summary>
        /// <param name="fieldOrientation">The orientation.</param>
        /// <returns><c>N</c>, <c>R</c>, <c>I</c> or <c>B</c>.</returns>
        /// <exception cref="NotImplementedException"><paramref name="fieldOrientation"/> is not a defined value.</exception>
        public string RenderFieldOrientation(FieldOrientation fieldOrientation)
        {
            switch (fieldOrientation)
            {
                case FieldOrientation.Normal:
                    return "N";
                case FieldOrientation.Rotated90:
                    return "R";
                case FieldOrientation.Rotated180:
                    return "I";
                case FieldOrientation.Rotated270:
                    return "B";
            }

            throw new NotImplementedException("Unknown Field Orientation");
        }

        /// <summary>Converts a Data Matrix quality level to its ZPL value.</summary>
        /// <param name="qualityLevel">The quality level.</param>
        /// <returns><c>0</c>, <c>50</c>, <c>80</c>, <c>100</c>, <c>140</c> or <c>200</c>.</returns>
        /// <exception cref="NotImplementedException"><paramref name="qualityLevel"/> is not a defined value.</exception>
        public string RenderQualityLevel(QualityLevel qualityLevel)
        {
            switch (qualityLevel)
            {
                case QualityLevel.ECC0:
                    return "0";
                case QualityLevel.ECC50:
                    return "50";
                case QualityLevel.ECC80:
                    return "80";
                case QualityLevel.ECC100:
                    return "100";
                case QualityLevel.ECC140:
                    return "140";
                case QualityLevel.ECC200:
                    return "200";
            }

            throw new NotImplementedException("Unknown Quality Level");
        }

        /// <summary>Converts a field justification to its ZPL value.</summary>
        /// <param name="fieldJustification">The justification.</param>
        /// <returns><c>0</c> (left), <c>1</c> (right), <c>2</c> (auto) or an empty string for <see cref="FieldJustification.None"/>.</returns>
        /// <exception cref="NotImplementedException"><paramref name="fieldJustification"/> is not a defined value.</exception>
        public string RenderFieldJustification(FieldJustification fieldJustification)
        {
            switch (fieldJustification)
            {
                case FieldJustification.None:
                    return string.Empty;
                case FieldJustification.Left:
                    return "0";
                case FieldJustification.Right:
                    return "1";
                case FieldJustification.Auto:
                    return "2";
            }

            throw new NotImplementedException("Unknown Field Justification");
        }

        /// <summary>Converts a line color to its ZPL letter.</summary>
        /// <param name="lineColor">The line color.</param>
        /// <returns><c>B</c> or <c>W</c>.</returns>
        /// <exception cref="NotImplementedException"><paramref name="lineColor"/> is not a defined value.</exception>
        public string RenderLineColor(LineColor lineColor)
        {
            switch (lineColor)
            {
                case LineColor.Black:
                    return "B";
                case LineColor.White:
                    return "W";
            }

            throw new NotImplementedException("Unknown Line Color");
        }

        /// <summary>Converts a QR Code error correction level to its ZPL letter.</summary>
        /// <param name="errorCorrectionLevel">The error correction level.</param>
        /// <returns><c>H</c>, <c>Q</c>, <c>M</c> or <c>L</c>.</returns>
        /// <exception cref="NotImplementedException"><paramref name="errorCorrectionLevel"/> is not a defined value.</exception>
        public string RenderErrorCorrectionLevel(ErrorCorrectionLevel errorCorrectionLevel)
        {
            switch (errorCorrectionLevel)
            {
                case ErrorCorrectionLevel.UltraHighReliability:
                    return "H";
                case ErrorCorrectionLevel.HighReliability:
                    return "Q";
                case ErrorCorrectionLevel.Standard:
                    return "M";
                case ErrorCorrectionLevel.HighDensity:
                    return "L";
            }

            throw new NotImplementedException("Unknown Error Correction Level");
        }

        /// <summary>Converts a boolean to the ZPL yes/no letter.</summary>
        /// <param name="value">The value.</param>
        /// <returns><c>Y</c> for <see langword="true"/>, otherwise <c>N</c>.</returns>
        public string RenderBoolean(bool value)
        {
            return value ? "Y" : "N";
        }

        /// <summary>Renders the element with default <see cref="ZplRenderOptions"/> and joins the lines with line feeds.</summary>
        /// <returns>The ZPL code, lines separated by <c>\n</c>.</returns>
        public string ToZplString()
        {
            return ToZplString(new ZplRenderOptions());
        }

        /// <summary>Renders the element and joins the lines with line feeds.</summary>
        /// <param name="context">Render options.</param>
        /// <returns>The ZPL code, lines separated by <c>\n</c>.</returns>
        public string ToZplString(ZplRenderOptions context)
        {
            return string.Join("\n", Render(context));
        }
    }
}
