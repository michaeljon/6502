using System;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class AppleBus : IBus
    {
        private static ushort ROM_BASE = 0xd000;

        private readonly AppleConfiguration configuration;

        public SoftSwitches SoftSwitches { get; init; }

        // main and auxiliary ram
        private readonly byte[] mainRam;

        private readonly byte[] auxRam;      // IIe only

        private readonly byte[] rom;         // $D000–$FFFF

        private int transactionCycles;

        public AppleBus(AppleConfiguration config)
        {
            configuration = config;
            SoftSwitches = new SoftSwitches(config);

            mainRam = new byte[64 * 1024];

            if (configuration.Model == AppleModel.AppleIIe && configuration.HasAuxMemory)
                auxRam = new byte[64 * 1024];

            rom = new byte[12 * 1024];
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
            if (address >= ROM_BASE)
                return rom[address - ROM_BASE];

            return mainRam[address];
        }

        public byte Read(ushort address)
        {
            Tick(1);

            // $C000–$C0FF soft switches
            if (SoftSwitches.Handles(address))
                return SoftSwitches.Read(address);

            // RAM ($0000–$BFFF)
            if (address < 0xC000)
                return ReadRam(address);

            // Slots / expansion (stubbed)
            if (address < ROM_BASE)
                return 0xFF;

            // ROM
            return rom[address - ROM_BASE];
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            if (SoftSwitches.Handles(address))
            {
                SoftSwitches.Write(address);
                return;
            }

            if (address < 0xC000)
            {
                WriteRam(address, value);
                return;
            }

            // Writes to ROM or slots ignored for now
        }

        public void LoadProgram(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            if (origin >= 0xD000)
            {
                Array.Copy(objectCode, 0, rom, origin - 0xD000, objectCode.Length);

                // power up initialization
                rom[MosTechnologiesCpu.RstVectorH - 0xD000] = (byte)((origin & 0xff00) >> 8);
                rom[MosTechnologiesCpu.RstVectorL - 0xD000] = (byte)(origin & 0xff);
            }
            else
            {
                Array.Copy(objectCode, 0, mainRam, origin, objectCode.Length);
            }
        }

        public byte this[ushort address]
        {
            get
            {
                return ReadRam(address);
            }

            set
            {
                WriteRam(address, value);
            }
        }

        private byte ReadRam(ushort address)
        {
            if (configuration.Model == AppleModel.AppleIIe &&
                SoftSwitches.AuxRead &&
                IsAuxAddress(address))
            {
                return auxRam[address];
            }

            return mainRam[address];
        }

        private void WriteRam(ushort address, byte value)
        {
            if (configuration.Model == AppleModel.AppleIIe &&
                SoftSwitches.AuxWrite &&
                IsAuxAddress(address))
            {
                auxRam[address] = value;
                return;
            }

            mainRam[address] = value;
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
    }
}
