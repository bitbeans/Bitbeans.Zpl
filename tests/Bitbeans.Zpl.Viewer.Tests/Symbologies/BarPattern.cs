using System.Linq;

namespace Bitbeans.Zpl.Viewer.Tests.Symbologies
{
    internal static class BarPattern
    {
        /// <summary>
        /// Converts a module array to a string of '1' (bar) and '0' (space).
        /// </summary>
        public static string ToText(bool[] modules)
        {
            return new string(modules.Select(module => module ? '1' : '0').ToArray());
        }
    }
}
