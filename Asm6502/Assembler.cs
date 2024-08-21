// #define ASSEMBLER_DEBUG_OUTPUT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Asm6502
{
#pragma warning disable IDE0290
    public partial class Assembler
    {
        [GeneratedRegex(@"^(\$[0-9A-F]{1,2})$")]
        private static partial Regex ArgumentParserShortRegex();

        [GeneratedRegex(@"^(\$[0-9A-F]{3,4}|\w+)$")]
        private static partial Regex ArgumentParserLongRegex();

        [GeneratedRegex(@"^(?<label>[_A-Z][_A-Z0-9]*)?\s+(?<opcode>[A-Z]{3})\s*(?<arg>[^\s;]+)?\s*(?<comment>;.*)?")]
        private static partial Regex CodeLineRegex();

        [GeneratedRegex(@"^[\*;](?<comment>.*)|^\s+;(?<comment>.*)")]
        private static partial Regex CommentLineRegex();

        [GeneratedRegex(@"^(?<label>[-A-Z][_A-Z0-9]*)?\s+(?<dir>(DB|DW))\s+(?<arg>[^\s;]+)\s*(?<comment>;.*)?")]
        private static partial Regex DataLineRegex();

        [GeneratedRegex(@"^\s+(?<dir>ORG)\s+(?<arg>[#$][0-9A-F]+)\s*(?<comment>;.*)?")]
        private static partial Regex DirectiveLineRegex();

        [GeneratedRegex(@"^(?<label>[-A-Z][_A-Z0-9]*):?\s*(?<comment>;.*)?")]
        private static partial Regex LabelledLineRegex();

        [GeneratedRegex(@"^(?<label>[-A-Z][_A-Z0-9]*)\s+(?<equiv>(=|EQU))\s+(?<arg>[#$][0-9A-F]+)\s*(?<comment>;.*)?")]
        private static partial Regex EquivalenceLineRegex();


        private readonly Regex codeLine = CodeLineRegex();
        private readonly Regex commentLine = CommentLineRegex();
        private readonly Regex dataLine = DataLineRegex();
        private readonly Regex directiveLine = DirectiveLineRegex();
        private readonly Regex labelledLine = LabelledLineRegex();
        private readonly Regex equivalenceLine = EquivalenceLineRegex();

        private readonly IEnumerable<string> program;

        private ushort currentOrgAddress;

        private ushort currentAddress;

        private readonly List<LineInformation> programLines = [];

        private readonly Dictionary<string, Symbol> symbolTable = [];

        public Assembler(string[] program, ushort startingAddress)
        {
            this.program = program;

            currentOrgAddress = startingAddress;
            currentAddress = currentOrgAddress;
        }

        public Dictionary<string, Symbol> SymbolTable => symbolTable;

        public List<LineInformation> Program => programLines;

        public byte[] Assemble()
        {
            GatherGlobals();
            ReadProgram();
            ResolveReferences();

            return GenerateBytes();
        }

        private void GatherGlobals()
        {
            var lineNumber = 1;

            foreach (var line in program.Select(l => l.TrimEnd()))
            {
                if (equivalenceLine.IsMatch(line))
                {
                    var node = ParseEqivalenceLine(lineNumber, line, equivalenceLine.MatchNamedCaptures(line));

                    UpdateSymbolTable(node);
                }

                lineNumber++;
            }
        }

        private void ReadProgram()
        {
            var lineNumber = 1;

            foreach (var line in program.Select(l => l.TrimEnd()))
            {
                LineInformation node = null;

                if (line.Trim().Length == 0)
                {
#if ASSEMBLER_DEBUG_OUTPUT
                    Console.WriteLine($"{lineNumber,8} EMPTY");
#endif

                    node = new LineInformation
                    {
                        LineType = LineType.Empty,
                        LineNumber = lineNumber,
                        CurrentOrg = currentOrgAddress
                    };
                }
                else if (dataLine.IsMatch(line))
                {
                    node = ParseDataLine(lineNumber, line, dataLine.MatchNamedCaptures(line));
                }
                else if (directiveLine.IsMatch(line))
                {
                    node = ParseDirectiveLine(lineNumber, line, directiveLine.MatchNamedCaptures(line));
                    currentAddress = currentOrgAddress = ResolveNumber(node.Value);
                }
                else if (equivalenceLine.IsMatch(line))
                {
                    node = ParseEqivalenceLine(lineNumber, line, equivalenceLine.MatchNamedCaptures(line));
                }
                else if (codeLine.IsMatch(line))
                {
                    node = ParseCodeLine(lineNumber, line, codeLine.MatchNamedCaptures(line));
                }
                else if (labelledLine.IsMatch(line))
                {
                    node = ParseLabelledLine(lineNumber, line, labelledLine.MatchNamedCaptures(line));
                }
                else if (commentLine.IsMatch(line))
                {
                    node = ParseCommentLine(lineNumber, line, commentLine.MatchNamedCaptures(line));
                }

                if (node != null)
                {
                    currentAddress += node.EffectiveSize;

                    // this time through only record the non-EQU values
                    if (node.LineType != LineType.Equivalence)
                    {
                        UpdateSymbolTable(node);
                    }

#if ASSEMBLER_DEBUG_OUTPUT
                    Console.WriteLine(node);
#endif                    
                    programLines.Add(node);
                }

                lineNumber++;
            }

        }

        private void ResolveReferences()
        {
            foreach (var instruction in programLines.Where(i => i.LineType == LineType.Code))
            {
                // if this is a symbol then replace its value and reparse
                if (symbolTable.TryGetValue(instruction.Argument.value, out var symbol) == true)
                {
                    if (symbol.IsEquivalence == false)
                    {
                        // todo - if this is a branch, then we need to resolve it differently
                        var raw = instruction.RawArgument.Replace(instruction.Argument.value, symbol.UnparsedValue);
                        instruction.Argument = ParseAddress(instruction.OpCode, raw);
                    }

                    instruction.Value = ExtractNumber(symbol.UnparsedValue, 0, 0);
                }
                else
                {
                    instruction.Value = instruction.Argument.value;
                }
            }
        }

        private byte[] GenerateBytes()
        {
            // 64k programs, pretty damned big actually
            var bytes = new byte[0x10000];
            ushort pc = 0;

            foreach (var instruction in programLines.Where(p => p.LineType == LineType.Code || p.LineType == LineType.Data))
            {
                Array.Copy(instruction.MachineCode, 0, bytes, pc, instruction.MachineCode.Length);
                pc += (ushort)instruction.MachineCode.Length;
            }

            return bytes;
        }

        private LineInformation ParseDataLine(int lineNumber, string line, IDictionary<string, string> captures)
        {
#if ASSEMBLER_DEBUG_OUTPUT
            Console.WriteLine($"{lineNumber,8} DATA    : {line,-80}");
#endif

            captures.TryGetValue("label", out string label);
            captures.TryGetValue("dir", out string dir);
            captures.TryGetValue("arg", out string arg);
            captures.TryGetValue("comment", out string comment);

            return new LineInformation
            {
                LineType = LineType.Data,
                LineNumber = lineNumber,
                CurrentOrg = currentOrgAddress,
                EffectiveAddress = currentAddress,
                Directive = Enum.Parse<Directive>(dir),
                Label = label,
                Value = ExtractNumber(arg, 0, 0),
                Comment = ExtractComment(comment),
                Line = line
            };
        }

        private LineInformation ParseDirectiveLine(int lineNumber, string line, IDictionary<string, string> captures)
        {
#if ASSEMBLER_DEBUG_OUTPUT
            Console.WriteLine($"{lineNumber,8} DIR     : {line,-80}");
#endif

            captures.TryGetValue("dir", out string dir);
            captures.TryGetValue("arg", out string arg);
            captures.TryGetValue("comment", out string comment);

            return new LineInformation
            {
                LineType = LineType.Directive,
                LineNumber = lineNumber,
                CurrentOrg = currentOrgAddress,
                EffectiveAddress = currentAddress,
                Directive = Enum.Parse<Directive>(dir),
                Value = ExtractNumber(arg, 0, 0),
                Comment = ExtractComment(comment),
                Line = line
            };
        }

        private LineInformation ParseEqivalenceLine(int lineNumber, string line, IDictionary<string, string> captures)
        {
#if ASSEMBLER_DEBUG_OUTPUT
            Console.WriteLine($"{lineNumber,8} EQU     : {line,-80}");
#endif

            captures.TryGetValue("label", out string label);
            captures.TryGetValue("arg", out string arg);
            captures.TryGetValue("comment", out string comment);

            return new LineInformation
            {
                LineType = LineType.Equivalence,
                LineNumber = lineNumber,
                CurrentOrg = currentOrgAddress,
                EffectiveAddress = currentAddress,
                Label = label,
                IsEquivalence = true,
                Value = ExtractNumber(arg, 0, 0),
                Comment = ExtractComment(comment),
                Line = line
            };
        }

        private LineInformation ParseCodeLine(int lineNumber, string line, IDictionary<string, string> captures)
        {
#if ASSEMBLER_DEBUG_OUTPUT
            Console.WriteLine($"{lineNumber,8} CODE    : {line,-80}");
#endif

            captures.TryGetValue("label", out string label);
            captures.TryGetValue("opcode", out string opcode);
            captures.TryGetValue("arg", out string arg);
            captures.TryGetValue("comment", out string comment);

            var opCode = Enum.Parse<OpCode>(opcode);

            var argument = ParseAddress(opCode, arg);
            if (symbolTable.TryGetValue(argument.value, out var symbol) == true && symbol.IsEquivalence == true)
            {
                // can immediately resolve this value
                var raw = arg.Replace(argument.value, symbol.UnparsedValue);
                argument = ParseAddress(opCode, raw);
            }

            return new LineInformation
            {
                LineType = LineType.Code,
                LineNumber = lineNumber,
                CurrentOrg = currentOrgAddress,
                EffectiveAddress = currentAddress,
                Label = label,
                OpCode = opCode,
                RawArgument = arg,
                Argument = argument,
                Comment = ExtractComment(comment),
                Line = line
            };
        }

        private LineInformation ParseLabelledLine(int lineNumber, string line, IDictionary<string, string> captures)
        {
#if ASSEMBLER_DEBUG_OUTPUT
            Console.WriteLine($"{lineNumber,8} LABEL   : {line,-80}");
#endif

            captures.TryGetValue("label", out string label);
            captures.TryGetValue("comment", out string comment);

            return new LineInformation
            {
                LineType = LineType.Label,
                LineNumber = lineNumber,
                CurrentOrg = currentOrgAddress,
                EffectiveAddress = currentAddress,
                Label = label,
                Comment = ExtractComment(comment),
                Line = line
            };
        }

        private LineInformation ParseCommentLine(int lineNumber, string line, IDictionary<string, string> captures)
        {
#if ASSEMBLER_DEBUG_OUTPUT
            Console.WriteLine($"{lineNumber,8} COMMENT : {line,-80}");
#endif

            captures.TryGetValue("comment", out string comment);

            return new LineInformation
            {
                LineType = LineType.Comment,
                LineNumber = lineNumber,
                CurrentOrg = currentOrgAddress,
                EffectiveAddress = currentAddress,
                Comment = ExtractComment(comment),
                Line = line
            };
        }

        private static (AddressingMode addressingMode, string value) ParseAddress(OpCode opCode, string addressString)
        {
            if (InstructionInformation.BranchingOperations.Contains(opCode))
            {
                TryMatch(addressString, "", "", out var address);

                return (AddressingMode.Relative, address);
            }

            return addressString switch
            {
                var s when string.IsNullOrEmpty(s) => (AddressingMode.Implicit, null),
                var s when s == "A" => (AddressingMode.Accumulator, null),

                var s when TryMatch(s, "#", "", out var address) => (AddressingMode.Immediate, address),

                // these are LISA-format
                var s when TryMatch(s, "*", "", out var address) => (AddressingMode.Relative, address),

                var s when TryMatch(s, "<", ",X", out var address) => (AddressingMode.ZeroPageXIndexed, address),
                var s when TryMatch(s, "", ",X", out var address) => (AddressingMode.AbsoluteXIndexed, address),
                var s when TryMatch(s, "(", "),Y", out var address) => (AddressingMode.IndirectYIndexed, address),
                var s when TryMatch(s, "(", ",X)", out var address) => (AddressingMode.XIndexedIndirect, address),
                var s when TryMatch(s, "(", ")", out var address) => (AddressingMode.Indirect, address),
                var s when TryMatch(s, "<", ",Y", out var address) => (AddressingMode.ZeroPageYIndexed, address),
                var s when TryMatch(s, "", ",Y", out var address) => (AddressingMode.AbsoluteYIndexed, address),
                var s when TryMatch(s, "<", "", out var address) => (AddressingMode.ZeroPage, address),

                var s when ArgumentParserLongRegex().IsMatch(s) => (AddressingMode.Absolute, ExtractNumber(addressString, 0, 0)),
                var s when ArgumentParserShortRegex().IsMatch(s) => (AddressingMode.ZeroPage, ExtractNumber(addressString, 0, 0)),

                _ => (AddressingMode.Unknown, null)
            };
        }

        private static bool TryMatch(string addressString, string prefix, string suffix, out string address)
        {
            address = null;

            if (addressString.StartsWith(prefix, StringComparison.InvariantCulture) &&
                addressString.EndsWith(suffix, StringComparison.InvariantCulture))
            {
                address = ExtractNumber(addressString, prefix.Length, suffix.Length);
                return true;
            }

            return false;
        }

        private static string ExtractNumber(string addressString, int startSkip, int endSkip)
        {
            return addressString.Substring(startSkip, addressString.Length - (startSkip + endSkip));
        }

        private static string ExtractComment(string comment)
        {
            if (comment == null)
            {
                return null;
            }

            comment = comment.Trim();

            if (comment.StartsWith("; ") == true)
            {
                comment = comment[2..];
            }

            return comment;
        }

        private void UpdateSymbolTable(LineInformation node)
        {
            if (string.IsNullOrEmpty(node.Label) == false)
            {
                if (symbolTable.TryGetValue(node.Label, out var symbol) == true)
                {
                    throw new SymbolRedefinedException(symbol, node.LineNumber);
                }

                ushort value = node.LineType switch
                {
                    LineType.Data => node.EffectiveAddress,
                    LineType.Label => node.EffectiveAddress,
                    LineType.Code => node.EffectiveAddress,
                    LineType.Equivalence => ResolveNumber(node.Value),
                    _ => 0
                };

                var symbolType = node.LineType switch
                {
                    LineType.Code => InstructionInformation.BranchingOperations.Contains(node.OpCode) ? SymbolType.RelativeAddress : SymbolType.AbsoluteAddress,
                    LineType.Data => SymbolType.AbsoluteAddress,
                    LineType.Label => SymbolType.AbsoluteAddress,
                    LineType.Equivalence => value <= 255 ? SymbolType.DefineByte : SymbolType.DefineWord,
                    _ => SymbolType.Indifferent
                };

                symbolTable.Add(
                    node.Label,
                    new Symbol
                    {
                        LineNumber = node.LineNumber,
                        SymbolType = symbolType,
                        Label = node.Label,
                        UnparsedValue = value <= 255 ? "$" + value.ToString("X2") : "$" + value.ToString("X4"),
                        Value = value,
                        Size = node.EffectiveSize,
                    });
            }
        }

        private ushort ResolveNumber(string number)
        {
            if (number.StartsWith('$') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.HexNumber);
            }
            else if (number.StartsWith("0x") == true)
            {
                return ushort.Parse(number[2..], NumberStyles.HexNumber);
            }
            else if (number.StartsWith('%') == true)
            {
                return ushort.Parse(number[1..], NumberStyles.BinaryNumber);
            }
            else if (number.StartsWith("0b") == true)
            {
                return ushort.Parse(number[2..], NumberStyles.BinaryNumber);
            }
            else
            {
                return ushort.Parse(number, NumberStyles.Integer);
            }
        }
    }

    public static class RegexExtensions
    {
        public static Dictionary<string, string> MatchNamedCaptures(this Regex regex, string input)
        {
            var namedCaptureDictionary = new Dictionary<string, string>();
            var groups = regex.Match(input).Groups;
            var groupNames = regex.GetGroupNames();

            foreach (string groupName in groupNames)
            {
                if (groups[groupName].Captures.Count > 0)
                {
                    namedCaptureDictionary.Add(groupName, groups[groupName].Value);
                }
            }

            return namedCaptureDictionary;
        }
    }
}