using System.Collections.Generic;

namespace InnoWerks.Emulators.Apple
{
    public interface ISoftSwitchStateProvider
    {
        Dictionary<SoftSwitch, bool> State { get; }
    }
}
