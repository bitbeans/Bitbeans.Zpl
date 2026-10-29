using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer
{
    public interface IZplAnalyzer
    {
        AnalyzeInfo Analyze(string zplData);
    }
}
