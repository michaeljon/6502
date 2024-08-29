using System;

namespace InnoWerks.Simulators
{
    public class Memory : IMemory
    {
        private readonly byte[] memory = new byte[64 * 1024];

        public byte Read(ushort address, bool countAccess = true)
        {
            return memory[address];
        }

        public void Write(ushort address, byte value, bool countAccess = true)
        {
            memory[address] = value;
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
