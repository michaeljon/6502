namespace InnoWerks.Emulators.Apple
{
    public sealed class AppleKeyboard : IKeyboard
    {
        private readonly SoftSwitches softSwitches;

        public AppleKeyboard(SoftSwitches softSwitches)
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
