using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Strobe : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Strobe";

        public Strobe(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool Handles(ushort address)
            => address == SoftSwitchAddress.STROBE;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Strobe({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.STROBE:
                    softSwitches.State[SoftSwitch.GameStrobe] = true;
                    break;
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Strobe({address:X4}, {value:X2})\n");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.GameStrobe] = false;
        }
    }
}
