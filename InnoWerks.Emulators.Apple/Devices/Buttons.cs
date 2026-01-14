using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Buttons : IDevice
    {
        private readonly bool[] state = new bool[3];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => $"Buttons";

        public bool Handles(ushort address)
            => address >= SoftSwitchAddress.BUTTON0 && address <= SoftSwitchAddress.BUTTON2;

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleButton({address:X4})\n");

            return address switch
            {
                SoftSwitchAddress.BUTTON0 => (byte)(state[0] ? 0x80 : 0x00),
                SoftSwitchAddress.BUTTON1 => (byte)(state[1] ? 0x80 : 0x00),
                SoftSwitchAddress.BUTTON2 => (byte)(state[2] ? 0x80 : 0x00),
                _ => 0x00,
            };
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleButton({address:X4}, {value:X2})\n");
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
