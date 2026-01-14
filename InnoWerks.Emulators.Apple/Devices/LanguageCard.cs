using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class LanguageCard : IDevice
    {
        private const ushort LANG_A3 = 0b00001000;
        private const ushort LANG_A0A1 = 0b00000011;

        private int preWrite;

        private bool lcBank1;

        private bool lcReadEnabled;

        private bool lcWriteEnabled;


        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "II/II+ Language Card";

        public bool Handles(ushort address)
            => address == 0xC011 || address == 0xC012 || (address >= 0xC080 && address <= 0xC08F);

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleLanguageCard({address:X4})\n");

            if (address == 0xC011)
            {
                return HandleC011();
            }
            else if (address == 0xC012)
            {
                return HandleC012();
            }
            else if (address >= 0xC080 && address <= 0xC08F)
            {
                return HandleReadC08x(address);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleLanguageCard({address:X4}, {value:X2})\n");

            HandleWriteC08x(address, value);
        }

        public void Reset()
        {
            // enable language card
            lcBank1 = false;
        }

        private byte HandleC011()
        {
            return (byte)(lcBank1 == false ? 0x80 : 0x00);
        }

        private byte HandleC012()
        {
            return (byte)(lcReadEnabled ? 0x80 : 0x00);
        }

        private byte HandleReadC08x(ushort address)
        {
            // Bank select
            if ((address & LANG_A3) != 0)
            {
                // 1 = any access sets Bank_1
                lcBank1 = true;
            }
            else
            {
                // 0 = any access resets Bank_1
                lcBank1 = false;
            }

            // Read enable
            if (((address & LANG_A0A1) == 0) || ((address & LANG_A0A1) == 3))
            {
                // 00, 11 - set READ_ENABLE
                lcReadEnabled = true;
            }
            else
            {
                // 01, 10 - reset READ_ENABLE
                lcReadEnabled = false;
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
                lcWriteEnabled = false;
            }

            if ((address & 0b00000001) == 0)
            {
                // read 0 or 2, set _WRITE_ENABLE, 00000001 - set WRITE_ENABLE'
                lcWriteEnabled = true;
            }

            SimDebugger.Info("lcBank1: {0}, lcReadEnabled: {1}, preWrite: {2}, lcWriteEnabled: {3}\n",
                lcBank1, lcReadEnabled, preWrite, lcWriteEnabled);

            // handle the MMU configuration here
            return 0x00;
        }

        private void HandleWriteC08x(ushort address, byte value)
        {
            // Bank select
            if ((address & LANG_A3) != 0)
            {
                // 1 = any access sets Bank_1
                lcBank1 = true;
            }
            else
            {
                // 0 = any access resets Bank_1
                lcBank1 = false;
            }

            // Read enable
            if (((address & LANG_A0A1) == 0) || ((address & LANG_A0A1) == 3))
            {
                // 00, 11 - set READ_ENABLE
                lcReadEnabled = true;
            }
            else
            {
                // 01, 10 - reset READ_ENABLE
                lcReadEnabled = false;
            }

            // PRE_WRITE -- any write, reests PRE_WRITE
            preWrite = 0;

            // Write Enable
            if ((address & 0b00000001) == 0)
            {
                // write 0 or 2
                lcWriteEnabled = true;
            }

            SimDebugger.Info("lcBank1: {0}, lcReadEnabled: {1}, preWrite: {2}, lcWriteEnabled: {3}\n",
                lcBank1, lcReadEnabled, preWrite, lcWriteEnabled);
        }
    }
}
