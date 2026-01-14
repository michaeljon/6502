using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleIIeSoftSwitches : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Apple IIe Soft Switches";

        private readonly List<ushort> handles =
        [
            // read
            SoftSwitchAddress.RDCXROM,
            SoftSwitchAddress.RDALTZP,
            SoftSwitchAddress.RDC3ROM,
            SoftSwitchAddress.RD80STORE,

            SoftSwitchAddress.RD80VID,

            SoftSwitchAddress.RDIOUDIS,
            SoftSwitchAddress.RDDHIRES,

            // write
            SoftSwitchAddress.SETSLOTCXROM,
            SoftSwitchAddress.SETINTCXROM,

            SoftSwitchAddress.SETINTC3ROM,
            SoftSwitchAddress.SETSLOTC3ROM,

            SoftSwitchAddress.CLR80VID,
            SoftSwitchAddress.SET80VID,

            SoftSwitchAddress.CLRALTCHAR,
            SoftSwitchAddress.SETALTCHAR,

            SoftSwitchAddress.IOUDISON,
            SoftSwitchAddress.IOUDISOFF,
        ];

        public bool Handles(ushort address)
            => handles.Contains(address);

        public AppleIIeSoftSwitches()
        {
            Reset();
        }

        public byte Read(ushort address)
        {
            SimDebugger.Info($"[SS] Read({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.RDCXROM: return (byte)(State[SoftSwitch.SlotRomEnabled] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDALTZP: return (byte)(State[SoftSwitch.ZpAux] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDC3ROM: return (byte)(State[SoftSwitch.Slot3RomEnabled] ? 0x80 : 0x00);
                case SoftSwitchAddress.RD80STORE: return (byte)(State[SoftSwitch.Store80] ? 0x80 : 0x00);

                case SoftSwitchAddress.RD80VID: return (byte)(State[SoftSwitch.EightyColumnFirmware] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDIOUDIS: return (byte)(State[SoftSwitch.IOU] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDDHIRES: return (byte)(State[SoftSwitch.DoubleHiRes] ? 0x80 : 0x00);
            }

            return 0;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"[SS] Write({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.SETSLOTCXROM: State[SoftSwitch.SlotRomEnabled] = true; break;
                case SoftSwitchAddress.SETINTCXROM: State[SoftSwitch.SlotRomEnabled] = false; break;

                case SoftSwitchAddress.SETINTC3ROM: State[SoftSwitch.Slot3RomEnabled] = false; break;
                case SoftSwitchAddress.SETSLOTC3ROM: State[SoftSwitch.Slot3RomEnabled] = true; break;

                case SoftSwitchAddress.CLR80VID: State[SoftSwitch.EightyColumnFirmware] = false; break;
                case SoftSwitchAddress.SET80VID: State[SoftSwitch.EightyColumnFirmware] = true; break;

                case SoftSwitchAddress.CLRALTCHAR: State[SoftSwitch.AltCharSet] = false; break;
                case SoftSwitchAddress.SETALTCHAR: State[SoftSwitch.AltCharSet] = true; break;

                case SoftSwitchAddress.IOUDISON: State[SoftSwitch.IOU] = true; break;
                case SoftSwitchAddress.IOUDISOFF: State[SoftSwitch.IOU] = false; break;
            }
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
