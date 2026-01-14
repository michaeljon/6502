using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Paddles : IDevice
    {
        private readonly bool[] state = new bool[4];

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
                SoftSwitchAddress.PADDLE0 => (byte)(state[0] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE1 => (byte)(state[1] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE2 => (byte)(state[2] ? 0x80 : 0x00),
                SoftSwitchAddress.PADDLE3 => (byte)(state[3] ? 0x80 : 0x00),
                _ => 0x00,
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandlePaddle({address:X4}, {value:X2})\n");
        }

        public void Reset()
        {
            for (var i = 0; i < state.Length; i++)
            {
                state[i] = false;
            }
        }
    }
}
