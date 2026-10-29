namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The result of analyzing ZPL text.</summary>
    public class AnalyzeInfo
    {
        /// <summary>Gets or sets the error messages of the analysis.</summary>
        public string[] Errors { get; set; }

        /// <summary>Gets or sets the commands that no analyzer handles.</summary>
        public string[] UnknownCommands { get; set; }

        /// <summary>Gets or sets the labels found in the ZPL text, one per <c>^XA</c>/<c>^XZ</c> block.</summary>
        public LabelInfo[] LabelInfos { get; set; }
    }
}
