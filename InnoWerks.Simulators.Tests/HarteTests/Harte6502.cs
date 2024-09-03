// #define DUMP_TEST_DATA

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1851

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class Harte6502 : HarteBase
    {
        private static readonly bool[] ignored = LoadIgnored(CpuClass.WDC6502);

        [Ignore]
        [TestMethod]
        public void RunAll6502Tests()
        {
            List<string> results = [];

            var files = Directory
                .GetFiles("/Users/michaeljon/src/6502/working/65x02/6502/v1", "*.json")
                .OrderBy(f => f);

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    var index = byte.Parse(Path.GetFileNameWithoutExtension(file), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

                    if (fs.Length == 0)
                    {
                        continue;
                    }

                    if (ignored[index] == false)
                    {
                        foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, SerializerOptions))
                        {
                            RunIndividualTest(CpuClass.WDC6502, test, results);
                        }
                    }
                }
            }

            foreach (var result in results)
            {
                TestContext.WriteLine(result);
            }

            Assert.AreEqual(0, results.Count, $"Failed some tests");
        }

        [Ignore]
        [DataTestMethod]
        [DataRow("20 55 13")]
        public void RunNamed6502Test(string testName)
        {
            if (string.IsNullOrEmpty(testName))
            {
                Assert.Inconclusive("No test name provided to RunNamed6502Test");
                return;
            }

            List<string> results = [];

            var batch = testName.Split(' ')[0];
            var file = $"/Users/michaeljon/src/6502/working/65x02/6502/v1/{batch}.json";

            TestContext.WriteLine($"Running test {testName}");
            TestContext.WriteLine($"OpCode: ${batch} {CpuInstructions.OpCode6502[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)].OpCode}");
            TestContext.WriteLine($"AddressingMode: {CpuInstructions.OpCode6502[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)].AddressingMode}");

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
                RunNamedBatch("00");
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
                RunNamedBatch("01");
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
                RunNamedBatch("02");
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
                RunNamedBatch("03");
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
                RunNamedBatch("04");
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
                RunNamedBatch("05");
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
                RunNamedBatch("06");
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
                RunNamedBatch("07");
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
                RunNamedBatch("08");
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
                RunNamedBatch("09");
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
                RunNamedBatch("0a");
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
                RunNamedBatch("0b");
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
                RunNamedBatch("0c");
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
                RunNamedBatch("0d");
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
                RunNamedBatch("0e");
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
                RunNamedBatch("0f");
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
                RunNamedBatch("10");
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
                RunNamedBatch("11");
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
                RunNamedBatch("12");
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
                RunNamedBatch("13");
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
                RunNamedBatch("14");
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
                RunNamedBatch("15");
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
                RunNamedBatch("16");
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
                RunNamedBatch("17");
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
                RunNamedBatch("18");
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
                RunNamedBatch("19");
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
                RunNamedBatch("1a");
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
                RunNamedBatch("1b");
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
                RunNamedBatch("1c");
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
                RunNamedBatch("1d");
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
                RunNamedBatch("1e");
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
                RunNamedBatch("1f");
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
                RunNamedBatch("20");
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
                RunNamedBatch("21");
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
                RunNamedBatch("22");
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
                RunNamedBatch("23");
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
                RunNamedBatch("24");
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
                RunNamedBatch("25");
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
                RunNamedBatch("26");
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
                RunNamedBatch("27");
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
                RunNamedBatch("28");
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
                RunNamedBatch("29");
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
                RunNamedBatch("2a");
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
                RunNamedBatch("2b");
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
                RunNamedBatch("2c");
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
                RunNamedBatch("2d");
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
                RunNamedBatch("2e");
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
                RunNamedBatch("2f");
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
                RunNamedBatch("30");
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
                RunNamedBatch("31");
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
                RunNamedBatch("32");
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
                RunNamedBatch("33");
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
                RunNamedBatch("34");
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
                RunNamedBatch("35");
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
                RunNamedBatch("36");
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
                RunNamedBatch("37");
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
                RunNamedBatch("38");
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
                RunNamedBatch("39");
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
                RunNamedBatch("3a");
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
                RunNamedBatch("3b");
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
                RunNamedBatch("3c");
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
                RunNamedBatch("3d");
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
                RunNamedBatch("3e");
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
                RunNamedBatch("3f");
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
                RunNamedBatch("40");
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
                RunNamedBatch("41");
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
                RunNamedBatch("42");
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
                RunNamedBatch("43");
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
                RunNamedBatch("44");
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
                RunNamedBatch("45");
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
                RunNamedBatch("46");
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
                RunNamedBatch("47");
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
                RunNamedBatch("48");
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
                RunNamedBatch("49");
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
                RunNamedBatch("4a");
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
                RunNamedBatch("4b");
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
                RunNamedBatch("4c");
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
                RunNamedBatch("4d");
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
                RunNamedBatch("4e");
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
                RunNamedBatch("4f");
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
                RunNamedBatch("50");
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
                RunNamedBatch("51");
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
                RunNamedBatch("52");
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
                RunNamedBatch("53");
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
                RunNamedBatch("54");
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
                RunNamedBatch("55");
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
                RunNamedBatch("56");
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
                RunNamedBatch("57");
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
                RunNamedBatch("58");
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
                RunNamedBatch("59");
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
                RunNamedBatch("5a");
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
                RunNamedBatch("5b");
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
                RunNamedBatch("5c");
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
                RunNamedBatch("5d");
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
                RunNamedBatch("5e");
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
                RunNamedBatch("5f");
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
                RunNamedBatch("60");
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
                RunNamedBatch("61");
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
                RunNamedBatch("62");
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
                RunNamedBatch("63");
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
                RunNamedBatch("64");
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
                RunNamedBatch("65");
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
                RunNamedBatch("66");
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
                RunNamedBatch("67");
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
                RunNamedBatch("68");
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
                RunNamedBatch("69");
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
                RunNamedBatch("6a");
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
                RunNamedBatch("6b");
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
                RunNamedBatch("6c");
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
                RunNamedBatch("6d");
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
                RunNamedBatch("6e");
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
                RunNamedBatch("6f");
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
                RunNamedBatch("70");
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
                RunNamedBatch("71");
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
                RunNamedBatch("72");
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
                RunNamedBatch("73");
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
                RunNamedBatch("74");
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
                RunNamedBatch("75");
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
                RunNamedBatch("76");
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
                RunNamedBatch("77");
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
                RunNamedBatch("78");
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
                RunNamedBatch("79");
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
                RunNamedBatch("7a");
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
                RunNamedBatch("7b");
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
                RunNamedBatch("7c");
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
                RunNamedBatch("7d");
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
                RunNamedBatch("7e");
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
                RunNamedBatch("7f");
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
                RunNamedBatch("80");
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
                RunNamedBatch("81");
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
                RunNamedBatch("82");
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
                RunNamedBatch("83");
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
                RunNamedBatch("84");
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
                RunNamedBatch("85");
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
                RunNamedBatch("86");
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
                RunNamedBatch("87");
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
                RunNamedBatch("88");
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
                RunNamedBatch("89");
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
                RunNamedBatch("8a");
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
                RunNamedBatch("8b");
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
                RunNamedBatch("8c");
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
                RunNamedBatch("8d");
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
                RunNamedBatch("8e");
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
                RunNamedBatch("8f");
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
                RunNamedBatch("90");
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
                RunNamedBatch("91");
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
                RunNamedBatch("92");
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
                RunNamedBatch("93");
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
                RunNamedBatch("94");
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
                RunNamedBatch("95");
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
                RunNamedBatch("96");
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
                RunNamedBatch("97");
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
                RunNamedBatch("98");
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
                RunNamedBatch("99");
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
                RunNamedBatch("9a");
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
                RunNamedBatch("9b");
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
                RunNamedBatch("9c");
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
                RunNamedBatch("9d");
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
                RunNamedBatch("9e");
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
                RunNamedBatch("9f");
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
                RunNamedBatch("a0");
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
                RunNamedBatch("a1");
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
                RunNamedBatch("a2");
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
                RunNamedBatch("a3");
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
                RunNamedBatch("a4");
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
                RunNamedBatch("a5");
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
                RunNamedBatch("a6");
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
                RunNamedBatch("a7");
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
                RunNamedBatch("a8");
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
                RunNamedBatch("a9");
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
                RunNamedBatch("aa");
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
                RunNamedBatch("ab");
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
                RunNamedBatch("ac");
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
                RunNamedBatch("ad");
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
                RunNamedBatch("ae");
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
                RunNamedBatch("af");
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
                RunNamedBatch("b0");
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
                RunNamedBatch("b1");
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
                RunNamedBatch("b2");
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
                RunNamedBatch("b3");
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
                RunNamedBatch("b4");
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
                RunNamedBatch("b5");
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
                RunNamedBatch("b6");
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
                RunNamedBatch("b7");
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
                RunNamedBatch("b8");
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
                RunNamedBatch("b9");
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
                RunNamedBatch("ba");
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
                RunNamedBatch("bb");
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
                RunNamedBatch("bc");
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
                RunNamedBatch("bd");
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
                RunNamedBatch("be");
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
                RunNamedBatch("bf");
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
                RunNamedBatch("c0");
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
                RunNamedBatch("c1");
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
                RunNamedBatch("c2");
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
                RunNamedBatch("c3");
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
                RunNamedBatch("c4");
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
                RunNamedBatch("c5");
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
                RunNamedBatch("c6");
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
                RunNamedBatch("c7");
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
                RunNamedBatch("c8");
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
                RunNamedBatch("c9");
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
                RunNamedBatch("ca");
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
                RunNamedBatch("cb");
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
                RunNamedBatch("cc");
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
                RunNamedBatch("cd");
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
                RunNamedBatch("ce");
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
                RunNamedBatch("cf");
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
                RunNamedBatch("d0");
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
                RunNamedBatch("d1");
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
                RunNamedBatch("d2");
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
                RunNamedBatch("d3");
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
                RunNamedBatch("d4");
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
                RunNamedBatch("d5");
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
                RunNamedBatch("d6");
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
                RunNamedBatch("d7");
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
                RunNamedBatch("d8");
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
                RunNamedBatch("d9");
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
                RunNamedBatch("da");
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
                RunNamedBatch("db");
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
                RunNamedBatch("dc");
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
                RunNamedBatch("dd");
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
                RunNamedBatch("de");
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
                RunNamedBatch("df");
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
                RunNamedBatch("e0");
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
                RunNamedBatch("e1");
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
                RunNamedBatch("e2");
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
                RunNamedBatch("e3");
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
                RunNamedBatch("e4");
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
                RunNamedBatch("e5");
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
                RunNamedBatch("e6");
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
                RunNamedBatch("e7");
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
                RunNamedBatch("e8");
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
                RunNamedBatch("e9");
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
                RunNamedBatch("ea");
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
                RunNamedBatch("eb");
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
                RunNamedBatch("ec");
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
                RunNamedBatch("ed");
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
                RunNamedBatch("ee");
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
                RunNamedBatch("ef");
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
                RunNamedBatch("f0");
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
                RunNamedBatch("f1");
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
                RunNamedBatch("f2");
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
                RunNamedBatch("f3");
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
                RunNamedBatch("f4");
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
                RunNamedBatch("f5");
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
                RunNamedBatch("f6");
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
                RunNamedBatch("f7");
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
                RunNamedBatch("f8");
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
                RunNamedBatch("f9");
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
                RunNamedBatch("fa");
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
                RunNamedBatch("fb");
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
                RunNamedBatch("fc");
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
                RunNamedBatch("fd");
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
                RunNamedBatch("fe");
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
                RunNamedBatch("ff");
            }
        }

        private void RunNamedBatch(string batch)
        {
            if (string.IsNullOrEmpty(batch))
            {
                Assert.Inconclusive("No batch name provided to RunNamed6502Batch");
                return;
            }

            List<string> results = [];

            var file = $"/Users/michaeljon/src/6502/working/65x02/6502/v1/{batch}.json";

            using (var fs = File.OpenRead(file))
            {
                var tests = JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, SerializerOptions);
                foreach (var test in tests)
                {
                    RunIndividualTest(CpuClass.WDC6502, test, results);
                }

                foreach (var result in results)
                {
                    TestContext.WriteLine(result);
                }

                Assert.AreEqual(0, results.Count, $"Failed some {results.Count} tests out of an expected {tests.Count}");
            }
        }
    }
}
