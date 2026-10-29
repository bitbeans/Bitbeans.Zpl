namespace Bitbeans.Zpl.Viewer.Models
{
    /// <summary>Base class of the field data of the next field: the content and, in derived classes, the parameters of a barcode or graphic symbol command.</summary>
    public class FieldDataBase
    {
        /// <summary>Gets or sets the field content set with <c>^FD</c> or <c>^FV</c>.</summary>
        public string Content { get; set; }
    }
}
