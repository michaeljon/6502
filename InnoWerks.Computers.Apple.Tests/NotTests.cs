using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1823, RCS1213

namespace InnoWerks.Computers.Apple
{
    [TestClass]
    public class NotTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void WriteSoftSwitchListByAddress()
        {
            using (var f = File.CreateText("SoftSwitchListByAddress.tsv"))
            {
                foreach (var (key, value) in SoftSwitchAddress.Lookup.OrderBy(p => p.Key))
                {
                    f.WriteLine($"{value,-22} ${key:X4}");
                }
            }
        }

        [TestMethod]
        public void WriteSoftSwitchListByName()
        {
            using (var f = File.CreateText("SoftSwitchListByName.tsv"))
            {
                foreach (var (key, value) in SoftSwitchAddress.Lookup.OrderBy(p => p.Value))
                {
                    f.WriteLine($"{value,-22} ${key:X4}");
                }
            }
        }
    }
}
