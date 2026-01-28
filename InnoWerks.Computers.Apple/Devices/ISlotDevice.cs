using InnoWerks.Simulators;

namespace InnoWerks.Computers.Apple
{
#pragma warning disable CA1819 // Properties should not return arrays

    public interface ISlotDevice : IDevice
    {
        public MemoryPage LoSlotRom { get; set; }

        public MemoryPage[] HiSlotRom { get; set; }

        bool HasAuxRom { get; }
    }
}
