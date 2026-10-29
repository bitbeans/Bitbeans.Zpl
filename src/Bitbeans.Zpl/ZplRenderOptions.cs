using System;

namespace Bitbeans.Zpl
{
    /// <summary>Settings that control how ZPL elements are rendered.</summary>
    public class ZplRenderOptions
    {
        /// <summary>Wrap the output in <c>^XA</c> and <c>^XZ</c>. Default: <see langword="true"/>.</summary>
        public bool AddStartEndFormat { get; set; }
        /// <summary>Add <c>^LH0,0</c> after the start of the format. Default: <see langword="true"/>.</summary>
        public bool AddDefaultLabelHome { get; set; }

        /// <summary>Render the <see cref="Elements.ZplElementBase.Comments"/> of elements as <c>^FX</c> comments. Default: <see langword="false"/>.</summary>
        public bool DisplayComments { get; set; }

        /// <summary>Add an empty line before each element to make the output easier to read. Default: <see langword="false"/>.</summary>
        public bool AddEmptyLineBeforeElementStart { get; set; }

        /// <summary>Resolution in dots per inch the element values (positions, sizes) are specified for. Together with <see cref="TargetPrintDpi"/> it defines <see cref="ScaleFactor"/>.</summary>
        /// <remarks>
        /// Default: 203
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than 0.</exception>
        public int SourcePrintDpi
        {
            get => _sourcePrintDpi;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0);
                _sourcePrintDpi = value;
            }
        }

        private int _sourcePrintDpi;
        private int _targetPrintDpi;

        /// <summary>Resolution in dots per inch of the printer the ZPL is generated for. Together with <see cref="SourcePrintDpi"/> it defines <see cref="ScaleFactor"/>.</summary>
        /// <remarks>
        /// Default: 203
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The value is not greater than 0.</exception>
        public int TargetPrintDpi
        {
            get => _targetPrintDpi;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, 0);
                _targetPrintDpi = value;
            }
        }

        /// <summary>Join the lines of each element into one line, so that every element is rendered on a single line. Default: <see langword="false"/>.</summary>
        public bool CompressedRendering { get; set; }

        /// <summary>Factor applied to dot values (<see cref="TargetPrintDpi"/> divided by <see cref="SourcePrintDpi"/>).</summary>
        /// <remarks>
        /// Positions, sizes, heights and module widths (<c>^BY</c>, magnification factors; see <see cref="ScaleModuleWidth(int)"/>) are scaled. Corner rounding indexes (<c>^GB</c>) are not.
        /// </remarks>
        public double ScaleFactor { get { return (double)TargetPrintDpi / SourcePrintDpi; } }

        /// <summary>Scales a dot value by <see cref="ScaleFactor"/>, rounding half away from zero. A non-zero input never becomes 0 (it yields at least 1, or at most -1), so thin lines stay visible.</summary>
        /// <param name="input">Value in dots at <see cref="SourcePrintDpi"/>.</param>
        /// <returns>Value in dots at <see cref="TargetPrintDpi"/>.</returns>
        public int Scale(int input)
        {
            int scaled = (int)Math.Round(input * ScaleFactor, MidpointRounding.AwayFromZero);
            return scaled == 0 ? Math.Sign(input) : scaled;
        }

        /// <summary>Scales a module width or magnification factor (<c>^BY</c> width, <c>^BQ</c>/<c>^BO</c> magnification) by <see cref="ScaleFactor"/>. The result is rounded and limited to the range 1 to 10 that ZPL allows.</summary>
        /// <param name="input">Value in dots at <see cref="SourcePrintDpi"/>.</param>
        /// <returns>The scaled value, 1 to 10.</returns>
        public int ScaleModuleWidth(int input)
        {
            return Math.Clamp(Scale(input), 1, 10);
        }

        /// <summary>Scales a value by <see cref="ScaleFactor"/> without rounding.</summary>
        /// <param name="input">Value in dots at <see cref="SourcePrintDpi"/>.</param>
        /// <returns>Value in dots at <see cref="TargetPrintDpi"/>.</returns>
        public double Scale(double input)
        {
            return input * ScaleFactor;
        }

        /// <summary>Creates options with default values: <c>^XA</c>/<c>^XZ</c> and <c>^LH0,0</c> are added, no scaling (203 dpi to 203 dpi).</summary>
        public ZplRenderOptions()
        {
            AddStartEndFormat = true;
            AddDefaultLabelHome = true;
            SourcePrintDpi = TargetPrintDpi = 203;
        }
    }
}
