using System.Collections.Generic;

namespace InnoWerks.Simulators
{
    public sealed class CpuTraceBuffer
    {
        private readonly CpuTraceEntry[] buffer;
        private int index;
        private int count;

        public int Capacity => buffer.Length;
        public int Count => count;

        public CpuTraceBuffer(int capacity)
        {
            buffer = new CpuTraceEntry[capacity];
        }

        public void Add(CpuTraceEntry entry)
        {
            buffer[index] = entry;
            index = (index + 1) % buffer.Length;
            if (count < buffer.Length)
                count++;
        }

        public IEnumerable<CpuTraceEntry> Entries
        {
            get
            {
                for (int i = 0; i < count; i++)
                {
                    int idx = (index - count + i + buffer.Length) % buffer.Length;
                    yield return buffer[idx];
                }
            }
        }
    }
}
