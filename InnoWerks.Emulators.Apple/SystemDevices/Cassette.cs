using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Cassette : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Cassette";

        public Cassette(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => address == SoftSwitchAddress.TAPEOUT || address == SoftSwitchAddress.TAPEIN;

        public bool HandlesWrite(ushort address) => false;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Cassette({address:X4}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            switch (address)
            {
                case SoftSwitchAddress.TAPEOUT:
                    softSwitches.State[SoftSwitch.TapeOut] = !softSwitches.State[SoftSwitch.TapeOut];
                    break;

                case SoftSwitchAddress.TAPEIN: return (byte)(softSwitches.State[SoftSwitch.TapeIn] ? 0x80 : 0x00);

                default:
                    throw new ArgumentOutOfRangeException(nameof(address), $"Read {address:X4} is not supported in this device");
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Cassette({address:X4}, {value:X2}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.TapeIn] = false;
            softSwitches.State[SoftSwitch.TapeOut] = false;
        }
    }
}
