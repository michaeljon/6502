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

#pragma warning disable CA5394 // Do not use insecure randomness
        private readonly byte floatingBus = (byte)(new Random().Next() & 0xFF);
#pragma warning restore CA5394 // Do not use insecure randomness

        private int transactionCycles;

        private readonly MemoryIIe memory;

        private readonly List<IDevice> systemDevices = [];

        private readonly Dictionary<int, IDevice> slotDevices = [];

        private readonly List<IDevice> softSwitchDevices = [];

        private readonly SoftSwitches softSwitches;

        private bool reportKeyboardLatchAll = true;

        public AppleBus(AppleConfiguration configuration, SoftSwitches softSwitches)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(softSwitches);

            this.configuration = configuration;
            this.softSwitches = softSwitches;

            memory = new MemoryIIe(configuration, softSwitches);
        }

        public IList<(ushort address, string name)> ConfiguredAddresses(bool forRead)
        {
            var configured = new List<(ushort address, string name)>();

            foreach (var device in systemDevices)
            {
                for (ushort address = 0xC000; address < 0xC100; address++)
                {
                    if (forRead == true ? device.HandlesRead(address) : device.HandlesWrite(address))
                    {
                        configured.Add((address, device.Name));
                    }
                }
            }

            foreach (var device in softSwitchDevices)
            {
                for (ushort address = 0xC000; address < 0xC100; address++)
                {
                    if (forRead == true ? device.HandlesRead(address) : device.HandlesWrite(address))
                    {
                        configured.Add((address, device.Name));
                    }
                }
            }

            foreach (var (slot, device) in slotDevices)
            {
                for (ushort address = 0xC000; address < 0xC100; address++)
                {
                    if (forRead == true ? device.HandlesRead(address) : device.HandlesWrite(address))
                    {
                        configured.Add((address, $"[{slot}] {device.Name}"));
                    }
                }
            }

            // we only report out that we support the keyboard read bits here
            if (forRead == true && reportKeyboardLatchAll == true)
            {
                for (ushort address = 0xC001; address < 0xC020; address++)
                {
                    configured.Add((address, "Keyboard latch handler KBD"));
                }
            }

            // we only report out that we support the keyboard write bits here
            if (forRead == false && reportKeyboardLatchAll == true)
            {
                for (ushort address = 0xC010; address < 0xC020; address++)
                {
                    configured.Add((address, "Keystrobe clear handler KBDSTRB"));
                }
            }

            return configured;
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

            if (0xC000 <= address && address <= 0xC07F)
            {
                foreach (var device in systemDevices)
                {
                    if (device.HandlesRead(address))
                    {
                        return (byte)(device.Read(address) | CheckKeyboardLatch(address));
                    }
                }

                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.HandlesRead(address))
                    {
                        return (byte)(softSwitchDevice.Read(address) | CheckKeyboardLatch(address));
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

                if (device.HandlesRead(address) == true)
                {
                    return device.Read(address);
                }

                SimDebugger.Info("Reached I/O read with device / with handler");
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

                    if (device.HandlesRead(address) == true)
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

                        if (device.HandlesRead(address) == true)
                        {
                            SimDebugger.Info("Read slot {0} C8XX ROM {1:X4}\n", slot, address);
                            return device.Read(address);
                        }
                    }
                }
            }

            return memory.Read(address);
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            if (0xC000 <= address && address <= 0xC07F)
            {
                CheckClearKeystrobe(address);

                foreach (var device in systemDevices)
                {
                    if (device.HandlesWrite(address))
                    {
                        device.Write(address, value);
                        return;
                    }
                }

                foreach (var softSwitchDevice in softSwitchDevices)
                {
                    if (softSwitchDevice.HandlesWrite(address))
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
                if (device?.HandlesWrite(address) == true)
                {
                    SimDebugger.Info("Write IO {0} {1:X4}\n", slot, address);
                    device.Write(address, value);
                    return;
                }

                SimDebugger.Info("Reached I/O write with device / with handler");
            }

            if (softSwitches.LcActive == false)
            {
                if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC700)
                {
                    var slot = (address >> 8) & 7;

                    slotDevices.TryGetValue(slot, out var device);
                    if (device?.HandlesWrite(address) == true)
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
                    if (device?.HandlesWrite(address) == true)
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

        private byte CheckKeyboardLatch(ushort address)
        {
            if (reportKeyboardLatchAll == false)
            {
                return 0x00;
            }

            // all these addresses return the KSTRB and ASCII value
            if (address >= 0xC001 && address <= 0xC00F)
            {
                return softSwitches.KeyStrobe ?
                    softSwitches.KeyLatch |= 0x80 :
                    softSwitches.KeyLatch;
            }

            // 0xC010 is handled directly by the keyboard as the "owning" device

            // if the IOU is disabled then we only handle the MMU soft switch
            if (softSwitches.State[SoftSwitch.IOUDisabled] == true)
            {
                return 0x00;
            }

            if (address >= 0xC001 && address <= 0xC01F)
            {
                return softSwitches.KeyLatch;
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
                softSwitches.KeyStrobe = false;
            }
        }
    }
}
