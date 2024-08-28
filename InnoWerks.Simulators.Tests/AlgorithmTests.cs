using System;
using System.IO;
using InnoWerks.Processors;
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
            byte[] program = File.ReadAllBytes(Filename);

            Array.Copy(program, 0, memory, Origin, program.Length);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) =>
                {
                    // Console.WriteLine($"value  {memory[0x6059]:X2} ");
                    // Console.WriteLine($"ubnd   {memory[0x605A]:X2} ");
                    // Console.WriteLine($"lbnd   {memory[0x605B]:X2} ");
                    // Console.WriteLine($"aryadr {memory[0xef]:X2}{memory[0xee]:X2} ");
                });

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            Assert.AreEqual(0x04, cpu.Registers.A);
            Assert.IsFalse(cpu.Registers.Carry);
        }

        [TestMethod]
        public void BinarySearchNegativeCase()
        {
            const string Filename = "Modules/BinarySearch/binarySearch";
            const ushort Origin = 0x6000;
            const ushort InitializationVector = 0x606f;

            byte[] memory = new byte[1024 * 64];
            byte[] program = File.ReadAllBytes(Filename);

            Array.Copy(program, 0, memory, Origin, program.Length);

            // power up initialization
            memory[Cpu.RstVectorH] = (InitializationVector & 0xff00) >> 8;
            memory[Cpu.RstVectorL] = InitializationVector & 0xff;

            var cpu = new Cpu(
                CpuClass.WDC65C02,
                (addr) => memory[addr],
                (addr, b) => memory[addr] = b,
                (cpu) =>
                {
                    Console.WriteLine($"value  {memory[0x6059]:X2} ");
                    Console.WriteLine($"ubnd   {memory[0x605A]:X2} ");
                    Console.WriteLine($"lbnd   {memory[0x605B]:X2} ");
                    Console.WriteLine($"aryadr {memory[0xef]:X2}{memory[0xee]:X2} ");
                });

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            Assert.IsTrue(cpu.Registers.Carry);
        }
    }
}
