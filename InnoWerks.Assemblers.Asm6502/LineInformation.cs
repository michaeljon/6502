using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InnoWerks.Assemblers
{
    public enum LineType
    {
        Code,

        Comment,

        FloatingComment,

        Data,

        Directive,

        Equivalence,

        Label,

        Empty
    }

    public class LineInformation
    {
        public LineType LineType { get; set; }

        public int LineNumber { get; set; }

        public ushort CurrentOrg { get; set; }

        public ushort EffectiveAddress { get; set; }

        public ushort EffectiveSize
        {
            get
            {
                if (LineType == LineType.Code)
                {
                    if (InstructionInformation.SingleByteAddressModes.Contains(AddressingMode))
                    {
                        return 2;
                    }
                    else if (InstructionInformation.TwoByteAddressModes.Contains(AddressingMode))
                    {
                        return 3;
                    }
                    else
                    {
                        return 1;
                    }
                }
                else if (LineType == LineType.Data)
                {
                    if (Directive == Directive.DB)
                    {
                        return 1;
                    }
                    else if (Directive == Directive.DW)
                    {
                        return 2;
                    }
                }

                return 0;
            }
        }

        public string Label { get; set; }

        public OpCode OpCode { get; set; }

        public Directive Directive { get; set; }

        public bool IsEquivalence { get; set; }

        public AddressingMode AddressingMode { get; set; }

        public string RawArgument { get; set; }

        public string RawArgumentWithReplacement { get; set; }

        public string ExtractedArgument { get; set; }

        public string ExtractedArgumentValue { get; set; }

        public string Value { get; set; }

        public int ApplicableOffset { get; set; }

        public string Comment { get; set; }

        public string Line { get; set; }

        public Symbol ResolvedSymbol { get; set; }

        public byte OpCodeByte
        {
            get
            {
                if (LineType != LineType.Code)
                {
                    return 0x00;
                }

                return InstructionInformation.Instructions[(OpCode, AddressingMode)];
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append("(\n");
            sb.AppendFormat(CultureInfo.InvariantCulture, "\t(lineNumber {0})\n", LineNumber);
            sb.AppendFormat(CultureInfo.InvariantCulture, "\t(type {0})\n", LineType);
            // sb.AppendFormat(CultureInfo.InvariantCulture, "\t(org ${0:X})\n", CurrentOrg);
            sb.AppendFormat(CultureInfo.InvariantCulture, "\t(size {0})\n", EffectiveSize);

            switch (LineType)
            {
                case LineType.Code:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(effectiveAddress ${0:X})\n", EffectiveAddress);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(label {0})\n", Label);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(opCode {0})\n", OpCode);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(opCodeByte ${0:X2})\n", OpCodeByte);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(addressMode {0})\n", AddressingMode);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(value {0})\n", Value);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(code {0})\n", MachineCodeAsString);

                    if (ApplicableOffset != 0)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "\t(asm {0}{1}{2}{3}{4})\n",
                            (Label ?? "").PadRight(10),
                            OpCode.ToString().PadRight(6),
                            RawArgumentWithReplacement,
                            ApplicableOffset < 0 ? "-" : "+",
                            Math.Abs(ApplicableOffset));
                    }
                    else
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture,
                            "\t(asm {0}{1}{2})\n",
                            (Label ?? "").PadRight(10),
                            OpCode.ToString().PadRight(6),
                            RawArgumentWithReplacement);
                    }

                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Comment:
                case LineType.FloatingComment:
                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Data:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(effectiveAddress ${0:X})\n", EffectiveAddress);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(label {0})\n", Label);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(directive {0})\n", Directive);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(value {0})\n", Value);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(code {0})\n", MachineCodeAsString);

                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Directive:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(directive {0})\n", Directive);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(value {0})\n", Value);
                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Equivalence:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(label {0})\n", Label);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(value {0})\n", Value);
                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Label:
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(label {0})\n", Label);
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(effectiveAddress ${0:X})\n", EffectiveAddress);
                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Empty:
                    break;
            }

            // sb.AppendFormat(CultureInfo.InvariantCulture, "\t(rawLine '{0}')\n", Line);

            sb.Append(')');

            return sb.ToString();
        }

        private byte[] machineCode;

#pragma warning disable CA1819
        public byte[] MachineCode
        {
            get
            {
                if (machineCode == null)
                {
                    if (LineType == LineType.Code)
                    {
                        // we need a dummy value
                        ushort value = string.IsNullOrEmpty(Value) == false ? (ushort)(ResolveNumber(Value) + ApplicableOffset) : (ushort)0;

                        machineCode = EffectiveSize switch
                        {
                            1 => [OpCodeByte],
                            2 => [OpCodeByte, (byte)(value & 0xff)],
                            3 => [OpCodeByte, (byte)(value & 0xff), (byte)((value >> 8) & 0xff)],
                            _ => [],// NOTREACHED
                        };
                    }
                    else if (LineType == LineType.Data)
                    {
                        ushort value = ResolveNumber(Value);

                        machineCode = (Directive == Directive.DB) ? [(byte)(value & 0xff)] : [(byte)(value & 0xff), (byte)((value >> 8) & 0xff)];
                    }
                    else
                    {
                        machineCode = [];
                    }

                }

                return machineCode;
            }
        }
#pragma warning restore CA1819

        public string MachineCodeAsString
        {
            get
            {
                return string.Join(' ', MachineCode.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
            }
        }

        private static ushort ResolveNumber(string number)
        {
            if (number.StartsWith('$') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            else if (number.StartsWith('%') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
            }
            else
            {
                return ushort.Parse(number, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
        }
    }
}
