using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Asm6502
{
    partial class Program
    {
#pragma warning disable SYSLIB1045

        static void Main(string[] args)
        {
            AssemblerRunner();
        }

        private static void AssemblerRunner()
        {
            // var inputLines = File.ReadAllLines("./tests/BruceClark6502_All.S");
            string[] inputLines = [
                "N2H      EQU   $0F",
                "         STA   N2H+1"
            ];

            var assembler = new Assembler(inputLines, 0x8000);
            var program = assembler.Assemble();

            Console.WriteLine("Program statements");
            foreach (var line in assembler.Program)
            {
                Console.WriteLine($"{line.LineNumber,-10} => {line.ToString().Replace("\n", " ").Replace("\t", "")}");
            }

            if (assembler.SymbolTable.Count > 0)
            {
                Console.WriteLine("Symbol table");

                foreach (var (label, symbol) in assembler.SymbolTable)
                {
                    Console.WriteLine($"{label,-10} => {symbol.ToString().Replace("\n", " ").Replace("\t", "")}");
                }
            }

            // File.WriteAllBytes("./tests/BruceClark6502_All.o", program);
        }

#if false
        private static void TestRegex()
        {
            //
            // implicit address mode AddressingMode.Implicit
            var imp = string.IsNullOrEmpty("");

            //
            // explicit accumulator
            var accumulatorRegex = new Regex(@"^A$");
            var a1 = accumulatorRegex.MatchNamedCaptures("A");

            //
            // immediate values are prefixed with a #   AddressingMode.Immediate
            var immediateRegex = new Regex(@"^#(?<prefix>[\$@%])?(?<arg>([0-9A-F]+|\w+))$");
            var i1 = immediateRegex.MatchNamedCaptures("#$2021");
            var i2 = immediateRegex.MatchNamedCaptures("#$20");
            var i3 = immediateRegex.MatchNamedCaptures("#@2021");
            var i4 = immediateRegex.MatchNamedCaptures("#@20");
            var i5 = immediateRegex.MatchNamedCaptures("#%01010101");
            var i6 = immediateRegex.MatchNamedCaptures("#LABEL");
            var i7 = immediateRegex.MatchNamedCaptures("#20432");

            //
            // relative AddressingMode.Relative
            var relativeRegex = new Regex(@"^(?<arg>(\w+))$");
            var r1 = relativeRegex.MatchNamedCaptures("LABEL");

            //
            // direct / absolute addressing AddressingMode.Absolute
            var absoluteAddressRegex = new Regex(@"^(?<arg>(\$[0-9A-F]{4}|\w+))$");
            var d1 = absoluteAddressRegex.MatchNamedCaptures("$2020");
            var d2 = absoluteAddressRegex.MatchNamedCaptures("$0020");
            var d3 = absoluteAddressRegex.MatchNamedCaptures("LABEL");

            //
            // indexed direct addressing Addressingmode.AbsoluteXIndexed | AddressingMode.AbsoluteYIndexed
            var indexedDirectRegex = new Regex(@"^(?<arg>(\$[0-9A-F]{4}|\w+)),(?<reg>[XY])$");
            var id1 = indexedDirectRegex.MatchNamedCaptures("$20,X");
            var id2 = indexedDirectRegex.MatchNamedCaptures("$20,Y");
            var id3 = indexedDirectRegex.MatchNamedCaptures("$1220,Y");
            var id4 = indexedDirectRegex.MatchNamedCaptures("$1220,Y");
            var id5 = indexedDirectRegex.MatchNamedCaptures("LABEL,X");
            var id6 = indexedDirectRegex.MatchNamedCaptures("LABEL,Y");

            //
            // zero-page direct addressing AddressingMode.ZeroPage
            var zeroPageDirectRegex = new Regex(@"^(?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+))$");
            var zp1 = zeroPageDirectRegex.MatchNamedCaptures("$20");
            var zp2 = zeroPageDirectRegex.MatchNamedCaptures("$0020");
            var zp3 = zeroPageDirectRegex.MatchNamedCaptures("LABEL");

            //
            // zero-page indexed direct addressing - AddressingMode.ZeroPageXIndexed | AddressingMode.ZeroPageYIndexex
            var zeroPageIndexedRegex = new Regex(@"^(?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+)),(?<reg>[XY])$");
            var zpi1 = zeroPageIndexedRegex.MatchNamedCaptures("$20,X");
            var zpi2 = zeroPageIndexedRegex.MatchNamedCaptures("$20,Y");
            var zpi3 = zeroPageIndexedRegex.MatchNamedCaptures("$0020,Y");
            var zpi4 = zeroPageIndexedRegex.MatchNamedCaptures("$0020,Y");
            var zpi5 = zeroPageIndexedRegex.MatchNamedCaptures("LABEL,X");
            var zpi6 = zeroPageIndexedRegex.MatchNamedCaptures("LABEL,Y");

            //
            // indirect addressing AddressingMode.Indirect
            var indirectRegex = new Regex(@"^\((?<arg>(\$[0-9A-F]{4}|\w+))\)$");
            var ind1 = indirectRegex.MatchNamedCaptures("($20)");
            var ind3 = indirectRegex.MatchNamedCaptures("($1220)");
            var ind6 = indirectRegex.MatchNamedCaptures("(LABEL)");

            //
            // pre-index indirect addressing AddressingMode.XIndexedIndirect
            var zeroPageIndexedDirectRegex = new Regex(@"^\((?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+)),X\)$");
            var zpid1 = zeroPageIndexedDirectRegex.MatchNamedCaptures("($20,X)");
            var zpid3 = zeroPageIndexedDirectRegex.MatchNamedCaptures("($0020,X)");
            var zpid5 = zeroPageIndexedDirectRegex.MatchNamedCaptures("(LABEL,X)");

            //
            // pre-index indirect addressing AddressingMode.IndirectYIndexed
            var zeroPagePostIndexedDirectRegex = new Regex(@"^\((?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+))\),Y$");
            var zppid1 = zeroPagePostIndexedDirectRegex.MatchNamedCaptures("($20),Y");
            var zppid3 = zeroPagePostIndexedDirectRegex.MatchNamedCaptures("($0020),Y");
            var zppid5 = zeroPagePostIndexedDirectRegex.MatchNamedCaptures("(LABEL),Y");
        }
#endif        
    }
}