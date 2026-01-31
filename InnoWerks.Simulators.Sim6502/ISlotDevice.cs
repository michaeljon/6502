namespace InnoWerks.Simulators
{
    public interface ISlotDevice
    {
        int Slot { get; }

        string Name { get; }

        bool HandlesRead(ushort address);

        bool HandlesWrite(ushort address);

        byte Read(ushort address);

        bool Write(ushort address, byte value);

        void Tick(int cycles);

        void Reset();
    }
#pragma warning restore CA1716
}
