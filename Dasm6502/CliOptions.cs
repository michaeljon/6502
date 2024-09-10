using CommandLine;

namespace Dasm6502
{
    public class CliOptions
    {
        [Option('i', "input", Required = true, HelpText = "Binary input fie")]
        public string Input { get; set; }

        [Option('o', "output", HelpText = "Output assembly. Will default to 'input.s'")]
        public string Output { get; set; }

        [Option('s', "origin", Required = false, Default = (ushort)0x300, HelpText = "Staring address of program code.")]
        public ushort Origin { get; set; }
    }
}
