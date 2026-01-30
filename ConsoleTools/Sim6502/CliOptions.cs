using CommandLine;
using InnoWerks.Processors;

namespace Sim6502
{
    public class CliOptions
    {
        [Option('i', "input", Required = true, HelpText = "Assembly input file")]
        public string Input { get; set; }

        [Option('s', "origin", Required = false, Default = (ushort)0x300, HelpText = "Staring address of program code.")]
        public ushort Origin { get; set; }

        [Option('c', "cpu-class", Required = false, Default = CpuClass.WDC65C02, HelpText = "CPU class to simulator. Defaults to WDC656C02.")]
        public CpuClass CpuClass { get; set; }
    }
}
