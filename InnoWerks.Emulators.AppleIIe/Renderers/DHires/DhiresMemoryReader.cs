using System;
using InnoWerks.Computers.Apple;

namespace InnoWerks.Emulators.AppleIIe
{
    public sealed class DhiresMemoryReader
    {
        private readonly Memory128k ram;
        private readonly MachineState machineState;

        public DhiresMemoryReader(Memory128k ram, MachineState machineState)
        {
            this.ram = ram;
            this.machineState = machineState;
        }

        public void ReadDhiresPage(DhiresBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            bool page2 = machineState.State[SoftSwitch.Page2];
            int pageBase = page2 ? 0x4000 : 0x2000;

            for (int y = 0; y < 192; y++)
            {
                int rowAddr =
                    pageBase +
                    ((y & 0x07) << 10) +       // (y % 8) * 0x400
                    (((y >> 3) & 0x07) << 7) + // ((y / 8) % 8) * 0x80
                    ((y >> 6) * 40);           // (y / 64) * 40

                for (int byteCol = 0; byteCol < 40; byteCol++)
                {
                    // AUX = "even" pixel, MAIN = "odd" pixel
                    byte mainByte = ram.GetMain((ushort)(rowAddr + byteCol));
                    byte auxByte = ram.GetAux((ushort)(rowAddr + byteCol));

                    for (int bit = 0; bit < 7; bit++)
                    {
                        bool auxBit = ((auxByte >> bit) & 1) != 0;
                        bool mainBit = ((mainByte >> bit) & 1) != 0;

                        int x = byteCol * 14 + bit * 2;
                        bool msb = (auxByte & 0x80) != 0 || (mainByte & 0x80) != 0;

                        buffer.SetPixel(y, x, auxBit, mainBit, msb);   // left
                        buffer.SetPixel(y, x + 1, auxBit, mainBit, msb); // right
                    }
                }
            }
        }
    }
}
