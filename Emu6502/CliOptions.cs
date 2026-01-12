using CommandLine;

namespace Emu6502
{
    public class CliOptions
    {
        [Option('s', "speed", Required = false, Default = 5, HelpText = "Speed at which to run the emulator in steps per second. Default is 5. O disables the timer.")]
        public int StepsPerSecond { get; set; }

        [Option('v', "single-step", Default = false, HelpText = "Turn on CPU instruction writing to stderr")]
        public bool SingleStep { get; set; }
    }
}
