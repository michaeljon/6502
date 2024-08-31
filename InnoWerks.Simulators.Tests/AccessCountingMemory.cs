using System;
using System.Collections.Generic;
using System.Linq;

namespace InnoWerks.Simulators.Tests
{
#pragma warning disable CA1002, CA1819, RCS1085, CA1851

    public class AccessCountingMemory : IMemory
    {
        private readonly byte[] memory = new byte[64 * 1024];

        private readonly List<JsonHarteTestBusAccess> busAccesses = new();

        private readonly int[] readCounts = new int[64 * 1024];

        private readonly int[] writeCounts = new int[64 * 1024];

        public byte Read(ushort address, bool countAccess = true)
        {
            if (countAccess == true)
            {
                busAccesses.Add(
                    new JsonHarteTestBusAccess()
                    {
                        Address = address,
                        Value = memory[address],
                        Type = CycleType.Read
                    }
                );

                readCounts[address]++;
            }

            return memory[address];
        }

        public ushort ReadWord(ushort address, bool countAccess = true)
        {
            if (countAccess == true)
            {
                busAccesses.Add(
                    new JsonHarteTestBusAccess()
                    {
                        Address = address,
                        Value = memory[address],
                        Type = CycleType.Read
                    }
                );

                busAccesses.Add(
                    new JsonHarteTestBusAccess()
                    {
                        Address = (ushort)(address + 1),
                        Value = memory[address + 1],
                        Type = CycleType.Read
                    }
                );

                readCounts[address]++;
                readCounts[address + 1]++;
            }

            var lo = memory[address];
            var hi = memory[address + 1];

            return (ushort)((hi << 8) | lo);
        }

        public void Write(ushort address, byte value, bool countAccess = true)
        {
            if (countAccess == true)
            {
                busAccesses.Add(
                    new JsonHarteTestBusAccess()
                    {
                        Address = address,
                        Value = value,
                        Type = CycleType.Write
                    }
                );

                writeCounts[address]++;
            }

            memory[address] = value;
        }

        public void WriteWord(ushort address, ushort value, bool countAccess = true)
        {
            if (countAccess == true)
            {
                busAccesses.Add(
                    new JsonHarteTestBusAccess()
                    {
                        Address = address,
                        Value = value,
                        Type = CycleType.Write
                    }
                );

                busAccesses.Add(
                    new JsonHarteTestBusAccess()
                    {
                        Address = (ushort)(address + 1),
                        Value = value,
                        Type = CycleType.Write
                    }
                );

                writeCounts[address]++;
                writeCounts[address + 1]++;
            }

            memory[address] = (byte)(value & 0x00ff);
            memory[address + 1] = (byte)((value >> 8) & 0xff);
        }

        public byte this[ushort address]
        {
            get
            {
                return memory[address];
            }

            set
            {
                memory[address] = value;
            }
        }

        public void Initialize(IEnumerable<JsonHarteRamEntry> mem)
        {
            ArgumentNullException.ThrowIfNull(mem);

            foreach (var m in mem)
            {
                memory[m.Address] = m.Value;
            }
        }

        public void LoadProgram(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, memory, origin, objectCode.Length);
        }

        public (bool matches, ushort differsAtAddr, byte expectedValue, byte actualValue) ValidateMemory(IEnumerable<JsonHarteRamEntry> mem)
        {
            ArgumentNullException.ThrowIfNull(mem);

            foreach (var m in mem)
            {
                if (m.Value != memory[m.Address])
                {
                    return (false, m.Address, m.Value, memory[m.Address]);
                }
            }

            return (true, 0, 0, 0);
        }

        public (bool matches, int differsAtIndex, JsonHarteTestBusAccess expectedValue, JsonHarteTestBusAccess actualValue) ValidateCycles(IEnumerable<JsonHarteTestBusAccess> expectedBusAccesses)
        {
            ArgumentNullException.ThrowIfNull(expectedBusAccesses);

            var expectedCount = expectedBusAccesses.Count();

            if (expectedCount != busAccesses.Count)
            {
                return (false, 0, null, null);
            }

            for (var a = 0; a < expectedCount; a++)
            {
                var expected = expectedBusAccesses.ElementAt(a);
                var actual = busAccesses[a];

                if (expected.Address != actual.Address || expected.Value != actual.Value || expected.Type != actual.Type)
                {
                    return (false, a, expected, actual);
                }
            }

            return (true, 0, null, null);
        }
    }
}
