using CommandLine;

namespace Asm6502
{
    public class CliOptions
    {
        [Option('i', "input", Required = true, HelpText = "Assembly input file")]
        public string Input { get; set; }

        [Option('o', "output", HelpText = "Output file for binary. Will default to 'input.o'")]
        public string Output { get; set; }

        [Option('s', "symbol", Default = false, HelpText = "Display symbol table upon completion.")]
        public bool PrintSymbolTable { get; set; }

        [Option('p', "print", Default = false, HelpText = "Print the program text.")]
        public bool PrintProgramText { get; set; }

        [Option('s', "origin", Required = false, Default = (ushort)0x300, HelpText = "Staring address of program code.")]
        public ushort Origin { get; set; }
    }
}
