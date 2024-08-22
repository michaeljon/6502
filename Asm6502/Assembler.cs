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
        #region Argument parser regular expressions
        [GeneratedRegex(@"^A$")]
        private static partial Regex AccumulatorRegex();

        [GeneratedRegex(@"^#(?<prefix>[\$@%])?(?<arg>([0-9A-F]+|\w+))$")]
        private static partial Regex ImmediateRegex();

        [GeneratedRegex(@"^(?<arg>(\w+))$")]
        private static partial Regex RelativeRegex();

        [GeneratedRegex(@"^(?<arg>(\$[0-9A-F]{4}|\w+))$")]
        private static partial Regex AbsoluteAddressRegex();

        [GeneratedRegex(@"^(?<arg>(\$[0-9A-F]{4}|\w+)),(?<reg>[XY])$")]
        private static partial Regex AbsoluteIndexedRegex();

        [GeneratedRegex(@"^\((?<arg>(\$[0-9A-F]{4}|\w+))\)$")]
        private static partial Regex IndirectRegex();

        [GeneratedRegex(@"^\((?<arg>(\$[0-9A-F]{4}|\w+)),X\)$")]
        private static partial Regex IndexedIndirectRegex();

        [GeneratedRegex(@"^(?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+))$")]
        private static partial Regex ZeroPageDirectRegex();

        [GeneratedRegex(@"^(?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+)),(?<reg>[XY])$")]
        private static partial Regex ZeroPageIndexedRegex();

        [GeneratedRegex(@"^\((?<arg>(\$[0-9A-F]{2}|\w+))\)$")]
        private static partial Regex ZeroPageIndirectRegex();

        [GeneratedRegex(@"^\((?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+)),X\)$")]
        private static partial Regex ZeroPageIndexedDirectRegex();

        [GeneratedRegex(@"^\((?<arg>(\$00[0-9A-F]{2}|\$[0-9A-F]{2}|\w+))\),Y$")]
        private static partial Regex ZeroPagePostIndexedDirectRegex();
        #endregion

        #region Argument parser regex instances
        private readonly Regex accumulatorRegex = AccumulatorRegex();
        private readonly Regex immediateRegex = ImmediateRegex();
        private readonly Regex relativeRegex = RelativeRegex();
        private readonly Regex absoluteAddressRegex = AbsoluteAddressRegex();
        private readonly Regex absoluteIndexedRegex = AbsoluteIndexedRegex();
        private readonly Regex indirectRegex = IndirectRegex();
        private readonly Regex indexedIndirectRegex = IndexedIndirectRegex();

        private readonly Regex zeroPageDirectRegex = ZeroPageDirectRegex();
        private readonly Regex zeroPageIndexedRegex = ZeroPageIndexedRegex();
        private readonly Regex zeroPageIndirectRegex = ZeroPageIndirectRegex();
        private readonly Regex zeroPageIndexedDirectRegex = ZeroPageIndexedDirectRegex();
        private readonly Regex zeroPagePostIndexedDirectRegex = ZeroPagePostIndexedDirectRegex();
        #endregion

        #region Instruction parser regular expressions
        // instruction expressions
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
        #endregion

        #region Instruction parser regex instances
        private readonly Regex codeLine = CodeLineRegex();
        private readonly Regex commentLine = CommentLineRegex();
        private readonly Regex dataLine = DataLineRegex();
        private readonly Regex directiveLine = DirectiveLineRegex();
        private readonly Regex labelledLine = LabelledLineRegex();
        private readonly Regex equivalenceLine = EquivalenceLineRegex();
        #endregion

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
                if (string.IsNullOrEmpty(instruction.ExtractedArgument) == false && symbolTable.TryGetValue(instruction.ExtractedArgument, out var symbol) == true)
                {
                    var raw = instruction.RawArgument.Replace(instruction.ExtractedArgument, symbol.UnparsedValue);

                    if (symbol.IsEquivalence == false)
                    {
                        // todo - if this is a branch, then we need to resolve it differently
                        if (InstructionInformation.BranchingOperations.Contains(instruction.OpCode))
                        {
                            var offset = (symbol.Value > instruction.EffectiveAddress) ?
                                (byte)((symbol.Value - (instruction.EffectiveAddress + instruction.EffectiveSize)) & 0xff) :
                                (byte)((symbol.Value - (instruction.EffectiveAddress + instruction.EffectiveSize)));

                            instruction.RawArgumentWithReplacement = instruction.RawArgument.Replace(instruction.ExtractedArgument, "{" + offset + "}");
                            instruction.Value = "$" + offset.ToString("X2");
                        }
                        else
                        {
                            (instruction.AddressingMode, instruction.ExtractedArgumentValue) = ParseAddress(instruction.OpCode, raw);
                            instruction.RawArgumentWithReplacement = instruction.RawArgument.Replace(instruction.ExtractedArgument, "{" + symbol.UnparsedValue + "}");
                            instruction.Value = ExtractNumber(symbol.UnparsedValue, 0, 0);
                        }
                    }
                    else
                    {
                        (instruction.AddressingMode, instruction.ExtractedArgumentValue) = ParseAddress(instruction.OpCode, raw);
                        instruction.RawArgumentWithReplacement = instruction.RawArgument.Replace(instruction.ExtractedArgument, "{" + symbol.UnparsedValue + "}");
                        instruction.Value = ExtractNumber(symbol.UnparsedValue, 0, 0);
                    }
                }
                else
                {
                    instruction.Value = instruction.ExtractedArgumentValue;
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

            return bytes[..pc];
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

            // let's see if we can take it apart
            var mathRegex = new Regex(@"(?<arg>(\w+))(?<operator>[-+])(?<offset>[0-9]+)");

            var initialArg = arg;
            var usesArgumentMath = false;
            var applicableOffset = 0;

            if (mathRegex.IsMatch(initialArg))
            {
                var parts = mathRegex.MatchNamedCaptures(initialArg);

                arg = parts["arg"];

                usesArgumentMath = true;
                applicableOffset = int.Parse($"{parts["operator"]}{parts["offset"]}");
            }

            var argument = ParseAddress(opCode, arg);
            var extractedArgument = argument.value;

            if (string.IsNullOrEmpty(argument.value) == false)
            {
                if (symbolTable.TryGetValue(argument.value, out var symbol) == true && symbol.IsEquivalence == true)
                {
                    // can immediately resolve this value
                    var raw = arg.Replace(argument.value, symbol.UnparsedValue);
                    argument = ParseAddress(opCode, raw);
                }
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
                RawArgumentWithReplacement = arg,
                ExtractedArgument = extractedArgument,
                ExtractedArgumentValue = argument.value,
                AddressingMode = argument.addressingMode,
                Comment = ExtractComment(comment),

                // todo:
                UsesArgumentMath = usesArgumentMath,
                ApplicableOffset = applicableOffset,

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

        private (AddressingMode addressingMode, string value) ParseAddress(OpCode opCode, string addressString)
        {
            if (InstructionInformation.BranchingOperations.Contains(opCode))
            {
                var captures = relativeRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.Relative, captures["arg"]);
            }

            if (string.IsNullOrEmpty(addressString) == true)
            {
                return (AddressingMode.Implied, null);
            }

            if (accumulatorRegex.IsMatch(addressString) == true)
            {
                return (AddressingMode.Accumulator, null);
            }

            if (immediateRegex.IsMatch(addressString) == true)
            {
                var captures = immediateRegex.MatchNamedCaptures(addressString);

                captures.TryGetValue("prefix", out string prefix);
                captures.TryGetValue("arg", out string arg);

                return (AddressingMode.Immediate, $"{prefix ?? ""}{arg}");
            }

            if (absoluteAddressRegex.IsMatch(addressString) == true)
            {
                var captures = absoluteAddressRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.Absolute, captures["arg"]);
            }

            if (absoluteIndexedRegex.IsMatch(addressString) == true)
            {
                var captures = absoluteIndexedRegex.MatchNamedCaptures(addressString);
                if (captures["reg"] == "X")
                {
                    return (AddressingMode.AbsoluteXIndexed, captures["arg"]);
                }
                else if (captures["reg"] == "Y")
                {
                    return (AddressingMode.AbsoluteYIndexed, captures["arg"]);
                }
                else
                {
                    throw new IndexOutOfRangeException($"{captures["reg"]} must be one of X or Y");
                }
            }

            if (zeroPageDirectRegex.IsMatch(addressString) == true)
            {
                var captures = zeroPageDirectRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.ZeroPage, captures["arg"]);
            }

            if (zeroPageIndirectRegex.IsMatch(addressString) == true)
            {
                var captures = zeroPageIndirectRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.ZeroPageIndirect, captures["arg"]);
            }

            if (indexedIndirectRegex.IsMatch(addressString) == true)
            {
                var captures = indexedIndirectRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.AbsoluteIndirect, captures["arg"]);
            }

            if (zeroPageIndexedDirectRegex.IsMatch(addressString) == true)
            {
                var captures = zeroPageIndexedDirectRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.XIndexedIndirect, captures["arg"]);
            }

            if (zeroPageIndexedRegex.IsMatch(addressString) == true)
            {
                var captures = zeroPageIndexedRegex.MatchNamedCaptures(addressString);
                if (captures["reg"] == "X")
                {
                    return (AddressingMode.ZeroPageXIndexed, captures["arg"]);
                }
                else if (captures["reg"] == "Y")
                {
                    if (opCode != OpCode.STX && opCode != OpCode.LDX)
                    {
                        // this can't be right, it's really an AbsoluteIndexed
                        return (AddressingMode.AbsoluteYIndexed, captures["arg"]);
                    }
                    else
                    {
                        // because only opcodes 96 (STX) and b6 (LDX) are available as zp,y
                        return (AddressingMode.ZeroPageYIndexed, captures["arg"]);
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException($"{captures["reg"]} must be one of X or Y");
                }
            }

            if (zeroPagePostIndexedDirectRegex.IsMatch(addressString) == true)
            {
                var captures = zeroPagePostIndexedDirectRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.IndirectYIndexed, captures["arg"]);
            }

            if (indirectRegex.IsMatch(addressString) == true)
            {
                var captures = indirectRegex.MatchNamedCaptures(addressString);
                return (AddressingMode.AbsoluteIndexedIndirect, captures["arg"]);
            }

            return (AddressingMode.Unknown, null);
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