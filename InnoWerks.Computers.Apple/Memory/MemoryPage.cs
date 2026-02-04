using System;
using System.Net.NetworkInformation;

namespace InnoWerks.Computers.Apple
{
    public enum MemoryPageType
    {
        Undefined,

        Ram,

        Rom,

        CardRom,

        LanguageCard
    }

    public class MemoryPage
    {
        public const int PageSize = 256;

#pragma warning disable CA1819 // Properties should not return arrays
        public byte[] Block { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays

        public MemoryPageType MemoryPageType { get; init; }

        public string Description { get; init; }

        public byte PageNumber { get; init; }

        public MemoryPage(MemoryPageType memoryPageType, string description, byte pageNumber)
        {
            Block = new byte[PageSize];

            MemoryPageType = memoryPageType;
            Description = description;
            PageNumber = pageNumber;
        }

        public override string ToString()
        {
            return $"{MemoryPageType} {Description} at ${PageNumber:X2} ({PageNumber})";
        }

        public void ZeroOut()
        {
            for (var i = 0; i < PageSize; i++)
            {
                Block[i] = 0x00;
            }
        }

        public static MemoryPage Zeros(MemoryPageType memoryPageType, byte pageNumber)
        {
            var page = new MemoryPage(memoryPageType, "0x00", pageNumber);
            for (var i = 0; i < PageSize; i++)
            {
                page.Block[i] = 0x00;
            }
            return page;
        }

        public static MemoryPage FFs(MemoryPageType memoryPageType, byte pageNumber)
        {
            var page = new MemoryPage(memoryPageType, "0xff", pageNumber);
            for (var i = 0; i < PageSize; i++)
            {
                page.Block[i] = 0xFF;
            }
            return page;
        }

        public static MemoryPage Random(MemoryPageType memoryPageType, byte pageNumber)
        {
            var r = new Random();

            var page = new MemoryPage(memoryPageType, "rnd", pageNumber);
#pragma warning disable CA5394
            r.NextBytes(page.Block);
#pragma warning restore CA5394
            return page;
        }

        public static MemoryPage Alternating(MemoryPageType memoryPageType, byte pageNumber)
        {
            var page = new MemoryPage(memoryPageType, "alt", pageNumber);
            for (var i = 0; i < PageSize / 2; i += 2)
            {
                page.Block[i] = 0x00;
                page.Block[i + 1] = 0xA0;
            }
            return page;
        }
    }
}

