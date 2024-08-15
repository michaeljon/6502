namespace InnoWerks.Simulators.MathTester
{
    internal sealed class Program
    {
        private static byte Dec(byte a) => (byte)((a - 1) & 0xff);

        private static void Main(string[] _)
        {
            byte a = 0;
            bool b = true;

            var x = a << 1 | (b ? 1 : 0);
        }
    }
}
