using System;

namespace Bitbeans.Zpl.Helpers
{
    /// <summary>Shared handling of the <c>^FH</c> hexadecimal escape mechanism: validation of the indicator character and escaping of characters that cannot be part of field data.</summary>
    internal static class ZplHexadecimal
    {
        /// <summary>The indicator used when no indicator is given: <c>^FH</c> without a parameter selects <c>_</c>.</summary>
        internal const char DefaultIndicator = '_';

        /// <summary>Validates a hexadecimal indicator: it must be a printable ASCII character other than the command prefixes <c>^</c> and <c>~</c>.</summary>
        /// <param name="indicator">The indicator, or <see langword="null"/> when <c>^FH</c> is not used.</param>
        /// <param name="paramName">Name of the constructor parameter, used in the exception.</param>
        /// <returns>The unchanged <paramref name="indicator"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="indicator"/> is <c>^</c>, <c>~</c>, a space, a control character or not a printable ASCII character.</exception>
        internal static char? Validate(char? indicator, string paramName)
        {
            if (indicator is char c && (c <= ' ' || c >= '~' || c == '^'))
            {
                throw new ArgumentException("The hexadecimal indicator must be a printable ASCII character other than '^' and '~'.", paramName);
            }

            return indicator;
        }

        /// <summary>Renders the <c>^FH</c> command for an indicator. The default indicator is rendered without parameter.</summary>
        /// <param name="indicator">The hexadecimal indicator.</param>
        /// <returns><c>^FH</c> or <c>^FH</c> followed by the indicator.</returns>
        internal static string RenderCommand(char indicator)
        {
            return indicator == DefaultIndicator ? "^FH" : "^FH" + indicator;
        }

        /// <summary>Checks whether a character must be escaped in field data that is sent with the given indicator: the command prefixes <c>^</c> and <c>~</c> and the indicator itself.</summary>
        /// <param name="input">The character.</param>
        /// <param name="indicator">The active hexadecimal indicator.</param>
        /// <returns><see langword="true"/> if the character must be written as indicator plus two hexadecimal digits.</returns>
        internal static bool MustEscape(char input, char indicator)
        {
            return input == '^' || input == '~' || input == indicator;
        }

        /// <summary>Writes a character as indicator plus two hexadecimal digits.</summary>
        /// <param name="input">The character; its code must be 255 or lower.</param>
        /// <param name="indicator">The hexadecimal indicator.</param>
        /// <returns>The escape sequence, e.g. <c>_5E</c> for <c>^</c>.</returns>
        internal static string Escape(char input, char indicator)
        {
            return indicator + ZplEngine.MapToHexadecimalValue(input);
        }

        /// <summary>Checks whether field data contains a command prefix (<c>^</c> or <c>~</c>) and therefore needs <c>^FH</c> to be sent safely.</summary>
        /// <param name="content">The field data.</param>
        /// <returns><see langword="true"/> if <c>^</c> or <c>~</c> is contained.</returns>
        internal static bool ContainsCommandPrefix(string content)
        {
            return content.AsSpan().IndexOfAny('^', '~') >= 0;
        }
    }
}
