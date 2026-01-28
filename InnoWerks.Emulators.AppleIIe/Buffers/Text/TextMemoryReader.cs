using System;
using System.Runtime.InteropServices;
using InnoWerks.Computers.Apple;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class TextMemoryReader
    {
        private readonly MemoryBlocks ram;
        private readonly MachineState machineState;

        public TextMemoryReader(MemoryBlocks ram, MachineState machineState)
        {
            this.ram = ram;
            this.machineState = machineState;
        }

        public void ReadTextPage(TextCell[,] buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            if (machineState.State[SoftSwitch.EightyColumnMode] == false)
            {
                Render40Column(buffer);
            }
            else
            {
                Render80Column(buffer);
            }
        }

        private void Render40Column(TextCell[,] buffer)
        {
            for (int row = 0; row < 24; row++)
            {
                bool page2 = machineState.State[SoftSwitch.Page2];

                for (int col = 0; col < 40; col++)
                {
                    ushort addr = GetTextAddress(row, col, page2);
                    byte value = ram.Read(addr);
                    buffer[row, col] = ConstructTextCell(value);
                }
            }
        }

        private void Render80Column(TextCell[,] buffer)
        {
            for (int row = 0; row < 24; row++)
            {
                for (int col = 0; col < 40; col++)
                {
                    ushort addr = GetTextAddress(row, col, false);

                    byte value = ram.GetAux(addr);
                    buffer[row, col * 2] = ConstructTextCell(value);

                    value = ram.GetMain(addr);
                    buffer[row, (col * 2) + 1] = ConstructTextCell(value);
                }
            }
        }

        private static ushort GetTextAddress(int row, int col, bool page2)
        {
            int pageOffset = page2 ? 0x800 : 0x400;

            return (ushort)(
                pageOffset +
                textRowBase[row & 0x07] +
                (row >> 3) * 40 +
                col
            );
        }

        private static readonly int[] textRowBase =
        [
            0x000, 0x080, 0x100, 0x180,
            0x200, 0x280, 0x300, 0x380
        ];

        private static TextCell ConstructTextCell(byte value)
        {
            var highBitSet = (value & 0x80) != 0;
            var attr = highBitSet ? TextAttributes.None : TextAttributes.Inverse;

            return new TextCell((byte)(value & 0x7F), attr);
        }
    }
}
