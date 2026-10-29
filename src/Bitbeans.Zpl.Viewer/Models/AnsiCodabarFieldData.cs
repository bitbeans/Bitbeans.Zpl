using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    public class AnsiCodabarFieldData : FieldDataBase
    {
        public FieldOrientation FieldOrientation { get; set; }
        public int? Height { get; set; }
        public bool PrintInterpretationLine { get; set; }
        public bool PrintInterpretationLineAboveCode { get; set; }
        public bool CheckDigit { get; set; }
        public char StartCharacter { get; set; }
        public char StopCharacter { get; set; }
    }
}
