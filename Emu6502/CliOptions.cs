using CommandLine;

namespace Emu6502
{
    public class CliOptions
    {
        [Option('r', "rom", Required = true, HelpText = "ROM to load")]
        public string RomFile { get; set; }

        [Option('l', "location", Required = true, HelpText = "Location at which to load the ROM.")]
        public ushort Location { get; set; }

        [Option('s', "speed", Required = false, Default = 5, HelpText = "Speed at which to run the emulator in steps per second. Default is 5. O disables the timer.")]
        public int StepsPerSecond { get; set; }

        [Option('v', "verbose-cpu", Default = false, HelpText = "Turn on CPU instruction writing to stderr")]
        public bool VerboseCpu { get; set; }
    }
}
