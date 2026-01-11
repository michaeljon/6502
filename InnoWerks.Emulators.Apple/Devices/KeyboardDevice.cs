using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class KeyboardDevice : IDevice
    {
        private byte keyLatch;
        private bool keyStrobe;

        public DevicePriority Priority => DevicePriority.System;

        public bool Handles(ushort address)
            => address == SoftSwitch.KBD || address == SoftSwitch.KBDSTROBE;

        public byte Read(ushort address)
        {
            switch (address)
            {
                case SoftSwitch.KBD:
                    return keyStrobe
                        ? (byte)(keyLatch | 0x80)
                        : keyLatch;

                case SoftSwitch.KBDSTROBE:
                    keyStrobe = false;
                    return 0;
            }

            return 0;
        }

        public void Write(ushort address, byte value)
        {
            if (address == SoftSwitch.KBDSTROBE)
            {
                keyStrobe = false;
                keyLatch &= 0x7f;
            }
        }

        public void Reset()
        {
            keyLatch = 0;
            keyStrobe = false;
        }

        /// <summary>
        /// Injects a key from the host system.
        /// </summary>
        public void InjectKey(byte ascii)
        {
            keyLatch = ascii;
            keyStrobe = true;
        }
    }
}
