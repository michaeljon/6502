using InnoWerks.Processors;

namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleIIeSoftSwitches : AppleSoftSwitchesBase
    {
        public override string Name => "Apple IIe Soft Switches";

        public AppleIIeSoftSwitches()
        {
            Reset();
        }

        public override byte Read(ushort address)
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

        public override void Write(ushort address, byte value)
        {
            SimDebugger.Info($"[SS] Write({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.CLR80STORE: State[SoftSwitch.Store80] = false; break;
                case SoftSwitchAddress.SET80STORE: State[SoftSwitch.Store80] = true; break;

                case SoftSwitchAddress.SETSLOTCXROM: State[SoftSwitch.SlotRomEnabled] = true; break;
                case SoftSwitchAddress.SETINTCXROM: State[SoftSwitch.SlotRomEnabled] = false; break;

                case SoftSwitchAddress.SETSTDZP: State[SoftSwitch.ZpAux] = false; break;
                case SoftSwitchAddress.SETALTZP: State[SoftSwitch.ZpAux] = true; break;

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

        public override void Reset()
        {
            WarmBoot();
        }
    }
}
