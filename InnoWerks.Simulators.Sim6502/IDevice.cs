namespace InnoWerks.Simulators
{
    public enum DevicePriority
    {
        SoftSwitch = 0,

        Slot = 1
    }

#pragma warning disable CA1716
    public interface IDevice
    {
        DevicePriority Priority { get; }

        int Slot { get; }

        string Name { get; }

        bool HandlesRead(ushort address);

        bool HandlesWrite(ushort address);

        (byte value, bool remapNeeded) Read(ushort address);

        bool Write(ushort address, byte value);

        void Tick(int cycles);

        void Reset();
    }
#pragma warning restore CA1716
}
