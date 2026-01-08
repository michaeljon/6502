using System;

namespace InnoWerks.Simulators
{
    public class Bus : IBus
    {
        private readonly byte[] memory = new byte[64 * 1024];

        private int transactionCycles;

        public void BeginTransaction()
        {
            transactionCycles = 0;
        }

        public int EndTransaction()
        {
            return transactionCycles;
        }

        public long CycleCount { get; private set; }

        public byte Peek(ushort address)
        {
            return memory[address];
        }

        public byte Read(ushort address)
        {
            IncCycles(1);

            return memory[address];
        }

        public void Write(ushort address, byte value)
        {
            IncCycles(1);

            memory[address] = value;
        }

        public ushort PeekWord(ushort address)
        {
            var lo = memory[address];
            var hi = memory[(ushort)(address + 1)];

            return (ushort)((hi << 8) | lo);
        }

        public ushort ReadWord(ushort address)
        {
            IncCycles(2);

            var lo = memory[address];
            var hi = memory[(ushort)(address + 1)];

            return (ushort)((hi << 8) | lo);
        }

        public void WriteWord(ushort address, ushort value)
        {
            IncCycles(2);

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

        private void IncCycles(int howMany)
        {
            CycleCount += howMany;
            transactionCycles += howMany;
        }
    }
}
