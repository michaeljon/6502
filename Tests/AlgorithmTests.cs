using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class AlgorithmTests : TestBase
    {
        [TestMethod]
        public void BinarySearchPositiveCase()
        {
            const string Filename = "Modules/BinarySearch/binarySearch";
            const ushort Origin = 0x6000;
            const ushort InitializationVector = 0x605c;

            byte[] memory = new byte[1024 * 64];

            using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                long length = fs.Length;
                fs.Read(memory, Origin, (int)length);
            }

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            Assert.AreEqual(0x04, cpu.A);
            Assert.IsFalse(cpu.Carry);
        }

        [TestMethod]
        public void BinarySearchNegativeCase()
        {
            const string Filename = "Modules/BinarySearch/binarySearch";
            const ushort Origin = 0x6000;
            const ushort InitializationVector = 0x606f;

            byte[] memory = new byte[1024 * 64];

            using (var fs = new FileStream(Filename, FileMode.Open, FileAccess.Read))
            {
                long length = fs.Length;
                fs.Read(memory, Origin, (int)length);
            }

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) => LoggerCallback(cpu, memory));

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            Assert.IsTrue(cpu.Carry);
        }
    }
}
