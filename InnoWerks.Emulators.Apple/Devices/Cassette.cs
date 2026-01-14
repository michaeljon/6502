using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Cassette : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

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
                    State[SoftSwitch.TapeOut] = !State[SoftSwitch.TapeOut];
                    return 0;

                case SoftSwitchAddress.TAPEIN: return (byte)(State[SoftSwitch.TapeIn] ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleCassette({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.TAPEOUT:
                    State[SoftSwitch.TapeOut] = !State[SoftSwitch.TapeOut];
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
