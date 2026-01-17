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

        private readonly List<ushort> handlesRead =
        [
            SoftSwitchAddress.CLRAN0,
            SoftSwitchAddress.SETAN0,
            SoftSwitchAddress.CLRAN1,
            SoftSwitchAddress.SETAN1,
            SoftSwitchAddress.CLRAN2,
            SoftSwitchAddress.SETAN2,

            // II/II+ only, on IIe this is a video switch
            // SoftSwitchAddress.CLRAN3,
            // SoftSwitchAddress.SETAN3,
        ];

        private readonly List<ushort> handlesWrite =
        [
            SoftSwitchAddress.CLRAN0,
            SoftSwitchAddress.SETAN0,
            SoftSwitchAddress.CLRAN1,
            SoftSwitchAddress.SETAN1,
            SoftSwitchAddress.CLRAN2,
            SoftSwitchAddress.SETAN2,

            // II/II+ only, on IIe this is a video switch
            // SoftSwitchAddress.CLRAN3,
            // SoftSwitchAddress.SETAN3,
        ];

        public Annunciators(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => handlesRead.Contains(address);

        public bool HandlesWrite(ushort address)
            => handlesWrite.Contains(address);

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
            softSwitches.State[SoftSwitch.Annunciator3] = true;
        }
    }
}
