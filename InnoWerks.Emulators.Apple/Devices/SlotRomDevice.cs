using System;
using InnoWerks.Processors;
using InnoWerks.Simulators;
using Microsoft.VisualBasic;

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

        public const ushort EXPANSION_ROM_BASE_ADDR = 0xC800;

        protected SoftSwitches softSwitches { get; }

        private readonly byte[] rom = new byte[256];

        private readonly byte[] expansionRom = new byte[2048];

        protected SlotRomDevice(int slot, string name, SoftSwitches softSwitches, byte[] romImage)
        {
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));
            ArgumentNullException.ThrowIfNull(romImage, nameof(romImage));

            ArgumentOutOfRangeException.ThrowIfGreaterThan(slot, 7, nameof(slot));
            ArgumentOutOfRangeException.ThrowIfLessThan(slot, 0, nameof(slot));

            if (romImage.Length < 256)
            {
                throw new ArgumentException("Device ROM must be at least 256 bytes");
            }

            Slot = slot;
            Name = name;

            this.softSwitches = softSwitches;

            if (romImage.Length > 256)
            {
                if (Slot < 1 || Slot > 4)
                {
                    throw new ArgumentException("Only devices in slots 1-4 can use extra ROM space $C800-$CFFF");
                }

                if (romImage.Length - 256 > 2048)
                {
                    throw new ArgumentException("Device ROM can be no longer than 256 bytes + 2k");
                }

                HasAuxRom = true;
                Array.Copy(romImage, 256, expansionRom, 0, romImage.Length - 256);
            }

            Array.Copy(romImage, 0, rom, 0, 256);
        }

        public DevicePriority Priority => DevicePriority.Slot;

        public int Slot { get; }

        public string Name { get; }

        public abstract bool Handles(ushort address);

        public abstract byte Read(ushort address);

        public abstract void Write(ushort address, byte value);

        public abstract void Tick(int cycles);

        public abstract void Reset();

        protected bool HasAuxRom { get; init; }

        // 16 bytes bytes
        protected ushort IoBaseAddressLo => (ushort)(IO_BASE_ADDR + (Slot * 0x10));

        protected ushort IoBaseAddressHi => (ushort)(IO_BASE_ADDR + (Slot * 0x10) + 0x0F);

        // 256 bytes
        protected ushort RomBaseAddressLo => (ushort)(ROM_BASE_ADDR + ((Slot - 1) * 0x100));

        protected ushort RomBaseAddressHi => (ushort)(ROM_BASE_ADDR + ((Slot - 1) * 0x100) + 0xFF);

        // 2048 bytes
        protected ushort ExpansionBaseAddressLo => (ushort)(EXPANSION_ROM_BASE_ADDR + ((Slot - 1) * 0x100));

        protected ushort ExpansionBaseAddressHi => (ushort)(EXPANSION_ROM_BASE_ADDR + ((Slot - 1) * 0x100) + 0x03FF);

        protected virtual bool IsIoReadRequest(ushort address)
        {
            // SimDebugger.Info("Slot {0} IsIoReadRequest({1:X4})\n", Slot, address);

            return IoBaseAddressLo <= address && address <= IoBaseAddressHi;
        }

        protected virtual bool IsRomReadRequest(ushort address)
        {
            // SimDebugger.Info("Slot {0} IsRomReadRequest({1:X4})\n", Slot, address);

            if (Slot > 0 && Slot <= 4)
            {
                // allow for expansion rom
                return (RomBaseAddressLo <= address && address <= RomBaseAddressHi) || (ExpansionBaseAddressLo <= address && address <= ExpansionBaseAddressHi);
            }
            else
            {
                // this is just a regular rom read
                return RomBaseAddressLo <= address && address <= RomBaseAddressHi;
            }
        }

        protected virtual byte ReadSlotRom(ushort address)
        {
            // SimDebugger.Info("Slot {0} ReadSlotRom({1:X4})\n", Slot, address);

            if (ExpansionBaseAddressLo <= address && address <= ExpansionBaseAddressHi)
            {
                ushort baseAddr = (ushort)(EXPANSION_ROM_BASE_ADDR + ((Slot - 1) * 0x200));
                return expansionRom[address - baseAddr];
            }
            else
            {
                ushort baseAddr = (ushort)(ROM_BASE_ADDR + ((Slot - 1) * 0x100));
                return rom[address - baseAddr];
            }
        }

        protected void WriteSlotRom(ushort address, byte value)
        {
            // Most slot ROMs ignore writes
        }
    }
#pragma warning restore CA1716, CA1707, CA1822
}
