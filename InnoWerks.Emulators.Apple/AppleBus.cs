using System;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    // Memory maps
    //
    // 0000–00FF  Zero Page
    // 0100–01FF  Stack
    // 0200–03FF  Text page 1
    // 0400–07FF  Text page 2
    // 0800–1FFF  Graphics / BASIC workspace
    // 2000–3FFF  Hi-res graphics page 1
    // 4000–5FFF  Hi-res graphics page 2
    // 6000–BFFF  RAM (48K max)
    // C000–C0FF  I/O soft switches
    // C100–C7FF  Slot ROMs
    // C800–CFFF  Expansion ROM
    // D000–FFFF  ROM (Integer BASIC or Applesoft BASIC + Monitor)

    public class AppleBus : IBus
    {
        private readonly AppleConfiguration configuration;

        public SoftSwitches SoftSwitches { get; init; }

        //
        // for now we're going to do this, later we'll change
        // this to a collection of devices
        //
        public AppleKeyboard Keyboard { get; }

        // main and auxiliary ram
        private readonly byte[] mainRam;

        private readonly byte[] auxRam;      // IIe only

        private readonly byte[][] romBanks;         // $D000–$FFFF

        private int transactionCycles;

        public AppleBus(AppleConfiguration config)
        {
            configuration = config;
            SoftSwitches = new SoftSwitches(config);

            // create some devices, this needs to come in via the config
            Keyboard = new AppleKeyboard(SoftSwitches);

            mainRam = new byte[64 * 1024];

            if (configuration.Model == AppleModel.AppleIIe && configuration.HasAuxMemory)
                auxRam = new byte[64 * 1024];

            // todo: come back around and replace this per configuration
            romBanks = new byte[2][];
            romBanks[0] = new byte[16 * 1024];
            romBanks[1] = new byte[16 * 1024];
        }

        public void BeginTransaction()
        {
            transactionCycles = 0;
        }

        public int EndTransaction()
        {
            return transactionCycles;
        }

        public ulong CycleCount { get; private set; }

        public byte Peek(ushort address)
        {
            return ReadImpl(address);
        }

        public void Poke(ushort address, byte value)
        {
            WriteImpl(address, value);
        }

        public byte Read(ushort address)
        {
            Tick(1);

            return ReadImpl(address);
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            WriteImpl(address, value);
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            // we assume a rom load is going to write the entire rom,
            // including the initialization vector values at 0xfffd, and 0xfffc
            if (configuration.Model == AppleModel.AppleIIe)
            {
                if (objectCode.Length != 32 * 1024)
                    throw new InvalidOperationException("IIe ROM must be 32 KB");

                Array.Copy(objectCode, 0, romBanks[0], 0, 16 * 1024);
                Array.Copy(objectCode, 16 * 1024, romBanks[1], 0, 16 * 1024);
            }
            else
            {
                Array.Copy(objectCode, 0, romBanks[0], 0, objectCode.Length);
            }
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, mainRam, origin, objectCode.Length);
        }

        private byte ReadImpl(ushort address)
        {
            // $C000–$C0FF soft switches
            if (SoftSwitches.Handles(address))
                return SoftSwitches.Read(address);

            // RAM ($0000–$BFFF)
            if (address < 0xC000)
            {
                if (configuration.Model == AppleModel.AppleIIe && SoftSwitches.AuxRead && IsAuxAddress(address))
                {
                    return auxRam[address];
                }

                return mainRam[address];
            }

            // Slot probe region $C100–$C1FF
            if (address >= 0xC100 && address < 0xD000)
            {
                // Boot ROM expects $A0 for empty slot responses
                return 0xA0;
            }

            return configuration.Model switch
            {
                AppleModel.AppleII or AppleModel.AppleIIPlus => ReadAppleII(address),
                AppleModel.AppleIIe => ReadAppleIIe(address),
                _ => 0xFF,
            };
        }

        private byte ReadAppleII(ushort address)
        {
            // $C100–$CFFF slots / expansion (no cards yet)
            if (address < 0xD000)
                return 0x00;

            // $D000–$FFFF ROM (12 KB)
            return romBanks[0][address - 0xD000];
        }

        private byte ReadAppleIIe(ushort address)
        {
            // ROM visible across $C000–$FFFF unless overridden
            if (SoftSwitches.RomEnabled)
            {
                int offset = address - 0xC000;
                return romBanks[SoftSwitches.RomBank][offset];
            }

            // ROM disabled → RAM
            return mainRam[address];
        }

        private void WriteImpl(ushort address, byte value)
        {
            if (SoftSwitches.Handles(address))
            {
                SoftSwitches.Write(address);
                return;
            }

            if (address < 0xC000)
            {
                var bank = (configuration.Model == AppleModel.AppleIIe && SoftSwitches.AuxWrite && IsAuxAddress(address))
                    ? auxRam
                    : mainRam;
                bank[address] = value;
                return;
            }

            switch (configuration.Model)
            {
                case AppleModel.AppleII:
                case AppleModel.AppleIIPlus:
                    WriteAppleII(address, value);
                    break;

                case AppleModel.AppleIIe:
                    WriteAppleIIe(address, value);
                    break;
            }

            // $C100–$CFFF slots / expansion (ignored)
            if (address < 0xD000)
                return;

            // $D000–$FFFF ROM or RAM
            if (configuration.Model == AppleModel.AppleIIe)
            {
                // ROM write-through enabled (rare, but firmware does this)
                if (SoftSwitches.RomWrite)
                {
                    mainRam[address] = value;
                }

                return;
            }

            // Apple II / II+ ROM is always read-only
        }

        private static void WriteAppleII(ushort address, byte value)
        {
            // Writes to $C100–$CFFF (slot / expansion ROM) ignored for now
            if (address < 0xD000)
                return;

            // Writes to ROM ($D000–$FFFF) ignored
        }

        private void WriteAppleIIe(ushort address, byte value)
        {
            // Writes to ROM are normally ignored
            if (SoftSwitches.RomWrite)
            {
                mainRam[address] = value; // ROM write-through
                return;
            }

            // All other writes above $C000 go nowhere (slots/expansion ignored for now)
        }

        private static bool IsAuxAddress(ushort address)
        {
            // Text/graphics pages & zero page behavior can be refined later
            return address < 0xC000;
        }

        private void Tick(int howMany)
        {
            CycleCount += (ulong)howMany;
            transactionCycles += howMany;
        }

        private static void LogRead(ushort address)
        {
            if (address == 0xfffc || address == 0xfffd)
            {
                Console.Error.WriteLine("Initialization vector read for address ${0:X4}", address);
            }
            else if (0xc080 <= address && address <= 0xc083)
            {
                Console.Error.WriteLine("ROM bank switch read ${0:X4}", address);
            }
        }
    }
}
