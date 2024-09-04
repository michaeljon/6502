using System;

namespace InnoWerks.Simulators
{
    public class Memory : IMemory
    {
        private readonly byte[] memory = new byte[64 * 1024];

        public byte Read(ushort address)
        {
            return memory[address];
        }

        public byte Peek(ushort address)
        {
            return memory[address];
        }

        public ushort ReadWord(ushort address)
        {
            var lo = memory[address];
            var hi = memory[(ushort)(address + 1)];

            return (ushort)((hi << 8) | lo);
        }

        public ushort PeekWord(ushort address)
        {
            var lo = memory[address];
            var hi = memory[(ushort)(address + 1)];

            return (ushort)((hi << 8) | lo);
        }

        public void Write(ushort address, byte value)
        {
            memory[address] = value;
        }

        public void WriteWord(ushort address, ushort value)
        {
            memory[address] = (byte)(value & 0x00ff);
            memory[(ushort)(address + 1)] = (byte)((value >> 8) & 0xff);
        }

        public void LoadProgram(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, memory, origin, objectCode.Length);
        }

        public byte this[ushort address]
        {
            get
            {
                return memory[address];
            }

            set
            {
                memory[address] = value;
            }
        }
    }
}
