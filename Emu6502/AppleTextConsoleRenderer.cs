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
            bool page2 = softSwitches.Page2;
            Span<char> line = stackalloc char[40];

            Console.SetCursorPosition(0, 0);

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

        public void RenderPage(byte page)
        {
            Console.SetCursorPosition(0, 0);
            PrintPage(page);
        }

        public void WriteDisplayRam(bool page2, bool asChar)
        {
            Console.Clear();

            byte page = page2 == false ? (byte)4 : (byte)8;
            int stride = 40;

            for (byte p = page; p < page + 4; p++)
            {
                for (var l = page * 0x100; l < (page + 1) * 0x100; l += stride)
                {
                    Console.Write("{0:X4}:  ", l);

                    for (var b = 0; b < stride; b++)
                    {
                        var val = bus.Peek((ushort)(l + b));

                        if (asChar == false)
                        {
                            Console.Write("{0:X2} ", val);
                        }
                        else
                        {
                            Console.Write("{0} ", DecodeAppleChar(val));
                        }
                    }

                    Console.WriteLine("");
                }

                Console.WriteLine("");
            }
        }

        private void PrintPage(byte page)
        {
            ArgumentNullException.ThrowIfNull(bus);

            for (var l = page * 0x100; l < (page + 1) * 0x100; l += 16)
            {
                Console.Write("{0:X4}:  ", l);

                for (var b = 0; b < 8; b++)
                {
                    Console.Write("{0:X2} ", bus.Peek((ushort)(l + b)));
                }

                Console.Write("  ");

                for (var b = 8; b < 16; b++)
                {
                    Console.Write("{0:X2} ", bus.Peek((ushort)(l + b)));
                }

                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}
