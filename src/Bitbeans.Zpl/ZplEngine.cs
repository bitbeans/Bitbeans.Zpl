using Bitbeans.Zpl.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bitbeans.Zpl
{
    /// <summary>A list of <see cref="ZplElementBase"/> elements that is rendered to a complete ZPL II label format.</summary>
    /// <remarks>
    /// Elements are rendered in list order. Elements with <see cref="ZplElementBase.IsEnabled"/> set to <see langword="false"/> are skipped.
    /// </remarks>
    public class ZplEngine : List<ZplElementBase>
    {
        /// <summary>
        /// Start an empty engine
        /// </summary>
        public ZplEngine() { }

        /// <summary>
        /// Start an engine with given elements
        /// </summary>
        /// <param name="elements">Zpl elements to be added</param>
        public ZplEngine(IEnumerable<ZplElementBase> elements) : base(elements) { }

        /// <summary>Renders all enabled elements to ZPL lines.</summary>
        /// <param name="context">Render options controlling the label start/end commands, comments, line layout and DPI scaling.</param>
        /// <returns>The ZPL lines: optionally <c>^XA</c> and <c>^LH0,0</c>, then the lines of each element (one joined line per element when <see cref="ZplRenderOptions.CompressedRendering"/> is set), optionally followed by <c>^XZ</c>.</returns>
        /// <remarks>
        /// When <see cref="ZplRenderOptions.DisplayComments"/> is set, the <see cref="ZplElementBase.Comments"/> of an element are written as an <c>^FX</c> comment; the characters <c>^</c> and <c>~</c> in comments are replaced.
        /// </remarks>
        public List<string> Render(ZplRenderOptions context)
        {
            var result = new List<string>();

            if (context.AddStartEndFormat)
            {
                result.Add("^XA");
            }

            if (context.AddDefaultLabelHome)
            {
                result.Add("^LH0,0");
            }

            foreach (var element in this.Where(x => x.IsEnabled))
            {
                //Empty line
                if (context.AddEmptyLineBeforeElementStart)
                {
                    result.Add("");
                }

                //Comments
                if (context.DisplayComments)
                {
                    if (element.Comments.Any())
                    {
                        result.Add("^FX");
                        element.Comments.ForEach(x => result.Add("//" + x.Replace("^", "[caret]").Replace("~", "[tilde]")));
                    }
                }

                //Actual element
                if (context.CompressedRendering)
                {
                    result.Add(string.Join("", element.Render(context)));
                }
                else
                {
                    result.AddRange(element.Render(context));
                }
            }

            if (context.AddStartEndFormat)
            {
                result.Add("^XZ");
            }

            return result;
        }

        /// <summary>Renders all enabled elements and joins the lines with line feeds.</summary>
        /// <param name="context">Render options controlling the label start/end commands, comments, line layout and DPI scaling.</param>
        /// <returns>The ZPL code, lines separated by <c>\n</c>.</returns>
        public string ToZplString(ZplRenderOptions context)
        {
            return string.Join("\n", Render(context));
        }

        /// <summary>Adds a raw ZPL fragment (<see cref="Elements.ZplRaw"/>) that is rendered unchanged.</summary>
        /// <param name="rawZplCode">The ZPL code to add.</param>
        public void AddRawZplCode(string rawZplCode)
        {
            Add(new ZplRaw(rawZplCode));
        }

        /// <summary>Converts a character to its two-digit upper case hexadecimal value, e.g. for <c>^FH</c> escapes.</summary>
        /// <param name="input">The character; its code must be 255 or lower.</param>
        /// <returns>Two upper case hexadecimal digits, e.g. <c>5F</c> for <c>_</c>.</returns>
        /// <exception cref="OverflowException"><paramref name="input"/> has a code above 255.</exception>
        static public string MapToHexadecimalValue(char input)
        {
            return Convert.ToByte(input).ToString("X2");
        }
    }
}
