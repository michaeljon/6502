using InnoWerks.Processors;

namespace InnoWerks.Assemblers.Tests
{
    [TestClass]
    public class ArgumentParsingTests
    {
        [TestMethod]
        public void CanReadImplied()
        {
            foreach (var opCode in InstructionInformation.ImpliedOperations)
            {
                var assembler = new Assembler(
                    [
                        $"LABEL {opCode}        ; implied operation, no value"
                    ],
                    0x0000
                );
                assembler.Assemble();

                var lineInformation = assembler.Program[0];

                Assert.AreEqual(opCode, lineInformation.OpCode);
                Assert.AreEqual(AddressingMode.Implied, lineInformation.AddressingMode);
                Assert.IsNull(lineInformation.Value);

                var expectedInstructionCode = InstructionInformation.Instructions[(opCode, lineInformation.AddressingMode)].code;
                CollectionAssert.AreEqual(new byte[] { expectedInstructionCode }, assembler.ObjectCode);
            }
        }

        [TestMethod]
        public void CanReadImplicitAccumulator()
        {
            foreach (var opCode in InstructionInformation.AccumulatorOperations)
            {
                var assembler = new Assembler(
                    [
                        $"LABEL {opCode}        ; accumulator operation, no value"
                    ],
                    0x0000
                );
                assembler.Assemble();

                var lineInformation = assembler.Program[0];

                Assert.AreEqual(opCode, lineInformation.OpCode);
                Assert.AreEqual(AddressingMode.Accumulator, lineInformation.AddressingMode);
                Assert.IsNull(lineInformation.Value);

                var expectedInstructionCode = InstructionInformation.Instructions[(opCode, lineInformation.AddressingMode)].code;
                CollectionAssert.AreEqual(new byte[] { expectedInstructionCode }, assembler.ObjectCode);
            }
        }

        [TestMethod]
        public void CanReadExplicitAccumulator()
        {
            foreach (var opCode in InstructionInformation.AccumulatorOperations)
            {
                var assembler = new Assembler(
                    [
                        $"LABEL {opCode} A      ; accumulator operation, no value"
                    ],
                    0x0000
                );
                assembler.Assemble();

                var lineInformation = assembler.Program[0];

                Assert.AreEqual(opCode, lineInformation.OpCode);
                Assert.AreEqual(AddressingMode.Accumulator, lineInformation.AddressingMode);
                Assert.IsNull(lineInformation.Value);

                var expectedInstructionCode = InstructionInformation.Instructions[(opCode, lineInformation.AddressingMode)].code;
                CollectionAssert.AreEqual(new byte[] { expectedInstructionCode }, assembler.ObjectCode);
            }
        }

        [TestMethod]
        public void CanReadStack()
        {
            foreach (var opCode in InstructionInformation.StackOperations)
            {
                var assembler = new Assembler(
                    [
                        $"LABEL {opCode}        ; stack operation, no value"
                    ],
                    0x0000
                );
                assembler.Assemble();

                var lineInformation = assembler.Program[0];

                Assert.AreEqual(opCode, lineInformation.OpCode);
                Assert.AreEqual(AddressingMode.Stack, lineInformation.AddressingMode);
                Assert.IsNull(lineInformation.Value);

                var expectedInstructionCode = InstructionInformation.Instructions[(opCode, lineInformation.AddressingMode)].code;
                CollectionAssert.AreEqual(new byte[] { expectedInstructionCode }, assembler.ObjectCode);
            }
        }
    }
}
