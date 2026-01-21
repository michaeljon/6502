using System;
using System.Collections.Generic;
using System.Diagnostics;

#pragma warning disable CA1822

namespace InnoWerks.Emulators.Apple
{
    public class MemoryPage
    {
        public const int PageSize = 256;

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Block { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays

        public string Type { get; init; }

        public int Index { get; init; }

        public MemoryPage(string type, int index)
        {
            Block = new byte[PageSize];
            Type = type;
            Index = index;
        }

        public override string ToString()
        {
            return $"Block: {Type} at {Index:X2}";
        }
    }

    public class MemoryBlocks
    {
        private const int NumberOfPages = 64 * 1024 / MemoryPage.PageSize;

        private readonly List<MemoryPage> mainMemory = [];
        private readonly List<MemoryPage> auxMemory = [];

        private readonly List<(MemoryPage ReadFrom, MemoryPage WriteTo)> activeMemory = [];

        private readonly SoftSwitches softSwitches;

        private byte GetPage(ushort address) => (byte)((address & 0xFF00) >> 8);

        private byte GetOffset(ushort address) => (byte)(address & 0x00FF);

        private readonly List<MemoryPage>[] lcRam;         // IIe only

        // swappable lo rom banks
        private readonly List<MemoryPage> loRom = [];           // $D000–$DFFF

        // switch-selectable
        private readonly List<MemoryPage> cxRom = [];           // $C000-$CFFF

        // single hi rom bank
        private readonly List<MemoryPage> hiRom = [];           // $E000–$FFFF

        public MemoryBlocks(SoftSwitches softSwitches)
        {
            this.softSwitches = softSwitches;

            for (var p = 0; p < NumberOfPages; p++)
            {
                mainMemory.Add(new MemoryPage("main", p));
                auxMemory.Add(new MemoryPage("aux", p));

                activeMemory.Add((mainMemory[p], auxMemory[p]));
            }

            // language card ram (should be private to MemoryIIe, really)
            lcRam = new List<MemoryPage>[2];
            lcRam[0] = new List<MemoryPage>();
            lcRam[1] = new List<MemoryPage>();

            for (var p = 0; p < NumberOfPages; p++)
            {
                lcRam[0].Add(new MemoryPage("lcRam[0]", p));
                lcRam[1].Add(new MemoryPage("lcRam[1]", p));
            }


            // 4k ROM bank 1
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                loRom.Add(new MemoryPage("loRom", p));
            }

            // 4k switch selectable
            for (var p = 0; p < 4 * 1024 / MemoryPage.PageSize; p++)
            {
                cxRom.Add(new MemoryPage("cxRom", p));
            }

            // 8k ROM
            for (var p = 0; p < 8 * 1024 / MemoryPage.PageSize; p++)
            {
                hiRom.Add(new MemoryPage("hiRom", p));
            }
        }

        public void Remap()
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            // reset the aux/main selector
            for (var loop = 0x00; loop < 0xC0; loop++)
            {
                activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
            }

            activeMemory[0xC0] = (null, null);

            for (var loop = 0xC1; loop < 0xD0; loop++)
            {
                activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
            }

            // zero page and stack      $00 - $01
            for (var loop = 0x00; loop < 0x02; loop++)
            {
                // memshadow[loop] = SW_ALTZP ? memaux + (loop << 8) : memmain + (loop << 8);

                if (softSwitches.State[SoftSwitch.ZpAux] == false)
                {
                    activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
                }
                else
                {
                    activeMemory[loop] = (auxMemory[loop], auxMemory[loop]);
                }
            }


