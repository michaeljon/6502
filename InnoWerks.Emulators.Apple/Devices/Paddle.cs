using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Paddles : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Paddles";

        public bool Handles(ushort address)
            => address >= SoftSwitchAddress.PADDLE0 && address <= SoftSwitchAddress.PADDLE3;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandlePaddle({address:X4})\n");

            return address switch
            {
                SoftSwitchAddress.PADDLE0 => (byte)(State[SoftSwitch.Paddle0] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE1 => (byte)(State[SoftSwitch.Paddle1] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE2 => (byte)(State[SoftSwitch.Paddle2] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE3 => (byte)(State[SoftSwitch.Paddle3] ? 0x80 : 0x00),
                _ => 0x00,
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandlePaddle({address:X4}, {value:X2})\n");
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
