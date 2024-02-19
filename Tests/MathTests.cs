using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class MathTests : TestBase
    {
        [TestMethod]
        public void BruceClark6502()
        {
            const string Filename = "Modules/BcdTest/BruceClark6502";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x0b;

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
                (cpu) => DummyLoggerCallback(cpu, memory))
            {
                SkipTimingWait = true
            };

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            if (memory[ERROR] != 0x00)
            {
                PrintPage(memory, 0x00);
                cpu.PrintStatus();
            }

            Assert.AreEqual(0x00, memory[ERROR]);
        }

        [TestMethod]
        public void BruceClark65C02()
        {
            const string Filename = "Modules/BcdTest/BruceClark65C02";
            const ushort Origin = 0x8000;
            const ushort InitializationVector = 0x8000;

            const ushort ERROR = 0x0b;

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
                (cpu) => DummyLoggerCallback(cpu, memory))
            {
                SkipTimingWait = true
            };

            cpu.Reset();

            // run
            cpu.Run(stopOnBreak: true, writeInstructions: false);

            if (memory[ERROR] != 0x00)
            {
                PrintPage(memory, 0x00);
                cpu.PrintStatus();
            }

            Assert.AreEqual(0x00, memory[ERROR]);
        }
    }
}
