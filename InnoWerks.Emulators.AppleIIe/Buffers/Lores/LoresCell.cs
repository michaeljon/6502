using System;
using Microsoft.Xna.Framework;

namespace InnoWerks.Emulators.AppleIIe
{
    public readonly struct LoresCell : IEquatable<LoresCell>
    {
        private static readonly Color[] loresPalette =
        [
            new Color(0,  0,  0),
            new Color(208,  0, 48),
            new Color(  0,  0,128),
            new Color(255,  0,255),
            new Color(  0,128,  0),
            new Color(128,128,128),
            new Color(  0,  0,255),
            new Color( 96,160,255),
            new Color(128, 80,  0),
            new Color(255,128,  0),
            new Color(192,192,192),
            new Color(255,144,128),
            new Color(  0,255,  0),
            new Color(255,255,  0),
            new Color( 64,255,144),
            new Color(255,255,255)
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
