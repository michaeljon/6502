#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

using Microsoft.Xna.Framework;

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class HiresBuffer
    {
        private readonly bool[,] pixels = new bool[192, 280];
        private readonly byte[,] sourceBytes = new byte[192, 280];

        public bool GetPixel(int y, int x) => pixels[y, x];
        public byte GetSourceByte(int y, int x) => sourceBytes[y, x];

        public void SetPixel(int y, int x, bool on, byte sourceByte)
        {
            pixels[y, x] = on;
            sourceBytes[y, x] = sourceByte;
        }
    }
}
