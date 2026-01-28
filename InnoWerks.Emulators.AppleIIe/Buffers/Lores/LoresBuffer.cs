#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoWerks.Emulators.AppleIIe
{
    public class LoresBuffer
    {
        private readonly LoresCell[,] graphicsBuffer = new LoresCell[24, 40];

        public LoresCell Get(int row, int col)
        {
            return graphicsBuffer[row, col];
        }

        public void Put(int row, int col, LoresCell cell)
        {
            graphicsBuffer[row, col] = cell;
        }
    }
}
