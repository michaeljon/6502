using InnoWerks.Processors;

namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleIISoftSwitches : AppleSoftSwitchesBase
    {
        public override string Name => "Apple II / II+ Soft Switches";

        public AppleIISoftSwitches()
        {
            Reset();
        }

        public override byte Read(ushort address)
        {
            if (address >= 0xC080 && address <= 0xC08F)
            {
                HandleLanguageCard(address);
                return 0x00;
            }

            if (address >= SoftSwitchAddress.TXTCLR && address <= SoftSwitchAddress.HIRES)
            {
                return HandleVideo(address);
            }

            SimDebugger.Info($"[SS] Read({address:X4})\n");

            switch (address)
            {
                // case 0xC000 -- keyboard, handled elsewhere
                // case 0xC010 -- keyboard, handled elsewhere

                case SoftSwitchAddress.TAPEOUT:
                    State[SoftSwitch.TapeOut] = !State[SoftSwitch.TapeOut];
                    return 0;

                case SoftSwitchAddress.SPKR:
                    State[SoftSwitch.Speaker] = !State[SoftSwitch.Speaker];
                    return 0;

                case SoftSwitchAddress.STROBE:
                    State[SoftSwitch.GameStrobe] = true;
                    return 0;

                case SoftSwitchAddress.CLRAN0: State[SoftSwitch.Annunciator0] = false; return 0;
                case SoftSwitchAddress.SETAN0: State[SoftSwitch.Annunciator0] = true; return 0;
                case SoftSwitchAddress.CLRAN1: State[SoftSwitch.Annunciator1] = false; return 0;
                case SoftSwitchAddress.SETAN1: State[SoftSwitch.Annunciator1] = true; return 0;
                case SoftSwitchAddress.CLRAN2: State[SoftSwitch.Annunciator2] = false; return 0;
                case SoftSwitchAddress.SETAN2: State[SoftSwitch.Annunciator2] = true; return 0;
                case SoftSwitchAddress.CLRAN3: State[SoftSwitch.Annunciator3] = false; return 0;
                case SoftSwitchAddress.SETAN3: State[SoftSwitch.Annunciator3] = true; return 0;

                case SoftSwitchAddress.TAPEIN: return (byte)(State[SoftSwitch.TapeIn] ? 0x80 : 0x00);

                case SoftSwitchAddress.BUTTON0: return (byte)(State[SoftSwitch.Button0] ? 0x80 : 0x00);
                case SoftSwitchAddress.BUTTON1: return (byte)(State[SoftSwitch.Button1] ? 0x80 : 0x00);
                case SoftSwitchAddress.BUTTON2: return (byte)(State[SoftSwitch.Button2] ? 0x80 : 0x00);

                case SoftSwitchAddress.PADDLE0: return (byte)(State[SoftSwitch.Paddle0] ? 0x80 : 0x00);
                case SoftSwitchAddress.PADDLE1: return (byte)(State[SoftSwitch.Paddle1] ? 0x80 : 0x00);
                case SoftSwitchAddress.PADDLE2: return (byte)(State[SoftSwitch.Paddle2] ? 0x80 : 0x00);
                case SoftSwitchAddress.PADDLE3: return (byte)(State[SoftSwitch.Paddle3] ? 0x80 : 0x00);
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
                case SoftSwitchAddress.TAPEOUT:
                    State[SoftSwitch.TapeOut] = !State[SoftSwitch.TapeOut];
                    break;

                case SoftSwitchAddress.SPKR:
                    State[SoftSwitch.Speaker] = !State[SoftSwitch.Speaker];
                    break;

                case SoftSwitchAddress.CLRAN0: State[SoftSwitch.Annunciator0] = false; break;
                case SoftSwitchAddress.SETAN0: State[SoftSwitch.Annunciator0] = true; break;
                case SoftSwitchAddress.CLRAN1: State[SoftSwitch.Annunciator1] = false; break;
                case SoftSwitchAddress.SETAN1: State[SoftSwitch.Annunciator1] = true; break;
                case SoftSwitchAddress.CLRAN2: State[SoftSwitch.Annunciator2] = false; break;
                case SoftSwitchAddress.SETAN2: State[SoftSwitch.Annunciator2] = true; break;
                case SoftSwitchAddress.CLRAN3: State[SoftSwitch.Annunciator3] = false; break;
                case SoftSwitchAddress.SETAN3: State[SoftSwitch.Annunciator3] = true; break;
            }
        }

        public override void Reset()
        {
            WarmBoot();
        }
    }
}
