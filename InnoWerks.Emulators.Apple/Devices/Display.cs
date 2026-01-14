using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Display : IDevice
    {
        public Dictionary<SoftSwitch, bool> State { get; } = [];

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Video Display";

        private readonly List<ushort> handles =
        [
            // read
            SoftSwitchAddress.RD80VID,
            SoftSwitchAddress.RDVBL,

            // write
            SoftSwitchAddress.CLR80VID,
            SoftSwitchAddress.SET80VID,

            SoftSwitchAddress.CLRALTCHAR,
            SoftSwitchAddress.SETALTCHAR,

            // read / write
            SoftSwitchAddress.TXTCLR,
            SoftSwitchAddress.TXTSET,
            SoftSwitchAddress.MIXCLR,
            SoftSwitchAddress.MIXSET,
            SoftSwitchAddress.TXTPAGE1,

            SoftSwitchAddress.TXTPAGE2,
            SoftSwitchAddress.TXTPAGE1,
            SoftSwitchAddress.LORES,
            SoftSwitchAddress.HIRES,
        ];

        private readonly IBus bus;

        public bool Handles(ushort address)
            => handles.Contains(address);

        public Display(IBus bus)
        {
            this.bus = bus;
        }

        public byte Read(ushort address)
        {
            SimDebugger.Info($"HandleDisplay({address:X4})\n");

            switch (address)
            {
                case SoftSwitchAddress.RD80VID: return (byte)(State[SoftSwitch.EightyColumnFirmware] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDVBL: return (byte)(State[SoftSwitch.VerticalBlank] ? 0x80 : 0x00);

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

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"HandleDisplay({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.CLR80VID: State[SoftSwitch.EightyColumnFirmware] = false; break;
                case SoftSwitchAddress.SET80VID: State[SoftSwitch.EightyColumnFirmware] = true; break;

                case SoftSwitchAddress.CLRALTCHAR: State[SoftSwitch.AltCharSet] = false; break;
                case SoftSwitchAddress.SETALTCHAR: State[SoftSwitch.AltCharSet] = true; break;

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

        public void Reset()
        {
            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                State[sw] = false;
            }

            // basic setup
            State[SoftSwitch.TextMode] = true;
        }

        public void Render()
        {
            bool page2 = State[SoftSwitch.Page2];
            Span<char> line = stackalloc char[40];

            Console.SetCursorPosition(0, 0);

            for (int row = 0; row < 24; row++)
            {
                for (int col = 0; col < 40; col++)
                {
                    ushort addr = GetTextAddress(row, col, page2);
                    byte b = bus.Peek(addr);

                    line[col] = DecodeAppleChar(b);
                }

                Console.WriteLine(line);
            }
        }

        private static char DecodeAppleChar(byte b)
        {
            // Ignore inverse/flash for now
            b &= 0x7F;

            // Apple II uses ASCII-ish set
            if (b < 0x20)
                return ' ';

            return (char)b;
        }

        private static ushort GetTextAddress(int row, int col, bool page2)
        {
            int pageOffset = page2 ? 0x800 : 0x400;

            return (ushort)(
                pageOffset +
                textRowBase[row & 0x07] +
                (row >> 3) * 40 +
                col
            );
        }

        private static readonly int[] textRowBase =
        [
            0x000, 0x080, 0x100, 0x180,
            0x200, 0x280, 0x300, 0x380
        ];
    }
}
