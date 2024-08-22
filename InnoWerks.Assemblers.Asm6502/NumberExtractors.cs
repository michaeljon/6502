using System;
using System.Globalization;

namespace InnoWerks.Assemblers
{
    public static class NumberExtractors
    {
        public static ushort ResolveNumber(string number)
        {
            ArgumentException.ThrowIfNullOrEmpty(number);

            if (number.StartsWith('$') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            else if (number.StartsWith("0x", StringComparison.Ordinal) == true)
            {
                return ushort.Parse(number[2..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            else if (number.StartsWith('%') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
            }
            else if (number.StartsWith("0b", StringComparison.Ordinal) == true)
            {
                return ushort.Parse(number[2..], NumberStyles.BinaryNumber, CultureInfo.InvariantCulture);
            }
            else
            {
                return ushort.Parse(number, NumberStyles.Integer, CultureInfo.InvariantCulture);
            }
        }
    }
}
