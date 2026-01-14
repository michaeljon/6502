using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class LanguageCard : IDevice, ISoftSwitchStateProvider
    {
        private const ushort LANG_A3 = 0b00001000;
        private const ushort LANG_A0A1 = 0b00000011;

        private int preWrite;

        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Language Card";

        public bool Handles(ushort address)
            => address == 0xC011 || address == 0xC012 || (address >= 0xC080 && address <= 0xC08F);

        public LanguageCard()
        {
            Reset();
        }

        public byte Read(ushort address)
        {
            if (address == 0xC011)
            {
                return HandleC011(address);
            }
            else if (address == 0xC012)
            {
                return HandleC012(address);
            }
            else if (address >= 0xC080 && address <= 0xC08F)
            {
                return HandleReadC08x(address);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            HandleWriteC08x(address, value);
        }

        public void Reset()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }

            // enable language card
            State[SoftSwitch.LcBank2] = true;
        }

        private byte HandleC011(ushort address)
        {
            return (byte)(State[SoftSwitch.LcBank2] ? 0x80 : 0x00);
        }

        private byte HandleC012(ushort address)
        {
            return (byte)(State[SoftSwitch.LanguageCardEnabled] ? 0x80 : 0x00);
        }

        private byte HandleReadC08x(ushort address)
        {
            // Bank select
            if ((address & LANG_A3) != 0)
            {
                // 1 = any access sets Bank_1
                State[SoftSwitch.LcBank2] = true;
            }
            else
            {
                // 0 = any access resets Bank_1
                State[SoftSwitch.LcBank2] = false;
            }

            // Read enable
            if (((address & LANG_A0A1) == 0) || ((address & LANG_A0A1) == 3))
            {
                // 00, 11 - set READ_ENABLE
                State[SoftSwitch.AuxRead] = true;
            }
            else
            {
                // 01, 10 - reset READ_ENABLE
                State[SoftSwitch.AuxRead] = false;
            }

            // PRE_WRITE
            int old_pre_write = preWrite;

            if ((address & 0b00000001) == 1)
            {
                // read 1 or 3, 00000001 - set PRE_WRITE
                preWrite = 1;
            }
            else
            {
                // read 0 or 2, 00000000 - reset PRE_WRITE
                preWrite = 0;
            }

            // Write Enable
            if ((old_pre_write == 1) && ((address & 0b00000001) == 1))
            {
                // PRE_WRITE set, read 1 or 3, 00000000 - reset WRITE_ENABLE'
                State[SoftSwitch.AuxWrite] = false;
            }

            if ((address & 0b00000001) == 0)
            {
                // read 0 or 2, set _WRITE_ENABLE, 00000001 - set WRITE_ENABLE'
                State[SoftSwitch.AuxWrite] = true;
            }

            SimDebugger.Info("FF_BANK_1: %d, FF_READ_ENABLE: %d, FF_PRE_WRITE: %d, _FF_WRITE_ENABLE: %d\n",
                State[SoftSwitch.LcBank2],
                State[SoftSwitch.AuxRead],
                preWrite,
                State[SoftSwitch.AuxWrite]);

            // handle the MMU configuration here
            return 0x00;
        }

        private void HandleWriteC08x(ushort address, byte value)
        {
            // Bank select
            if ((address & LANG_A3) != 0)
            {
                // 1 = any access sets Bank_1
                State[SoftSwitch.LcBank2] = true;
            }
            else
            {
                // 0 = any access resets Bank_1
                State[SoftSwitch.LcBank2] = false;
            }

            // Read enable
            if (((address & LANG_A0A1) == 0) || ((address & LANG_A0A1) == 3))
            {
                // 00, 11 - set READ_ENABLE
                State[SoftSwitch.AuxRead] = true;
            }
            else
            {
                // 01, 10 - reset READ_ENABLE
                State[SoftSwitch.AuxRead] = false;
            }

            // PRE_WRITE -- any write, reests PRE_WRITE
            preWrite = 0;

            // Write Enable
            if ((address & 0b00000001) == 0)
            {
                // write 0 or 2
                State[SoftSwitch.AuxWrite] = true;
            }

            SimDebugger.Info("FF_BANK_1: %d, FF_READ_ENABLE: %d, FF_PRE_WRITE: %d, _FF_WRITE_ENABLE: %d\n",
                State[SoftSwitch.LcBank2],
                State[SoftSwitch.AuxRead],
                preWrite,
                State[SoftSwitch.AuxWrite]);
        }
    }
}
