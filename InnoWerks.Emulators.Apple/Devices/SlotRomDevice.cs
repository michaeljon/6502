using System;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    // Core soft switches
    //   $C000..C07F On Board Resources
    //
    // Slot soft switches
    //   $C080..C08F Slot 0 /DEVSEL area (16 byte register file)
    //   $C090..C09F Slot 1 /DEVSEL area
    //   ... repeated for Slot 2..6
    //   $C0F0..C0FF Slot 7 /DEVSEL
    //
    // Slot ROM
    //   $C100..C1FF Slot 1 /IOSEL area (256 bytes 'PROM')
    //   ... repeated for Slot 2..6
    //   $C700..C7FF Slot 7 /IOSEL area
    //
    // Shared ROM addresses
    //   $C800..CFFF Common area for all Slots (2 KiB 'ROM')

#pragma warning disable CA1716, CA1707, CA1822
    public abstract class SlotRomDevice : IDevice
    {
        public const ushort IO_BASE_ADDR = 0xC080;

        public const ushort ROM_BASE_ADDR = 0xC100;

        private readonly byte[] rom = new byte[256];

        private readonly byte[] auxRom = new byte[2048];

        protected SlotRomDevice(int slot, byte[] romImage)
        {
            ArgumentNullException.ThrowIfNull(romImage, nameof(romImage));

            ArgumentOutOfRangeException.ThrowIfGreaterThan(slot, 7, nameof(slot));
            ArgumentOutOfRangeException.ThrowIfLessThan(slot, 0, nameof(slot));

            if (romImage.Length < 256)
            {
                throw new ArgumentException("Device ROM must be at least 256 bytes");
            }

            Slot = slot;

            if (romImage.Length > 256)
            {
                if (romImage.Length - 256 > 2048)
                {
                    throw new ArgumentException("Device ROM can be no longer than 256 bytes + 2k");
                }

                HasAuxRom = true;
                Array.Copy(romImage, 256, auxRom, 0, romImage.Length - 256);
            }

            Array.Copy(romImage, 0, rom, 0, 256);

            rom = romImage;
        }

        public DevicePriority Priority => DevicePriority.Slot;

        public int Slot { get; }

        public abstract bool Handles(ushort address);

        public abstract byte Read(ushort address);

        public abstract void Write(ushort address, byte value);

        public abstract void Reset();

        protected bool HasAuxRom { get; init; }

        protected ushort IoBaseAddressLo => (ushort)(IO_BASE_ADDR + (Slot * 0x10));

        protected ushort IoBaseAddressHi => (ushort)(IO_BASE_ADDR + (Slot * 0x10) + 0x0F);

        protected ushort RomBaseAddressLo => (ushort)(ROM_BASE_ADDR + (Slot * 0x100));

        protected ushort RomBaseAddressHi => (ushort)(ROM_BASE_ADDR + (Slot * 0x100) + 0xFF);

        protected bool IsIoReadRequest(ushort address)
        {
            return IoBaseAddressLo <= address && address <= IoBaseAddressHi;
        }

        protected bool IsRomReadRequest(ushort address)
        {
            // we need to worry about reading $CFFFF

            return RomBaseAddressLo <= address && address <= RomBaseAddressHi;
        }

        protected byte ReadSlotRom(ushort address)
        {
            ushort baseAddr = (ushort)(ROM_BASE_ADDR + (Slot * 0x100));
            return rom[address - baseAddr];
        }

        protected void WriteSlotRom(ushort address, byte value)
        {
            // Most slot ROMs ignore writes
        }
    }
#pragma warning restore CA1716, CA1707, CA1822
}
