using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Annunciators : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Annunciators";

        public Annunciators(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool Handles(ushort address)
            => address >= SoftSwitchAddress.CLRAN0 && address <= SoftSwitchAddress.SETAN3;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Annunciator({address:X4})\n");

            switch (address)
            {
                // hanldle if IOU == true case
                case SoftSwitchAddress.CLRAN0: softSwitches.State[SoftSwitch.Annunciator0] = false; return 0;
                case SoftSwitchAddress.SETAN0: softSwitches.State[SoftSwitch.Annunciator0] = true; return 0;
                case SoftSwitchAddress.CLRAN1: softSwitches.State[SoftSwitch.Annunciator1] = false; return 0;
                case SoftSwitchAddress.SETAN1: softSwitches.State[SoftSwitch.Annunciator1] = true; return 0;
                case SoftSwitchAddress.CLRAN2: softSwitches.State[SoftSwitch.Annunciator2] = false; return 0;
                case SoftSwitchAddress.SETAN2: softSwitches.State[SoftSwitch.Annunciator2] = true; return 0;
                case SoftSwitchAddress.CLRAN3: softSwitches.State[SoftSwitch.Annunciator3] = false; return 0;
                case SoftSwitchAddress.SETAN3: softSwitches.State[SoftSwitch.Annunciator3] = true; return 0;
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Annunciator({address:X4}, {value:X2})\n");

            switch (address)
            {
                // handle weirdness with IOU
                case SoftSwitchAddress.CLRAN0: softSwitches.State[SoftSwitch.Annunciator0] = false; break;
                case SoftSwitchAddress.SETAN0: softSwitches.State[SoftSwitch.Annunciator0] = true; break;
                case SoftSwitchAddress.CLRAN1: softSwitches.State[SoftSwitch.Annunciator1] = false; break;
                case SoftSwitchAddress.SETAN1: softSwitches.State[SoftSwitch.Annunciator1] = true; break;
                case SoftSwitchAddress.CLRAN2: softSwitches.State[SoftSwitch.Annunciator2] = false; break;
                case SoftSwitchAddress.SETAN2: softSwitches.State[SoftSwitch.Annunciator2] = true; break;
                case SoftSwitchAddress.CLRAN3: softSwitches.State[SoftSwitch.Annunciator3] = false; break;
                case SoftSwitchAddress.SETAN3: softSwitches.State[SoftSwitch.Annunciator3] = true; break;
            }
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.Annunciator0] = false;
            softSwitches.State[SoftSwitch.Annunciator1] = false;
            softSwitches.State[SoftSwitch.Annunciator2] = false;
            softSwitches.State[SoftSwitch.Annunciator3] = false;
        }
    }
}
