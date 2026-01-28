using System;
using Microsoft.Xna.Framework;

namespace InnoWerks.Emulators.AppleIIe
{
    public readonly struct LoresCell : IEquatable<LoresCell>
    {
        private static readonly Color[] loresPalette =
        [
            Color.Black,
            Color.Magenta,
            Color.DarkBlue,
            Color.Purple,
            Color.DarkGreen,
            Color.Gray,
            Color.MediumBlue,
            Color.LightBlue,

            // repeated or placeholder for full 16
            Color.Brown,
            Color.Orange,
            Color.Gray,
            Color.Pink,
            Color.LightGreen,
            Color.Yellow,
            Color.Aqua,
            Color.White
        ];

        public static Color GetPaletteColor(int index) => loresPalette[index];

        public static int PaletteSize => loresPalette.Length;

        public readonly Color Top => loresPalette[value & 0xF0 >> 4];

        public readonly byte TopIndex => (byte)(value & 0xF0 >> 4);

        public readonly Color Bottom => loresPalette[value & 0x0F];

        public readonly byte BottomIndex => (byte)(value & 0x0F);

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
    }
}
