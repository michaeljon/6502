using System;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class Keyboard : IDevice
    {
        private byte keyLatch;
        private bool keyStrobe;

        private bool openApple;
        private bool solidApple;
        private bool shiftKey;

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

                case SoftSwitchAddress.OPENAPPLE: return (byte)(openApple ? 0x80 : 0x00);
                case SoftSwitchAddress.SOLIDAPPLE: return (byte)(solidApple ? 0x80 : 0x00);
                case SoftSwitchAddress.SHIFT: return (byte)(shiftKey ? 0x80 : 0x00);
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

        public void OpenApple()
        {
            openApple = true;
            keyStrobe = true;
        }

        public void SolidApple()
        {
            solidApple = true;
            keyStrobe = true;
        }

        public void ShiftKey()
        {
            shiftKey = true;
            keyStrobe = true;
        }
    }
}
