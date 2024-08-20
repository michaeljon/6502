using System;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Security;
using System.Text;

namespace Asm6502
{
    public enum LineType
    {
        Code,

        Comment,

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
                    if (InstructionInformation.SingleByteAddressModes.Contains(Argument.addressingMode))
                    {
                        return 2;
                    }
                    else if (InstructionInformation.TwoByteAddressModes.Contains(Argument.addressingMode))
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

        public (AddressingMode addressingMode, string value) Argument { get; set; }

        public string Value { get; set; }

        public string Comment { get; set; }

        public string Line { get; set; }

        public byte OpCodeByte
        {
            get
            {
                if (LineType != LineType.Code)
                {
                    return 0x00;
                }

                return InstructionInformation.Instructions[(OpCode, Argument.addressingMode)];
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
                    sb.AppendFormat(CultureInfo.InvariantCulture, "\t(argument {0})\n", Argument);

                    if (string.IsNullOrEmpty(Comment) == false)
                    {
                        sb.AppendFormat(CultureInfo.InvariantCulture, "\t(comment '{0}')\n", Comment);
                    }
                    break;

                case LineType.Comment:
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

        public byte[] MachineCode
        {
            get
            {
                if (LineType == LineType.Code)
                {
                    // for now, need to get the rest
                    return [OpCodeByte];
                }
                else if (LineType == LineType.Data)
                {
                    ushort value = ResolveNumber(Value);

                    return (Directive == Directive.DB) ? [(byte)(value & 0xff)] : [(byte)(value & 0xff), (byte)((value >> 8) & 0xff)];
                }
                else
                {
                    return [];
                }
            }
        }

        public string Bytes
        {
            get
            {
                return Convert.ToHexString(MachineCode);
            }
        }

        private static ushort ResolveNumber(string number)
        {
            if (number.StartsWith('$') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.HexNumber);
            }
            else if (number.StartsWith('%') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.BinaryNumber);
            }
            else
            {
                return ushort.Parse(number, NumberStyles.Integer);
            }
        }
    }
}