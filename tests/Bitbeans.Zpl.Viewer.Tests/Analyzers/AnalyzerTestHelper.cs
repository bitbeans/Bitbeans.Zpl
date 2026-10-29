using System.Linq;
using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.CommandAnalyzers;
using Bitbeans.Zpl.Viewer.Models;

namespace Bitbeans.Zpl.Viewer.Tests.Analyzers
{
    /// <summary>
    /// Shared helpers to feed ZPL through the analyzer and inspect the result.
    /// </summary>
    internal static class AnalyzerTestHelper
    {
        /// <summary>
        /// Analyzes the zpl with a fresh analyzer (the virtual printer is stateful).
        /// </summary>
        public static AnalyzeInfo Analyze(string zpl, IPrinterStorage storage = null)
        {
            return new ZplAnalyzer(storage ?? new PrinterStorage()).Analyze(zpl);
        }

        /// <summary>
        /// Wraps the commands into ^XA ... ^XZ.
        /// </summary>
        public static string Label(string commands)
        {
            return "^XA" + commands + "^XZ";
        }

        /// <summary>
        /// Analyzes a single label and returns its elements; asserts no errors and no unknown commands.
        /// </summary>
        public static ZplElementBase[] Elements(string commands)
        {
            AnalyzeInfo info = Analyze(Label(commands));
            Assert.IsEmpty(info.Errors, string.Join("\n", info.Errors));
            Assert.IsEmpty(info.UnknownCommands);
            Assert.HasCount(1, info.LabelInfos);
            return info.LabelInfos[0].ZplElements;
        }

        /// <summary>
        /// Analyzes a single label that must contain exactly one element of the given type.
        /// </summary>
        public static T Single<T>(string commands) where T : ZplElementBase
        {
            ZplElementBase[] elements = Elements(commands);
            Assert.HasCount(1, elements);
            Assert.IsInstanceOfType<T>(elements[0]);
            return (T)elements[0];
        }

        /// <summary>
        /// Runs one analyzer directly on a command using the given printer.
        /// </summary>
        public static ZplElementBase Run(IZplCommandAnalyzer analyzer, string command, VirtualPrinter printer, IPrinterStorage storage = null)
        {
            return analyzer.Analyze(command, printer, storage ?? new PrinterStorage());
        }

        /// <summary>
        /// Runs one analyzer on a command with a fresh printer and returns the pending field data.
        /// </summary>
        public static T FieldData<T>(IZplCommandAnalyzer analyzer, string command) where T : FieldDataBase
        {
            VirtualPrinter printer = new();
            Assert.IsNull(Run(analyzer, command, printer));
            Assert.IsInstanceOfType<T>(printer.NextElementFieldData);
            return (T)printer.NextElementFieldData;
        }

        public static string[] Errors(string zpl)
        {
            return Analyze(zpl).Errors;
        }

        public static ZplElementBase[] AllElements(AnalyzeInfo info)
        {
            return info.LabelInfos.SelectMany(l => l.ZplElements).ToArray();
        }
    }
}
