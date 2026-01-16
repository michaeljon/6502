using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InnoWerks.Processors;
using InnoWerks.Simulators;

namespace InnoWerks.Emulators.Apple
{
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

        private readonly SoftSwitches softSwitches;

        public AppleBus(AppleConfiguration configuration, SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(softSwitches);

            this.configuration = configuration;
            this.softSwitches = softSwitches;

            memory = new MemoryIIe(configuration, softSwitches);
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
        }

        public byte Read(ushort address)
        {
            Tick(1);

            if (0xC000 <= address && address <= 0xC07F)
            {
                foreach (var device in systemDevices)
                {
                    if (device.Handles(address))
                    {
                        floatingBus = device.Read(address);
                        return floatingBus;
                    }
                }

                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.Handles(address))
                    {
                        return softSwitchDevice.Read(address);
                    }
                }
            }

            if (0xC080 <= address && address <= 0xC0FF)
            {
                var slot = (address >> 4) & 7;

                slotDevices.TryGetValue(slot, out var device);

                if (device == null)
                {
                    return floatingBus;
                }

                if (device.Handles(address) == true)
                {
                    return device.Read(address);
                }
            }

            if (softSwitches.LcActive == false)
            {
                if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC7FF)
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
                else if (0xC800 <= address && address <= 0xCFFF)
                {
                    if (softSwitches.State[SoftSwitch.IntC8RomEnabled] == false)
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

            if (0xC000 <= address && address <= 0xC07F)
            {
                foreach (var device in systemDevices)
                {
                    if (device.Handles(address))
                    {
                        device.Write(address, value);
                        return;
                    }
                }

                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.Handles(address))
                    {
                        softSwitchDevice.Write(address, value);
                        return;
                    }
                }
            }

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

            if (softSwitches.LcActive == false)
            {
                if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC700)
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
                else if (softSwitches.State[SoftSwitch.IntC8RomEnabled] == false && 0xC800 <= address && address <= 0xCFFF)
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

        private void Tick(int cycles)
        {
            CycleCount += (ulong)cycles;

            // now tell the devices they got clocked
            foreach (var device in systemDevices)
            {
                device.Tick(cycles);
            }

            foreach (var device in softSwitchDevices)
            {
                device.Tick(cycles);
            }

            foreach (var (slot, device) in slotDevices)
            {
                device.Tick(cycles);
            }

            memory.Tick(cycles);

            transactionCycles += cycles;
        }
    }
}
