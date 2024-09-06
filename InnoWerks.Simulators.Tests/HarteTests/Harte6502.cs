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

        // [Ignore]
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
            var testName = "03 5c fe";
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
            RunNamedBatch(CpuClass.WDC6502, "00");
        }

        [TestMethod]
        public void RunIndividual6502Test01()
        {
            RunNamedBatch(CpuClass.WDC6502, "01");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test02()
        {
            RunNamedBatch(CpuClass.WDC6502, "02");
        }

        [TestMethod]
        public void RunIndividual6502Test03()
        {
            RunNamedBatch(CpuClass.WDC6502, "03");
        }

        [TestMethod]
        public void RunIndividual6502Test04()
        {
            RunNamedBatch(CpuClass.WDC6502, "04");
        }

        [TestMethod]
        public void RunIndividual6502Test05()
        {
            RunNamedBatch(CpuClass.WDC6502, "05");
        }

        [TestMethod]
        public void RunIndividual6502Test06()
        {
            RunNamedBatch(CpuClass.WDC6502, "06");
        }

        [TestMethod]
        public void RunIndividual6502Test07()
        {
            RunNamedBatch(CpuClass.WDC6502, "07");
        }

        [TestMethod]
        public void RunIndividual6502Test08()
        {
            RunNamedBatch(CpuClass.WDC6502, "08");
        }

        [TestMethod]
        public void RunIndividual6502Test09()
        {
            RunNamedBatch(CpuClass.WDC6502, "09");
        }

        [TestMethod]
        public void RunIndividual6502Test0A()
        {
            RunNamedBatch(CpuClass.WDC6502, "0a");
        }

        [TestMethod]
        public void RunIndividual6502Test0B()
        {
            RunNamedBatch(CpuClass.WDC6502, "0b");
        }

        [TestMethod]
        public void RunIndividual6502Test0C()
        {
            RunNamedBatch(CpuClass.WDC6502, "0c");
        }

        [TestMethod]
        public void RunIndividual6502Test0D()
        {
            RunNamedBatch(CpuClass.WDC6502, "0d");
        }

        [TestMethod]
        public void RunIndividual6502Test0E()
        {
            RunNamedBatch(CpuClass.WDC6502, "0e");
        }

        [TestMethod]
        public void RunIndividual6502Test0F()
        {
            RunNamedBatch(CpuClass.WDC6502, "0f");
        }

        [TestMethod]
        public void RunIndividual6502Test10()
        {
            RunNamedBatch(CpuClass.WDC6502, "10");
        }

        [TestMethod]
        public void RunIndividual6502Test11()
        {
            RunNamedBatch(CpuClass.WDC6502, "11");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test12()
        {
            RunNamedBatch(CpuClass.WDC6502, "12");
        }

        [TestMethod]
        public void RunIndividual6502Test13()
        {
            RunNamedBatch(CpuClass.WDC6502, "13");
        }

        [TestMethod]
        public void RunIndividual6502Test14()
        {
            RunNamedBatch(CpuClass.WDC6502, "14");
        }

        [TestMethod]
        public void RunIndividual6502Test15()
        {
            RunNamedBatch(CpuClass.WDC6502, "15");
        }

        [TestMethod]
        public void RunIndividual6502Test16()
        {
            RunNamedBatch(CpuClass.WDC6502, "16");
        }

        [TestMethod]
        public void RunIndividual6502Test17()
        {
            RunNamedBatch(CpuClass.WDC6502, "17");
        }

        [TestMethod]
        public void RunIndividual6502Test18()
        {
            RunNamedBatch(CpuClass.WDC6502, "18");
        }

        [TestMethod]
        public void RunIndividual6502Test19()
        {
            RunNamedBatch(CpuClass.WDC6502, "19");
        }

        [TestMethod]
        public void RunIndividual6502Test1A()
        {
            RunNamedBatch(CpuClass.WDC6502, "1a");
        }

        [TestMethod]
        public void RunIndividual6502Test1B()
        {
            RunNamedBatch(CpuClass.WDC6502, "1b");
        }

        [TestMethod]
        public void RunIndividual6502Test1C()
        {
            RunNamedBatch(CpuClass.WDC6502, "1c");
        }

        [TestMethod]
        public void RunIndividual6502Test1D()
        {
            RunNamedBatch(CpuClass.WDC6502, "1d");
        }

        [TestMethod]
        public void RunIndividual6502Test1E()
        {
            RunNamedBatch(CpuClass.WDC6502, "1e");
        }

        [TestMethod]
        public void RunIndividual6502Test1F()
        {
            RunNamedBatch(CpuClass.WDC6502, "1f");
        }

        [TestMethod]
        public void RunIndividual6502Test20()
        {
            RunNamedBatch(CpuClass.WDC6502, "20");
        }

        [TestMethod]
        public void RunIndividual6502Test21()
        {
            RunNamedBatch(CpuClass.WDC6502, "21");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test22()
        {
            RunNamedBatch(CpuClass.WDC6502, "22");
        }

        [TestMethod]
        public void RunIndividual6502Test23()
        {
            RunNamedBatch(CpuClass.WDC6502, "23");
        }

        [TestMethod]
        public void RunIndividual6502Test24()
        {
            RunNamedBatch(CpuClass.WDC6502, "24");
        }

        [TestMethod]
        public void RunIndividual6502Test25()
        {
            RunNamedBatch(CpuClass.WDC6502, "25");
        }

        [TestMethod]
        public void RunIndividual6502Test26()
        {
            RunNamedBatch(CpuClass.WDC6502, "26");
        }

        [TestMethod]
        public void RunIndividual6502Test27()
        {
            RunNamedBatch(CpuClass.WDC6502, "27");
        }

        [TestMethod]
        public void RunIndividual6502Test28()
        {
            RunNamedBatch(CpuClass.WDC6502, "28");
        }

        [TestMethod]
        public void RunIndividual6502Test29()
        {
            RunNamedBatch(CpuClass.WDC6502, "29");
        }

        [TestMethod]
        public void RunIndividual6502Test2A()
        {
            RunNamedBatch(CpuClass.WDC6502, "2a");
        }

        [TestMethod]
        public void RunIndividual6502Test2B()
        {
            RunNamedBatch(CpuClass.WDC6502, "2b");
        }

        [TestMethod]
        public void RunIndividual6502Test2C()
        {
            RunNamedBatch(CpuClass.WDC6502, "2c");
        }

        [TestMethod]
        public void RunIndividual6502Test2D()
        {
            RunNamedBatch(CpuClass.WDC6502, "2d");
        }

        [TestMethod]
        public void RunIndividual6502Test2E()
        {
            RunNamedBatch(CpuClass.WDC6502, "2e");
        }

        [TestMethod]
        public void RunIndividual6502Test2F()
        {
            RunNamedBatch(CpuClass.WDC6502, "2f");
        }

        [TestMethod]
        public void RunIndividual6502Test30()
        {
            RunNamedBatch(CpuClass.WDC6502, "30");
        }

        [TestMethod]
        public void RunIndividual6502Test31()
        {
            RunNamedBatch(CpuClass.WDC6502, "31");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test32()
        {
            RunNamedBatch(CpuClass.WDC6502, "32");
        }

        [TestMethod]
        public void RunIndividual6502Test33()
        {
            RunNamedBatch(CpuClass.WDC6502, "33");
        }

        [TestMethod]
        public void RunIndividual6502Test34()
        {
            RunNamedBatch(CpuClass.WDC6502, "34");
        }

        [TestMethod]
        public void RunIndividual6502Test35()
        {
            RunNamedBatch(CpuClass.WDC6502, "35");
        }

        [TestMethod]
        public void RunIndividual6502Test36()
        {
            RunNamedBatch(CpuClass.WDC6502, "36");
        }

        [TestMethod]
        public void RunIndividual6502Test37()
        {
            RunNamedBatch(CpuClass.WDC6502, "37");
        }

        [TestMethod]
        public void RunIndividual6502Test38()
        {
            RunNamedBatch(CpuClass.WDC6502, "38");
        }

        [TestMethod]
        public void RunIndividual6502Test39()
        {
            RunNamedBatch(CpuClass.WDC6502, "39");
        }

        [TestMethod]
        public void RunIndividual6502Test3A()
        {
            RunNamedBatch(CpuClass.WDC6502, "3a");
        }

        [TestMethod]
        public void RunIndividual6502Test3B()
        {
            RunNamedBatch(CpuClass.WDC6502, "3b");
        }

        [TestMethod]
        public void RunIndividual6502Test3C()
        {
            RunNamedBatch(CpuClass.WDC6502, "3c");
        }

        [TestMethod]
        public void RunIndividual6502Test3D()
        {
            RunNamedBatch(CpuClass.WDC6502, "3d");
        }

        [TestMethod]
        public void RunIndividual6502Test3E()
        {
            RunNamedBatch(CpuClass.WDC6502, "3e");
        }

        [TestMethod]
        public void RunIndividual6502Test3F()
        {
            RunNamedBatch(CpuClass.WDC6502, "3f");
        }

        [TestMethod]
        public void RunIndividual6502Test40()
        {
            RunNamedBatch(CpuClass.WDC6502, "40");
        }

        [TestMethod]
        public void RunIndividual6502Test41()
        {
            RunNamedBatch(CpuClass.WDC6502, "41");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test42()
        {
            RunNamedBatch(CpuClass.WDC6502, "42");
        }

        [TestMethod]
        public void RunIndividual6502Test43()
        {
            RunNamedBatch(CpuClass.WDC6502, "43");
        }

        [TestMethod]
        public void RunIndividual6502Test44()
        {
            RunNamedBatch(CpuClass.WDC6502, "44");
        }

        [TestMethod]
        public void RunIndividual6502Test45()
        {
            RunNamedBatch(CpuClass.WDC6502, "45");
        }

        [TestMethod]
        public void RunIndividual6502Test46()
        {
            RunNamedBatch(CpuClass.WDC6502, "46");
        }

        [TestMethod]
        public void RunIndividual6502Test47()
        {
            RunNamedBatch(CpuClass.WDC6502, "47");
        }

        [TestMethod]
        public void RunIndividual6502Test48()
        {
            RunNamedBatch(CpuClass.WDC6502, "48");
        }

        [TestMethod]
        public void RunIndividual6502Test49()
        {
            RunNamedBatch(CpuClass.WDC6502, "49");
        }

        [TestMethod]
        public void RunIndividual6502Test4A()
        {
            RunNamedBatch(CpuClass.WDC6502, "4a");
        }

        [TestMethod]
        public void RunIndividual6502Test4B()
        {
            RunNamedBatch(CpuClass.WDC6502, "4b");
        }

        [TestMethod]
        public void RunIndividual6502Test4C()
        {
            RunNamedBatch(CpuClass.WDC6502, "4c");
        }

        [TestMethod]
        public void RunIndividual6502Test4D()
        {
            RunNamedBatch(CpuClass.WDC6502, "4d");
        }

        [TestMethod]
        public void RunIndividual6502Test4E()
        {
            RunNamedBatch(CpuClass.WDC6502, "4e");
        }

        [TestMethod]
        public void RunIndividual6502Test4F()
        {
            RunNamedBatch(CpuClass.WDC6502, "4f");
        }

        [TestMethod]
        public void RunIndividual6502Test50()
        {
            RunNamedBatch(CpuClass.WDC6502, "50");
        }

        [TestMethod]
        public void RunIndividual6502Test51()
        {
            RunNamedBatch(CpuClass.WDC6502, "51");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test52()
        {
            RunNamedBatch(CpuClass.WDC6502, "52");
        }

        [TestMethod]
        public void RunIndividual6502Test53()
        {
            RunNamedBatch(CpuClass.WDC6502, "53");
        }

        [TestMethod]
        public void RunIndividual6502Test54()
        {
            RunNamedBatch(CpuClass.WDC6502, "54");
        }

        [TestMethod]
        public void RunIndividual6502Test55()
        {
            RunNamedBatch(CpuClass.WDC6502, "55");
        }

        [TestMethod]
        public void RunIndividual6502Test56()
        {
            RunNamedBatch(CpuClass.WDC6502, "56");
        }

        [TestMethod]
        public void RunIndividual6502Test57()
        {
            RunNamedBatch(CpuClass.WDC6502, "57");
        }

        [TestMethod]
        public void RunIndividual6502Test58()
        {
            RunNamedBatch(CpuClass.WDC6502, "58");
        }

        [TestMethod]
        public void RunIndividual6502Test59()
        {
            RunNamedBatch(CpuClass.WDC6502, "59");
        }

        [TestMethod]
        public void RunIndividual6502Test5A()
        {
            RunNamedBatch(CpuClass.WDC6502, "5a");
        }

        [TestMethod]
        public void RunIndividual6502Test5B()
        {
            RunNamedBatch(CpuClass.WDC6502, "5b");
        }

        [TestMethod]
        public void RunIndividual6502Test5C()
        {
            RunNamedBatch(CpuClass.WDC6502, "5c");
        }

        [TestMethod]
        public void RunIndividual6502Test5D()
        {
            RunNamedBatch(CpuClass.WDC6502, "5d");
        }

        [TestMethod]
        public void RunIndividual6502Test5E()
        {
            RunNamedBatch(CpuClass.WDC6502, "5e");
        }

        [TestMethod]
        public void RunIndividual6502Test5F()
        {
            RunNamedBatch(CpuClass.WDC6502, "5f");
        }

        [TestMethod]
        public void RunIndividual6502Test60()
        {
            RunNamedBatch(CpuClass.WDC6502, "60");
        }

        [TestMethod]
        public void RunIndividual6502Test61()
        {
            RunNamedBatch(CpuClass.WDC6502, "61");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test62()
        {
            RunNamedBatch(CpuClass.WDC6502, "62");
        }

        [TestMethod]
        public void RunIndividual6502Test63()
        {
            RunNamedBatch(CpuClass.WDC6502, "63");
        }

        [TestMethod]
        public void RunIndividual6502Test64()
        {
            RunNamedBatch(CpuClass.WDC6502, "64");
        }

        [TestMethod]
        public void RunIndividual6502Test65()
        {
            RunNamedBatch(CpuClass.WDC6502, "65");
        }

        [TestMethod]
        public void RunIndividual6502Test66()
        {
            RunNamedBatch(CpuClass.WDC6502, "66");
        }

        [TestMethod]
        public void RunIndividual6502Test67()
        {
            RunNamedBatch(CpuClass.WDC6502, "67");
        }

        [TestMethod]
        public void RunIndividual6502Test68()
        {
            RunNamedBatch(CpuClass.WDC6502, "68");
        }

        [TestMethod]
        public void RunIndividual6502Test69()
        {
            RunNamedBatch(CpuClass.WDC6502, "69");
        }

        [TestMethod]
        public void RunIndividual6502Test6A()
        {
            RunNamedBatch(CpuClass.WDC6502, "6a");
        }

        [TestMethod]
        public void RunIndividual6502Test6B()
        {
            RunNamedBatch(CpuClass.WDC6502, "6b");
        }

        [TestMethod]
        public void RunIndividual6502Test6C()
        {
            RunNamedBatch(CpuClass.WDC6502, "6c");
        }

        [TestMethod]
        public void RunIndividual6502Test6D()
        {
            RunNamedBatch(CpuClass.WDC6502, "6d");
        }

        [TestMethod]
        public void RunIndividual6502Test6E()
        {
            RunNamedBatch(CpuClass.WDC6502, "6e");
        }

        [TestMethod]
        public void RunIndividual6502Test6F()
        {
            RunNamedBatch(CpuClass.WDC6502, "6f");
        }

        [TestMethod]
        public void RunIndividual6502Test70()
        {
            RunNamedBatch(CpuClass.WDC6502, "70");
        }

        [TestMethod]
        public void RunIndividual6502Test71()
        {
            RunNamedBatch(CpuClass.WDC6502, "71");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test72()
        {
            RunNamedBatch(CpuClass.WDC6502, "72");
        }

        [TestMethod]
        public void RunIndividual6502Test73()
        {
            RunNamedBatch(CpuClass.WDC6502, "73");
        }

        [TestMethod]
        public void RunIndividual6502Test74()
        {
            RunNamedBatch(CpuClass.WDC6502, "74");
        }

        [TestMethod]
        public void RunIndividual6502Test75()
        {
            RunNamedBatch(CpuClass.WDC6502, "75");
        }

        [TestMethod]
        public void RunIndividual6502Test76()
        {
            RunNamedBatch(CpuClass.WDC6502, "76");
        }

        [TestMethod]
        public void RunIndividual6502Test77()
        {
            RunNamedBatch(CpuClass.WDC6502, "77");
        }

        [TestMethod]
        public void RunIndividual6502Test78()
        {
            RunNamedBatch(CpuClass.WDC6502, "78");
        }

        [TestMethod]
        public void RunIndividual6502Test79()
        {
            RunNamedBatch(CpuClass.WDC6502, "79");
        }

        [TestMethod]
        public void RunIndividual6502Test7A()
        {
            RunNamedBatch(CpuClass.WDC6502, "7a");
        }

        [TestMethod]
        public void RunIndividual6502Test7B()
        {
            RunNamedBatch(CpuClass.WDC6502, "7b");
        }

        [TestMethod]
        public void RunIndividual6502Test7C()
        {
            RunNamedBatch(CpuClass.WDC6502, "7c");
        }

        [TestMethod]
        public void RunIndividual6502Test7D()
        {
            RunNamedBatch(CpuClass.WDC6502, "7d");
        }

        [TestMethod]
        public void RunIndividual6502Test7E()
        {
            RunNamedBatch(CpuClass.WDC6502, "7e");
        }

        [TestMethod]
        public void RunIndividual6502Test7F()
        {
            RunNamedBatch(CpuClass.WDC6502, "7f");
        }

        [TestMethod]
        public void RunIndividual6502Test80()
        {
            RunNamedBatch(CpuClass.WDC6502, "80");
        }

        [TestMethod]
        public void RunIndividual6502Test81()
        {
            RunNamedBatch(CpuClass.WDC6502, "81");
        }

        [TestMethod]
        public void RunIndividual6502Test82()
        {
            RunNamedBatch(CpuClass.WDC6502, "82");
        }

        [TestMethod]
        public void RunIndividual6502Test83()
        {
            RunNamedBatch(CpuClass.WDC6502, "83");
        }

        [TestMethod]
        public void RunIndividual6502Test84()
        {
            RunNamedBatch(CpuClass.WDC6502, "84");
        }

        [TestMethod]
        public void RunIndividual6502Test85()
        {
            RunNamedBatch(CpuClass.WDC6502, "85");
        }

        [TestMethod]
        public void RunIndividual6502Test86()
        {
            RunNamedBatch(CpuClass.WDC6502, "86");
        }

        [TestMethod]
        public void RunIndividual6502Test87()
        {
            RunNamedBatch(CpuClass.WDC6502, "87");
        }

        [TestMethod]
        public void RunIndividual6502Test88()
        {
            RunNamedBatch(CpuClass.WDC6502, "88");
        }

        [TestMethod]
        public void RunIndividual6502Test89()
        {
            RunNamedBatch(CpuClass.WDC6502, "89");
        }

        [TestMethod]
        public void RunIndividual6502Test8A()
        {
            RunNamedBatch(CpuClass.WDC6502, "8a");
        }

        [TestMethod]
        public void RunIndividual6502Test8B()
        {
            RunNamedBatch(CpuClass.WDC6502, "8b");
        }

        [TestMethod]
        public void RunIndividual6502Test8C()
        {
            RunNamedBatch(CpuClass.WDC6502, "8c");
        }

        [TestMethod]
        public void RunIndividual6502Test8D()
        {
            RunNamedBatch(CpuClass.WDC6502, "8d");
        }

        [TestMethod]
        public void RunIndividual6502Test8E()
        {
            RunNamedBatch(CpuClass.WDC6502, "8e");
        }

        [TestMethod]
        public void RunIndividual6502Test8F()
        {
            RunNamedBatch(CpuClass.WDC6502, "8f");
        }

        [TestMethod]
        public void RunIndividual6502Test90()
        {
            RunNamedBatch(CpuClass.WDC6502, "90");
        }

        [TestMethod]
        public void RunIndividual6502Test91()
        {
            RunNamedBatch(CpuClass.WDC6502, "91");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502Test92()
        {
            RunNamedBatch(CpuClass.WDC6502, "92");
        }

        [TestMethod]
        public void RunIndividual6502Test93()
        {
            RunNamedBatch(CpuClass.WDC6502, "93");
        }

        [TestMethod]
        public void RunIndividual6502Test94()
        {
            RunNamedBatch(CpuClass.WDC6502, "94");
        }

        [TestMethod]
        public void RunIndividual6502Test95()
        {
            RunNamedBatch(CpuClass.WDC6502, "95");
        }

        [TestMethod]
        public void RunIndividual6502Test96()
        {
            RunNamedBatch(CpuClass.WDC6502, "96");
        }

        [TestMethod]
        public void RunIndividual6502Test97()
        {
            RunNamedBatch(CpuClass.WDC6502, "97");
        }

        [TestMethod]
        public void RunIndividual6502Test98()
        {
            RunNamedBatch(CpuClass.WDC6502, "98");
        }

        [TestMethod]
        public void RunIndividual6502Test99()
        {
            RunNamedBatch(CpuClass.WDC6502, "99");
        }

        [TestMethod]
        public void RunIndividual6502Test9A()
        {
            RunNamedBatch(CpuClass.WDC6502, "9a");
        }

        [TestMethod]
        public void RunIndividual6502Test9B()
        {
            RunNamedBatch(CpuClass.WDC6502, "9b");
        }

        [TestMethod]
        public void RunIndividual6502Test9C()
        {
            RunNamedBatch(CpuClass.WDC6502, "9c");
        }

        [TestMethod]
        public void RunIndividual6502Test9D()
        {
            RunNamedBatch(CpuClass.WDC6502, "9d");
        }

        [TestMethod]
        public void RunIndividual6502Test9E()
        {
            RunNamedBatch(CpuClass.WDC6502, "9e");
        }

        [TestMethod]
        public void RunIndividual6502Test9F()
        {
            RunNamedBatch(CpuClass.WDC6502, "9f");
        }

        [TestMethod]
        public void RunIndividual6502TestA0()
        {
            RunNamedBatch(CpuClass.WDC6502, "a0");
        }

        [TestMethod]
        public void RunIndividual6502TestA1()
        {
            RunNamedBatch(CpuClass.WDC6502, "a1");
        }

        [TestMethod]
        public void RunIndividual6502TestA2()
        {
            RunNamedBatch(CpuClass.WDC6502, "a2");
        }

        [TestMethod]
        public void RunIndividual6502TestA3()
        {
            RunNamedBatch(CpuClass.WDC6502, "a3");
        }

        [TestMethod]
        public void RunIndividual6502TestA4()
        {
            RunNamedBatch(CpuClass.WDC6502, "a4");
        }

        [TestMethod]
        public void RunIndividual6502TestA5()
        {
            RunNamedBatch(CpuClass.WDC6502, "a5");
        }

        [TestMethod]
        public void RunIndividual6502TestA6()
        {
            RunNamedBatch(CpuClass.WDC6502, "a6");
        }

        [TestMethod]
        public void RunIndividual6502TestA7()
        {
            RunNamedBatch(CpuClass.WDC6502, "a7");
        }

        [TestMethod]
        public void RunIndividual6502TestA8()
        {
            RunNamedBatch(CpuClass.WDC6502, "a8");
        }

        [TestMethod]
        public void RunIndividual6502TestA9()
        {
            RunNamedBatch(CpuClass.WDC6502, "a9");
        }

        [TestMethod]
        public void RunIndividual6502TestAA()
        {
            RunNamedBatch(CpuClass.WDC6502, "aa");
        }

        [TestMethod]
        public void RunIndividual6502TestAB()
        {
            RunNamedBatch(CpuClass.WDC6502, "ab");
        }

        [TestMethod]
        public void RunIndividual6502TestAC()
        {
            RunNamedBatch(CpuClass.WDC6502, "ac");
        }

        [TestMethod]
        public void RunIndividual6502TestAD()
        {
            RunNamedBatch(CpuClass.WDC6502, "ad");
        }

        [TestMethod]
        public void RunIndividual6502TestAE()
        {
            RunNamedBatch(CpuClass.WDC6502, "ae");
        }

        [TestMethod]
        public void RunIndividual6502TestAF()
        {
            RunNamedBatch(CpuClass.WDC6502, "af");
        }

        [TestMethod]
        public void RunIndividual6502TestB0()
        {
            RunNamedBatch(CpuClass.WDC6502, "b0");
        }

        [TestMethod]
        public void RunIndividual6502TestB1()
        {
            RunNamedBatch(CpuClass.WDC6502, "b1");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502TestB2()
        {
            RunNamedBatch(CpuClass.WDC6502, "b2");
        }

        [TestMethod]
        public void RunIndividual6502TestB3()
        {
            RunNamedBatch(CpuClass.WDC6502, "b3");
        }

        [TestMethod]
        public void RunIndividual6502TestB4()
        {
            RunNamedBatch(CpuClass.WDC6502, "b4");
        }

        [TestMethod]
        public void RunIndividual6502TestB5()
        {
            RunNamedBatch(CpuClass.WDC6502, "b5");
        }

        [TestMethod]
        public void RunIndividual6502TestB6()
        {
            RunNamedBatch(CpuClass.WDC6502, "b6");
        }

        [TestMethod]
        public void RunIndividual6502TestB7()
        {
            RunNamedBatch(CpuClass.WDC6502, "b7");
        }

        [TestMethod]
        public void RunIndividual6502TestB8()
        {
            RunNamedBatch(CpuClass.WDC6502, "b8");
        }

        [TestMethod]
        public void RunIndividual6502TestB9()
        {
            RunNamedBatch(CpuClass.WDC6502, "b9");
        }

        [TestMethod]
        public void RunIndividual6502TestBA()
        {
            RunNamedBatch(CpuClass.WDC6502, "ba");
        }

        [TestMethod]
        public void RunIndividual6502TestBB()
        {
            RunNamedBatch(CpuClass.WDC6502, "bb");
        }

        [TestMethod]
        public void RunIndividual6502TestBC()
        {
            RunNamedBatch(CpuClass.WDC6502, "bc");
        }

        [TestMethod]
        public void RunIndividual6502TestBD()
        {
            RunNamedBatch(CpuClass.WDC6502, "bd");
        }

        [TestMethod]
        public void RunIndividual6502TestBE()
        {
            RunNamedBatch(CpuClass.WDC6502, "be");
        }

        [TestMethod]
        public void RunIndividual6502TestBF()
        {
            RunNamedBatch(CpuClass.WDC6502, "bf");
        }

        [TestMethod]
        public void RunIndividual6502TestC0()
        {
            RunNamedBatch(CpuClass.WDC6502, "c0");
        }

        [TestMethod]
        public void RunIndividual6502TestC1()
        {
            RunNamedBatch(CpuClass.WDC6502, "c1");
        }

        [TestMethod]
        public void RunIndividual6502TestC2()
        {
            RunNamedBatch(CpuClass.WDC6502, "c2");
        }

        [TestMethod]
        public void RunIndividual6502TestC3()
        {
            RunNamedBatch(CpuClass.WDC6502, "c3");
        }

        [TestMethod]
        public void RunIndividual6502TestC4()
        {
            RunNamedBatch(CpuClass.WDC6502, "c4");
        }

        [TestMethod]
        public void RunIndividual6502TestC5()
        {
            RunNamedBatch(CpuClass.WDC6502, "c5");
        }

        [TestMethod]
        public void RunIndividual6502TestC6()
        {
            RunNamedBatch(CpuClass.WDC6502, "c6");
        }

        [TestMethod]
        public void RunIndividual6502TestC7()
        {
            RunNamedBatch(CpuClass.WDC6502, "c7");
        }

        [TestMethod]
        public void RunIndividual6502TestC8()
        {
            RunNamedBatch(CpuClass.WDC6502, "c8");
        }

        [TestMethod]
        public void RunIndividual6502TestC9()
        {
            RunNamedBatch(CpuClass.WDC6502, "c9");
        }

        [TestMethod]
        public void RunIndividual6502TestCA()
        {
            RunNamedBatch(CpuClass.WDC6502, "ca");
        }

        [TestMethod]
        public void RunIndividual6502TestCB()
        {
            RunNamedBatch(CpuClass.WDC6502, "cb");
        }

        [TestMethod]
        public void RunIndividual6502TestCC()
        {
            RunNamedBatch(CpuClass.WDC6502, "cc");
        }

        [TestMethod]
        public void RunIndividual6502TestCD()
        {
            RunNamedBatch(CpuClass.WDC6502, "cd");
        }

        [TestMethod]
        public void RunIndividual6502TestCE()
        {
            RunNamedBatch(CpuClass.WDC6502, "ce");
        }

        [TestMethod]
        public void RunIndividual6502TestCF()
        {
            RunNamedBatch(CpuClass.WDC6502, "cf");
        }

        [TestMethod]
        public void RunIndividual6502TestD0()
        {
            RunNamedBatch(CpuClass.WDC6502, "d0");
        }

        [TestMethod]
        public void RunIndividual6502TestD1()
        {
            RunNamedBatch(CpuClass.WDC6502, "d1");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502TestD2()
        {
            RunNamedBatch(CpuClass.WDC6502, "d2");
        }

        [TestMethod]
        public void RunIndividual6502TestD3()
        {
            RunNamedBatch(CpuClass.WDC6502, "d3");
        }

        [TestMethod]
        public void RunIndividual6502TestD4()
        {
            RunNamedBatch(CpuClass.WDC6502, "d4");
        }

        [TestMethod]
        public void RunIndividual6502TestD5()
        {
            RunNamedBatch(CpuClass.WDC6502, "d5");
        }

        [TestMethod]
        public void RunIndividual6502TestD6()
        {
            RunNamedBatch(CpuClass.WDC6502, "d6");
        }

        [TestMethod]
        public void RunIndividual6502TestD7()
        {
            RunNamedBatch(CpuClass.WDC6502, "d7");
        }

        [TestMethod]
        public void RunIndividual6502TestD8()
        {
            RunNamedBatch(CpuClass.WDC6502, "d8");
        }

        [TestMethod]
        public void RunIndividual6502TestD9()
        {
            RunNamedBatch(CpuClass.WDC6502, "d9");
        }

        [TestMethod]
        public void RunIndividual6502TestDA()
        {
            RunNamedBatch(CpuClass.WDC6502, "da");
        }

        [TestMethod]
        public void RunIndividual6502TestDB()
        {
            RunNamedBatch(CpuClass.WDC6502, "db");
        }

        [TestMethod]
        public void RunIndividual6502TestDC()
        {
            RunNamedBatch(CpuClass.WDC6502, "dc");
        }

        [TestMethod]
        public void RunIndividual6502TestDD()
        {
            RunNamedBatch(CpuClass.WDC6502, "dd");
        }

        [TestMethod]
        public void RunIndividual6502TestDE()
        {
            RunNamedBatch(CpuClass.WDC6502, "de");
        }

        [TestMethod]
        public void RunIndividual6502TestDF()
        {
            RunNamedBatch(CpuClass.WDC6502, "df");
        }

        [TestMethod]
        public void RunIndividual6502TestE0()
        {
            RunNamedBatch(CpuClass.WDC6502, "e0");
        }

        [TestMethod]
        public void RunIndividual6502TestE1()
        {
            RunNamedBatch(CpuClass.WDC6502, "e1");
        }

        [TestMethod]
        public void RunIndividual6502TestE2()
        {
            RunNamedBatch(CpuClass.WDC6502, "e2");
        }

        [TestMethod]
        public void RunIndividual6502TestE3()
        {
            RunNamedBatch(CpuClass.WDC6502, "e3");
        }

        [TestMethod]
        public void RunIndividual6502TestE4()
        {
            RunNamedBatch(CpuClass.WDC6502, "e4");
        }

        [TestMethod]
        public void RunIndividual6502TestE5()
        {
            RunNamedBatch(CpuClass.WDC6502, "e5");
        }

        [TestMethod]
        public void RunIndividual6502TestE6()
        {
            RunNamedBatch(CpuClass.WDC6502, "e6");
        }

        [TestMethod]
        public void RunIndividual6502TestE7()
        {
            RunNamedBatch(CpuClass.WDC6502, "e7");
        }

        [TestMethod]
        public void RunIndividual6502TestE8()
        {
            RunNamedBatch(CpuClass.WDC6502, "e8");
        }

        [TestMethod]
        public void RunIndividual6502TestE9()
        {
            RunNamedBatch(CpuClass.WDC6502, "e9");
        }

        [TestMethod]
        public void RunIndividual6502TestEA()
        {
            RunNamedBatch(CpuClass.WDC6502, "ea");
        }

        [TestMethod]
        public void RunIndividual6502TestEB()
        {
            RunNamedBatch(CpuClass.WDC6502, "eb");
        }

        [TestMethod]
        public void RunIndividual6502TestEC()
        {
            RunNamedBatch(CpuClass.WDC6502, "ec");
        }

        [TestMethod]
        public void RunIndividual6502TestED()
        {
            RunNamedBatch(CpuClass.WDC6502, "ed");
        }

        [TestMethod]
        public void RunIndividual6502TestEE()
        {
            RunNamedBatch(CpuClass.WDC6502, "ee");
        }

        [TestMethod]
        public void RunIndividual6502TestEF()
        {
            RunNamedBatch(CpuClass.WDC6502, "ef");
        }

        [TestMethod]
        public void RunIndividual6502TestF0()
        {
            RunNamedBatch(CpuClass.WDC6502, "f0");
        }

        [TestMethod]
        public void RunIndividual6502TestF1()
        {
            RunNamedBatch(CpuClass.WDC6502, "f1");
        }

        [TestMethod]
        [ExpectedException(typeof(IllegalOpCodeException))]
        public void RunIndividual6502TestF2()
        {
            RunNamedBatch(CpuClass.WDC6502, "f2");
        }

        [TestMethod]
        public void RunIndividual6502TestF3()
        {
            RunNamedBatch(CpuClass.WDC6502, "f3");
        }

        [TestMethod]
        public void RunIndividual6502TestF4()
        {
            RunNamedBatch(CpuClass.WDC6502, "f4");
        }

        [TestMethod]
        public void RunIndividual6502TestF5()
        {
            RunNamedBatch(CpuClass.WDC6502, "f5");
        }

        [TestMethod]
        public void RunIndividual6502TestF6()
        {
            RunNamedBatch(CpuClass.WDC6502, "f6");
        }

        [TestMethod]
        public void RunIndividual6502TestF7()
        {
            RunNamedBatch(CpuClass.WDC6502, "f7");
        }

        [TestMethod]
        public void RunIndividual6502TestF8()
        {
            RunNamedBatch(CpuClass.WDC6502, "f8");
        }

        [TestMethod]
        public void RunIndividual6502TestF9()
        {
            RunNamedBatch(CpuClass.WDC6502, "f9");
        }

        [TestMethod]
        public void RunIndividual6502TestFA()
        {
            RunNamedBatch(CpuClass.WDC6502, "fa");
        }

        [TestMethod]
        public void RunIndividual6502TestFB()
        {
            RunNamedBatch(CpuClass.WDC6502, "fb");
        }

        [TestMethod]
        public void RunIndividual6502TestFC()
        {
            RunNamedBatch(CpuClass.WDC6502, "fc");
        }

        [TestMethod]
        public void RunIndividual6502TestFD()
        {
            RunNamedBatch(CpuClass.WDC6502, "fd");
        }

        [TestMethod]
        public void RunIndividual6502TestFE()
        {
            RunNamedBatch(CpuClass.WDC6502, "fe");
        }

        [TestMethod]
        public void RunIndividual6502TestFF()
        {
            RunNamedBatch(CpuClass.WDC6502, "ff");
        }
    }
}
