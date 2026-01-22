using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
    public class AppleBus : IBus
    {
        private readonly AppleConfiguration configuration;

        private int transactionCycles;

        public MemoryBlocks BackingStore { get; init; }

        // there are 8 slots, 0 - 7, most of the time, but slot 0 is not used
        // we keep the numbering for convenience
        private readonly SlotRomDevice[] slotDevices = new SlotRomDevice[8];

        private readonly List<IDevice> softSwitchDevices = [];

        private readonly MachineState machineState;

        private bool reportKeyboardLatchAll = true;

        public AppleBus(AppleConfiguration configuration, MachineState machineState)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(machineState);

            this.configuration = configuration;
            this.machineState = machineState;

            BackingStore = new MemoryBlocks(machineState);
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

                    BackingStore.LoadSlotCxRom(device.Slot, slotDevice.Rom);
                    if (slotDevice.ExpansionRom != null)
                    {
                        BackingStore.LoadSlotC8Rom(device.Slot, slotDevice.ExpansionRom);
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
                return BackingStore.Read(address);
            }
            else if (address < 0xC08F)
            {
                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.HandlesRead(address))
                    {
                        var (value, remapNeeded) = softSwitchDevice.Read(address);
                        value |= CheckKeyboardLatch(address);

                        if (remapNeeded)
                        {
                            BackingStore.Remap();
                        }

                        return value;
                    }
                }

                return 0x00;
            }
            else if (address < 0xC0FF)
            {
                var slot = (address >> 4) & 7;

                return DoSlotRead(slot, address);
            }
            else if (address < 0xC800)
            {
                //
                // this should just fall through to the underlying memory map
                //
                if (machineState.State[SoftSwitch.SlotRomEnabled] == true)
                {
                    var slot = (address >> 8) & 7;

                    return DoSlotRead(slot, address);
                }
            }
            else if (address < 0xD000)
            {
                //
                // this should just fall through to the underlying memory map
                //
                if (machineState.State[SoftSwitch.IntC8RomEnabled] == false)
                {
                    var slot = (address >> 9) & 3;

                    return DoSlotRead(slot, address);
                }
            }

            return BackingStore.Read(address);
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            HandleC3xxAndCfff(address);

            if (address < 0xC000)
            {
                BackingStore.Write(address, value);
            }
            else if (address < 0xC08F)
            {
                CheckClearKeystrobe(address);

                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.HandlesWrite(address) && softSwitchDevice.Write(address, value))
                    {
                        BackingStore.Remap();
                    }
                }
            }
            else if (address < 0xC0FF)
            {
                var slot = (address >> 4) & 7;

                DoSlotWrite(slot, address, value);
            }
            else if (address < 0xC800)
            {
                //
                // this should just fall through to the underlying memory map
                //
                if (machineState.State[SoftSwitch.SlotRomEnabled] == true)
                {
                    var slot = (address >> 8) & 7;

                    DoSlotWrite(slot, address, value);
                }
            }
            else if (address < 0xD000)
            {
                //
                // this should just fall through to the underlying memory map
                //
                if (machineState.State[SoftSwitch.IntC8RomEnabled] == false)
                {
                    var slot = (address >> 9) & 3;

                    DoSlotWrite(slot, address, value);
                }
            }
            else
            {
                BackingStore.Write(address, value);
            }
        }

        public byte Peek(ushort address)
        {
            return BackingStore.Read(address);
        }

        public void Poke(ushort address, byte value)
        {
            BackingStore.Write(address, value);
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);
            BackingStore.LoadProgramToRom(objectCode);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);
            BackingStore.LoadProgramToRam(objectCode, origin);
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

            BackingStore.Remap();

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

            return 0x00;
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
            //
            // apple iie technical ref ch 6 page 133-134 talks about
            // the peripheral listening to $CFFF and that it needs to
            // write its slot # to $07F8 before its expansion rom
            // is enabled
            //
            // that means that the test and reset above (and probably
            // below in Write) are likely wrong wrong wrong
            //

            bool remapNeeded = false;

            // handle the SoftSwitch.IntC8RomEnabled state
            if (machineState.State[SoftSwitch.Slot3RomEnabled] == false && address >> 8 == 0xC3)
            {
                if (machineState.State[SoftSwitch.IntC8RomEnabled] == false)
                {
                    machineState.State[SoftSwitch.IntC8RomEnabled] = true;
                    remapNeeded = true;
                }
            }

            if (machineState.State[SoftSwitch.Slot3RomEnabled] == true && address == 0xCFFF)
            {
                machineState.State[SoftSwitch.IntC8RomEnabled] = false;
                remapNeeded = true;
            }

            if (remapNeeded)
            {
                BackingStore.Remap();
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
