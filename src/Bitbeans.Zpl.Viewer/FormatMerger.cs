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
        /// <summary>
        /// Merges the formats and throws an <see cref="InvalidOperationException"/> if a recalled format was not
        /// downloaded. Use <see cref="MergeFormats(List{LabelInfo}, ICollection{string})"/> to receive the errors instead.
        /// </summary>
        /// <inheritdoc />
        public List<LabelInfo> MergeFormats(List<LabelInfo> rawLabelInfos)
        {
            List<string> errors = [];
            List<LabelInfo> mergedLabelInfos = this.MergeFormats(rawLabelInfos, errors);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException(errors[0]);
            }

            return mergedLabelInfos;
        }

        /// <inheritdoc />
        public List<LabelInfo> MergeFormats(List<LabelInfo> rawLabelInfos, ICollection<string> errors)
        {
            List<LabelInfo> mergedLabelInfos = [];
            // Format label infos indexed by storage device and download format name (see GetFormatKey)
            Dictionary<string, LabelInfo> templateFormats = [];

            foreach (LabelInfo rawLabelInfo in rawLabelInfos)
            {
                List<ZplElementBase> elements = GetMergedElements(rawLabelInfo, templateFormats, errors);
                LabelInfo labelInfo = new() { ZplElements = elements.ToArray() };

                if (rawLabelInfo.DownloadFormatName != null)
                {
                    // A later ^DF to the same device and name replaces the stored format
                    templateFormats[GetFormatKey(rawLabelInfo.DownloadFormatName)] = labelInfo;
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
            Dictionary<string, LabelInfo> templateFormats,
            ICollection<string> errors)
        {
            List<ZplElementBase> elements = [];

            foreach (ZplElementBase zplElement in rawLabelInfo.ZplElements)
            {
                if (zplElement is ZplRecallFormat recallFormat)
                {
                    if (templateFormats.TryGetValue(GetFormatKey(recallFormat.FormatName), out LabelInfo formatLabelInfo))
                    {
                        elements.AddRange(GetMergedElements(rawLabelInfo, formatLabelInfo));
                    }
                    else
                    {
                        // The label keeps its own elements
                        errors.Add($"Could not find format {recallFormat.FormatName}");
                    }
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

            if (rawLabelInfo.DownloadFormatName == null)
            {
                ApplyFieldNumberData(rawLabelInfo, elements);
            }

            return elements;
        }

        /// <summary>
        /// Fields of a label that define their own field number and data (<c>^FO..^FN1^FDdata^FS</c>) are printed,
        /// as the data is provided to all fields with this number. A format keeps its fields as templates.
        /// </summary>
        private static void ApplyFieldNumberData(LabelInfo rawLabelInfo, List<ZplElementBase> elements)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] is ZplFieldNumber fieldNumber)
                {
                    ZplRecallFieldNumber recallFieldNumber = rawLabelInfo.ZplElements
                        .OfType<ZplRecallFieldNumber>()
                        .LastOrDefault(r => r.Number == fieldNumber.Number);
                    if (recallFieldNumber != null)
                    {
                        elements[i] = CreateElementWithContent(fieldNumber.FormatElement, recallFieldNumber.Text);
                    }
                }
            }
        }

        /// <summary>
        /// Key of a stored format: storage device (default <c>R:</c>) and upper case name without extension.
        /// </summary>
        internal static string GetFormatKey(string formatName)
        {
            string name = formatName.Trim();
            char device = 'R';
            // Strip the optional storage device ("R:FORMAT.ZPL"), Path does not treat ':' as separator on all platforms
            if (name.Length > 1 && name[1] == ':')
            {
                device = char.ToUpperInvariant(name[0]);
                name = name.Substring(2);
            }

            return device + ":" + Path.GetFileNameWithoutExtension(name).ToUpperInvariant();
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
