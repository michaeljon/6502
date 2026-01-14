using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class MemoryIIe : IDevice, ISoftSwitchStateProvider
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Memory IIe";

        private readonly List<ushort> handles =
        [
            // read
            SoftSwitchAddress.RDLCBNK2,
            SoftSwitchAddress.RDLCRAM,

            SoftSwitchAddress.RDRAMRD,
            SoftSwitchAddress.RDRAMWRT,

            SoftSwitchAddress.RDCXROM,
            SoftSwitchAddress.RDALTZP,
            SoftSwitchAddress.RDC3ROM,

            SoftSwitchAddress.RD80STORE,
            SoftSwitchAddress.RDTEXT,
            SoftSwitchAddress.RDMIXED,
            SoftSwitchAddress.RDPAGE2,
            SoftSwitchAddress.RDLCBNK2,

            // write
            SoftSwitchAddress.CLR80STORE,
            SoftSwitchAddress.SET80STORE,

            SoftSwitchAddress.RDMAINRAM,
            SoftSwitchAddress.RDCARDRAM,

            SoftSwitchAddress.WRMAINRAM,
            SoftSwitchAddress.WRCARDRAM,

            SoftSwitchAddress.SETSTDZP,
            SoftSwitchAddress.SETALTZP,
        ];

        public bool Handles(ushort address)
            => handles.Contains(address);

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleMemory({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.RDLCBNK2: return (byte)(State[SoftSwitch.LcBank2] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDLCRAM: return (byte)(State[SoftSwitch.LcWriteEnabled] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDRAMRD: return (byte)(State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDRAMWRT: return (byte)(State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDCXROM: return (byte)(State[SoftSwitch.SlotRomEnabled] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDALTZP: return (byte)(State[SoftSwitch.ZpAux] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDC3ROM: return (byte)(State[SoftSwitch.Slot3RomEnabled] ? 0x80 : 0x00);

                case SoftSwitchAddress.RD80STORE: return (byte)(State[SoftSwitch.Store80] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDTEXT: return (byte)(State[SoftSwitch.TextMode] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDMIXED: return (byte)(State[SoftSwitch.MixedMode] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDPAGE2: return (byte)(State[SoftSwitch.Page2] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDHIRES: return (byte)(State[SoftSwitch.HiRes] ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleMemory({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.CLR80STORE: State[SoftSwitch.Store80] = false; break;
                case SoftSwitchAddress.SET80STORE: State[SoftSwitch.Store80] = true; break;

                case SoftSwitchAddress.RDMAINRAM: State[SoftSwitch.AuxRead] = false; break;
                case SoftSwitchAddress.RDCARDRAM: State[SoftSwitch.AuxRead] = true; break;

                case SoftSwitchAddress.WRMAINRAM: State[SoftSwitch.AuxWrite] = false; break;
                case SoftSwitchAddress.WRCARDRAM: State[SoftSwitch.AuxWrite] = true; break;

                case SoftSwitchAddress.SETSTDZP: State[SoftSwitch.ZpAux] = false; break;
                case SoftSwitchAddress.SETALTZP: State[SoftSwitch.ZpAux] = true; break;
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
