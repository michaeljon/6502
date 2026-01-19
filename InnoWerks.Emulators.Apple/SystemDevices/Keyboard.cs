using System;
using System.Collections.Generic;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class Keyboard : IDevice
    {
        private readonly SoftSwitches softSwitches;

        private readonly List<ushort> handlesRead =
        [
            // read
            SoftSwitchAddress.KBD,
            SoftSwitchAddress.OPENAPPLE,
            SoftSwitchAddress.SOLIDAPPLE,
            SoftSwitchAddress.SHIFT,

            // read/write
            SoftSwitchAddress.KBDSTRB,
        ];

        private readonly List<ushort> handlesWrite =
        [
            // read/write
            SoftSwitchAddress.KBDSTRB,
        ];

        public DevicePriority Priority => DevicePriority.System;

        public int Slot => -1;

        public string Name => "Keyboard";

        public Keyboard(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => handlesRead.Contains(address);

        public bool HandlesWrite(ushort address)
            => handlesWrite.Contains(address);

        public byte Read(ushort address)
        {
            // SimDebugger.Info($"Read Keyboard({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.KBD:
                    return softSwitches.KeyStrobe
                        ? (byte)(softSwitches.KeyLatch | 0x80)
                        : softSwitches.KeyLatch;

                case SoftSwitchAddress.KBDSTRB:
                    softSwitches.KeyStrobe = false;
                    break;

                case SoftSwitchAddress.OPENAPPLE: return (byte)(softSwitches.State[SoftSwitch.OpenApple] ? 0x80 : 0x00);
                case SoftSwitchAddress.SOLIDAPPLE: return (byte)(softSwitches.State[SoftSwitch.SolidApple] ? 0x80 : 0x00);
                case SoftSwitchAddress.SHIFT: return (byte)(softSwitches.State[SoftSwitch.ShiftKey] ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            // SimDebugger.Info($"Write Keyboard({address:X4}, {value:X2})\n");

            if (address == SoftSwitchAddress.KBDSTRB)
            {
                softSwitches.KeyStrobe = false;
                softSwitches.KeyLatch &= 0x7f;  // leave the value, clear the high bit
            }
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.KeyStrobe = false;
            softSwitches.KeyLatch = 0x00;
        }

        /// <summary>
        /// Injects a key from the host system.
        /// </summary>
        public void InjectKey(byte ascii)
        {
            softSwitches.KeyStrobe = true;
            softSwitches.KeyLatch = ascii;
        }

        public void OpenApple()
        {
            softSwitches.State[SoftSwitch.OpenApple] = true;
        }

        public void SolidApple()
        {
            softSwitches.State[SoftSwitch.SolidApple] = true;
        }

        public void ShiftKey()
        {
            softSwitches.State[SoftSwitch.ShiftKey] = true;
        }
    }
}
