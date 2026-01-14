using InnoWerks.Processors;

namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleIISoftSwitches : AppleSoftSwitchesBase
    {
        public override string Name => "Apple II / II+ Soft Switches";

        public AppleIISoftSwitches()
        {
            Reset();
        }

        public override byte Read(ushort address)
        {
            SimDebugger.Info($"[SS] Read({address:X4})\n");

            return 0;
        }

        public override void Write(ushort address, byte value)
        {
            SimDebugger.Info($"[SS] Write({address:X4}, {value:X2})\n");
        }

        public override void Reset()
        {
            WarmBoot();
        }
    }
}
