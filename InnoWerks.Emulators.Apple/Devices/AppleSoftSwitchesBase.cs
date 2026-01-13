using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public abstract class AppleSoftSwitchesBase : IDevice, ISoftSwitchStateProvider
    {
        private int lcWriteArm;

        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public abstract string Name { get; }

        public bool Handles(ushort address)
            => address >= 0xC000 && address <= 0xC0FF;

        public abstract byte Read(ushort address);

        public abstract void Write(ushort address, byte value);

        public abstract void Reset();

        protected void HandleLanguageCard(ushort address)
        {
            SimDebugger.Info($"HandleLanguageCard({address:X4})\n");

            switch (address)
            {
                case 0xC080: // $C080  RAM bank 2, read, write protect
                case 0xC084: // $C084  RAM bank 2, read, write protect
                    State[SoftSwitch.LanguageCardEnabled] = true;
                    State[SoftSwitch.LcBank2] = true;
                    State[SoftSwitch.LcWriteEnabled] = false;
                    lcWriteArm = 0;
                    break;

                case 0xC081: // $C081  RAM bank 2, read, write enable (arm)
                case 0xC085: // $C085  RAM bank 2, read, write enable (arm)
                    State[SoftSwitch.LanguageCardEnabled] = true;
                    State[SoftSwitch.LcBank2] = true;
                    lcWriteArm++;
                    if (lcWriteArm >= 2)
                        State[SoftSwitch.LcWriteEnabled] = true;
                    break;

                case 0xC082: // $C082  ROM, write protect
                case 0xC086: // $C086  ROM, write protect
                case 0xC08A: // $C08A  ROM, write protect
                case 0xC08E: // $C08E  ROM, write protect
                    State[SoftSwitch.LanguageCardEnabled] = false;
                    State[SoftSwitch.LcWriteEnabled] = false;
                    lcWriteArm = 0;
                    break;

                case 0xC083: // $C083  RAM bank 2, read/write (arm)
                case 0xC087: // $C087  RAM bank 2, read/write (arm)
                    State[SoftSwitch.LanguageCardEnabled] = true;
                    State[SoftSwitch.LcBank2] = true;
                    lcWriteArm++;
                    if (lcWriteArm >= 2)
                        State[SoftSwitch.LcWriteEnabled] = true;
                    break;

                case 0xC088: // $C088  RAM bank 1, read, write protect
                case 0xC08C: // $C08C  RAM bank 1, read, write protect
                    State[SoftSwitch.LanguageCardEnabled] = true;
                    State[SoftSwitch.LcBank2] = false;
                    State[SoftSwitch.LcWriteEnabled] = false;
                    lcWriteArm = 0;
                    break;

                case 0xC089: // $C089  RAM bank 1, read, write enable (arm)
                case 0xC08D: // $C08D  RAM bank 1, read, write enable (arm)
                    State[SoftSwitch.LanguageCardEnabled] = true;
                    State[SoftSwitch.LcBank2] = false;
                    lcWriteArm++;
                    if (lcWriteArm >= 2)
                        State[SoftSwitch.LcWriteEnabled] = true;
                    break;

                case 0xC08B: // $C08B  RAM bank 1, read/write (arm)
                case 0xC08F: // $C08F  RAM bank 1, read/write (arm)
                    State[SoftSwitch.LanguageCardEnabled] = true;
                    State[SoftSwitch.LcBank2] = false;
                    lcWriteArm++;
                    if (lcWriteArm >= 2)
                        State[SoftSwitch.LcWriteEnabled] = true;
                    break;
            }
        }

        protected byte HandleVideo(ushort address)
        {
            SimDebugger.Info($"HandleVideo({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.TXTCLR: State[SoftSwitch.TextMode] = false; return 0;
                case SoftSwitchAddress.TXTSET: State[SoftSwitch.TextMode] = true; return 0;
                case SoftSwitchAddress.MIXCLR: State[SoftSwitch.MixedMode] = false; return 0;
                case SoftSwitchAddress.MIXSET: State[SoftSwitch.MixedMode] = true; return 0;
                case SoftSwitchAddress.TXTPAGE1: State[SoftSwitch.Page2] = false; return 0;

                // handle IIe case where 80STORE is set
                case SoftSwitchAddress.TXTPAGE2: State[SoftSwitch.Page2] = true; return 0;
                case SoftSwitchAddress.LORES: State[SoftSwitch.HiRes] = false; return 0;
                case SoftSwitchAddress.HIRES: State[SoftSwitch.HiRes] = true; return 0;
            }

            return 0x00;
        }

        protected void HandleVideo(ushort address, byte value)
        {
            SimDebugger.Info($"HandleVideo({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.TXTCLR: State[SoftSwitch.TextMode] = false; break;
                case SoftSwitchAddress.TXTSET: State[SoftSwitch.TextMode] = true; break;
                case SoftSwitchAddress.MIXCLR: State[SoftSwitch.MixedMode] = false; break;
                case SoftSwitchAddress.MIXSET: State[SoftSwitch.MixedMode] = true; break;
                case SoftSwitchAddress.TXTPAGE1: State[SoftSwitch.Page2] = false; break;

                // handle IIe case where 80STORE is set
                case SoftSwitchAddress.TXTPAGE2: State[SoftSwitch.Page2] = true; break;
                case SoftSwitchAddress.LORES: State[SoftSwitch.HiRes] = false; break;
                case SoftSwitchAddress.HIRES: State[SoftSwitch.HiRes] = true; break;
            }
        }

        protected void WarmBoot()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }

            // basic setup
            lcWriteArm = 0;
            State[SoftSwitch.TextMode] = true;

            // enable language card
            State[SoftSwitch.LcBank2] = true;

            // route $C8XX-CFFF to the slots
            State[SoftSwitch.IntC8RomEnabled] = true;
        }
    }
}
