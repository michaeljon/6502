using CommandLine;

namespace Emu6502
{
    public class CliOptions
    {
        [Option('s', "speed", Required = false, Default = 5, HelpText = "Speed at which to run the emulator in steps per second. Default is 5. O disables the timer.")]
        public int StepsPerSecond { get; set; }

        [Option('s', "single-step", Default = false, HelpText = "Ask for the CPU to write instructions to the screen")]
        public bool SingleStep { get; set; }

        [Option('v', "trace", Default = false, HelpText = "Turn on CPU instruction writing to stderr")]
        public bool Trace { get; set; }
    }
}
