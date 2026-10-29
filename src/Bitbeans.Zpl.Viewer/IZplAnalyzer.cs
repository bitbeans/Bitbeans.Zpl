using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>Analyzes ZPL text into labels and elements.</summary>
    public interface IZplAnalyzer
    {
        /// <summary>Analyzes ZPL text.</summary>
        /// <param name="zplData">The ZPL source text.</param>
        /// <returns>The analysis result with the labels, unknown commands and errors.</returns>
        AnalyzeInfo Analyze(string zplData);
    }
}
