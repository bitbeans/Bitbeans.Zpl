using Bitbeans.Zpl.Viewer.Models;

using System.Collections.Generic;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>
    /// Helper service responsible of merging <see cref="LabelInfo"/>s
    /// based on "format" data (templating).
    /// </summary>
    public interface IFormatMerger
    {
        /// <summary>
        /// Merge format label infos (with ^DF command)
        /// with values label infos (with ^XF command)
        /// based on field numbers (^FN command).
        /// </summary>
        /// <param name="rawLabelInfos">Raw label infos as read by ZplAnalyzer</param>
        /// <returns>Merged label info list</returns>
        List<LabelInfo> MergeFormats(List<LabelInfo> rawLabelInfos);

        /// <summary>
        /// Merge format label infos and report problems (for example the recall of a format that was not
        /// downloaded) in <paramref name="errors"/> instead of throwing. The default implementation calls
        /// <see cref="MergeFormats(List{LabelInfo})"/> and reports nothing.
        /// </summary>
        /// <param name="rawLabelInfos">Raw label infos as read by ZplAnalyzer</param>
        /// <param name="errors">Receives the error messages</param>
        /// <returns>Merged label info list</returns>
        List<LabelInfo> MergeFormats(List<LabelInfo> rawLabelInfos, ICollection<string> errors)
        {
            return this.MergeFormats(rawLabelInfos);
        }
    }
}
