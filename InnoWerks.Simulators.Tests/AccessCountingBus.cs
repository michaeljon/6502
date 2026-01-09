using System;
using System.Collections.Generic;
using System.Linq;

namespace InnoWerks.Simulators.Tests
{
#pragma warning disable CA1002, CA1819, RCS1085, CA1851

    public class AccessCountingBus : IBus
    {
        private readonly byte[] memory = new byte[64 * 1024];

        private readonly int[] readCounts = new int[64 * 1024];

        private readonly int[] writeCounts = new int[64 * 1024];

        public AccessCountingBus()
        {
            BusAccesses = [];
        }

        private int transactionCycles;

        public void BeginTransaction()
        {
            transactionCycles = 0;
        }

        public int EndTransaction()
        {
            return transactionCycles;
        }

        public long CycleCount { get; private set; }

        public byte Peek(ushort address)
        {
            return memory[address];
        }

        public byte Read(ushort address)
        {
            IncCycles(1);

            BusAccesses.Add(
                new JsonHarteTestBusAccess()
                {
                    Address = address,
                    Value = memory[address],
                    Type = CycleType.Read
                }
            );

            readCounts[address]++;

            return memory[address];
        }

        public void Write(ushort address, byte value)
        {
            IncCycles(1);

            BusAccesses.Add(
                new JsonHarteTestBusAccess()
                {
                    Address = address,
                    Value = value,
                    Type = CycleType.Write
                }
            );

            writeCounts[address]++;

            memory[address] = value;
        }

        public void Initialize(IEnumerable<JsonHarteRamEntry> mem)
        {
            ArgumentNullException.ThrowIfNull(mem);

            CycleCount = 0;
            transactionCycles = 0;

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

            if (expectedCount != BusAccesses.Count)
            {
                return (false, 0, null, null);
            }

            for (var a = 0; a < expectedCount; a++)
            {
                var expected = expectedBusAccesses.ElementAt(a);
                var actual = BusAccesses[a];

                if (expected.Address != actual.Address || expected.Value != actual.Value || expected.Type != actual.Type)
                {
                    return (false, a, expected, actual);
                }
            }

            return (true, 0, null, null);
        }

        public List<JsonHarteTestBusAccess> BusAccesses { get; init; }

        private void IncCycles(int howMany)
        {
            CycleCount += howMany;
            transactionCycles += howMany;
        }
    }
}
