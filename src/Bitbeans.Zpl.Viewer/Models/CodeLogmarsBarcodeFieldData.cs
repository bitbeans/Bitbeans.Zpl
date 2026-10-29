using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models;

public class CodeLogmarsBarcodeFieldData : FieldDataBase
{
    public FieldOrientation FieldOrientation { get; set; }
    public int? Height { get; set; }
    public bool PrintInterpretationLineAboveCode { get; set; }
}
