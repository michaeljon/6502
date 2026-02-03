using System;
using System.IO;
using System.Runtime.InteropServices;
using WozParse.Chunks;

namespace WozParse;

static class Program
{
    static void Main(string[] args)
    {
        WozParser.Parse("dos-33-system-master.woz");
    }
}
