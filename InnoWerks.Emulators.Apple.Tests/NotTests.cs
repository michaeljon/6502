using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#pragma warning disable CA1823, RCS1213

namespace InnoWerks.Emulators.Apple
{
    [TestClass]
    public class NotTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void WriteSoftSwitchListByAddress()
        {
            foreach (var (key, value) in SoftSwitchAddress.Lookup.OrderBy(p => p.Key))
            {
                TestContext.WriteLine($"{value,-22} ${key:X4}");
            }
        }

        [TestMethod]
        public void WriteSoftSwitchListByName()
        {
            foreach (var (key, value) in SoftSwitchAddress.Lookup.OrderBy(p => p.Value))
            {
                TestContext.WriteLine($"{value,-22} ${key:X4}");
            }
        }
    }
}
