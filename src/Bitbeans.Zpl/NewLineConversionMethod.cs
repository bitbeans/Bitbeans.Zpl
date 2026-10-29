namespace Bitbeans.Zpl
{
    /// <summary>Defines how line feed characters in text are converted when it is written to field data.</summary>
    public enum NewLineConversionMethod
    {
        /// <summary>Replace each line feed by a space.</summary>
        ToSpace,
        /// <summary>Remove line feeds.</summary>
        ToEmpty,
        /// <summary>Replace each line feed by the ZPL line break <c>\&amp;</c> (only effective inside a field block, <c>^FB</c>).</summary>
        ToZplNewLine,
    }
}
