namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleKeyboard : IKeyboard
    {
        private readonly SoftSwitchRam softSwitches;

        public AppleKeyboard(SoftSwitchRam softSwitches)
        {
            this.softSwitches = softSwitches;
        }

        public void KeyDown(byte appleKey)
        {
            softSwitches.KeyLatch = appleKey;
            softSwitches.KeyStrobe = true;
        }
    }
}
