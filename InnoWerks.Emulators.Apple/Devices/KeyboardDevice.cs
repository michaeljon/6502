using System;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class KeyboardDevice : IDevice
    {
        private byte keyLatch;
        private bool keyStrobe;

        public DevicePriority Priority => DevicePriority.System;

        public int Slot => -1;

        public string Name => "Keyboard";

        public bool Handles(ushort address)
            => address == SoftSwitchAddress.KBD || address == SoftSwitchAddress.KBDSTRB;

        public byte Read(ushort address)
        {
            switch (address)
            {
                case SoftSwitchAddress.KBD:
                    return keyStrobe
                        ? (byte)(keyLatch | 0x80)
                        : keyLatch;

                case SoftSwitchAddress.KBDSTRB:
                    keyStrobe = false;
                    break;
            }

            return 0;
        }

        public void Write(ushort address, byte value)
        {
            if (address == SoftSwitchAddress.KBDSTRB)
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