            // primary working memory   $02 - $C0
            for (var loop = 0x02; loop < 0xC0; loop++)
            {
                // memshadow[loop] = SW_AUXREAD ? memaux+(loop << 8)
                // 	: memmain+(loop << 8);

                // memwrite[loop]  = ((SW_AUXREAD != 0) == (SW_AUXWRITE != 0))
                // 	? mem+(loop << 8)
                // 	: SW_AUXWRITE	? memaux+(loop << 8)
                // 					: memmain+(loop << 8);

                switch (softSwitches.AuxReadAuxWriteBitmask)
                {
                    case 0x00:
                        activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
                        break;

                    case 0x01:
                        activeMemory[loop] = (mainMemory[loop], auxMemory[loop]);
                        break;

                    case 0x10:
                        activeMemory[loop] = (auxMemory[loop], mainMemory[loop]);
                        break;

                    case 0x11:
                        activeMemory[loop] = (auxMemory[loop], auxMemory[loop]);
                        break;
                }
            }

            if (softSwitches.State[SoftSwitch.Store80] == true)
            {
                for (var loop = 0x04; loop < 0x08; loop++)
                {
                    // memshadow[loop] = SW_PAGE2	? memaux+(loop << 8)
                    // 							: memmain+(loop << 8);
                    // memwrite[loop]  = mem+(loop << 8);

                    if (softSwitches.State[SoftSwitch.Page2] == true)
                    {
                        activeMemory[loop] = (auxMemory[loop], mainMemory[loop]);
                    }
                    else
                    {
                        activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
                    }
                }

                if (softSwitches.State[SoftSwitch.HiRes] == true)
                {
                    for (var loop = 0x20; loop < 0x40; loop++)
                    {
                        // memshadow[loop] = SW_PAGE2	? memaux+(loop << 8)
                        // 							: memmain+(loop << 8);
                        // memwrite[loop]  = mem+(loop << 8);

                        if (softSwitches.State[SoftSwitch.Page2] == true)
                        {
                            activeMemory[loop] = (auxMemory[loop], mainMemory[loop]);
                        }
                        else
                        {
                            activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
                        }
                    }
                }
            }

            // INT ROM                  $C0 - $CF   cxRom
            for (var loop = 0xC1; loop < 0xC8; loop++)
            {
                if (softSwitches.LcActive == true)
                {
                    var r = softSwitches.State[SoftSwitch.AuxRead] == true ? auxMemory[loop - 0xC1] : cxRom[loop - 0xC0];
                    var w = softSwitches.State[SoftSwitch.AuxWrite] == true ? auxMemory[loop - 0xC1] : null;

                    activeMemory[loop] = (r, w);
                    continue;
                }

                // const UINT uSlotOffset = (loop & 0x0f) * 0x100;
                // if (loop == 0xC3)
                // 	memshadow[loop] = (SW_SLOTC3ROM && !SW_INTCXROM)	? pCxRomPeripheral+uSlotOffset	// C300..C3FF - Slot 3 ROM (all 0x00's)
                // 														: pCxRomInternal+uSlotOffset;	// C300..C3FF - Internal ROM
                // else
                // 	memshadow[loop] = !SW_INTCXROM	? pCxRomPeripheral+uSlotOffset						// C000..C7FF - SSC/Disk][/etc
                // 									: pCxRomInternal+uSlotOffset;						// C000..C7FF - Internal ROM

                if (loop == 0xC3)
                {
                    if (softSwitches.State[SoftSwitch.Slot3RomEnabled] == true && softSwitches.State[SoftSwitch.SlotRomEnabled] == true)
                    {
                        // use slot rom from device
                        activeMemory[loop] = (null, null);
                    }
                    else
                    {
                        // use internal page c3
                        activeMemory[loop] = (cxRom[loop - 0xC0], null);
                    }
                }
                else
                {
                    if (softSwitches.State[SoftSwitch.SlotRomEnabled] == false)
                    {
                        // use internal page c3
                        activeMemory[loop] = (cxRom[loop - 0xC0], null);
                    }
                    else
                    {
                        // use slot rom from device, else use internal
                        activeMemory[loop] = (null, null);
                    }
                }
            }

