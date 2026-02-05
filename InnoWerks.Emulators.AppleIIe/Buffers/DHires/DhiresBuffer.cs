#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class DhiresBuffer
    {
        private readonly bool[,] pixels = new bool[192, 560];
        private readonly byte[,] sourceBytes = new byte[192, 560];

        public bool GetPixel(int y, int x) => pixels[y, x];
        public byte GetSourceByte(int y, int x) => sourceBytes[y, x];

        public void SetPixel(int y, int x, bool on, byte sourceByte)
        {
            pixels[y, x] = on;
            sourceBytes[y, x] = sourceByte;
        }
    }
}
