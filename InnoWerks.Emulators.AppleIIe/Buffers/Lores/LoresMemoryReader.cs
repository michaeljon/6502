using System;
using System.Runtime.InteropServices;
using InnoWerks.Computers.Apple;

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class LoresMemoryReader
    {
        private readonly MemoryBlocks ram;
        private readonly MachineState machineState;

        public LoresMemoryReader(MemoryBlocks ram, MachineState machineState)
        {
            this.ram = ram;
            this.machineState = machineState;
        }

        public void ReadLoresPage(LoresBuffer loresBuffer)
        {
            ArgumentNullException.ThrowIfNull(loresBuffer);

            // might want to keep this in the loop so
            // switcing mid-render would work
            bool page2 = machineState.State[SoftSwitch.Page2];

            for (int row = 0; row < 24; row++)
            {
                for (int col = 0; col < 40; col++)
                {
                    ushort addr = GetTextAddress(row, col, page2);
                    byte value = ram.Read(addr);

                    loresBuffer.Put(row, col, ConstructTLoresCell(value));
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

        private static LoresCell ConstructTLoresCell(byte value)
        {
            return new LoresCell(value);
        }
    }
}
