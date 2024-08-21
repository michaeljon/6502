using System;

namespace Asm6502
{
#pragma warning disable IDE0290

    public class SymbolRedefinedException : Exception
    {
        public SymbolRedefinedException(Symbol symbol, int lineNumber) : base($"Symbol {symbol} at {lineNumber} has already been defined.") { }
    }
}