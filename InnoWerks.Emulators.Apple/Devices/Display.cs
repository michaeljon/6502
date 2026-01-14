using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Display : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Video Display";

        public bool Handles(ushort address)
            => (address >= SoftSwitchAddress.TXTCLR && address <= SoftSwitchAddress.HIRES) || address == SoftSwitchAddress.RDVBL;

        public Display()
        {
            Reset();
        }

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleDisplay({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.TXTCLR: State[SoftSwitch.TextMode] = false; return 0;
                case SoftSwitchAddress.TXTSET: State[SoftSwitch.TextMode] = true; return 0;
                case SoftSwitchAddress.MIXCLR: State[SoftSwitch.MixedMode] = false; return 0;
                case SoftSwitchAddress.MIXSET: State[SoftSwitch.MixedMode] = true; return 0;
                case SoftSwitchAddress.TXTPAGE1: State[SoftSwitch.Page2] = false; return 0;

                // handle IIe case where 80STORE is set
                case SoftSwitchAddress.TXTPAGE2: State[SoftSwitch.Page2] = true; return 0;
                case SoftSwitchAddress.LORES: State[SoftSwitch.HiRes] = false; return 0;
                case SoftSwitchAddress.HIRES: State[SoftSwitch.HiRes] = true; return 0;

                case SoftSwitchAddress.RDVBL: return (byte)(State[SoftSwitch.VerticalBlank] ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleDisplay({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.TXTCLR: State[SoftSwitch.TextMode] = false; break;
                case SoftSwitchAddress.TXTSET: State[SoftSwitch.TextMode] = true; break;
                case SoftSwitchAddress.MIXCLR: State[SoftSwitch.MixedMode] = false; break;
                case SoftSwitchAddress.MIXSET: State[SoftSwitch.MixedMode] = true; break;
                case SoftSwitchAddress.TXTPAGE1: State[SoftSwitch.Page2] = false; break;

                // handle IIe case where 80STORE is set
                case SoftSwitchAddress.TXTPAGE2: State[SoftSwitch.Page2] = true; break;
                case SoftSwitchAddress.LORES: State[SoftSwitch.HiRes] = false; break;
                case SoftSwitchAddress.HIRES: State[SoftSwitch.HiRes] = true; break;
            }
        }

        public void Reset()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }

            // basic setup
            State[SoftSwitch.TextMode] = true;
        }
    }
}