            for (var loop = 0xC8; loop < 0xD0; loop++)
            {
                // memdirty[loop] = 0;	// mem(cache) can't be dirty for ROM (but STA $Cnnn will set the dirty flag)
                // const UINT uRomOffset = (loop & 0x0f) * 0x100;
                // memshadow[loop] = (!SW_INTCXROM && !INTC8ROM)	? pCxRomPeripheral+uRomOffset			// C800..CFFF - Peripheral ROM (GH#486)
                // 												: pCxRomInternal+uRomOffset;			// C800..CFFF - Internal ROM

                // if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && softSwitches.State[SoftSwitch.IntC8RomEnabled] == false)
                // {
                //     // use slot rom from device
                //     activeMemory[loop] = (cxRom[loop - 0xC8], null);
                // }
                // else
                // {
                //     // use internal page c8
                //     activeMemory[loop] = (mainMemory[loop - 0xC8], mainMemory[loop - 0xC8]);
                // }

                if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true)
                {
                    // we can only read from one of two places: the internal ROM or the device ROM
                    if (softSwitches.State[SoftSwitch.IntC8RomEnabled] == true)
                    {
                        // this is the internal ROM, and it's read-only
                        activeMemory[loop] = (cxRom[loop - 0xC8], null);
                    }
                    else
                    {
                        // this indicates to the bus that it's slot ROM and it's read-only
                        activeMemory[loop] = (null, null);
                    }
                }
                else
                {
                    // in this case we're not using the C8 as ROM
                    switch (softSwitches.AuxReadAuxWriteBitmask)
                    {
                        case 0x00:
                            activeMemory[loop] = (mainMemory[loop - 0xC8], mainMemory[loop - 0xC8]);
                            break;

                        case 0x01:
                            activeMemory[loop] = (mainMemory[loop - 0xC8], auxMemory[loop - 0xC8]);
                            break;

                        case 0x10:
                            activeMemory[loop] = (auxMemory[loop - 0xC8], mainMemory[loop - 0xC8]);
                            break;

                        case 0x11:
                            activeMemory[loop] = (auxMemory[loop - 0xC8], auxMemory[loop - 0xC8]);
                            break;
                    }
                }
            }

            for (var loop = 0xD0; loop < 0xE0; loop++)
            {
                // const int bankoffset = (SW_BANK2 ? 0 : 0x1000);
                // memshadow[loop] = SW_HIGHRAM ? SW_ALTZP	? memaux+(loop << 8)-bankoffset
                // 										: g_pMemMainLanguageCard+((loop-0xC0)<<8)-bankoffset
                // 							 : memrom+((loop-0xD0) * 0x100)+romoffset;

                // memwrite[loop]  = SW_WRITERAM	? SW_HIGHRAM	? mem+(loop << 8)
                // 												: SW_ALTZP	? memaux+(loop << 8)-bankoffset
                // 															: g_pMemMainLanguageCard+((loop-0xC0)<<8)-bankoffset
                // 								: NULL;

                var bank = (ushort)(softSwitches.State[SoftSwitch.LcBank1] ? 1 : 0);

                MemoryPage r = loRom[loop - 0xD0];
                MemoryPage w = null;

                if (softSwitches.LcActive)
                {
                    r = softSwitches.State[SoftSwitch.ZpAux] ?
                        auxMemory[loop] :
                        lcRam[bank][loop - 0xD0];
                }

                if (softSwitches.State[SoftSwitch.AuxWrite])
                {
                    w = softSwitches.LcActive ?
                        mainMemory[loop] :
                        lcRam[bank][loop - 0xD0];
                }

                activeMemory[loop] = (r, w);
            }

            for (var loop = 0xE0; loop < 0x100; loop++)
            {
                // memshadow[loop] = SW_HIGHRAM	? SW_ALTZP	? memaux+(loop << 8)
                // 											: g_pMemMainLanguageCard+((loop-0xC0)<<8)
                // 								: memrom+((loop-0xD0) * 0x100)+romoffset;

                // memwrite[loop]  = SW_WRITERAM	? SW_HIGHRAM	? mem+(loop << 8)
                // 												: SW_ALTZP	? memaux+(loop << 8)
                // 															: g_pMemMainLanguageCard+((loop-0xC0)<<8)
                // 								: NULL;

                MemoryPage r = null;
                MemoryPage w = null;

                if (softSwitches.LcActive)
                {
                    if (softSwitches.State[SoftSwitch.ZpAux])
                    {
                        r = auxMemory[loop];
                    }
                    else
                    {
                        r = lcRam[0][loop - 0xE0];
                    }
                }
                else
                {
                    r = hiRom[loop - 0xE0];
                }

                if (softSwitches.State[SoftSwitch.AuxWrite])
                {
                    if (softSwitches.LcActive)
                    {
                        w = mainMemory[loop];
                    }
                    else
                    {
                        w = lcRam[0][loop - 0xE0];
                    }
                }

                activeMemory[loop] = (r, w);
            }

