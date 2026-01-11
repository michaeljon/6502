namespace InnoWerks.Simulators
{
#pragma warning disable CA1716
    public interface IDevice
    {
        bool Handles(ushort address);

        byte Read(ushort address);

        void Write(ushort address, byte value);

        void Reset();
    }
#pragma warning restore CA1716
}
