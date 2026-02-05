#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

using System;

namespace InnoWerks.Emulators.AppleIIe
{
    public readonly struct DhiresPixel : IEquatable<DhiresPixel>
    {
        public readonly bool AuxBit { get; init; }
        public readonly bool MainBit { get; init; }
        public readonly bool MSB { get; init; }

        public DhiresPixel(bool auxBit, bool mainBit, bool msb)
        {
            AuxBit = auxBit;
            MainBit = mainBit;
            MSB = msb;
        }

        public bool IsOn => AuxBit || MainBit;

        public override readonly bool Equals(object obj)
        {
            return ((DhiresPixel)obj).MainBit == MainBit &&
                   ((DhiresPixel)obj).AuxBit == AuxBit &&
                   ((DhiresPixel)obj).MSB == MSB;
        }

        public override readonly int GetHashCode()
        {
            return MSB.GetHashCode();
        }

        public readonly bool Equals(DhiresPixel other)
        {
            return other.MainBit == MainBit &&
                   other.AuxBit == AuxBit &&
                   other.MSB == MSB;
        }

        public static bool operator ==(DhiresPixel left, DhiresPixel right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DhiresPixel left, DhiresPixel right)
        {
            return !(left == right);
        }
    }

    public sealed class DhiresBuffer
    {
        private readonly DhiresPixel[,] pixels = new DhiresPixel[192, 560];

        public void SetPixel(int y, int x, bool auxBit, bool mainBit, bool msb) => pixels[y, x] = new DhiresPixel(auxBit, mainBit, msb);

        public DhiresPixel GetPixel(int y, int x) => pixels[y, x];
    }
}