            // BSR / ROM                $E0 - $FF   mainMemory / auxMemory / hiRom
            // Bank 2                   $D0 - $DF   lcRam
            // Bank 1                   $D0 - $DF   lcRam / loRom
            // INT ROM                  $C0 - $CF   cxRom
            // Hi RAM                   $60 - $BF   mainMemory / auxMemory
            // Hi-res Page 2            $40 - $5F
            // Hi-res Page 1            $20 - $3F
            // RAM                      $0C - $1F
            // Text Page 2              $08 - $0B
            // Text Page 1              $04 - $07
            // BASIC workspace          $02 - $03
            // zero page and stack      $00 - $01
        }

        public byte Read(ushort address)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            if (activeMemory[page].ReadFrom != null)
            {
                return activeMemory[page].ReadFrom.Block[offset];
            }

            return 0xFF;
        }

        public void Write(ushort address, byte value)
        {
            var page = GetPage(address);
            var offset = GetOffset(address);

            if (activeMemory[page].WriteTo != null)
            {
                activeMemory[page].WriteTo.Block[offset] = value;
            }
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            if (objectCode.Length != 32 * 1024)
            {
                throw new NotImplementedException("IIe ROM must be 32k");
            }

            for (var page = 0; page < 4 * 1024 / MemoryPage.PageSize; page++)
            {
                // load the first 4k from the 16k block at the end into cx rom
                Array.Copy(objectCode, (16 * 1024) + (page * 0x100), cxRom[page].Block, 0, 0x100);
            }

            for (var page = 0; page < 4 * 1024 / MemoryPage.PageSize; page++)
            {
                // load the first 4k from the 16k block at the end into lo rom
                Array.Copy(objectCode, (20 * 1024) + (page * 0x100), loRom[page].Block, 0, 0x100);
            }

            for (var page = 0; page < 8 * 1024 / MemoryPage.PageSize; page++)
            {
                // load the remaining 8k from the 16k block into hi rom
                Array.Copy(objectCode, (24 * 1024) + (page * 0x100), hiRom[page].Block, 0, 0x100);
            }
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            // copy the pages, then copy the remainder
            // Array.Copy(objectCode, 0, mainRam, origin, objectCode.Length);
        }

        public MemoryPage ResolveRead(ushort address)
        {
            var page = GetPage(address);

            return activeMemory[page].ReadFrom;
        }

        public MemoryPage ResolveWrite(ushort address)
        {
            var page = GetPage(address);

            return activeMemory[page].WriteTo;
        }

        internal void DumpPage(MemoryPage memoryPage)
        {
            Debug.WriteLine("MemoryPage {0}", memoryPage);

            DumpPage(memoryPage.Block);
        }

        internal void DumpPage(byte[] page)
        {
            Debug.Write("       ");
            for (var b = 0; b < 32; b++)
            {
                if (b > 0x00 && b % 0x08 == 0)
                {
                    Debug.Write("  ");
                }

                Debug.Write($"{b:X2} ");
            }

            Debug.WriteLine("");

            for (var l = 0; l < 0x100; l += 32)
            {
                Debug.Write($"{l:X4}:  ");

                for (var b = 0; b < 32; b++)
                {
                    if (b > 0x00 && b % 0x08 == 0)
                    {
                        Debug.Write("  ");
                    }

                    Debug.Write($"{page[(ushort)(l + b)]:X2} ");
                }

                Debug.WriteLine("");
            }

            Debug.WriteLine("");
        }
    }
}
