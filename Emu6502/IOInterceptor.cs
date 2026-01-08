#pragma warning disable CA1823, IDE0005

using System;
using System.Collections.Generic;
using InnoWerks.Simulators;


namespace Emu6502
{
    public class IOInterceptor : IBus
    {
        private static readonly Dictionary<int, ushort> lineToBaseAddress = new()
        {
            {0, 0x400}, {1, 0x480}, {2, 0x500}, {3, 0x580}, {4, 0x600}, {5, 0x680}, {6, 0x700}, {7, 0x780},
            {8, 0x428}, {9, 0x4a8}, {10, 0x528}, {11, 0x5a8}, {12, 0x628}, {13, 0x6a8}, {14, 0x728}, {15, 0x7a8},
            {16, 0x450}, {17, 0x4d0}, {18, 0x550}, {19, 0x5d0}, {20, 0x650}, {21, 0x6d0}, {22, 0x750}, {23, 0x7d0},
        };

        private static readonly (ushort lo, ushort hi, int line)[] baseAddressToLine =
        [
            (0x400, 0x400 + 0x28, 0), (0x480, 0x480 + 0x28, 1), (0x500, 0x500 + 0x28, 2), (0x580, 0x580 + 0x28, 3),
            (0x600, 0x600 + 0x28, 4), (0x680, 0x680 + 0x28, 5), (0x700, 0x700 + 0x28, 6), (0x780, 0x780 + 0x28, 7),

            (0x428, 0x428 + 0x28, 8), (0x4a8, 0x4a8 + 0x28, 9), (0x528, 0x528 + 0x28, 10), (0x5a8, 0x5a8 + 0x28, 11),
            (0x628, 0x628 + 0x28, 12), (0x6a8, 0x6a8 + 0x28, 13), (0x728, 0x728 + 0x28, 14), (0x7a8, 0x7a8 + 0x28, 15),

            (0x450, 0x450 + 0x28, 16), (0x4d0, 0x4d0 + 0x28, 17), (0x550, 0x550 + 0x28, 18), (0x5d0, 0x5d0 + 0x28, 19),
            (0x650, 0x650 + 0x28, 20), (0x6d0, 0x6d0 + 0x28, 21), (0x750, 0x750 + 0x28, 22), (0x7d0, 0x7d0 + 0x28, 23),
        ];

        private readonly Bus bus = new();

        public void BeginTransaction()
        {
            bus.BeginTransaction();
        }

        public int EndTransaction()
        {
            return bus.EndTransaction();
        }

        public long CycleCount { get; private set; }

        public byte this[ushort address]
        {
            get => bus[address];
            set => bus[address] = value;
        }

        public void LoadProgram(byte[] objectCode, ushort origin)
        {
            bus.LoadProgram(objectCode, origin);
        }

        public byte Peek(ushort address)
        {
            return bus.Peek(address);
        }

        public ushort PeekWord(ushort address)
        {
            return bus.PeekWord(address);
        }

        public byte Read(ushort address)
        {
            if (address >= 0x0400 && address <= 0x07ff)
            {
                // Console.Error.WriteLine($"R screen {address:X4} ");
            }
            else if (0xc000 <= address && address <= 0xc00f)
            {
                // Console.Error.WriteLine($"R kbd {address:X4}");
            }
            else if (0xc010 <= address && address <= 0xc01f)
            {
                Console.Error.WriteLine($"R kbd strobe {address:X4}");

                // clear the keyboard and strobe
                bus[0xc000] &= 0x7f;
                bus[0xc010] &= 0x7f;
            }

            return bus.Read(address);
        }

        public ushort ReadWord(ushort address)
        {
            return bus.ReadWord(address);
        }

        public void Write(ushort address, byte value)
        {
            // allow for intercept
            // handler.Write(memory, address, value);

            if (address >= 0x0400 && address <= 0x07ff)
            {
                var (row, col) = GenerateRowColFromAddress(address);

                Console.CursorVisible = false;
                Console.SetCursorPosition(col, row);
                Console.Write($"{Convert.ToChar(value & 0x7f)}");
                Console.CursorVisible = true;
            }
            else if (0xc000 <= address && address <= 0xc00f)
            {
                // Console.Error.WriteLine($"W kbd {address:X4} {value:X2}");
            }
            else if (0xc010 <= address && address <= 0xc01f)
            {
                // Console.Error.WriteLine($"W kbd strobe {address:X4} {value:X2}");

                // // clear the keyboard and strobe
                // memory[0xc000] &= 0x7f;
                // memory[0xc010] &= 0x7f;
            }

            bus.Write(address, value);
        }

        public void WriteWord(ushort address, ushort value)
        {
            bus.WriteWord(address, value);
        }

        private static ushort GenerateLineFromAddress2(int lineNumber)
        {
            return lineToBaseAddress[lineNumber];
        }

        private static (int row, int col) GenerateRowColFromAddress(ushort address)
        {
            foreach (var (lo, hi, line) in baseAddressToLine)
            {
                if (lo <= address && address < hi)
                {
                    return (line, address - lo);
                }
            }

            return (-1, -1);
        }
    }
}
