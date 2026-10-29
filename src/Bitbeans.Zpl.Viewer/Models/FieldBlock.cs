using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>The parameters of a field block (<c>^FB</c>).</summary>
    public class FieldBlock
    {
        /// <summary>Gets or sets the width of the text block lines in dots (<c>a</c> parameter).</summary>
        public int WidthOfTextBlockLine { get; set; }
        /// <summary>Gets or sets the maximum number of lines in the text block (<c>b</c> parameter).</summary>
        public int MaximumNumberOfLinesInTextBlock { get; set; }
        /// <summary>Gets or sets the space added to (or, if negative, removed from) the line spacing in dots (<c>c</c> parameter).</summary>
        public int AddOrDeleteSpaceBetweenLines { get; set; }
        /// <summary>Gets or sets the text justification (<c>d</c> parameter).</summary>
        public TextJustification TextJustification { get; set; }
        /// <summary>Gets or sets the hanging indent of the second and remaining lines in dots (<c>e</c> parameter).</summary>
        public int HangingIndentOfTheSecondAndRemainingLines { get; set; }
    }
}
