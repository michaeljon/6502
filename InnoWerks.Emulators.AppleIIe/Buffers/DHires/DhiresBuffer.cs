#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

using System;

namespace InnoWerks.Emulators.AppleIIe
{
    public readonly struct DhiresPixel : IEquatable<DhiresPixel>
    {
        public readonly bool AuxBit { get; init; }
        public readonly bool MainBit { get; init; }
        public readonly byte SourceByte { get; init; }

        public DhiresPixel(bool auxBit, bool mainBit, byte sourceByte)
        {
            AuxBit = auxBit;
            MainBit = mainBit;
            SourceByte = sourceByte;
        }

        public bool IsOn => AuxBit || MainBit;

        public override readonly bool Equals(object obj)
        {
            return ((DhiresPixel)obj).MainBit == MainBit &&
                   ((DhiresPixel)obj).AuxBit == AuxBit &&
                   ((DhiresPixel)obj).SourceByte == SourceByte;
        }

        public override readonly int GetHashCode()
        {
            return SourceByte.GetHashCode();
        }

        public readonly bool Equals(DhiresPixel other)
        {
            return other.MainBit == MainBit &&
                   other.AuxBit == AuxBit &&
                   other.SourceByte == SourceByte;
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

        public void SetPixel(int y, int x, bool auxBit, bool mainBit, byte sourceByte) => pixels[y, x] = new DhiresPixel(auxBit, mainBit, sourceByte);

        public DhiresPixel GetPixel(int y, int x) => pixels[y, x];
    }
}
