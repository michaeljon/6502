namespace InnoWerks.Emulators.Apple
{
    public sealed class SoftSwitches
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

        // Keyboard
        public byte KeyLatch { get; set; }

        public bool KeyStrobe { get; set; }

        public static bool Handles(ushort address)
            => address >= 0xC000 && address <= 0xC0FF;

        public SoftSwitches(AppleConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public byte Read(ushort address)
        {
            switch (address)
            {
                case 0xC000:
                    return KeyStrobe
                        ? (byte)(KeyLatch | 0x80)
                        : (byte)KeyLatch;

                case 0xC010:
                    KeyStrobe = false;
                    return 0;

                case 0xC050: TextMode = false; return 0;
                case 0xC051: TextMode = true; return 0;

                case 0xC054: Page2 = false; return 0;
                case 0xC055: Page2 = true; return 0;

                // IIe video / identification
                case 0xC056: return 0x00;  // no mixed mode (or whatever default)
                case 0xC057: return 0x00;  // no 80-column card installed

                // IIe aux memory
                case 0xC002: AuxRead = false; return 0;
                case 0xC003: AuxRead = true; return 0;
                case 0xC004: AuxWrite = false; return 0;
                case 0xC005: AuxWrite = true; return 0;

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

        public void Write(ushort address)
        {
            switch (address)
            {
                // Video switches
                case 0xC050: TextMode = false; break;
                case 0xC051: TextMode = true; break;
                case 0xC052: MixedMode = false; break;
                case 0xC053: MixedMode = true; break;
                case 0xC054: Page2 = false; break;
                case 0xC055: Page2 = true; break;

                case 0xC056: /* typically write ignored */ break;
                case 0xC057: /* typically write ignored */ break;

                // Aux memory
                case 0xC002: AuxRead = false; break;
                case 0xC003: AuxRead = true; break;
                case 0xC004: AuxWrite = false; break;
                case 0xC005: AuxWrite = true; break;

                // ROM enable / bank select
                case 0xC080: RomEnabled = true; RomBank = 0; RamRead = false; break;
                case 0xC081: RomEnabled = true; RomBank = 1; RamRead = false; break;
                case 0xC082: RomEnabled = false; RamRead = true; break;
                case 0xC083: RomWrite = true; break;

                // Keyboard write: writing here typically clears the strobe
                case 0xC000:
                case 0xC010:
                    KeyStrobe = false;
                    break;

                    // unimplemented: do nothing
            }
        }
    }
}
