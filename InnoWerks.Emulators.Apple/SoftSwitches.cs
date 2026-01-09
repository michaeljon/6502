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
                case 0xC000: return KeyLatch;       // KBD
                case 0xC010: return 0;              // KBDSTRB

                case 0xC050: TextMode = false; return 0;
                case 0xC051: TextMode = true; return 0;

                case 0xC054: Page2 = false; return 0;
                case 0xC055: Page2 = true; return 0;

                // IIe aux memory
                case 0xC002: AuxRead = false; return 0;
                case 0xC003: AuxRead = true; return 0;
                case 0xC004: AuxWrite = false; return 0;
                case 0xC005: AuxWrite = true; return 0;
            }

            return 0xFF;
        }

        public void Write(ushort address)
        {
            Read(address); // same side effects
        }
    }
}
