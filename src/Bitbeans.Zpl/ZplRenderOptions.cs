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
        public int SourcePrintDpi { get; set; }

        /// <summary>Resolution in dots per inch of the printer the ZPL is generated for. Together with <see cref="SourcePrintDpi"/> it defines <see cref="ScaleFactor"/>.</summary>
        /// <remarks>
        /// Default: 203
        /// </remarks>
        public int TargetPrintDpi { get; set; }

        /// <summary>Join the lines of each element into one line, so that every element is rendered on a single line. Default: <see langword="false"/>.</summary>
        public bool CompressedRendering { get; set; }

        /// <summary>Factor applied to dot values (<see cref="TargetPrintDpi"/> divided by <see cref="SourcePrintDpi"/>).</summary>
        /// <remarks>
        /// Positions, sizes and heights are scaled; module widths (<c>^BY</c>) are not.
        /// </remarks>
        public double ScaleFactor { get { return (double)TargetPrintDpi / SourcePrintDpi; } }

        /// <summary>Scales a dot value by <see cref="ScaleFactor"/>, truncating toward zero.</summary>
        /// <param name="input">Value in dots at <see cref="SourcePrintDpi"/>.</param>
        /// <returns>Value in dots at <see cref="TargetPrintDpi"/>.</returns>
        public int Scale(int input)
        {
            return (int)(input * ScaleFactor);
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
