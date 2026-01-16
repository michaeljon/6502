using System;
using System.Collections.Generic;

#pragma warning disable CA1822

namespace InnoWerks.Emulators.Apple
{
    public class MemoryBlocks
    {
        private const int PageSize = 256;

        private const int NumberOfPags = 64 * 1024 / PageSize;

        private readonly List<byte[]> mainMemory = new(NumberOfPags);
        private readonly List<byte[]> auxMemory = new(NumberOfPags);

        private readonly List<(byte[] readFrom, byte[] writeTo)> activeMemory = new(NumberOfPags);

        private byte GetPage(ushort address) => (byte)((address & 0xFF00) >> 8);

        private byte GetOffset(ushort address) => (byte)(address & 0x00FF);

        private readonly List<byte[]>[] lcRam;         // IIe only

        // swappable lo rom banks
        private readonly List<byte[]> loRom = new(16 * 1024 / PageSize);           // $D000–$DFFF

        // switch-selectable
        private readonly List<byte[]> cxRom = new(16 * 1024 / PageSize);           // $C000-$CFFF

        // single hi rom bank
        private readonly List<byte[]> hiRom = new(32 * 1024 / PageSize);           // $E000–$FFFF

        public void Remap(SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(softSwitches, nameof(softSwitches));

            // reset the aux/main selector
            for (var loop = 0x00; loop < 0xC0; loop++)
            {
                activeMemory[loop] = (mainMemory[loop], mainMemory[loop]);
            }

            for (var loop = 0xC0; loop < 0xD0; loop++)
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

            // INT ROM                  $C0 - $CF   cxRom
            for (var loop = 0xC0; loop < 0xC8; loop++)
            {
                // const UINT uSlotOffset = (loop & 0x0f) * 0x100;
                // if (loop == 0xC3)
                // 	memshadow[loop] = (SW_SLOTC3ROM && !SW_INTCXROM)	? pCxRomPeripheral+uSlotOffset	// C300..C3FF - Slot 3 ROM (all 0x00's)
                // 														: pCxRomInternal+uSlotOffset;	// C300..C3FF - Internal ROM
                // else
                // 	memshadow[loop] = !SW_INTCXROM	? pCxRomPeripheral+uSlotOffset						// C000..C7FF - SSC/Disk][/etc
                // 									: pCxRomInternal+uSlotOffset;						// C000..C7FF - Internal ROM

                if (loop == 0xC3)
                {
                    if (softSwitches.State[SoftSwitch.Slot3RomEnabled] && softSwitches.State[SoftSwitch.SlotRomEnabled] == false)
                    {
                        // use slot rom from device
                        activeMemory[loop] = (null, null);
                    }
                    else
                    {
                        // use internal page c3
                        activeMemory[loop] = (cxRom[loop - 0xC3], null);
                    }
                }
                else
                {
                    if (softSwitches.State[SoftSwitch.SlotRomEnabled] == false)
                    {
                        // use slot rom from device, else use internal
                        activeMemory[loop] = (null, null);
                    }
                    else
                    {
                        // use internal page c3
                        activeMemory[loop] = (cxRom[loop - 0xC3], null);
                    }
                }
            }

            for (var loop = 0xC8; loop < 0xD0; loop++)
            {
                // memdirty[loop] = 0;	// mem(cache) can't be dirty for ROM (but STA $Cnnn will set the dirty flag)
                // const UINT uRomOffset = (loop & 0x0f) * 0x100;
                // memshadow[loop] = (!SW_INTCXROM && !INTC8ROM)	? pCxRomPeripheral+uRomOffset			// C800..CFFF - Peripheral ROM (GH#486)
                // 												: pCxRomInternal+uRomOffset;			// C800..CFFF - Internal ROM

                if (loop == 0xC3)
                {
                    if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && softSwitches.State[SoftSwitch.IntC8RomEnabled] == false)
                    {
                        // use slot rom from device
                        activeMemory[loop] = (null, null);
                    }
                    else
                    {
                        // use internal page c3
                        activeMemory[loop] = (cxRom[loop - 0xC3], null);
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

            }

            if (softSwitches.State[SoftSwitch.Store80] == true)
            {
                for (var loop = 0x04; loop < 0x08; loop++)
                {
                    // memshadow[loop] = SW_PAGE2	? memaux+(loop << 8)
                    // 							: memmain+(loop << 8);
                    // memwrite[loop]  = mem+(loop << 8);
                }

                if (softSwitches.State[SoftSwitch.HiRes] == true)
                {
                    for (var loop = 0x20; loop < 0x40; loop++)
                    {
                        // memshadow[loop] = SW_PAGE2	? memaux+(loop << 8)
                        // 							: memmain+(loop << 8);
                        // memwrite[loop]  = mem+(loop << 8);
                    }

                }
            }
            else
            {

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

        public MemoryBlocks()
        {
            for (int p = 0; p < NumberOfPags; p++)
            {
                mainMemory[p] = new byte[PageSize];
                auxMemory[p] = new byte[PageSize];
            }

            // language card ram (should be private to MemoryIIe, really)
            lcRam = new List<byte[]>[2];
            lcRam[0] = new List<byte[]>(4 * 1024 / PageSize);   // 4k RAM bank 1
            lcRam[1] = new List<byte[]>(4 * 1024 / PageSize);   // 4k RAM bank 2

            // 4k ROM bank 1
            for (int p = 0; p < loRom.Count; p++)
            {
                loRom[p] = new byte[PageSize];
            }

            // 4k switch selectable
            for (int p = 0; p < cxRom.Count; p++)
            {
                cxRom[p] = new byte[PageSize];
            }

            // 8k ROM
            for (int p = 0; p < hiRom.Count; p++)
            {
                hiRom[p] = new byte[PageSize];
            }
        }
    }
}
