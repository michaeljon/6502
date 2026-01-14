using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Speaker : IDevice
    {
        private bool spkr;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Speaker";

        public bool Handles(ushort address)
            => address == SoftSwitchAddress.SPKR;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleSpeaker({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.SPKR:
                    spkr = !spkr;
                    return 0;
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleSpeaker({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.SPKR:
                    spkr = !spkr;
                    break;
            }
        }

        public void Reset()
        {
            spkr = false;
        }
    }
}
