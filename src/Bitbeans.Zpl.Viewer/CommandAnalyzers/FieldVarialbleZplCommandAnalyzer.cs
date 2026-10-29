namespace Bitbeans.Zpl.Viewer.CommandAnalyzers
{
    // TODO: fix virtual printer, must enable the MC command
    // TODO: factor out common parts from FieldDataZplCommandAnalyzer so both can inherit
    // This is currently just a hack to be able to visualize single ups zpl.
    // The FV command is normally used with the MC command when printing multiple labels of a same pattern.
    // The MC command allows us to "save" the first label as a template
    // using FDs as static elements of template and FVs as variable parts.
    // Subsequent labels only require FV to draw the variable parts.
    /// <summary>Analyzes <c>^FV</c> (field variable). It behaves like <see cref="FieldDataZplCommandAnalyzer"/> (^FD) and does not implement the ^MC template handling.</summary>
    public class FieldVariableZplCommandAnalyzer : FieldDataZplCommandAnalyzer
    {
        /// <summary>Initializes a new instance of the <see cref="FieldVariableZplCommandAnalyzer"/> class for the <c>^FV</c> command.</summary>
        public FieldVariableZplCommandAnalyzer() : base("^FV") { }
    }
}
