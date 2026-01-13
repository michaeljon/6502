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
            if (address >= 0xC080 && address <= 0xC08F)
            {
                // HandleLanguageCard(address);
                return 0x00;
            }

            if (address >= SoftSwitchAddress.TXTCLR && address <= SoftSwitchAddress.HIRES)
            {
                return HandleVideo(address);
            }

            SimDebugger.Info($"[SS] Read({address:X4})\n");

            switch (address)
            {
                // case SoftSwitchAddress.KBD -- keyboard, handled elsewhere

                case SoftSwitchAddress.RDLCBNK2: return (byte)(State[SoftSwitch.LcBank2] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDLCRAM: return (byte)(State[SoftSwitch.LanguageCardEnabled] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDRAMRD: return (byte)(State[SoftSwitch.AuxRead] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDRAMWRT: return (byte)(State[SoftSwitch.AuxWrite] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDCXROM: return (byte)(State[SoftSwitch.SlotRomEnabled] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDALTZP: return (byte)(State[SoftSwitch.ZpAux] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDC3ROM: return (byte)(State[SoftSwitch.Slot3RomEnabled] ? 0x80 : 0x00);
                case SoftSwitchAddress.RD80STORE: return (byte)(State[SoftSwitch.Store80] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDVBL: return (byte)(State[SoftSwitch.VerticalBlank] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDTEXT: return (byte)(State[SoftSwitch.TextMode] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDMIXED: return (byte)(State[SoftSwitch.MixedMode] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDPAGE2: return (byte)(State[SoftSwitch.Page2] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDHIRES: return (byte)(State[SoftSwitch.HiRes] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDALTCHR: return (byte)(State[SoftSwitch.AltCharSet] ? 0x80 : 0x00);
                case SoftSwitchAddress.RD80VID: return (byte)(State[SoftSwitch.EightyColumnFirmware] ? 0x80 : 0x00);

                // case SoftSwitchAddress.KBDSTRB -- keyboard, handled elsewhere

                case SoftSwitchAddress.TAPEOUT:
                    State[SoftSwitch.TapeOut] = !State[SoftSwitch.TapeOut];
                    return 0;

                case SoftSwitchAddress.SPKR:
                    State[SoftSwitch.Speaker] = !State[SoftSwitch.Speaker];
                    return 0;

                case SoftSwitchAddress.STROBE:
                    State[SoftSwitch.GameStrobe] = true;
                    return 0;

                // hanldle if IOU == true case
                case SoftSwitchAddress.CLRAN0: State[SoftSwitch.Annunciator0] = false; return 0;
                case SoftSwitchAddress.SETAN0: State[SoftSwitch.Annunciator0] = true; return 0;
                case SoftSwitchAddress.CLRAN1: State[SoftSwitch.Annunciator1] = false; return 0;
                case SoftSwitchAddress.SETAN1: State[SoftSwitch.Annunciator1] = true; return 0;
                case SoftSwitchAddress.CLRAN2: State[SoftSwitch.Annunciator2] = false; return 0;
                case SoftSwitchAddress.SETAN2: State[SoftSwitch.Annunciator2] = true; return 0;
                case SoftSwitchAddress.CLRAN3: State[SoftSwitch.Annunciator3] = false; return 0;
                case SoftSwitchAddress.SETAN3: State[SoftSwitch.Annunciator3] = true; return 0;

                case SoftSwitchAddress.TAPEIN: return (byte)(State[SoftSwitch.TapeIn] ? 0x80 : 0x00);

                case SoftSwitchAddress.OPENAPPLE: return (byte)(State[SoftSwitch.OpenApple] ? 0x80 : 0x00);
                case SoftSwitchAddress.SOLIDAPPLE: return (byte)(State[SoftSwitch.SolidApple] ? 0x80 : 0x00);
                case SoftSwitchAddress.SHIFT: return (byte)(State[SoftSwitch.ShiftKey] ? 0x80 : 0x00);

                case SoftSwitchAddress.PADDLE0: return (byte)(State[SoftSwitch.Paddle0] ? 0x80 : 0x00);
                case SoftSwitchAddress.PADDLE1: return (byte)(State[SoftSwitch.Paddle1] ? 0x80 : 0x00);
                case SoftSwitchAddress.PADDLE2: return (byte)(State[SoftSwitch.Paddle2] ? 0x80 : 0x00);
                case SoftSwitchAddress.PADDLE3: return (byte)(State[SoftSwitch.Paddle3] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDIOUDIS: return (byte)(State[SoftSwitch.IOU] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDDHIRES: return (byte)(State[SoftSwitch.DoubleHiRes] ? 0x80 : 0x00);
            }

            return 0;
        }

        public override void Write(ushort address, byte value)
        {
            SimDebugger.Info($"[SS] Write({address:X4}, {value:X2})\n");

            if (address >= SoftSwitchAddress.TXTCLR && address <= SoftSwitchAddress.HIRES)
            {
                HandleVideo(address, value);
            }

            switch (address)
            {
                case SoftSwitchAddress.CLR80STORE: State[SoftSwitch.Store80] = false; break;
                case SoftSwitchAddress.SET80STORE: State[SoftSwitch.Store80] = true; break;

                case SoftSwitchAddress.RDMAINRAM: State[SoftSwitch.AuxRead] = false; break;
                case SoftSwitchAddress.RDCARDRAM: State[SoftSwitch.AuxRead] = true; break;
                case SoftSwitchAddress.WRMAINRAM: State[SoftSwitch.AuxWrite] = false; break;
                case SoftSwitchAddress.WRCARDRAM: State[SoftSwitch.AuxWrite] = true; break;

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

                // case 0xC010 -- keyboard, handled elsewhere

                case SoftSwitchAddress.TAPEOUT:
                    State[SoftSwitch.TapeOut] = !State[SoftSwitch.TapeOut];
                    break;

                case SoftSwitchAddress.SPKR:
                    State[SoftSwitch.Speaker] = !State[SoftSwitch.Speaker];
                    break;

                // handle weirdness with IOU
                case SoftSwitchAddress.CLRAN0: State[SoftSwitch.Annunciator0] = false; break;
                case SoftSwitchAddress.SETAN0: State[SoftSwitch.Annunciator0] = true; break;
                case SoftSwitchAddress.CLRAN1: State[SoftSwitch.Annunciator1] = false; break;
                case SoftSwitchAddress.SETAN1: State[SoftSwitch.Annunciator1] = true; break;
                case SoftSwitchAddress.CLRAN2: State[SoftSwitch.Annunciator2] = false; break;
                case SoftSwitchAddress.SETAN2: State[SoftSwitch.Annunciator2] = true; break;
                case SoftSwitchAddress.CLRAN3: State[SoftSwitch.Annunciator3] = false; break;
                case SoftSwitchAddress.SETAN3: State[SoftSwitch.Annunciator3] = true; break;

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
