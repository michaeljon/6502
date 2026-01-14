using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleIISoftSwitches : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Apple II / II+ Soft Switches";

        public bool Handles(ushort address) => false;

        public AppleIISoftSwitches()
        {
            Reset();
        }

        public byte Read(ushort address)
        {
            SimDebugger.Info($"[SS] Read({address:X4})\n");

            return 0;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"[SS] Write({address:X4}, {value:X2})\n");
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
