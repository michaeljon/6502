#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoWerks.Emulators.AppleIIe
{
    public class LoresBuffer
    {
        private readonly LoresCell[,] graphicsBuffer;

        public int Columns { get; set; }

        public LoresBuffer(int columns)
        {
            Columns = columns;
            graphicsBuffer = new LoresCell[24, columns];
        }

        public LoresCell Get(int row, int col) => graphicsBuffer[row, col];

        public void Put(int row, int col, LoresCell cell) => graphicsBuffer[row, col] = cell;
    }
}
