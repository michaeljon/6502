using System.Globalization;
using System.Text;

namespace Asm6502
{
#pragma warning disable IDE0290

    public enum SymbolType
    {
        Indifferent,

        DefineByte,

        DefineWord,

        RelativeAddress,

        AbsoluteAddress,
    }

    public class Symbol
    {
        public SymbolType SymbolType { get; set; }

        public bool IsEquivalence => SymbolType == SymbolType.DefineByte || SymbolType == SymbolType.DefineWord;

        public int LineNumber { get; set; }

        public string Label { get; set; }

        public string UnparsedValue { get; set; }

        public ushort Value { get; set; }

        public int Size { get; set; }

        public byte AsByte => (byte)(Value & 0xff);

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("(\n");

            sb.AppendFormat(CultureInfo.InvariantCulture, "\t(lineNumber {0})\n", LineNumber);
            sb.AppendFormat(CultureInfo.InvariantCulture, "\t(type {0})\n", SymbolType);

            if (string.IsNullOrEmpty(Label) == false)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "\t(label {0})\n", Label);
            }

            switch (SymbolType)
            {
                case SymbolType.DefineByte:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(value ${0:x2})\n", Value);
                    break;

                case SymbolType.DefineWord:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(value ${0:x4})\n", Value);
                    break;

                case SymbolType.RelativeAddress:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(offset {0:x2})\n", (sbyte)Value);
                    break;

                case SymbolType.AbsoluteAddress:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(addr ${0:x4})\n", Value);
                    break;
            }

            sb.AppendFormat(CultureInfo.InvariantCulture, "\t(size {0})\n", Size);

            sb.Append(')');

            return sb.ToString();
        }
    }
}