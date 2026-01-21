using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Paddles : IDevice
    {
        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Paddles";

        public Paddles(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            this.softSwitches = softSwitches;
        }

        public bool HandlesRead(ushort address)
            => (address >= SoftSwitchAddress.PADDLE0 && address <= SoftSwitchAddress.PADDLE3) || address == SoftSwitchAddress.PTRIG;

        public bool HandlesWrite(ushort address) => false;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Paddle({address:X4}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            return address switch
            {
                SoftSwitchAddress.PADDLE0 => (byte)(softSwitches.State[SoftSwitch.Paddle0] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE1 => (byte)(softSwitches.State[SoftSwitch.Paddle1] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE2 => (byte)(softSwitches.State[SoftSwitch.Paddle2] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE3 => (byte)(softSwitches.State[SoftSwitch.Paddle3] ? 0x80 : 0x00),

                // this should start a timer, for now just ignore it
                SoftSwitchAddress.PTRIG => 0x00,

                _ =>
                    throw new ArgumentOutOfRangeException(nameof(address), $"Read {address:X4} is not supported in this device")
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Paddle({address:X4}, {value:X2}) -> {SoftSwitchAddress.LookupAddress(address)}\n");

            throw new ArgumentOutOfRangeException(nameof(address), $"Write {address:X4} is not supported in this device");
        }

        public void Tick(int cycles) {/* NO-OP */ }

        public void Reset()
        {
            softSwitches.State[SoftSwitch.Paddle0] = false;
            softSwitches.State[SoftSwitch.Paddle1] = false;
            softSwitches.State[SoftSwitch.Paddle2] = false;
            softSwitches.State[SoftSwitch.Paddle3] = false;
        }
    }
}
