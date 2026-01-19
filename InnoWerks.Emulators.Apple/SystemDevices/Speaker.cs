using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Speaker : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Speaker";

        public Speaker(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => address == SoftSwitchAddress.SPKR;

        public bool HandlesWrite(ushort address) => false;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Speaker({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.SPKR:
                    softSwitches.State[SoftSwitch.Speaker] = !softSwitches.State[SoftSwitch.Speaker];
                    break;
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Speaker({address:X4}, {value:X2})\n");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.Speaker] = false;
        }
    }
}
