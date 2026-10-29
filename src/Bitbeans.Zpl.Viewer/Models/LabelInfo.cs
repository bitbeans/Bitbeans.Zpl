using Bitbeans.Zpl.Elements;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>A label found in the ZPL text.</summary>
    public class LabelInfo
    {
        /// <summary>Gets or sets the name under which the label is stored as a format (<c>^DF</c>), or <see langword="null"/> if it is not a downloaded format.</summary>
        public string DownloadFormatName { get; set; }
        /// <summary>Gets or sets the elements of the label.</summary>
        public ZplElementBase[] ZplElements { get; set; }
    }
}
