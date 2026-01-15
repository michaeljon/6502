using System;
using System.Collections.Generic;
using System.Linq;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class Display : IDevice
    {
        private const int CyclesPerLine = 65;
        private const int LinesPerFrame = 262;
        private const int VBlankStart = 192;

        private readonly SoftSwitches softSwitches;

        public DevicePriority Priority => DevicePriority.SoftSwitch;

        public int Slot => -1;

        public string Name => "Video Display";

        private readonly List<ushort> handles =
        [
            SoftSwitchAddress.CLRALTCHAR,
            SoftSwitchAddress.SETALTCHAR,
            SoftSwitchAddress.RDALTCHR,

            SoftSwitchAddress.CLR80VID,
            SoftSwitchAddress.SET80VID,
            SoftSwitchAddress.RD80VID,

            SoftSwitchAddress.CLR80STORE,
            SoftSwitchAddress.SET80STORE,
            SoftSwitchAddress.RD80STORE,

            SoftSwitchAddress.TXTPAGE1,
            SoftSwitchAddress.TXTPAGE2,
            SoftSwitchAddress.RDPAGE2,

            SoftSwitchAddress.TXTCLR,
            SoftSwitchAddress.TXTSET,
            SoftSwitchAddress.RDTEXT,

            SoftSwitchAddress.MIXCLR,
            SoftSwitchAddress.MIXSET,
            SoftSwitchAddress.RDMIXED,

            SoftSwitchAddress.LORES,
            SoftSwitchAddress.HIRES,
            SoftSwitchAddress.RDHIRES,

            SoftSwitchAddress.IOUDISON,
            SoftSwitchAddress.IOUDISOFF,
            SoftSwitchAddress.RDIOUDIS,

            SoftSwitchAddress.DHIRESON,
            SoftSwitchAddress.DHIRESOFF,
            SoftSwitchAddress.RDDHIRES,

            SoftSwitchAddress.RDVBL,
        ];

        private readonly IBus bus;

        private ulong hCycle;
        private int vLine;
        private bool phase;

        public bool Handles(ushort address)
            => handles.Contains(address);

        public Display(IBus bus, SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(bus);
            ArgumentNullException.ThrowIfNull(softSwitches);

            this.bus = bus;
            this.softSwitches = softSwitches;
        }

        public byte Read(ushort address)
        {
            SimDebugger.Info($"Read Display({address:X4})\n");

            switch (address)
            {
                // this looks like a keyboard here, because it is
                case SoftSwitchAddress.KBD:
                    return softSwitches.State[SoftSwitch.KeyboardStrobe]
                        ? (byte)(softSwitches.KeyLatch | 0x80)
                        : softSwitches.KeyLatch;

                case SoftSwitchAddress.RDALTCHR:
                    if (softSwitches.State[SoftSwitch.VerticalBlank])
                    {
                        return 0x00;
                    }

                    bool altCharSet = softSwitches.State[SoftSwitch.AltCharSet];

                    if (softSwitches.State[SoftSwitch.Store80] && softSwitches.State[SoftSwitch.TextMode])
                    {
                        altCharSet = false;
                    }

                    // again: live sampling
                    return (byte)((altCharSet ^ phase) ? 0x80 : 0x00);

                case SoftSwitchAddress.RD80VID:
                    if (softSwitches.State[SoftSwitch.VerticalBlank])
                    {
                        return 0x00;
                    }

                    bool video80 = softSwitches.State[SoftSwitch.EightyColumnMode]
                                && (!softSwitches.State[SoftSwitch.TextMode] || !softSwitches.State[SoftSwitch.MixedMode]);

                    // real hardware: this is sampled from the video generator
                    return (byte)((video80 ^ phase) ? 0x80 : 0x00);

                case SoftSwitchAddress.RD80STORE: return (byte)(softSwitches.State[SoftSwitch.Store80] ? 0x80 : 0x00);

                case SoftSwitchAddress.TXTPAGE1: softSwitches.State[SoftSwitch.Page2] = false; return 0;
                case SoftSwitchAddress.TXTPAGE2: softSwitches.State[SoftSwitch.Page2] = true; return 0;
                case SoftSwitchAddress.RDPAGE2: return (byte)(softSwitches.State[SoftSwitch.Page2] ? 0x80 : 0x00);

                case SoftSwitchAddress.TXTCLR: softSwitches.State[SoftSwitch.TextMode] = false; return 0;
                case SoftSwitchAddress.TXTSET: softSwitches.State[SoftSwitch.TextMode] = true; return 0;
                case SoftSwitchAddress.RDTEXT: return (byte)(softSwitches.State[SoftSwitch.TextMode] ? 0x80 : 0x00);

                case SoftSwitchAddress.MIXCLR: softSwitches.State[SoftSwitch.MixedMode] = false; return 0;
                case SoftSwitchAddress.MIXSET: softSwitches.State[SoftSwitch.MixedMode] = true; return 0;
                case SoftSwitchAddress.RDMIXED: return (byte)(softSwitches.State[SoftSwitch.MixedMode] ? 0x80 : 0x00);

                case SoftSwitchAddress.LORES: softSwitches.State[SoftSwitch.HiRes] = false; return 0;
                case SoftSwitchAddress.HIRES: softSwitches.State[SoftSwitch.HiRes] = true; return 0;
                case SoftSwitchAddress.RDHIRES: return (byte)(softSwitches.State[SoftSwitch.HiRes] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDIOUDIS: return (byte)(softSwitches.State[SoftSwitch.IOUDisabled] ? 0x80 : 0x00);

                case SoftSwitchAddress.DHIRESON:
                    if (softSwitches.State[SoftSwitch.IOUDisabled] == true)
                    {
                        softSwitches.State[SoftSwitch.DoubleHiRes] = true;
                    }
                    return 0;
                case SoftSwitchAddress.DHIRESOFF:
                    if (softSwitches.State[SoftSwitch.IOUDisabled] == true)
                    {
                        softSwitches.State[SoftSwitch.DoubleHiRes] = false;
                    }
                    return 0;
                case SoftSwitchAddress.RDDHIRES: return (byte)(softSwitches.State[SoftSwitch.DoubleHiRes] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDVBL: return (byte)(softSwitches.State[SoftSwitch.VerticalBlank] ? 0x80 : 0x00);
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            SimDebugger.Info($"Write Display({address:X4}, {value:X2})\n");

            switch (address)
            {
                case SoftSwitchAddress.CLRALTCHAR: softSwitches.State[SoftSwitch.AltCharSet] = false; break;
                case SoftSwitchAddress.SETALTCHAR: softSwitches.State[SoftSwitch.AltCharSet] = true; break;

                case SoftSwitchAddress.CLR80VID: softSwitches.State[SoftSwitch.EightyColumnMode] = false; break;
                case SoftSwitchAddress.SET80VID: softSwitches.State[SoftSwitch.EightyColumnMode] = true; break;

                case SoftSwitchAddress.CLR80STORE: softSwitches.State[SoftSwitch.Store80] = false; break;
                case SoftSwitchAddress.SET80STORE: softSwitches.State[SoftSwitch.Store80] = true; break;

                case SoftSwitchAddress.TXTPAGE1: softSwitches.State[SoftSwitch.Page2] = false; break;
                case SoftSwitchAddress.TXTPAGE2: softSwitches.State[SoftSwitch.Page2] = true; break;

                case SoftSwitchAddress.TXTCLR: softSwitches.State[SoftSwitch.TextMode] = false; break;
                case SoftSwitchAddress.TXTSET: softSwitches.State[SoftSwitch.TextMode] = true; break;

                case SoftSwitchAddress.MIXCLR: softSwitches.State[SoftSwitch.MixedMode] = false; break;
                case SoftSwitchAddress.MIXSET: softSwitches.State[SoftSwitch.MixedMode] = true; break;

                case SoftSwitchAddress.LORES: softSwitches.State[SoftSwitch.HiRes] = false; break;
                case SoftSwitchAddress.HIRES: softSwitches.State[SoftSwitch.HiRes] = true; break;

                case SoftSwitchAddress.IOUDISON: softSwitches.State[SoftSwitch.IOUDisabled] = true; return;
                case SoftSwitchAddress.IOUDISOFF: softSwitches.State[SoftSwitch.IOUDisabled] = false; return;

                case SoftSwitchAddress.DHIRESON:
                    if (softSwitches.State[SoftSwitch.IOUDisabled] == true)
                    {
                        softSwitches.State[SoftSwitch.DoubleHiRes] = true;
                    }
                    return;
                case SoftSwitchAddress.DHIRESOFF:
                    if (softSwitches.State[SoftSwitch.IOUDisabled] == true)
                    {
                        softSwitches.State[SoftSwitch.DoubleHiRes] = false;
                    }
                    return;
            }
        }

        public void Tick(int cycles)
        {
            for (var i = 0; i < cycles; i++)
            {
                hCycle++;

                phase = !phase;

                if (hCycle == CyclesPerLine)
                {
                    hCycle = 0;
                    vLine++;

                    if (vLine == VBlankStart)
                    {
                        softSwitches.State[SoftSwitch.VerticalBlank] = true;
                    }

                    if (vLine == LinesPerFrame)
                    {
                        vLine = 0;
                        softSwitches.State[SoftSwitch.VerticalBlank] = false;
                    }
                }
            }
        }

        public void Reset()
        {
            // basic setup
            softSwitches.State[SoftSwitch.TextMode] = true;
            softSwitches.State[SoftSwitch.IOUDisabled] = true;
        }

        public void Render()
        {
            bool page2 = softSwitches.State[SoftSwitch.Page2];
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
