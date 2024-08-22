using System;

namespace InnoWerks.Assemblers
{
    public class SymbolRedefinedException : Exception
    {
        public SymbolRedefinedException() { }

        public SymbolRedefinedException(string message) : base(message) { }

        public SymbolRedefinedException(string message, Exception innerException) : base(message, innerException) { }

        public SymbolRedefinedException(Symbol symbol, int lineNumber) : base($"Symbol {symbol} at {lineNumber} has already been defined.") { }
    }
}
