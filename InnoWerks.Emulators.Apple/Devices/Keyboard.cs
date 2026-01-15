using System;
using System.Collections.Generic;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class Keyboard : IDevice
    {
        private readonly SoftSwitches softSwitches;

        private readonly List<ushort> handles =
        [
            // read
            SoftSwitchAddress.KBD,
            SoftSwitchAddress.OPENAPPLE,
            SoftSwitchAddress.SOLIDAPPLE,
            SoftSwitchAddress.SHIFT,

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

        public bool Handles(ushort address)
            => handles.Contains(address);

        public byte Read(ushort address)
        {
            // SimDebugger.Info($"Read Keyboard({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.KBD:
                    return softSwitches.State[SoftSwitch.KeyboardStrobe]
                        ? (byte)(softSwitches.KeyLatch | 0x80)
                        : softSwitches.KeyLatch;

                case SoftSwitchAddress.KBDSTRB:
                    softSwitches.State[SoftSwitch.KeyboardStrobe] = false;
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
                softSwitches.State[SoftSwitch.KeyboardStrobe] = false;
                softSwitches.KeyLatch &= 0x7f;
            }
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.KeyLatch = 0x00;
            softSwitches.State[SoftSwitch.KeyboardStrobe] = false;
        }

        /// <summary>
        /// Injects a key from the host system.
        /// </summary>
        public void InjectKey(byte ascii)
        {
            softSwitches.KeyLatch = ascii;
            softSwitches.State[SoftSwitch.KeyboardStrobe] = true;
        }

        public void OpenApple()
        {
            softSwitches.State[SoftSwitch.OpenApple] = true;
            softSwitches.State[SoftSwitch.KeyboardStrobe] = true;
        }

        public void SolidApple()
        {
            softSwitches.State[SoftSwitch.SolidApple] = true;
            softSwitches.State[SoftSwitch.KeyboardStrobe] = true;
        }

        public void ShiftKey()
        {
            softSwitches.State[SoftSwitch.ShiftKey] = true;
            softSwitches.State[SoftSwitch.KeyboardStrobe] = true;
        }
    }
}
