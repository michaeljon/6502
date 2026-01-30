using System;
using System.Net.NetworkInformation;

namespace InnoWerks.Computers.Apple
{
    public class MemoryPage
    {
        public const int PageSize = 256;

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Block { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays

        public string Type { get; init; }

        public byte PageNumber { get; init; }

        public MemoryPage(string type, byte pageNumber)
        {
            Block = new byte[PageSize];
            Type = type;
            PageNumber = pageNumber;
        }

        public override string ToString()
        {
            return $"{Type} at ${PageNumber:X2}";
        }

        public static MemoryPage Zeros(byte pageNumber)
        {
            var page = new MemoryPage("0x00", pageNumber);
            for (var i = 0; i < PageSize; i++)
            {
                page.Block[i] = 0x00;
            }
            return page;
        }

        public static MemoryPage FFs(byte pageNumber)
        {
            var page = new MemoryPage("0xff", pageNumber);
            for (var i = 0; i < PageSize; i++)
            {
                page.Block[i] = 0xFF;
            }
            return page;
        }

        public static MemoryPage Random(byte pageNumber)
        {
            var r = new Random();

            var page = new MemoryPage("rnd", pageNumber);
#pragma warning disable CA5394
            r.NextBytes(page.Block);
#pragma warning restore CA5394
            return page;
        }

        public static MemoryPage Alternating(byte pageNumber)
        {
            var page = new MemoryPage("alt", pageNumber);
            for (var i = 0; i < PageSize / 2; i += 2)
            {
                page.Block[i] = 0x00;
                page.Block[i + 1] = 0xA0;
            }
            return page;
        }
    }
}

