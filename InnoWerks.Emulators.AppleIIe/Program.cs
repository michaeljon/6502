using System;
using CommandLine;

namespace InnoWerks.Emulators.AppleIIe
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<CliOptions>(args);

            result.MapResult(
                Run,

                errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.Error.WriteLine(error.ToString());
                    }

                    return 1;
                }
            );
        }

        private static int Run(CliOptions options)
        {
            using var game = new Emulator(options);
            game.Run();

            return 0;
        }
    }
}
