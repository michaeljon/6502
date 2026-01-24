using System;
using System.Net.NetworkInformation;

namespace InnoWerks.Emulators.Apple
{
    public class MemoryPage
    {
        public const int PageSize = 256;

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Block { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays

        public string Type { get; init; }

        public int PageNumber { get; init; }

        public MemoryPage(string type, int pageNumber)
        {
            Block = new byte[PageSize];
            Type = type;
            PageNumber = pageNumber;
        }

        public override string ToString()
        {
            return $"{Type} at ${PageNumber:X2}";
        }

        private static MemoryPage zeros;

        public static MemoryPage Zeros
        {
            get
            {
                if (zeros == null)
                {
                    zeros = new MemoryPage("zero", 0x00);
                    for (var i = 0; i < PageSize; i++)
                    {
                        zeros.Block[i] = 0x00;
                    }
                }

                return zeros;
            }
        }

        private static MemoryPage ffs;

        public static MemoryPage FFs
        {
            get
            {
                if (ffs == null)
                {
                    ffs = new MemoryPage("ff", 0x00);
                    for (var i = 0; i < PageSize; i++)
                    {
                        ffs.Block[i] = 0x00;
                    }
                }

                return ffs;
            }
        }

        private static MemoryPage random;

        public static MemoryPage Random
        {
            get
            {
                if (random == null)
                {
                    var r = new Random();

                    random = new MemoryPage("random", 0x00);
#pragma warning disable CA5394
                    r.NextBytes(random.Block);
#pragma warning restore CA5394
                }

                return random;
            }
        }

        private static MemoryPage alternating;

        public static MemoryPage Alternating
        {
            get
            {
                if (alternating == null)
                {
                    alternating = new MemoryPage("zero", 0x00);
                    for (var i = 0; i < PageSize / 2; i += 2)
                    {
                        alternating.Block[i] = 0x00;
                        alternating.Block[i + 1] = 0xA0;
                    }
                }

                return alternating;
            }
        }
    }
}
