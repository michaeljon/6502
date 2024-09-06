// #define VERBOSE_BATCH_OUTPUT

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using InnoWerks.Processors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InnoWerks.Simulators.Tests
{
    [TestClass]
    public class Harte65C02 : HarteBase
    {
        private static readonly bool[] ignored = LoadIgnored(CpuClass.WDC65C02);

        protected override string BasePath => Environment.ExpandEnvironmentVariables("%HOME%/src/6502/working/65x02/wdc65c02/v1");

        // [Ignore]
        [TestMethod]
        public void RunAll65C02Tests()
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
                        foreach (var test in JsonSerializer.Deserialize<List<JsonHarteTestStructure>>(fs, SerializerOptions).Take(100))
                        {
                            RunIndividualTest(CpuClass.WDC65C02, test, results);
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
        public void RunNamed65C02Test()
        {
            var testName = "7c a5 b0";

            List<string> results = [];

            var batch = testName.Split(' ')[0];
            var file = $"{BasePath}/{batch}.json";

            var ocd = CpuInstructions.OpCode65C02[byte.Parse(batch, NumberStyles.HexNumber, CultureInfo.InvariantCulture)];

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

                RunIndividualTest(CpuClass.WDC65C02, test, results);
            }

            foreach (var result in results)
            {
                TestContext.WriteLine(result);
            }

            Assert.AreEqual(0, results.Count, $"Failed some tests");
        }

        [TestMethod]
        public void RunIndividual65C02Test00()
        {
            RunNamedBatch(CpuClass.WDC65C02, "00");
        }

        [TestMethod]
        public void RunIndividual65C02Test01()
        {
            RunNamedBatch(CpuClass.WDC65C02, "01");
        }

        [TestMethod]
        public void RunIndividual65C02Test02()
        {
            RunNamedBatch(CpuClass.WDC65C02, "02");
        }

        [TestMethod]
        public void RunIndividual65C02Test03()
        {
            RunNamedBatch(CpuClass.WDC65C02, "03");
        }

        [TestMethod]
        public void RunIndividual65C02Test04()
        {
            RunNamedBatch(CpuClass.WDC65C02, "04");
        }

        [TestMethod]
        public void RunIndividual65C02Test05()
        {
            RunNamedBatch(CpuClass.WDC65C02, "05");
        }

        [TestMethod]
        public void RunIndividual65C02Test06()
        {
            RunNamedBatch(CpuClass.WDC65C02, "06");
        }

        [TestMethod]
        public void RunIndividual65C02Test07()
        {
            RunNamedBatch(CpuClass.WDC65C02, "07");
        }

        [TestMethod]
        public void RunIndividual65C02Test08()
        {
            RunNamedBatch(CpuClass.WDC65C02, "08");
        }

        [TestMethod]
        public void RunIndividual65C02Test09()
        {
            RunNamedBatch(CpuClass.WDC65C02, "09");
        }

        [TestMethod]
        public void RunIndividual65C02Test0A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "0a");
        }

        [TestMethod]
        public void RunIndividual65C02Test0B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "0b");
        }

        [TestMethod]
        public void RunIndividual65C02Test0C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "0c");
        }

        [TestMethod]
        public void RunIndividual65C02Test0D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "0d");
        }

        [TestMethod]
        public void RunIndividual65C02Test0E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "0e");
        }

        [TestMethod]
        public void RunIndividual65C02Test0F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "0f");
        }

        [TestMethod]
        public void RunIndividual65C02Test10()
        {
            RunNamedBatch(CpuClass.WDC65C02, "10");
        }

        [TestMethod]
        public void RunIndividual65C02Test11()
        {
            RunNamedBatch(CpuClass.WDC65C02, "11");
        }

        [TestMethod]
        public void RunIndividual65C02Test12()
        {
            RunNamedBatch(CpuClass.WDC65C02, "12");
        }

        [TestMethod]
        public void RunIndividual65C02Test13()
        {
            RunNamedBatch(CpuClass.WDC65C02, "13");
        }

        [TestMethod]
        public void RunIndividual65C02Test14()
        {
            RunNamedBatch(CpuClass.WDC65C02, "14");
        }

        [TestMethod]
        public void RunIndividual65C02Test15()
        {
            RunNamedBatch(CpuClass.WDC65C02, "15");
        }

        [TestMethod]
        public void RunIndividual65C02Test16()
        {
            RunNamedBatch(CpuClass.WDC65C02, "16");
        }

        [TestMethod]
        public void RunIndividual65C02Test17()
        {
            RunNamedBatch(CpuClass.WDC65C02, "17");
        }

        [TestMethod]
        public void RunIndividual65C02Test18()
        {
            RunNamedBatch(CpuClass.WDC65C02, "18");
        }

        [TestMethod]
        public void RunIndividual65C02Test19()
        {
            RunNamedBatch(CpuClass.WDC65C02, "19");
        }

        [TestMethod]
        public void RunIndividual65C02Test1A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "1a");
        }

        [TestMethod]
        public void RunIndividual65C02Test1B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "1b");
        }

        [TestMethod]
        public void RunIndividual65C02Test1C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "1c");
        }

        [TestMethod]
        public void RunIndividual65C02Test1D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "1d");
        }

        [TestMethod]
        public void RunIndividual65C02Test1E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "1e");
        }

        [TestMethod]
        public void RunIndividual65C02Test1F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "1f");
        }

        [TestMethod]
        public void RunIndividual65C02Test20()
        {
            RunNamedBatch(CpuClass.WDC65C02, "20");
        }

        [TestMethod]
        public void RunIndividual65C02Test21()
        {
            RunNamedBatch(CpuClass.WDC65C02, "21");
        }

        [TestMethod]
        public void RunIndividual65C02Test22()
        {
            RunNamedBatch(CpuClass.WDC65C02, "22");
        }

        [TestMethod]
        public void RunIndividual65C02Test23()
        {
            RunNamedBatch(CpuClass.WDC65C02, "23");
        }

        [TestMethod]
        public void RunIndividual65C02Test24()
        {
            RunNamedBatch(CpuClass.WDC65C02, "24");
        }

        [TestMethod]
        public void RunIndividual65C02Test25()
        {
            RunNamedBatch(CpuClass.WDC65C02, "25");
        }

        [TestMethod]
        public void RunIndividual65C02Test26()
        {
            RunNamedBatch(CpuClass.WDC65C02, "26");
        }

        [TestMethod]
        public void RunIndividual65C02Test27()
        {
            RunNamedBatch(CpuClass.WDC65C02, "27");
        }

        [TestMethod]
        public void RunIndividual65C02Test28()
        {
            RunNamedBatch(CpuClass.WDC65C02, "28");
        }

        [TestMethod]
        public void RunIndividual65C02Test29()
        {
            RunNamedBatch(CpuClass.WDC65C02, "29");
        }

        [TestMethod]
        public void RunIndividual65C02Test2A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "2a");
        }

        [TestMethod]
        public void RunIndividual65C02Test2B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "2b");
        }

        [TestMethod]
        public void RunIndividual65C02Test2C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "2c");
        }

        [TestMethod]
        public void RunIndividual65C02Test2D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "2d");
        }

        [TestMethod]
        public void RunIndividual65C02Test2E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "2e");
        }

        [TestMethod]
        public void RunIndividual65C02Test2F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "2f");
        }

        [TestMethod]
        public void RunIndividual65C02Test30()
        {
            RunNamedBatch(CpuClass.WDC65C02, "30");
        }

        [TestMethod]
        public void RunIndividual65C02Test31()
        {
            RunNamedBatch(CpuClass.WDC65C02, "31");
        }

        [TestMethod]
        public void RunIndividual65C02Test32()
        {
            RunNamedBatch(CpuClass.WDC65C02, "32");
        }

        [TestMethod]
        public void RunIndividual65C02Test33()
        {
            RunNamedBatch(CpuClass.WDC65C02, "33");
        }

        [TestMethod]
        public void RunIndividual65C02Test34()
        {
            RunNamedBatch(CpuClass.WDC65C02, "34");
        }

        [TestMethod]
        public void RunIndividual65C02Test35()
        {
            RunNamedBatch(CpuClass.WDC65C02, "35");
        }

        [TestMethod]
        public void RunIndividual65C02Test36()
        {
            RunNamedBatch(CpuClass.WDC65C02, "36");
        }

        [TestMethod]
        public void RunIndividual65C02Test37()
        {
            RunNamedBatch(CpuClass.WDC65C02, "37");
        }

        [TestMethod]
        public void RunIndividual65C02Test38()
        {
            RunNamedBatch(CpuClass.WDC65C02, "38");
        }

        [TestMethod]
        public void RunIndividual65C02Test39()
        {
            RunNamedBatch(CpuClass.WDC65C02, "39");
        }

        [TestMethod]
        public void RunIndividual65C02Test3A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "3a");
        }

        [TestMethod]
        public void RunIndividual65C02Test3B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "3b");
        }

        [TestMethod]
        public void RunIndividual65C02Test3C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "3c");
        }

        [TestMethod]
        public void RunIndividual65C02Test3D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "3d");
        }

        [TestMethod]
        public void RunIndividual65C02Test3E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "3e");
        }

        [TestMethod]
        public void RunIndividual65C02Test3F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "3f");
        }

        [TestMethod]
        public void RunIndividual65C02Test40()
        {
            RunNamedBatch(CpuClass.WDC65C02, "40");
        }

        [TestMethod]
        public void RunIndividual65C02Test41()
        {
            RunNamedBatch(CpuClass.WDC65C02, "41");
        }

        [TestMethod]
        public void RunIndividual65C02Test42()
        {
            RunNamedBatch(CpuClass.WDC65C02, "42");
        }

        [TestMethod]
        public void RunIndividual65C02Test43()
        {
            RunNamedBatch(CpuClass.WDC65C02, "43");
        }

        [TestMethod]
        public void RunIndividual65C02Test44()
        {
            RunNamedBatch(CpuClass.WDC65C02, "44");
        }

        [TestMethod]
        public void RunIndividual65C02Test45()
        {
            RunNamedBatch(CpuClass.WDC65C02, "45");
        }

        [TestMethod]
        public void RunIndividual65C02Test46()
        {
            RunNamedBatch(CpuClass.WDC65C02, "46");
        }

        [TestMethod]
        public void RunIndividual65C02Test47()
        {
            RunNamedBatch(CpuClass.WDC65C02, "47");
        }

        [TestMethod]
        public void RunIndividual65C02Test48()
        {
            RunNamedBatch(CpuClass.WDC65C02, "48");
        }

        [TestMethod]
        public void RunIndividual65C02Test49()
        {
            RunNamedBatch(CpuClass.WDC65C02, "49");
        }

        [TestMethod]
        public void RunIndividual65C02Test4A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "4a");
        }

        [TestMethod]
        public void RunIndividual65C02Test4B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "4b");
        }

        [TestMethod]
        public void RunIndividual65C02Test4C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "4c");
        }

        [TestMethod]
        public void RunIndividual65C02Test4D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "4d");
        }

        [TestMethod]
        public void RunIndividual65C02Test4E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "4e");
        }

        [TestMethod]
        public void RunIndividual65C02Test4F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "4f");
        }

        [TestMethod]
        public void RunIndividual65C02Test50()
        {
            RunNamedBatch(CpuClass.WDC65C02, "50");
        }

        [TestMethod]
        public void RunIndividual65C02Test51()
        {
            RunNamedBatch(CpuClass.WDC65C02, "51");
        }

        [TestMethod]
        public void RunIndividual65C02Test52()
        {
            RunNamedBatch(CpuClass.WDC65C02, "52");
        }

        [TestMethod]
        public void RunIndividual65C02Test53()
        {
            RunNamedBatch(CpuClass.WDC65C02, "53");
        }

        [TestMethod]
        public void RunIndividual65C02Test54()
        {
            RunNamedBatch(CpuClass.WDC65C02, "54");
        }

        [TestMethod]
        public void RunIndividual65C02Test55()
        {
            RunNamedBatch(CpuClass.WDC65C02, "55");
        }

        [TestMethod]
        public void RunIndividual65C02Test56()
        {
            RunNamedBatch(CpuClass.WDC65C02, "56");
        }

        [TestMethod]
        public void RunIndividual65C02Test57()
        {
            RunNamedBatch(CpuClass.WDC65C02, "57");
        }

        [TestMethod]
        public void RunIndividual65C02Test58()
        {
            RunNamedBatch(CpuClass.WDC65C02, "58");
        }

        [TestMethod]
        public void RunIndividual65C02Test59()
        {
            RunNamedBatch(CpuClass.WDC65C02, "59");
        }

        [TestMethod]
        public void RunIndividual65C02Test5A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "5a");
        }

        [TestMethod]
        public void RunIndividual65C02Test5B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "5b");
        }

        [TestMethod]
        public void RunIndividual65C02Test5C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "5c");
        }

        [TestMethod]
        public void RunIndividual65C02Test5D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "5d");
        }

        [TestMethod]
        public void RunIndividual65C02Test5E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "5e");
        }

        [TestMethod]
        public void RunIndividual65C02Test5F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "5f");
        }

        [TestMethod]
        public void RunIndividual65C02Test60()
        {
            RunNamedBatch(CpuClass.WDC65C02, "60");
        }

        [TestMethod]
        public void RunIndividual65C02Test61()
        {
            RunNamedBatch(CpuClass.WDC65C02, "61");
        }

        [TestMethod]
        public void RunIndividual65C02Test62()
        {
            RunNamedBatch(CpuClass.WDC65C02, "62");
        }

        [TestMethod]
        public void RunIndividual65C02Test63()
        {
            RunNamedBatch(CpuClass.WDC65C02, "63");
        }

        [TestMethod]
        public void RunIndividual65C02Test64()
        {
            RunNamedBatch(CpuClass.WDC65C02, "64");
        }

        [TestMethod]
        public void RunIndividual65C02Test65()
        {
            RunNamedBatch(CpuClass.WDC65C02, "65");
        }

        [TestMethod]
        public void RunIndividual65C02Test66()
        {
            RunNamedBatch(CpuClass.WDC65C02, "66");
        }

        [TestMethod]
        public void RunIndividual65C02Test67()
        {
            RunNamedBatch(CpuClass.WDC65C02, "67");
        }

        [TestMethod]
        public void RunIndividual65C02Test68()
        {
            RunNamedBatch(CpuClass.WDC65C02, "68");
        }

        [TestMethod]
        public void RunIndividual65C02Test69()
        {
            RunNamedBatch(CpuClass.WDC65C02, "69");
        }

        [TestMethod]
        public void RunIndividual65C02Test6A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "6a");
        }

        [TestMethod]
        public void RunIndividual65C02Test6B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "6b");
        }

        [TestMethod]
        public void RunIndividual65C02Test6C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "6c");
        }

        [TestMethod]
        public void RunIndividual65C02Test6D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "6d");
        }

        [TestMethod]
        public void RunIndividual65C02Test6E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "6e");
        }

        [TestMethod]
        public void RunIndividual65C02Test6F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "6f");
        }

        [TestMethod]
        public void RunIndividual65C02Test70()
        {
            RunNamedBatch(CpuClass.WDC65C02, "70");
        }

        [TestMethod]
        public void RunIndividual65C02Test71()
        {
            RunNamedBatch(CpuClass.WDC65C02, "71");
        }

        [TestMethod]
        public void RunIndividual65C02Test72()
        {
            RunNamedBatch(CpuClass.WDC65C02, "72");
        }

        [TestMethod]
        public void RunIndividual65C02Test73()
        {
            RunNamedBatch(CpuClass.WDC65C02, "73");
        }

        [TestMethod]
        public void RunIndividual65C02Test74()
        {
            RunNamedBatch(CpuClass.WDC65C02, "74");
        }

        [TestMethod]
        public void RunIndividual65C02Test75()
        {
            RunNamedBatch(CpuClass.WDC65C02, "75");
        }

        [TestMethod]
        public void RunIndividual65C02Test76()
        {
            RunNamedBatch(CpuClass.WDC65C02, "76");
        }

        [TestMethod]
        public void RunIndividual65C02Test77()
        {
            RunNamedBatch(CpuClass.WDC65C02, "77");
        }

        [TestMethod]
        public void RunIndividual65C02Test78()
        {
            RunNamedBatch(CpuClass.WDC65C02, "78");
        }

        [TestMethod]
        public void RunIndividual65C02Test79()
        {
            RunNamedBatch(CpuClass.WDC65C02, "79");
        }

        [TestMethod]
        public void RunIndividual65C02Test7A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "7a");
        }

        [TestMethod]
        public void RunIndividual65C02Test7B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "7b");
        }

        [TestMethod]
        public void RunIndividual65C02Test7C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "7c");
        }

        [TestMethod]
        public void RunIndividual65C02Test7D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "7d");
        }

        [TestMethod]
        public void RunIndividual65C02Test7E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "7e");
        }

        [TestMethod]
        public void RunIndividual65C02Test7F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "7f");
        }

        [TestMethod]
        public void RunIndividual65C02Test80()
        {
            RunNamedBatch(CpuClass.WDC65C02, "80");
        }

        [TestMethod]
        public void RunIndividual65C02Test81()
        {
            RunNamedBatch(CpuClass.WDC65C02, "81");
        }

        [TestMethod]
        public void RunIndividual65C02Test82()
        {
            RunNamedBatch(CpuClass.WDC65C02, "82");
        }

        [TestMethod]
        public void RunIndividual65C02Test83()
        {
            RunNamedBatch(CpuClass.WDC65C02, "83");
        }

        [TestMethod]
        public void RunIndividual65C02Test84()
        {
            RunNamedBatch(CpuClass.WDC65C02, "84");
        }

        [TestMethod]
        public void RunIndividual65C02Test85()
        {
            RunNamedBatch(CpuClass.WDC65C02, "85");
        }

        [TestMethod]
        public void RunIndividual65C02Test86()
        {
            RunNamedBatch(CpuClass.WDC65C02, "86");
        }

        [TestMethod]
        public void RunIndividual65C02Test87()
        {
            RunNamedBatch(CpuClass.WDC65C02, "87");
        }

        [TestMethod]
        public void RunIndividual65C02Test88()
        {
            RunNamedBatch(CpuClass.WDC65C02, "88");
        }

        [TestMethod]
        public void RunIndividual65C02Test89()
        {
            RunNamedBatch(CpuClass.WDC65C02, "89");
        }

        [TestMethod]
        public void RunIndividual65C02Test8A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "8a");
        }

        [TestMethod]
        public void RunIndividual65C02Test8B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "8b");
        }

        [TestMethod]
        public void RunIndividual65C02Test8C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "8c");
        }

        [TestMethod]
        public void RunIndividual65C02Test8D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "8d");
        }

        [TestMethod]
        public void RunIndividual65C02Test8E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "8e");
        }

        [TestMethod]
        public void RunIndividual65C02Test8F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "8f");
        }

        [TestMethod]
        public void RunIndividual65C02Test90()
        {
            RunNamedBatch(CpuClass.WDC65C02, "90");
        }

        [TestMethod]
        public void RunIndividual65C02Test91()
        {
            RunNamedBatch(CpuClass.WDC65C02, "91");
        }

        [TestMethod]
        public void RunIndividual65C02Test92()
        {
            RunNamedBatch(CpuClass.WDC65C02, "92");
        }

        [TestMethod]
        public void RunIndividual65C02Test93()
        {
            RunNamedBatch(CpuClass.WDC65C02, "93");
        }

        [TestMethod]
        public void RunIndividual65C02Test94()
        {
            RunNamedBatch(CpuClass.WDC65C02, "94");
        }

        [TestMethod]
        public void RunIndividual65C02Test95()
        {
            RunNamedBatch(CpuClass.WDC65C02, "95");
        }

        [TestMethod]
        public void RunIndividual65C02Test96()
        {
            RunNamedBatch(CpuClass.WDC65C02, "96");
        }

        [TestMethod]
        public void RunIndividual65C02Test97()
        {
            RunNamedBatch(CpuClass.WDC65C02, "97");
        }

        [TestMethod]
        public void RunIndividual65C02Test98()
        {
            RunNamedBatch(CpuClass.WDC65C02, "98");
        }

        [TestMethod]
        public void RunIndividual65C02Test99()
        {
            RunNamedBatch(CpuClass.WDC65C02, "99");
        }

        [TestMethod]
        public void RunIndividual65C02Test9A()
        {
            RunNamedBatch(CpuClass.WDC65C02, "9a");
        }

        [TestMethod]
        public void RunIndividual65C02Test9B()
        {
            RunNamedBatch(CpuClass.WDC65C02, "9b");
        }

        [TestMethod]
        public void RunIndividual65C02Test9C()
        {
            RunNamedBatch(CpuClass.WDC65C02, "9c");
        }

        [TestMethod]
        public void RunIndividual65C02Test9D()
        {
            RunNamedBatch(CpuClass.WDC65C02, "9d");
        }

        [TestMethod]
        public void RunIndividual65C02Test9E()
        {
            RunNamedBatch(CpuClass.WDC65C02, "9e");
        }

        [TestMethod]
        public void RunIndividual65C02Test9F()
        {
            RunNamedBatch(CpuClass.WDC65C02, "9f");
        }

        [TestMethod]
        public void RunIndividual65C02TestA0()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a0");
        }

        [TestMethod]
        public void RunIndividual65C02TestA1()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a1");
        }

        [TestMethod]
        public void RunIndividual65C02TestA2()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a2");
        }

        [TestMethod]
        public void RunIndividual65C02TestA3()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a3");
        }

        [TestMethod]
        public void RunIndividual65C02TestA4()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a4");
        }

        [TestMethod]
        public void RunIndividual65C02TestA5()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a5");
        }

        [TestMethod]
        public void RunIndividual65C02TestA6()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a6");
        }

        [TestMethod]
        public void RunIndividual65C02TestA7()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a7");
        }

        [TestMethod]
        public void RunIndividual65C02TestA8()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a8");
        }

        [TestMethod]
        public void RunIndividual65C02TestA9()
        {
            RunNamedBatch(CpuClass.WDC65C02, "a9");
        }

        [TestMethod]
        public void RunIndividual65C02TestAA()
        {
            RunNamedBatch(CpuClass.WDC65C02, "aa");
        }

        [TestMethod]
        public void RunIndividual65C02TestAB()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ab");
        }

        [TestMethod]
        public void RunIndividual65C02TestAC()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ac");
        }

        [TestMethod]
        public void RunIndividual65C02TestAD()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ad");
        }

        [TestMethod]
        public void RunIndividual65C02TestAE()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ae");
        }

        [TestMethod]
        public void RunIndividual65C02TestAF()
        {
            RunNamedBatch(CpuClass.WDC65C02, "af");
        }

        [TestMethod]
        public void RunIndividual65C02TestB0()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b0");
        }

        [TestMethod]
        public void RunIndividual65C02TestB1()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b1");
        }

        [TestMethod]
        public void RunIndividual65C02TestB2()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b2");
        }

        [TestMethod]
        public void RunIndividual65C02TestB3()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b3");
        }

        [TestMethod]
        public void RunIndividual65C02TestB4()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b4");
        }

        [TestMethod]
        public void RunIndividual65C02TestB5()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b5");
        }

        [TestMethod]
        public void RunIndividual65C02TestB6()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b6");
        }

        [TestMethod]
        public void RunIndividual65C02TestB7()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b7");
        }

        [TestMethod]
        public void RunIndividual65C02TestB8()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b8");
        }

        [TestMethod]
        public void RunIndividual65C02TestB9()
        {
            RunNamedBatch(CpuClass.WDC65C02, "b9");
        }

        [TestMethod]
        public void RunIndividual65C02TestBA()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ba");
        }

        [TestMethod]
        public void RunIndividual65C02TestBB()
        {
            RunNamedBatch(CpuClass.WDC65C02, "bb");
        }

        [TestMethod]
        public void RunIndividual65C02TestBC()
        {
            RunNamedBatch(CpuClass.WDC65C02, "bc");
        }

        [TestMethod]
        public void RunIndividual65C02TestBD()
        {
            RunNamedBatch(CpuClass.WDC65C02, "bd");
        }

        [TestMethod]
        public void RunIndividual65C02TestBE()
        {
            RunNamedBatch(CpuClass.WDC65C02, "be");
        }

        [TestMethod]
        public void RunIndividual65C02TestBF()
        {
            RunNamedBatch(CpuClass.WDC65C02, "bf");
        }

        [TestMethod]
        public void RunIndividual65C02TestC0()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c0");
        }

        [TestMethod]
        public void RunIndividual65C02TestC1()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c1");
        }

        [TestMethod]
        public void RunIndividual65C02TestC2()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c2");
        }

        [TestMethod]
        public void RunIndividual65C02TestC3()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c3");
        }

        [TestMethod]
        public void RunIndividual65C02TestC4()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c4");
        }

        [TestMethod]
        public void RunIndividual65C02TestC5()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c5");
        }

        [TestMethod]
        public void RunIndividual65C02TestC6()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c6");
        }

        [TestMethod]
        public void RunIndividual65C02TestC7()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c7");
        }

        [TestMethod]
        public void RunIndividual65C02TestC8()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c8");
        }

        [TestMethod]
        public void RunIndividual65C02TestC9()
        {
            RunNamedBatch(CpuClass.WDC65C02, "c9");
        }

        [TestMethod]
        public void RunIndividual65C02TestCA()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ca");
        }

        [TestMethod]
        public void RunIndividual65C02TestCB()
        {
            RunNamedBatch(CpuClass.WDC65C02, "cb");
        }

        [TestMethod]
        public void RunIndividual65C02TestCC()
        {
            RunNamedBatch(CpuClass.WDC65C02, "cc");
        }

        [TestMethod]
        public void RunIndividual65C02TestCD()
        {
            RunNamedBatch(CpuClass.WDC65C02, "cd");
        }

        [TestMethod]
        public void RunIndividual65C02TestCE()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ce");
        }

        [TestMethod]
        public void RunIndividual65C02TestCF()
        {
            RunNamedBatch(CpuClass.WDC65C02, "cf");
        }

        [TestMethod]
        public void RunIndividual65C02TestD0()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d0");
        }

        [TestMethod]
        public void RunIndividual65C02TestD1()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d1");
        }

        [TestMethod]
        public void RunIndividual65C02TestD2()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d2");
        }

        [TestMethod]
        public void RunIndividual65C02TestD3()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d3");
        }

        [TestMethod]
        public void RunIndividual65C02TestD4()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d4");
        }

        [TestMethod]
        public void RunIndividual65C02TestD5()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d5");
        }

        [TestMethod]
        public void RunIndividual65C02TestD6()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d6");
        }

        [TestMethod]
        public void RunIndividual65C02TestD7()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d7");
        }

        [TestMethod]
        public void RunIndividual65C02TestD8()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d8");
        }

        [TestMethod]
        public void RunIndividual65C02TestD9()
        {
            RunNamedBatch(CpuClass.WDC65C02, "d9");
        }

        [TestMethod]
        public void RunIndividual65C02TestDA()
        {
            RunNamedBatch(CpuClass.WDC65C02, "da");
        }

        [TestMethod]
        public void RunIndividual65C02TestDB()
        {
            RunNamedBatch(CpuClass.WDC65C02, "db");
        }

        [TestMethod]
        public void RunIndividual65C02TestDC()
        {
            RunNamedBatch(CpuClass.WDC65C02, "dc");
        }

        [TestMethod]
        public void RunIndividual65C02TestDD()
        {
            RunNamedBatch(CpuClass.WDC65C02, "dd");
        }

        [TestMethod]
        public void RunIndividual65C02TestDE()
        {
            RunNamedBatch(CpuClass.WDC65C02, "de");
        }

        [TestMethod]
        public void RunIndividual65C02TestDF()
        {
            RunNamedBatch(CpuClass.WDC65C02, "df");
        }

        [TestMethod]
        public void RunIndividual65C02TestE0()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e0");
        }

        [TestMethod]
        public void RunIndividual65C02TestE1()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e1");
        }

        [TestMethod]
        public void RunIndividual65C02TestE2()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e2");
        }

        [TestMethod]
        public void RunIndividual65C02TestE3()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e3");
        }

        [TestMethod]
        public void RunIndividual65C02TestE4()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e4");
        }

        [TestMethod]
        public void RunIndividual65C02TestE5()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e5");
        }

        [TestMethod]
        public void RunIndividual65C02TestE6()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e6");
        }

        [TestMethod]
        public void RunIndividual65C02TestE7()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e7");
        }

        [TestMethod]
        public void RunIndividual65C02TestE8()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e8");
        }

        [TestMethod]
        public void RunIndividual65C02TestE9()
        {
            RunNamedBatch(CpuClass.WDC65C02, "e9");
        }

        [TestMethod]
        public void RunIndividual65C02TestEA()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ea");
        }

        [TestMethod]
        public void RunIndividual65C02TestEB()
        {
            RunNamedBatch(CpuClass.WDC65C02, "eb");
        }

        [TestMethod]
        public void RunIndividual65C02TestEC()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ec");
        }

        [TestMethod]
        public void RunIndividual65C02TestED()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ed");
        }

        [TestMethod]
        public void RunIndividual65C02TestEE()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ee");
        }

        [TestMethod]
        public void RunIndividual65C02TestEF()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ef");
        }

        [TestMethod]
        public void RunIndividual65C02TestF0()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f0");
        }

        [TestMethod]
        public void RunIndividual65C02TestF1()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f1");
        }

        [TestMethod]
        public void RunIndividual65C02TestF2()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f2");
        }

        [TestMethod]
        public void RunIndividual65C02TestF3()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f3");
        }

        [TestMethod]
        public void RunIndividual65C02TestF4()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f4");
        }

        [TestMethod]
        public void RunIndividual65C02TestF5()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f5");
        }

        [TestMethod]
        public void RunIndividual65C02TestF6()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f6");
        }

        [TestMethod]
        public void RunIndividual65C02TestF7()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f7");
        }

        [TestMethod]
        public void RunIndividual65C02TestF8()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f8");
        }

        [TestMethod]
        public void RunIndividual65C02TestF9()
        {
            RunNamedBatch(CpuClass.WDC65C02, "f9");
        }

        [TestMethod]
        public void RunIndividual65C02TestFA()
        {
            RunNamedBatch(CpuClass.WDC65C02, "fa");
        }

        [TestMethod]
        public void RunIndividual65C02TestFB()
        {
            RunNamedBatch(CpuClass.WDC65C02, "fb");
        }

        [TestMethod]
        public void RunIndividual65C02TestFC()
        {
            RunNamedBatch(CpuClass.WDC65C02, "fc");
        }

        [TestMethod]
        public void RunIndividual65C02TestFD()
        {
            RunNamedBatch(CpuClass.WDC65C02, "fd");
        }

        [TestMethod]
        public void RunIndividual65C02TestFE()
        {
            RunNamedBatch(CpuClass.WDC65C02, "fe");
        }

        [TestMethod]
        public void RunIndividual65C02TestFF()
        {
            RunNamedBatch(CpuClass.WDC65C02, "ff");
        }
    }
}
