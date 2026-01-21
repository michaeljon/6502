using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Strobe : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Strobe";

        public Strobe(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => address == SoftSwitchAddress.STROBE;

        public bool HandlesWrite(ushort address) => false;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Strobe({address:X4}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            switch (address)
            {
                case SoftSwitchAddress.STROBE:
                    softSwitches.State[SoftSwitch.GameStrobe] = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(address), $"Read {address:X4} is not supported in this device");
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Strobe({address:X4}, {value:X2}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.GameStrobe] = false;
        }
    }
}
