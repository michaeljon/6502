using CommandLine;

namespace Asm6502
{
    public class CliOptions
    {
        [Option('i', "input", Required = true, HelpText = "Assembly input file")]
        public string Input { get; set; }

        [Option('o', "output", HelpText = "Output file for binary. Will default to 'input.o'")]
        public string Output { get; set; }

        [Option('v', "verbose", Default = false, HelpText = "Display symbol table upon completion.")]
        public bool Verbose { get; set; }

        [Option('d', "debug", Default = false, HelpText = "Display internal program AST upon completion.")]
        public bool Debug { get; set; }
    }
}
