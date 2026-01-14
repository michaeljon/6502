using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Strobe : IDevice
    {
#pragma warning disable CS0414
        private bool strobe;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Strobe";

        public bool Handles(ushort address)
            => address == SoftSwitchAddress.STROBE;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleStrobe({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.STROBE:
                    strobe = true;
                    return 0;
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleStrobe({address:X4}, {value:X2})\n");
        }

        public void Reset()
        {
            strobe = false;
        }
    }
}
