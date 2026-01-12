using System.Net.Sockets;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class SoftSwitchRam : IDevice
    {
        private readonly AppleConfiguration configuration;

        // Common switches
        public bool TextMode { get; private set; } = true;

        public bool MixedMode { get; private set; }

        public bool Page2 { get; private set; }

        // IIe
        public bool AuxRead { get; private set; }

        public bool AuxWrite { get; private set; }

        public bool RomEnabled { get; private set; } = true;

        public int RomBank { get; private set; } // 0 or 1

        public bool RomRead { get; private set; } = true;

        public bool RomWrite { get; private set; }

        public bool RamRead { get; private set; }

        public bool RamWrite { get; private set; }

        public bool UseInternalRom { get; private set; } = true;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "SoftSwitches";

        public bool Handles(ushort address)
            => address >= SoftSwitch.BASE && address <= SoftSwitch.TOP;

        public SoftSwitchRam(AppleConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
                case SoftSwitch.TEXTOFF:
                    TextMode = false;
                    return 0;
                case SoftSwitch.TEXTON:
                    TextMode = true;
                    return 0;

                case SoftSwitch.PAGE2OFF:
                    Page2 = false;
                    return 0;
                case SoftSwitch.PAGE2ON:
                    Page2 = true;
                    return 0;

                // IIe video / identification
                case SoftSwitch.HIRESOFF:
                    return 0x00;  // no mixed mode (or whatever default)
                case SoftSwitch.HIRESON:
                    return 0x00;  // no 80-column card installed

                // IIe aux memory
                case SoftSwitch.RAMRDOFF:
                    AuxRead = false;
                    return 0;
                case SoftSwitch.RAMDRON:
                    AuxRead = true;
                    return 0;
                case SoftSwitch.RAMWRTOFF:
                    AuxWrite = false;
                    return 0;
                case SoftSwitch.RAMWRTON:
                    AuxWrite = true;
                    return 0;

                case SoftSwitch.INTCXROMON:
                    UseInternalRom = true;
                    return 0;

                case SoftSwitch.INTCXROMOFF:
                    UseInternalRom = false;
                    return 0;

                case 0xC080:
                    RomEnabled = true;
                    RomBank = 0;
                    RamRead = false;
                    return 0;

                case 0xC081:
                    RomEnabled = true;
                    RomBank = 1;
                    RamRead = false;
                    return 0;

                case 0xC082:
                    RomEnabled = false;
                    RamRead = true;
                    return 0;

                case 0xC083:
                    RomWrite = true;
                    return 0;
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            switch (address)
            {
                // Video switches
                case SoftSwitch.TEXTOFF:
                    TextMode = false;
                    break;
                case SoftSwitch.TEXTON:
                    TextMode = true;
                    break;

                case 0xC052:
                    MixedMode = false;
                    break;
                case 0xC053:
                    MixedMode = true;
                    break;

                case SoftSwitch.PAGE2OFF:
                    Page2 = false;
                    break;
                case SoftSwitch.PAGE2ON:
                    Page2 = true;
                    break;

                case SoftSwitch.HIRESOFF: /* typically write ignored */ break;
                case SoftSwitch.HIRESON: /* typically write ignored */ break;

                // Aux memory
                case SoftSwitch.RAMRDOFF:
                    AuxRead = false;
                    break;
                case SoftSwitch.RAMDRON:
                    AuxRead = true;
                    break;
                case SoftSwitch.RAMWRTOFF:
                    AuxWrite = false;
                    break;
                case SoftSwitch.RAMWRTON:
                    AuxWrite = true;
                    break;

                // ROM enable / bank select
                case 0xC080:
                    RomEnabled = true;
                    RomBank = 0;
                    RamRead = false;
                    break;
                case 0xC081:
                    RomEnabled = true;
                    RomBank = 1;
                    RamRead = false;
                    break;
                case 0xC082:
                    RomEnabled = false;
                    RamRead = true;
                    break;
                case 0xC083:
                    RomWrite = true;
                    break;

                case SoftSwitch.INTCXROMON:
                    UseInternalRom = true;
                    break;

                case SoftSwitch.INTCXROMOFF:
                    UseInternalRom = false;
                    break;

                    // unimplemented: do nothing
            }
        }

        public void Reset()
        {
            UseInternalRom = true;

            TextMode = true;
            RomEnabled = true;
            RomBank = 0;
            RomRead = true;
        }
    }
}
