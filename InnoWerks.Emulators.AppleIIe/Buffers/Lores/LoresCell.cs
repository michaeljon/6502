using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace InnoWerks.Emulators.AppleIIe
{
    public readonly struct LoresCell : IEquatable<LoresCell>
    {
        private static readonly Color[] loresPaletteEven =
        [
           new Color(0x00, 0x00, 0x00),    //  "#000000", black
           new Color(0xDD, 0x00, 0x33),    //  "#DD0033", red
           new Color(0x00, 0x00, 0x99),    //  "#000099", dk blue
           new Color(0xDD, 0x22, 0xDD),    //  "#DD22DD", purple
           new Color(0x00, 0x77, 0x22),    //  "#007722", dk green
           new Color(0x00, 0x00, 0x55),    //  "#555555", gray
           new Color(0x22, 0x22, 0xFF),    //  "#2222FF", med blue
           new Color(0x66, 0xAA, 0xFF),    //  "#66AAFF", lt blue
           new Color(0x88, 0x55, 0x00),    //  "#885500", brown
           new Color(0xFF, 0x66, 0x00),    //  "#FF6600", orange
           new Color(0xAA, 0xAA, 0xAA),    //  "#AAAAAA", grey
           new Color(0xFF, 0x99, 0x88),    //  "#FF9988", pink
           new Color(0x11, 0xDD, 0x00),    //  "#11DD00", lt green
           new Color(0xFF, 0xFF, 0x00),    //  "#FFFF00", yellow
           new Color(0x4A, 0xFD, 0xC5),    //  "#4AFDC5", aqua
           new Color(0xFF, 0xFF, 0xFF),    //  "#FFFFFF"  white
        ];

        private static readonly Color[] loresPaletteOdd = [.. loresPaletteEven.Select(c => Bias(c, 0.88f))];

        public readonly byte TopIndex => (byte)(value & 0x0F);
        public readonly byte BottomIndex => (byte)((value & 0xF0) >> 4);

        public readonly Color Top(int col, bool hires) => (col & 1) == 1 && hires ? loresPaletteOdd[TopIndex] : loresPaletteEven[TopIndex];
        public readonly Color Bottom(int col, bool hires) => (col & 1) == 1 && hires ? loresPaletteOdd[BottomIndex] : loresPaletteEven[BottomIndex];

        private readonly byte value;

        public LoresCell(byte value)
        {
            this.value = value;
        }

        public override bool Equals(object obj) => ((LoresCell)obj).value == value;

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public bool Equals(LoresCell other)
        {
            return other.value == value;
        }

        public static bool operator ==(LoresCell left, LoresCell right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LoresCell left, LoresCell right)
        {
            return !(left == right);
        }

        private static Color Bias(Color c, float scale)
        {
            return new Color(
                (byte)(c.R * scale),
                (byte)(c.G * scale),
                (byte)(c.B * scale)
            );
        }
    }
}
