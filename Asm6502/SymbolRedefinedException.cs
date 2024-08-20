using System;

namespace Asm6502
{
#pragma warning disable IDE0290

    public class SymbolRedefinedException : Exception
    {
        public SymbolRedefinedException(string symbol) : base($"Symbol {symbol} has already been defined.") { }
    }
}