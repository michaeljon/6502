// #define VERBOSE_BATCH_OUTPUT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1822

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class Harte6502 : HarteBase
    {
        private static readonly bool[] ignored = LoadIgnored(CpuClass.WDC6502);

        protected override string BasePath => Environment.ExpandEnvironmentVariables("%HOME%/src/6502/working/65x02/6502/v1");

        [Ignore]
        [TestMethod]
        public void RunAll6502Tests()
        {
            List<string> results = [];

            var files = Directory
                .GetFiles(BasePath, "*.json")
                .OrderBy(f => f);

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    if (fs.Length == 0)
                    {
                        continue;
                    }

                    var index = byte.Parse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    if (ignored[index] == false)
                    {
                        foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, SerializerOptions))
                        {
                            RunIndividualTest(CpuClass.WDC6502, test, results);
                        }
                    }
                }
            }

#if VERBOSE_BATCH_OUTPUT
            foreach (var result in results)
            {
                TestContext.WriteLine(result);
            }
#endif
            Assert.AreEqual(0, results.Count, $"Failed some tests");
        }

        [Ignore]
        [TestMethod]
        public void RunNamed6502Test()
        {
            var testName = "6c ff 70";
            List<string> results = [];

            var batch = testName.Split(' ')[0];
            var file = $"{BasePath}/{batch}.json";

            var ocd = CpuInstructions.OpCode6502[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)];

            TestContext.WriteLine($"Running test {testName}");
            TestContext.WriteLine($"OpCode: ${batch} {ocd.OpCode} {ocd.AddressingMode}");
            TestContext.WriteLine("");

            using (var fs = File.OpenRead(file))
            {
                var tests = JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, SerializerOptions);
                var test = tests.Find(t => t.Name == testName);

                if (test == null)
                {
                    Assert.Inconclusive($"Unable to locate test {testName}");
                    return;
                }

                var json = JsonSerializer.Serialize(test.Clone(), SerializerOptions);
                File.WriteAllText("foo.json", json);

                RunIndividualTest(CpuClass.WDC6502, test, results);
            }

            foreach (var result in results)
            {
                TestContext.WriteLine(result);
            }

            Assert.AreEqual(0, results.Count, $"Failed some tests");
        }

        [TestMethod]
        public void RunIndividual6502Test00()
        {
            if (ignored[byte.Parse("00", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 00 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "00");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test01()
        {
            if (ignored[byte.Parse("01", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 01 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "01");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test02()
        {
            if (ignored[byte.Parse("02", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 02 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "02");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test03()
        {
            if (ignored[byte.Parse("03", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 03 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "03");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test04()
        {
            if (ignored[byte.Parse("04", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 04 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "04");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test05()
        {
            if (ignored[byte.Parse("05", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 05 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "05");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test06()
        {
            if (ignored[byte.Parse("06", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 06 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "06");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test07()
        {
            if (ignored[byte.Parse("07", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 07 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "07");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test08()
        {
            if (ignored[byte.Parse("08", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 08 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "08");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test09()
        {
            if (ignored[byte.Parse("09", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 09 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "09");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test0A()
        {
            if (ignored[byte.Parse("0a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 0a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "0a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test0B()
        {
            if (ignored[byte.Parse("0b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 0b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "0b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test0C()
        {
            if (ignored[byte.Parse("0c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 0c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "0c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test0D()
        {
            if (ignored[byte.Parse("0d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 0d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "0d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test0E()
        {
            if (ignored[byte.Parse("0e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 0e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "0e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test0F()
        {
            if (ignored[byte.Parse("0f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 0f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "0f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test10()
        {
            if (ignored[byte.Parse("10", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 10 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "10");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test11()
        {
            if (ignored[byte.Parse("11", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 11 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "11");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test12()
        {
            if (ignored[byte.Parse("12", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 12 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "12");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test13()
        {
            if (ignored[byte.Parse("13", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 13 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "13");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test14()
        {
            if (ignored[byte.Parse("14", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 14 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "14");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test15()
        {
            if (ignored[byte.Parse("15", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 15 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "15");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test16()
        {
            if (ignored[byte.Parse("16", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 16 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "16");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test17()
        {
            if (ignored[byte.Parse("17", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 17 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "17");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test18()
        {
            if (ignored[byte.Parse("18", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 18 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "18");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test19()
        {
            if (ignored[byte.Parse("19", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 19 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "19");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test1A()
        {
            if (ignored[byte.Parse("1a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 1a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "1a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test1B()
        {
            if (ignored[byte.Parse("1b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 1b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "1b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test1C()
        {
            if (ignored[byte.Parse("1c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 1c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "1c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test1D()
        {
            if (ignored[byte.Parse("1d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 1d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "1d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test1E()
        {
            if (ignored[byte.Parse("1e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 1e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "1e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test1F()
        {
            if (ignored[byte.Parse("1f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 1f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "1f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test20()
        {
            if (ignored[byte.Parse("20", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 20 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "20");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test21()
        {
            if (ignored[byte.Parse("21", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 21 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "21");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test22()
        {
            if (ignored[byte.Parse("22", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 22 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "22");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test23()
        {
            if (ignored[byte.Parse("23", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 23 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "23");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test24()
        {
            if (ignored[byte.Parse("24", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 24 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "24");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test25()
        {
            if (ignored[byte.Parse("25", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 25 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "25");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test26()
        {
            if (ignored[byte.Parse("26", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 26 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "26");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test27()
        {
            if (ignored[byte.Parse("27", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 27 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "27");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test28()
        {
            if (ignored[byte.Parse("28", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 28 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "28");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test29()
        {
            if (ignored[byte.Parse("29", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 29 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "29");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test2A()
        {
            if (ignored[byte.Parse("2a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 2a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "2a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test2B()
        {
            if (ignored[byte.Parse("2b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 2b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "2b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test2C()
        {
            if (ignored[byte.Parse("2c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 2c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "2c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test2D()
        {
            if (ignored[byte.Parse("2d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 2d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "2d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test2E()
        {
            if (ignored[byte.Parse("2e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 2e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "2e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test2F()
        {
            if (ignored[byte.Parse("2f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 2f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "2f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test30()
        {
            if (ignored[byte.Parse("30", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 30 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "30");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test31()
        {
            if (ignored[byte.Parse("31", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 31 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "31");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test32()
        {
            if (ignored[byte.Parse("32", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 32 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "32");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test33()
        {
            if (ignored[byte.Parse("33", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 33 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "33");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test34()
        {
            if (ignored[byte.Parse("34", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 34 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "34");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test35()
        {
            if (ignored[byte.Parse("35", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 35 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "35");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test36()
        {
            if (ignored[byte.Parse("36", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 36 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "36");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test37()
        {
            if (ignored[byte.Parse("37", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 37 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "37");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test38()
        {
            if (ignored[byte.Parse("38", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 38 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "38");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test39()
        {
            if (ignored[byte.Parse("39", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 39 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "39");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test3A()
        {
            if (ignored[byte.Parse("3a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 3a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "3a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test3B()
        {
            if (ignored[byte.Parse("3b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 3b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "3b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test3C()
        {
            if (ignored[byte.Parse("3c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 3c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "3c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test3D()
        {
            if (ignored[byte.Parse("3d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 3d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "3d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test3E()
        {
            if (ignored[byte.Parse("3e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 3e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "3e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test3F()
        {
            if (ignored[byte.Parse("3f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 3f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "3f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test40()
        {
            if (ignored[byte.Parse("40", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 40 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "40");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test41()
        {
            if (ignored[byte.Parse("41", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 41 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "41");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test42()
        {
            if (ignored[byte.Parse("42", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 42 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "42");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test43()
        {
            if (ignored[byte.Parse("43", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 43 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "43");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test44()
        {
            if (ignored[byte.Parse("44", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 44 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "44");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test45()
        {
            if (ignored[byte.Parse("45", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 45 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "45");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test46()
        {
            if (ignored[byte.Parse("46", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 46 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "46");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test47()
        {
            if (ignored[byte.Parse("47", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 47 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "47");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test48()
        {
            if (ignored[byte.Parse("48", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 48 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "48");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test49()
        {
            if (ignored[byte.Parse("49", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 49 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "49");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test4A()
        {
            if (ignored[byte.Parse("4a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 4a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "4a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test4B()
        {
            if (ignored[byte.Parse("4b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 4b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "4b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test4C()
        {
            if (ignored[byte.Parse("4c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 4c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "4c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test4D()
        {
            if (ignored[byte.Parse("4d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 4d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "4d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test4E()
        {
            if (ignored[byte.Parse("4e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 4e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "4e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test4F()
        {
            if (ignored[byte.Parse("4f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 4f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "4f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test50()
        {
            if (ignored[byte.Parse("50", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 50 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "50");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test51()
        {
            if (ignored[byte.Parse("51", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 51 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "51");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test52()
        {
            if (ignored[byte.Parse("52", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 52 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "52");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test53()
        {
            if (ignored[byte.Parse("53", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 53 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "53");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test54()
        {
            if (ignored[byte.Parse("54", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 54 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "54");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test55()
        {
            if (ignored[byte.Parse("55", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 55 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "55");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test56()
        {
            if (ignored[byte.Parse("56", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 56 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "56");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test57()
        {
            if (ignored[byte.Parse("57", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 57 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "57");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test58()
        {
            if (ignored[byte.Parse("58", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 58 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "58");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test59()
        {
            if (ignored[byte.Parse("59", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 59 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "59");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test5A()
        {
            if (ignored[byte.Parse("5a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 5a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "5a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test5B()
        {
            if (ignored[byte.Parse("5b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 5b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "5b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test5C()
        {
            if (ignored[byte.Parse("5c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 5c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "5c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test5D()
        {
            if (ignored[byte.Parse("5d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 5d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "5d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test5E()
        {
            if (ignored[byte.Parse("5e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 5e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "5e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test5F()
        {
            if (ignored[byte.Parse("5f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 5f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "5f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test60()
        {
            if (ignored[byte.Parse("60", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 60 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "60");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test61()
        {
            if (ignored[byte.Parse("61", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 61 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "61");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test62()
        {
            if (ignored[byte.Parse("62", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 62 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "62");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test63()
        {
            if (ignored[byte.Parse("63", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 63 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "63");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test64()
        {
            if (ignored[byte.Parse("64", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 64 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "64");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test65()
        {
            if (ignored[byte.Parse("65", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 65 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "65");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test66()
        {
            if (ignored[byte.Parse("66", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 66 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "66");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test67()
        {
            if (ignored[byte.Parse("67", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 67 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "67");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test68()
        {
            if (ignored[byte.Parse("68", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 68 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "68");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test69()
        {
            if (ignored[byte.Parse("69", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 69 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "69");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test6A()
        {
            if (ignored[byte.Parse("6a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 6a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "6a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test6B()
        {
            if (ignored[byte.Parse("6b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 6b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "6b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test6C()
        {
            if (ignored[byte.Parse("6c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 6c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "6c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test6D()
        {
            if (ignored[byte.Parse("6d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 6d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "6d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test6E()
        {
            if (ignored[byte.Parse("6e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 6e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "6e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test6F()
        {
            if (ignored[byte.Parse("6f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 6f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "6f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test70()
        {
            if (ignored[byte.Parse("70", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 70 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "70");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test71()
        {
            if (ignored[byte.Parse("71", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 71 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "71");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test72()
        {
            if (ignored[byte.Parse("72", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 72 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "72");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test73()
        {
            if (ignored[byte.Parse("73", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 73 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "73");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test74()
        {
            if (ignored[byte.Parse("74", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 74 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "74");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test75()
        {
            if (ignored[byte.Parse("75", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 75 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "75");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test76()
        {
            if (ignored[byte.Parse("76", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 76 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "76");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test77()
        {
            if (ignored[byte.Parse("77", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 77 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "77");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test78()
        {
            if (ignored[byte.Parse("78", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 78 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "78");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test79()
        {
            if (ignored[byte.Parse("79", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 79 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "79");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test7A()
        {
            if (ignored[byte.Parse("7a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 7a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "7a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test7B()
        {
            if (ignored[byte.Parse("7b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 7b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "7b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test7C()
        {
            if (ignored[byte.Parse("7c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 7c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "7c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test7D()
        {
            if (ignored[byte.Parse("7d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 7d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "7d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test7E()
        {
            if (ignored[byte.Parse("7e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 7e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "7e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test7F()
        {
            if (ignored[byte.Parse("7f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 7f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "7f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test80()
        {
            if (ignored[byte.Parse("80", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 80 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "80");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test81()
        {
            if (ignored[byte.Parse("81", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 81 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "81");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test82()
        {
            if (ignored[byte.Parse("82", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 82 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "82");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test83()
        {
            if (ignored[byte.Parse("83", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 83 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "83");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test84()
        {
            if (ignored[byte.Parse("84", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 84 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "84");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test85()
        {
            if (ignored[byte.Parse("85", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 85 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "85");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test86()
        {
            if (ignored[byte.Parse("86", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 86 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "86");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test87()
        {
            if (ignored[byte.Parse("87", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 87 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "87");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test88()
        {
            if (ignored[byte.Parse("88", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 88 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "88");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test89()
        {
            if (ignored[byte.Parse("89", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 89 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "89");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test8A()
        {
            if (ignored[byte.Parse("8a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 8a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "8a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test8B()
        {
            if (ignored[byte.Parse("8b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 8b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "8b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test8C()
        {
            if (ignored[byte.Parse("8c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 8c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "8c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test8D()
        {
            if (ignored[byte.Parse("8d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 8d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "8d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test8E()
        {
            if (ignored[byte.Parse("8e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 8e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "8e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test8F()
        {
            if (ignored[byte.Parse("8f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 8f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "8f");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test90()
        {
            if (ignored[byte.Parse("90", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 90 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "90");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test91()
        {
            if (ignored[byte.Parse("91", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 91 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "91");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test92()
        {
            if (ignored[byte.Parse("92", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 92 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "92");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test93()
        {
            if (ignored[byte.Parse("93", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 93 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "93");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test94()
        {
            if (ignored[byte.Parse("94", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 94 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "94");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test95()
        {
            if (ignored[byte.Parse("95", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 95 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "95");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test96()
        {
            if (ignored[byte.Parse("96", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 96 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "96");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test97()
        {
            if (ignored[byte.Parse("97", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 97 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "97");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test98()
        {
            if (ignored[byte.Parse("98", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 98 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "98");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test99()
        {
            if (ignored[byte.Parse("99", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 99 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "99");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test9A()
        {
            if (ignored[byte.Parse("9a", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 9a is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "9a");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test9B()
        {
            if (ignored[byte.Parse("9b", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 9b is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "9b");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test9C()
        {
            if (ignored[byte.Parse("9c", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 9c is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "9c");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test9D()
        {
            if (ignored[byte.Parse("9d", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 9d is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "9d");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test9E()
        {
            if (ignored[byte.Parse("9e", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 9e is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "9e");
            }
        }

        [TestMethod]
        public void RunIndividual6502Test9F()
        {
            if (ignored[byte.Parse("9f", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test 9f is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "9f");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA0()
        {
            if (ignored[byte.Parse("a0", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a0 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a0");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA1()
        {
            if (ignored[byte.Parse("a1", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a1 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a1");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA2()
        {
            if (ignored[byte.Parse("a2", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a2 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a2");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA3()
        {
            if (ignored[byte.Parse("a3", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a3 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a3");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA4()
        {
            if (ignored[byte.Parse("a4", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a4 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a4");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA5()
        {
            if (ignored[byte.Parse("a5", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a5 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a5");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA6()
        {
            if (ignored[byte.Parse("a6", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a6 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a6");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA7()
        {
            if (ignored[byte.Parse("a7", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a7 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a7");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA8()
        {
            if (ignored[byte.Parse("a8", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a8 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a8");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestA9()
        {
            if (ignored[byte.Parse("a9", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test a9 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "a9");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestAA()
        {
            if (ignored[byte.Parse("aa", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test aa is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "aa");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestAB()
        {
            if (ignored[byte.Parse("ab", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ab is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ab");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestAC()
        {
            if (ignored[byte.Parse("ac", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ac is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ac");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestAD()
        {
            if (ignored[byte.Parse("ad", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ad is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ad");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestAE()
        {
            if (ignored[byte.Parse("ae", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ae is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ae");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestAF()
        {
            if (ignored[byte.Parse("af", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test af is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "af");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB0()
        {
            if (ignored[byte.Parse("b0", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b0 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b0");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB1()
        {
            if (ignored[byte.Parse("b1", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b1 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b1");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB2()
        {
            if (ignored[byte.Parse("b2", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b2 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b2");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB3()
        {
            if (ignored[byte.Parse("b3", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b3 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b3");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB4()
        {
            if (ignored[byte.Parse("b4", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b4 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b4");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB5()
        {
            if (ignored[byte.Parse("b5", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b5 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b5");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB6()
        {
            if (ignored[byte.Parse("b6", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b6 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b6");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB7()
        {
            if (ignored[byte.Parse("b7", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b7 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b7");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB8()
        {
            if (ignored[byte.Parse("b8", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b8 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b8");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestB9()
        {
            if (ignored[byte.Parse("b9", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test b9 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "b9");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestBA()
        {
            if (ignored[byte.Parse("ba", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ba is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ba");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestBB()
        {
            if (ignored[byte.Parse("bb", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test bb is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "bb");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestBC()
        {
            if (ignored[byte.Parse("bc", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test bc is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "bc");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestBD()
        {
            if (ignored[byte.Parse("bd", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test bd is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "bd");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestBE()
        {
            if (ignored[byte.Parse("be", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test be is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "be");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestBF()
        {
            if (ignored[byte.Parse("bf", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test bf is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "bf");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC0()
        {
            if (ignored[byte.Parse("c0", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c0 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c0");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC1()
        {
            if (ignored[byte.Parse("c1", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c1 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c1");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC2()
        {
            if (ignored[byte.Parse("c2", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c2 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c2");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC3()
        {
            if (ignored[byte.Parse("c3", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c3 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c3");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC4()
        {
            if (ignored[byte.Parse("c4", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c4 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c4");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC5()
        {
            if (ignored[byte.Parse("c5", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c5 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c5");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC6()
        {
            if (ignored[byte.Parse("c6", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c6 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c6");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC7()
        {
            if (ignored[byte.Parse("c7", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c7 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c7");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC8()
        {
            if (ignored[byte.Parse("c8", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c8 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c8");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestC9()
        {
            if (ignored[byte.Parse("c9", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test c9 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "c9");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestCA()
        {
            if (ignored[byte.Parse("ca", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ca is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ca");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestCB()
        {
            if (ignored[byte.Parse("cb", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test cb is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "cb");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestCC()
        {
            if (ignored[byte.Parse("cc", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test cc is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "cc");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestCD()
        {
            if (ignored[byte.Parse("cd", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test cd is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "cd");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestCE()
        {
            if (ignored[byte.Parse("ce", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ce is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ce");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestCF()
        {
            if (ignored[byte.Parse("cf", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test cf is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "cf");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD0()
        {
            if (ignored[byte.Parse("d0", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d0 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d0");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD1()
        {
            if (ignored[byte.Parse("d1", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d1 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d1");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD2()
        {
            if (ignored[byte.Parse("d2", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d2 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d2");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD3()
        {
            if (ignored[byte.Parse("d3", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d3 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d3");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD4()
        {
            if (ignored[byte.Parse("d4", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d4 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d4");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD5()
        {
            if (ignored[byte.Parse("d5", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d5 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d5");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD6()
        {
            if (ignored[byte.Parse("d6", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d6 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d6");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD7()
        {
            if (ignored[byte.Parse("d7", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d7 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d7");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD8()
        {
            if (ignored[byte.Parse("d8", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d8 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d8");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestD9()
        {
            if (ignored[byte.Parse("d9", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test d9 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "d9");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestDA()
        {
            if (ignored[byte.Parse("da", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test da is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "da");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestDB()
        {
            if (ignored[byte.Parse("db", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test db is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "db");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestDC()
        {
            if (ignored[byte.Parse("dc", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test dc is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "dc");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestDD()
        {
            if (ignored[byte.Parse("dd", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test dd is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "dd");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestDE()
        {
            if (ignored[byte.Parse("de", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test de is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "de");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestDF()
        {
            if (ignored[byte.Parse("df", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test df is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "df");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE0()
        {
            if (ignored[byte.Parse("e0", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e0 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e0");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE1()
        {
            if (ignored[byte.Parse("e1", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e1 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e1");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE2()
        {
            if (ignored[byte.Parse("e2", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e2 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e2");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE3()
        {
            if (ignored[byte.Parse("e3", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e3 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e3");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE4()
        {
            if (ignored[byte.Parse("e4", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e4 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e4");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE5()
        {
            if (ignored[byte.Parse("e5", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e5 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e5");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE6()
        {
            if (ignored[byte.Parse("e6", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e6 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e6");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE7()
        {
            if (ignored[byte.Parse("e7", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e7 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e7");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE8()
        {
            if (ignored[byte.Parse("e8", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e8 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e8");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestE9()
        {
            if (ignored[byte.Parse("e9", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test e9 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "e9");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestEA()
        {
            if (ignored[byte.Parse("ea", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ea is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ea");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestEB()
        {
            if (ignored[byte.Parse("eb", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test eb is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "eb");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestEC()
        {
            if (ignored[byte.Parse("ec", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ec is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ec");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestED()
        {
            if (ignored[byte.Parse("ed", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ed is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ed");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestEE()
        {
            if (ignored[byte.Parse("ee", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ee is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ee");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestEF()
        {
            if (ignored[byte.Parse("ef", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ef is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ef");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF0()
        {
            if (ignored[byte.Parse("f0", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f0 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f0");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF1()
        {
            if (ignored[byte.Parse("f1", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f1 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f1");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF2()
        {
            if (ignored[byte.Parse("f2", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f2 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f2");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF3()
        {
            if (ignored[byte.Parse("f3", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f3 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f3");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF4()
        {
            if (ignored[byte.Parse("f4", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f4 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f4");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF5()
        {
            if (ignored[byte.Parse("f5", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f5 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f5");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF6()
        {
            if (ignored[byte.Parse("f6", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f6 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f6");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF7()
        {
            if (ignored[byte.Parse("f7", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f7 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f7");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF8()
        {
            if (ignored[byte.Parse("f8", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f8 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f8");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestF9()
        {
            if (ignored[byte.Parse("f9", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test f9 is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "f9");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestFA()
        {
            if (ignored[byte.Parse("fa", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test fa is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "fa");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestFB()
        {
            if (ignored[byte.Parse("fb", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test fb is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "fb");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestFC()
        {
            if (ignored[byte.Parse("fc", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test fc is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "fc");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestFD()
        {
            if (ignored[byte.Parse("fd", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test fd is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "fd");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestFE()
        {
            if (ignored[byte.Parse("fe", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test fe is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "fe");
            }
        }

        [TestMethod]
        public void RunIndividual6502TestFF()
        {
            if (ignored[byte.Parse("ff", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)
            {
                // Assert.Inconclusive($"Test ff is marked as ignored");
            }
            else
            {
                RunNamedBatch(CpuClass.WDC6502, "ff");
            }
        }
    }
}
