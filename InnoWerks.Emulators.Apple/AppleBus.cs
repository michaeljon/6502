using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InnoWerks.Processors;
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

#pragma warning disable CA5394 // Do not use insecure randomness
        private byte floatingBus = (byte)(new Random().Next() & 0xFF);
#pragma warning restore CA5394 // Do not use insecure randomness

        private int transactionCycles;

        private readonly MemoryIIe memory;

        private readonly List<IDevice> systemDevices = [];

        private readonly Dictionary<int, IDevice> slotDevices = [];

        private readonly List<IDevice> softSwitchDevices = [];

        private Dictionary<SoftSwitch, bool> state { get; } = [];

        public AppleBus(AppleConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            this.configuration = configuration;
            this.memory = new MemoryIIe(configuration);

            foreach (SoftSwitch sw in Enum.GetValues<SoftSwitch>().OrderBy(v => v))
            {
                state[sw] = false;
            }
        }

        public void AddDevice(IDevice device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            // this really shouldn't be here
            device.Reset();

            switch (device.Priority)
            {
                case DevicePriority.System:
                    systemDevices.Add(device);
                    break;

                case DevicePriority.SoftSwitch:
                    softSwitchDevices.Add(device);
                    break;

                case DevicePriority.Slot:
                    slotDevices.Add(device.Slot, device);
                    break;
            }
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

        public void SetCpu(ICpu cpu)
        {
            ArgumentNullException.ThrowIfNull(cpu, nameof(cpu));
            memory.SetCpu(cpu);
        }

        public byte Read(ushort address)
        {
            Tick(1);

            // // Maybe toggle INTC8ROM
            // if ((address & 0xFF00) == 0xC300 && state[SoftSwitch.Slot3RomEnabled] == false)
            // {
            //     state[SoftSwitch.IntC8RomEnabled] = false;
            //     return 0x00;
            // }

            // if (address == 0xCFFF)
            // {
            //     state[SoftSwitch.IntC8RomEnabled] = true;
            //     return 0x00;
            // }

            // RAM ($0000–$BFFF)
            if (address < 0xC000)
            {
                floatingBus = memory.Read(address);
                return floatingBus;
            }

            foreach (var device in systemDevices)
            {
                if (device.Handles(address))
                {
                    floatingBus = device.Read(address);
                    return floatingBus;
                }
            }

            // this block, if the address is handled, short-circuit returns
            switch (address)
            {
                case SoftSwitchAddress.RDCXROM: return (byte)(state[SoftSwitch.SlotRomEnabled] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDC3ROM: return (byte)(state[SoftSwitch.Slot3RomEnabled] ? 0x80 : 0x00);

                case SoftSwitchAddress.RDIOUDIS: return (byte)(state[SoftSwitch.IOU] ? 0x80 : 0x00);
                case SoftSwitchAddress.RDDHIRES: return (byte)(state[SoftSwitch.DoubleHiRes] ? 0x80 : 0x00);
            }

            foreach (var softSwitchDevice in softSwitchDevices)
            {
                if (softSwitchDevice.Handles(address))
                {
                    return softSwitchDevice.Read(address);
                }
            }

            /*
            if address in $C080–$C0FF:
                slot = (address >> 4) & 7
                route to slot[slot].io[address & $0F]

            elif address in $C100–$C7FF:
                slot = (address >> 8) & 7
                route to slot[slot].rom[address & $FF]

            elif address in $C800–$CFFF:
                slot = (address >> 9) & 3   # slots 1–4 only
                route to slot[slot].expansion_rom[address & $1FF]
            */

            if (0xC080 <= address && address <= 0xC0FF)
            {
                var slot = (address >> 4) & 7;

                slotDevices.TryGetValue(slot, out var device);
                if (device?.Handles(address) == true)
                {
                    // SimDebugger.Info("Read slot {0} IO {1:X4}\n", slot, address);
                    return device.Read(address);
                }
            }

            bool lcActive = memory.State[SoftSwitch.LcReadEnabled] || memory.State[SoftSwitch.LcWriteEnabled];
            if (lcActive == false)
            {
                if (state[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC700)
                {
                    var slot = (address >> 8) & 7;

                    slotDevices.TryGetValue(slot, out var device);

                    if (device == null)
                    {
                        return floatingBus;
                    }

                    if (device.Handles(address) == true)
                    {
                        // SimDebugger.Info("Read slot {0} ROM {1:X4}\n", slot, address);
                        return device.Read(address);
                    }

                    return floatingBus;
                }
                else if (state[SoftSwitch.IntC8RomEnabled] == false && 0xC800 <= address && address <= 0xCFFF)
                {
                    if (state[SoftSwitch.IntC8RomEnabled] == false)
                    {
                        return memory.Read(address);
                    }
                    else
                    {
                        var slot = (address >> 9) & 3;

                        slotDevices.TryGetValue(slot, out var device);

                        if (device == null)
                        {
                            return floatingBus;
                        }

                        if (device.Handles(address) == true)
                        {
                            SimDebugger.Info("Read slot {0} C8XX ROM {1:X4}\n", slot, address);
                            return device.Read(address);
                        }
                    }
                }
            }

            floatingBus = memory.Read(address);
            return floatingBus;
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            if (address < 0xC000)
            {
                memory.Write(address, value);
                return;
            }

            foreach (var device in systemDevices)
            {
                if (device.Handles(address))
                {
                    device.Write(address, value);
                    return;
                }
            }

            // this block, if the address is handle, short-circuit returns
            switch (address)
            {
                case SoftSwitchAddress.SETSLOTCXROM: state[SoftSwitch.SlotRomEnabled] = true; return;
                case SoftSwitchAddress.SETINTCXROM: state[SoftSwitch.SlotRomEnabled] = false; return;

                case SoftSwitchAddress.SETINTC3ROM: state[SoftSwitch.Slot3RomEnabled] = false; return;
                case SoftSwitchAddress.SETSLOTC3ROM: state[SoftSwitch.Slot3RomEnabled] = true; return;

                case SoftSwitchAddress.IOUDISON: state[SoftSwitch.IOU] = true; return;
                case SoftSwitchAddress.IOUDISOFF: state[SoftSwitch.IOU] = false; return;
            }

            foreach (var softSwitchDevice in softSwitchDevices)
            {
                if (softSwitchDevice.Handles(address))
                {
                    softSwitchDevice.Write(address, value);
                    return;
                }
            }

            /*
            if address in $C080–$C0FF:
                slot = (address >> 4) & 7
                route to slot[slot].io[address & $0F]

            elif address in $C100–$C7FF:
                slot = (address >> 8) & 7
                route to slot[slot].rom[address & $FF]

            elif address in $C800–$CFFF:
                slot = (address >> 9) & 3   # slots 1–4 only
                route to slot[slot].expansion_rom[address & $1FF]
            */

            if (0xC080 <= address && address <= 0xC0FF)
            {
                var slot = (address >> 4) & 7;

                slotDevices.TryGetValue(slot, out var device);
                if (device?.Handles(address) == true)
                {
                    SimDebugger.Info("Write IO {0} {1:X4}\n", slot, address);
                    device.Write(address, value);
                    return;
                }
            }

            bool lcActive = memory.State[SoftSwitch.LcReadEnabled] || memory.State[SoftSwitch.LcWriteEnabled];
            if (lcActive == false)
            {
                if (state[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC700)
                {
                    var slot = (address >> 8) & 7;

                    slotDevices.TryGetValue(slot, out var device);
                    if (device?.Handles(address) == true)
                    {
                        SimDebugger.Info("Write ROM {0} {1:X4}\n", slot, address);
                        device.Write(address, value);
                        return;
                    }
                }
                else if (state[SoftSwitch.IntC8RomEnabled] == false && 0xC800 <= address && address <= 0xCFFF)
                {
                    var slot = (address >> 9) & 3;

                    slotDevices.TryGetValue(slot, out var device);
                    if (device?.Handles(address) == true)
                    {
                        SimDebugger.Info("Write ExROM {0} {1:X4}\n", slot, address);
                        device.Write(address, value);
                        return;
                    }
                }
            }

            memory.Write(address, value);
        }

        public byte Peek(ushort address)
        {
            return memory.Read(address);
        }

        public void Poke(ushort address, byte value)
        {
            memory.Write(address, value);
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            memory.LoadProgramToRom(objectCode);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            memory.LoadProgramToRam(objectCode, origin);
        }

        private void Tick(int howMany)
        {
            CycleCount += (ulong)howMany;
            transactionCycles += howMany;
        }
    }
}
