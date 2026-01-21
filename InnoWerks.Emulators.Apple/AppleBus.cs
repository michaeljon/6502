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

        private readonly MemoryBlocks memoryBlocks;

        private readonly List<IDevice> systemDevices = [];

        // there are 8 slots, 0 - 7, most of the time, but slot 0 is not used
        // we keep the numbering for convenience
        private readonly IDevice[] slotDevices = new IDevice[8];

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
            memoryBlocks = new MemoryBlocks(softSwitches);
            memoryBlocks.Remap();
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
                    if (slotDevices[device.Slot] != null)
                    {
                        throw new ArgumentException($"There is already a device {slotDevices[device.Slot].Name} in slot {device.Slot}");
                    }

                    slotDevices[device.Slot] = device;

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

            // handle the SoftSwitch.IntC8RomEnabled state
            if (softSwitches.State[SoftSwitch.Slot3RomEnabled] == false && (address & 0xC300) == 0xC300)
            {
                softSwitches.State[SoftSwitch.IntC8RomEnabled] = true;
                memoryBlocks.Remap();
            }

            if (softSwitches.State[SoftSwitch.Slot3RomEnabled] == true && address == 0xCFFF)
            {
                softSwitches.State[SoftSwitch.IntC8RomEnabled] = false;
                memoryBlocks.Remap();
            }

            if (0xC000 <= address && address <= 0xC08F)
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
                        var result = (byte)(softSwitchDevice.Read(address) | CheckKeyboardLatch(address));
                        memoryBlocks.Remap();
                        return result;
                    }
                }
            }

            else if (0xC090 <= address && address <= 0xC0FF)
            {
                var slot = (address >> 4) & 7;
                var device = slotDevices[slot];

                if (device == null)
                {
                    return floatingBus;
                }

                if (device.HandlesRead(address) == true)
                {
                    return device.Read(address);
                }

                SimDebugger.Info("Reached I/O read from {0:X4} with device in {1} that doesn't handle", address, slot);
            }

            else if (softSwitches.LcActive == false)
            {
                if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC7FF)
                {
                    var slot = (address >> 8) & 7;
                    var device = slotDevices[slot];

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
                    if (softSwitches.State[SoftSwitch.IntC8RomEnabled] == true)
                    {
                        return memory.Read(address);
                    }
                    else
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
                        var slot = (address >> 9) & 3;
                        var device = slotDevices[slot];

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

            var real = memory.Read(address);

            //if (memoryBlocks.ResolveRead(address) != null)
            {
                var fake = memoryBlocks.Read(address);

                if (fake != real)
                {
#pragma warning disable CA2201 // Do not raise reserved exception types
                    throw new Exception($"Reads do not match fake={fake:X2} real={real:X2}");
#pragma warning restore CA2201 // Do not raise reserved exception types
                }
            }

            return real;
        }

        public void Write(ushort address, byte value)
        {
            Tick(1);

            // handle the SoftSwitch.IntC8RomEnabled state
            if (softSwitches.State[SoftSwitch.Slot3RomEnabled] == false && (address & 0xC300) == 0xC300)
            {
                softSwitches.State[SoftSwitch.IntC8RomEnabled] = true;
            }

            if (softSwitches.State[SoftSwitch.Slot3RomEnabled] == true && address == 0xCFFF)
            {
                softSwitches.State[SoftSwitch.IntC8RomEnabled] = false;
            }

            if (0xC000 <= address && address <= 0xC08F)
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
                        memoryBlocks.Remap();
                        return;
                    }
                }
            }

            else if (0xC090 <= address && address <= 0xC0FF)
            {
                var slot = (address >> 4) & 7;
                var device = slotDevices[slot];

                if (device?.HandlesWrite(address) == true)
                {
                    SimDebugger.Info("Write IO {0} {1:X4}\n", slot, address);
                    device.Write(address, value);
                    return;
                }

                SimDebugger.Info("Reached I/O write to {0:X4} with device in {1} that doesn't handle", address, slot);
            }

            else if (softSwitches.LcActive == false)
            {
                if (softSwitches.State[SoftSwitch.SlotRomEnabled] == true && 0xC100 <= address && address <= 0xC700)
                {
                    var slot = (address >> 8) & 7;
                    var device = slotDevices[slot];

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
                    var device = slotDevices[slot];

                    if (device?.HandlesWrite(address) == true)
                    {
                        SimDebugger.Info("Write ExROM {0} {1:X4}\n", slot, address);
                        device.Write(address, value);
                        return;
                    }
                }
            }

            var target = memoryBlocks.ResolveWrite(address);

            memoryBlocks.Write(address, value);
            memory.Write(address, value);
        }

        public byte Peek(ushort address)
        {
            return memory.Peek(address);
        }

        public void Poke(ushort address, byte value)
        {
            memory.Write(address, value);
        }

        public void LoadProgramToRom(byte[] objectCode)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            memory.LoadProgramToRom(objectCode);
            memoryBlocks.LoadProgramToRom(objectCode);
        }

        public void LoadProgramToRam(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            memory.LoadProgramToRam(objectCode, origin);
            memoryBlocks.LoadProgramToRam(objectCode, origin);
        }

        public void Reset()
        {
            foreach (var device in softSwitchDevices)
            {
                device.Reset();
            }

            foreach (var device in systemDevices)
            {
                device.Reset();
            }

            for (var slot = 0; slot < slotDevices.Length; slot++)
            {
                slotDevices[slot]?.Reset();
            }

            memoryBlocks.Remap();

            transactionCycles = 0;
            CycleCount = 0;
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

            for (var slot = 0; slot < slotDevices.Length; slot++)
            {
                slotDevices[slot]?.Tick(cycles);
            }

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

            for (var slot = 0; slot < slotDevices.Length; slot++)
            {
                var device = slotDevices[slot];
                if (device != null)
                {
                    for (ushort address = 0xC000; address < 0xC100; address++)
                    {
                        if (forRead == true ? device.HandlesRead(address) : device.HandlesWrite(address))
                        {
                            configured.Add((address, $"[{slot}] {device.Name}"));
                        }
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
    }
}
