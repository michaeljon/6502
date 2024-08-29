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

        public void Initialize(IEnumerable<List<int>> mem)
        {
            ArgumentNullException.ThrowIfNull(mem);

            foreach (var m in mem)
            {
                memory[m[0]] = (byte)m[1];
            }
        }

        public void LoadProgram(byte[] objectCode, ushort origin)
        {
            ArgumentNullException.ThrowIfNull(objectCode);

            Array.Copy(objectCode, 0, memory, origin, objectCode.Length);
        }

        public (bool matches, ushort differsAtAddr, byte expectedValue, byte actualValue) ValidateMemory(IEnumerable<List<int>> mem)
        {
            ArgumentNullException.ThrowIfNull(mem);

            foreach (var m in mem)
            {
                if (m[1] != memory[m[0]])
                {
                    return (false, (ushort)m[0], (byte)m[1], memory[m[0]]);
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
