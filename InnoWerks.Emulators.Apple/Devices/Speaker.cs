using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Speaker : IDevice
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

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
                    State[SoftSwitch.Speaker] = !State[SoftSwitch.Speaker];
                    return 0;

                case SoftSwitchAddress.TAPEIN: return (byte)(State[SoftSwitch.TapeIn] ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleSpeaker({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.SPKR:
                    State[SoftSwitch.Speaker] = !State[SoftSwitch.Speaker];
                    break;
            }
        }

        public void Reset()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }
        }
    }
}
