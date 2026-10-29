namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The barcode defaults set with <c>^BY</c>.</summary>
    public class BarcodeInfo
    {
        /// <summary>Gets or sets the module width in dots (<c>w</c> parameter of <c>^BY</c>); the default is 2.</summary>
        public int ModuleWidth { get; set; } = 2;
        /// <summary>Gets or sets the wide bar to narrow bar width ratio (<c>r</c> parameter of <c>^BY</c>); the default is 3.0.</summary>
        public double WideBarToNarrowBarWidthRatio { get; set; } = 3.0;
        /// <summary>Gets or sets the barcode height in dots (<c>h</c> parameter of <c>^BY</c>); the default is 10.</summary>
        public int Height { get; set; } = 10;
    }
}
