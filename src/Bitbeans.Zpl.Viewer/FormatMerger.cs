using Bitbeans.Zpl.Elements;
using Bitbeans.Zpl.Viewer.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bitbeans.Zpl.Viewer
{
    /// <summary>
    /// Helper class responsible of merging several LabelInfo based on format data (templating).
    /// </summary>
    public class FormatMerger : IFormatMerger
    {
        /// <inheritdoc />
        public List<LabelInfo> MergeFormats(List<LabelInfo> rawLabelInfos)
        {
            List<LabelInfo> mergedLabelInfos = [];
            // Format label infos indexed by download format name
            Dictionary<string, LabelInfo> templateFormats = [];

            foreach (LabelInfo rawLabelInfo in rawLabelInfos)
            {
                List<ZplElementBase> elements = GetMergedElements(rawLabelInfo, templateFormats);
                LabelInfo labelInfo = new() { ZplElements = elements.ToArray() };

                if (rawLabelInfo.DownloadFormatName != null)
                {
                    templateFormats.Add(rawLabelInfo.DownloadFormatName, labelInfo);
                }
                else
                {
                    mergedLabelInfos.Add(labelInfo);
                }
            }

            return mergedLabelInfos;
        }

        private static List<ZplElementBase> GetMergedElements(
            LabelInfo rawLabelInfo,
            Dictionary<string, LabelInfo> templateFormats)
        {
            List<ZplElementBase> elements = [];

            foreach (ZplElementBase zplElement in rawLabelInfo.ZplElements)
            {
                if (zplElement is ZplRecallFormat recallFormat)
                {
                    LabelInfo formatLabelInfo = GetFormatLabelInfo(recallFormat, templateFormats);
                    elements.AddRange(GetMergedElements(rawLabelInfo, formatLabelInfo));
                }
                else if (zplElement is ZplRecallFieldNumber recallFieldNumber)
                {
                    for (int i = 0; i < elements.Count; i++)
                    {
                        if (elements[i] is ZplFieldNumber fieldNumber && fieldNumber.Number == recallFieldNumber.Number)
                        {
                            elements[i] = CreateElementWithContent(fieldNumber.FormatElement, recallFieldNumber.Text);
                        }
                    }
                }
                else
                {
                    elements.Add(zplElement);
                }
            }

            return elements;
        }

        private static readonly MethodInfo memberwiseCloneMethod =
            typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Copies a format element and sets the content of the copy. A format can be recalled by
        /// several labels, so the template element itself must keep its (empty) content.
        /// </summary>
        private static ZplElementBase CreateElementWithContent(ZplElementBase formatElement, string content)
        {
            ZplElementBase copy = (ZplElementBase)memberwiseCloneMethod.Invoke(formatElement, null);
            ((IFormatElement)copy).SetTemplateContent(content);
            return copy;
        }

        private static LabelInfo GetFormatLabelInfo(
            ZplRecallFormat recallFormat,
            Dictionary<string, LabelInfo> templateFormats)
        {
            // Strip the optional storage device ("R:FORMAT.ZPL"), Path does not treat ':' as separator on all platforms
            string fileName = recallFormat.FormatName.Length > 1 && recallFormat.FormatName[1] == ':'
                ? recallFormat.FormatName.Substring(2)
                : recallFormat.FormatName;
            string formatName = Path.GetFileNameWithoutExtension(fileName);
            if (!templateFormats.TryGetValue(formatName, out LabelInfo value))
            {
                throw new InvalidOperationException($"Could not find format {recallFormat.FormatName}");
            }

            return value;
        }

        /// <summary>
        /// Merge two label infos into a single list of elements.
        /// </summary>
        /// <param name="valuesLabelInfo">Label info providing values</param>
        /// <param name="formatLabelInfo">Label info providing format</param>
        private static IEnumerable<ZplElementBase> GetMergedElements(
            LabelInfo valuesLabelInfo,
            LabelInfo formatLabelInfo)
        {
            foreach (ZplElementBase formatElement in formatLabelInfo.ZplElements)
            {
                if (formatElement is ZplFieldNumber fieldNumber)
                {
                    // The format element of a field number is never null (checked by its constructor)
                    ZplRecallFieldNumber recallFieldNumber = (ZplRecallFieldNumber)valuesLabelInfo.ZplElements.FirstOrDefault(e =>
                        (e as ZplRecallFieldNumber)?.Number == fieldNumber.Number);
                    if (recallFieldNumber != null)
                    {
                        yield return CreateElementWithContent(fieldNumber.FormatElement, recallFieldNumber.Text);
                    }
                }
                else
                {
                    yield return formatElement;
                }
            }
        }
    }
}
