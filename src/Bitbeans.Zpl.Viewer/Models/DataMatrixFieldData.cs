using Bitbeans.Zpl;

namespace Bitbeans.Zpl.Viewer.Models
{
    public class DataMatrixFieldData : FieldDataBase
    {
        public FieldOrientation FieldOrientation { get; set; }
        public int? Height { get; set; }
        public QualityLevel QualityLevel { get; set; }
        public int? Columns { get; set; }
        public int? Rows { get; set; }
        public int Format { get; set; } = 6;
        public char EscapeSequence { get; set; } = '~';
        public int? AspectRatio { get; set; }
    }
}
