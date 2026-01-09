using System;
using InnoWerks.Emulators.Apple;
using InnoWerks.Simulators;

namespace Emu6502
{
    public sealed class AppleTextConsoleRenderer
    {
        private readonly IBus bus;
        private readonly SoftSwitches softSwitches;

        public AppleTextConsoleRenderer(IBus bus, SoftSwitches softSwitches)
        {
            this.bus = bus;
            this.softSwitches = softSwitches;
        }

        public void Render()
        {
            Console.SetCursorPosition(0, 0);

            bool page2 = softSwitches.Page2;
            Span<char> line = stackalloc char[40];

            for (int row = 0; row < 24; row++)
            {
                for (int col = 0; col < 40; col++)
                {
                    ushort addr = GetTextAddress(row, col, page2);
                    byte b = bus.Peek(addr);

                    line[col] = DecodeAppleChar(b);
                }

                Console.WriteLine(line);
            }
        }

        private static char DecodeAppleChar(byte b)
        {
            // Ignore inverse/flash for now
            b &= 0x7F;

            // Apple II uses ASCII-ish set
            if (b < 0x20)
                return ' ';

            return (char)b;
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
    }
}
