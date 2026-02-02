using System;
using System.IO;
using System.Runtime.InteropServices;
using WozParse.Chunks;

namespace WozParse;

static class Program
{
    static void Main(string[] args)
    {
        byte[] byteArray = File.ReadAllBytes("dos-33-system-master.woz");
        Span<byte> bytes = new(byteArray);

        var streamPosition = 0;
        var preamble = Preamble.Read(bytes, ref streamPosition);
        Console.WriteLine(preamble);

        var info = Info.Read(bytes, ref streamPosition);
        Console.WriteLine(info);
    }
}
