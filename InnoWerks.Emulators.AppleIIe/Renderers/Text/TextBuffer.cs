#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoWerks.Emulators.AppleIIe
{
    public class TextBuffer
    {
        private readonly TextCell[,] textBuffer;

        public int Columns { get; set; }

        public TextBuffer(int columns)
        {
            Columns = columns;
            textBuffer = new TextCell[24, columns];
        }

        public TextCell Get(int row, int col) => textBuffer[row, col];

        public void Put(int row, int col, TextCell cell) => textBuffer[row, col] = cell;
    }
}
