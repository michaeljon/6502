using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Cassette : IDevice
    {
        private bool tapeIn;

        private bool tapeOut;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Cassette";

        public bool Handles(ushort address)
            => address == SoftSwitchAddress.TAPEOUT || address == SoftSwitchAddress.TAPEIN;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleCassette({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.TAPEOUT:
                    tapeOut = !tapeOut;
                    return 0;

                case SoftSwitchAddress.TAPEIN: return (byte)(tapeIn ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleCassette({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.TAPEOUT:
                    tapeOut = !tapeOut;
                    break;
            }
        }

        public void Reset()
        {
            tapeIn = false;
            tapeOut = false;
        }
    }
}
