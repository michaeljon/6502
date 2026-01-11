using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class SlotRomDevice : IDevice
    {
        private readonly int slot;
        private readonly byte[] rom; // 256 or larger

        public SlotRomDevice(int slot, byte[] romImage)
        {
            this.slot = slot;
            rom = romImage;
        }

        public DevicePriority Priority => DevicePriority.Slot;

        public bool Handles(ushort address)
        {
            ushort baseAddr = (ushort)(0xC100 + (slot * 0x100));
            return address >= baseAddr && address < baseAddr + rom.Length;
        }

        public byte Read(ushort address)
        {
            ushort baseAddr = (ushort)(0xC100 + (slot * 0x100));
            return rom[address - baseAddr];
        }

        public void Write(ushort address, byte value)
        {
            // Most slot ROMs ignore writes
        }

        public void Reset() { }
    }
}
