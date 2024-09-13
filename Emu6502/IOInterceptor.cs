#pragma warning disable CA1823, IDE0005

using System;
using System.Collections.Generic;
using InnoWerks.Simulators;


namespace Emu6502
{
    public class IOInterceptor : IMemory
    {
        private readonly Memory memory = new();

        private readonly IOHandler handler = new();

        private Dictionary<int, int> screenMap = new()
        {
            { 0, 0x400 },
            { 1, 0x480 },
            { 2, 0x500 },
            { 3, 0x580 },
            { 4, 0x600 },
            { 5, 0x680 },
            { 6, 0x700 },
            { 7, 0x780 },

            { 8, 0x428 },
            { 9, 0x4a8 },
            { 10, 0x528 },
            { 11, 0x5a8 },
            { 12, 0x628 },
            { 13, 0x6a8 },
            { 14, 0x728 },
            { 15, 0x7a8 },

            { 16, 0x450 },
            { 17, 0x4d0 },
            { 18, 0x550 },
            { 19, 0x5d0 },
            { 20, 0x650 },
            { 21, 0x6d0 },
            { 22, 0x750 },
            { 23, 0x7d0 },
        };

        public byte this[ushort address]
        {
            get => Read(address);
            set => Write(address, value);
        }

        public void LoadProgram(byte[] objectCode, ushort origin)
        {
            memory.LoadProgram(objectCode, origin);
        }

        public byte Peek(ushort address)
        {
            return memory.Peek(address);
        }

        public ushort PeekWord(ushort address)
        {
            return memory.PeekWord(address);
        }

        public byte Read(ushort address)
        {
            // allow for intercept
            // handler.Read(memory, address);

            if (address >= 0x0400 && address <= 0x07ff)
            {
                // Console.WriteLine($"R screen {address:X4} ");
            }
            else if (address == 0xc000)
            {
                // Console.WriteLine($"R kbd");
            }
            else if (address == 0xc010)
            {
                // Console.WriteLine($"R kbd strobe");
                memory[0xc000] &= 0x7f;
            }

            return memory.Read(address);
        }

        public ushort ReadWord(ushort address)
        {
            return memory.ReadWord(address);
        }

        public void Write(ushort address, byte value)
        {
            // allow for intercept
            // handler.Write(memory, address, value);

            if (address >= 0x0400 && address <= 0x07ff)
            {
                Console.SetCursorPosition(0, 0);

                for (var line = 0; line < 24; line++)
                {
                    var lineBase = screenMap[line];
                    Console.SetCursorPosition(0, line);

                    for (var column = 0; column < 40; column++)
                    {
                        var c = memory[(ushort)(lineBase + column)] & 0x7f;
                        Console.Write($"{Convert.ToChar(c & 0x7f)}");
                    }
                }
            }
            else if (address == 0xc000)
            {
                // Console.WriteLine($"W kbd");
            }
            else if (address == 0xc010)
            {
                // Console.WriteLine($"W kbd strobe");
            }

            memory.Write(address, value);
        }

        public void WriteWord(ushort address, ushort value)
        {
            memory.WriteWord(address, value);
        }
    }
}
