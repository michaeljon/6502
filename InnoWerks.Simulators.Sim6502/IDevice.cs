namespace InnoWerks.Simulators
{
    public enum DevicePriority
    {
        System = 0,

        SoftSwitch = 1,

        Slot = 2
    }


#pragma warning disable CA1716
    public interface IDevice
    {
        DevicePriority Priority { get; }

        int Slot { get; }

        string Name { get; }

        bool Handles(ushort address);

        byte Read(ushort address);

        void Write(ushort address, byte value);

        void Reset();
    }
#pragma warning restore CA1716
}
