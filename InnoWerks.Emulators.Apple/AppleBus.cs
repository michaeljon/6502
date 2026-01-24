using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Transactions;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class AppleBus : IBus
    {
        private readonly AppleConfiguration configuration;

        private int transactionCycles;

        private readonly MemoryBlocks memoryBlocks;

        // there are 8 slots, 0 - 7, most of the time, but slot 0 is not used
        // we keep the numbering for convenience
        private readonly SlotRomDevice[] slotDevices = new SlotRomDevice[8];

        private readonly List<IDevice> softSwitchDevices = [];

        private readonly MachineState machineState;

        private bool reportKeyboardLatchAll = true;

        public AppleBus(AppleConfiguration configuration, MemoryBlocks memoryBlocks, MachineState machineState)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(memoryBlocks);
            ArgumentNullException.ThrowIfNull(machineState);

            this.configuration = configuration;
            this.memoryBlocks = memoryBlocks;
            this.machineState = machineState;
        }

        public void AddDevice(IDevice device)
        {
            ArgumentNullException.ThrowIfNull(device, nameof(device));

            // this really shouldn't be here
            device.Reset();

            switch (device.Priority)
            {
                case DevicePriority.SoftSwitch:
                    softSwitchDevices.Add(device);
                    break;

                case DevicePriority.Slot:
                    if (slotDevices[device.Slot] != null)
                    {
                        throw new ArgumentException($"There is already a device {slotDevices[device.Slot].Name} in slot {device.Slot}");
                    }

                    var slotDevice = (SlotRomDevice)device;
                    slotDevices[device.Slot] = slotDevice;

                    memoryBlocks.LoadSlotCxRom(device.Slot, slotDevice.Rom);
                    if (slotDevice.ExpansionRom != null)
                    {
                        memoryBlocks.LoadSlotC8Rom(device.Slot, slotDevice.ExpansionRom);
                    }

                    if (device.Slot == 1)
                    {
                        reportKeyboardLatchAll = false;
                    }

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
        }

        public byte Read(ushort address)
        {
            Tick(1);

            HandleC3xxAndCfff(address);

            if (address < 0xC000)
            {
                return memoryBlocks.Read(address);
            }
            else if (address < 0xC090)
            {
                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.HandlesRead(address))
                    {
                        var (value, remapNeeded) = softSwitchDevice.Read(address);
                        value |= CheckKeyboardLatch(address);

                        if (remapNeeded)
                        {
                            memoryBlocks.Remap();
                        }

                        return value;
                    }
                }

                return 0x00;
            }
            else if (address < 0xC0FF)
            {
                var slot = ((address >> 4) & 7) - 8;

                return DoSlotRead(slot, address);
            }

            return memoryBlocks.Read(address);
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            HandleC3xxAndCfff(address);

            if (address < 0xC000)
            {
                memoryBlocks.Write(address, value);
            }
            else if (address < 0xC090)
            {
                CheckClearKeystrobe(address);

                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.HandlesWrite(address) && softSwitchDevice.Write(address, value))
                    {
                        memoryBlocks.Remap();
                    }
                }
            }
            else if (address < 0xC0FF)
            {
                var slot = ((address >> 4) & 7) - 8;

                DoSlotWrite(slot, address, value);
            }
            else
            {
                memoryBlocks.Write(address, value);
            }
        }

        public byte Peek(ushort address)
        {
            return memoryBlocks.Read(address);
        }

        public void Poke(ushort address, byte value)
        {
            memoryBlocks.Write(address, value);
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);
            memoryBlocks.LoadProgramToRom(objectCode);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);
            memoryBlocks.LoadProgramToRam(objectCode, origin);
        }

        public void Reset()
        {
            foreach (var device in softSwitchDevices)
            {
                device.Reset();
            }

            for (var slot = 0; slot < slotDevices.Length; slot++)
            {
                slotDevices[slot]?.Reset();
            }

            machineState.CurrentSlot = -1;

            memoryBlocks.Remap();

            transactionCycles = 0;
            CycleCount = 0;
        }

        private void Tick(int cycles)
        {
            CycleCount += (ulong)cycles;

            foreach (var device in softSwitchDevices)
            {
                device.Tick(cycles);
            }

            for (var slot = 0; slot < slotDevices.Length; slot++)
            {
                slotDevices[slot]?.Tick(cycles);
            }

            transactionCycles += cycles;
        }

        private byte DoSlotRead(int slot, ushort address)
        {
            var slotDevice = slotDevices[slot];

            if (slotDevice?.HandlesRead(address) == true)
            {
                var (value, _) = slotDevice.Read(address);
                return value;
            }

            return 0xFF;
        }

        private void DoSlotWrite(int slot, ushort address, byte value)
        {
            var slotDevice = slotDevices[slot];

            if (slotDevice?.HandlesWrite(address) == true)
            {
                SimDebugger.Info("Write IO {0} {1:X4}\n", slot, address);
                slotDevice.Write(address, value);
            }
        }

        private void HandleC3xxAndCfff(ushort address)
        {
            bool remapNeeded = false;

            if (machineState.State[SoftSwitch.Slot3RomEnabled] == false && address >> 8 == 0xC3)
            {
                // Debugger.Break();
                machineState.State[SoftSwitch.IntC8RomEnabled] = false;
                remapNeeded = true;
            }

            if (address == 0xCFFF)
            {
                // Debugger.Break();
                machineState.State[SoftSwitch.IntC8RomEnabled] = true;
                remapNeeded = true;
            }

            if (remapNeeded)
            {
                memoryBlocks.Remap();
            }
        }

        private byte CheckKeyboardLatch(ushort address)
        {
            if (reportKeyboardLatchAll == false)
            {
                return 0x00;
            }

            // all these addresses return the KSTRB and ASCII value
            if (address >= 0xC001 && address <= 0xC00F)
            {
                return machineState.KeyStrobe ?
                    machineState.KeyLatch |= 0x80 :
                    machineState.KeyLatch;
            }

            // 0xC010 is handled directly by the keyboard as the "owning" device

            // if the IOU is disabled then we only handle the MMU soft switch
            if (machineState.State[SoftSwitch.IOUDisabled] == true)
            {
                return 0x00;
            }

            if (address >= 0xC001 && address <= 0xC01F)
            {
                return machineState.KeyLatch;
            }

            return 0x00;
        }

        private void CheckClearKeystrobe(ushort address)
        {
            if (reportKeyboardLatchAll == false)
            {
                return;
            }

            if (address >= 0xC010 && address <= 0xC01F)
            {
                machineState.KeyStrobe = false;
            }
        }
    }
}
