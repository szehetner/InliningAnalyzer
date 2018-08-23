using System;

namespace InliningAnalyzer
{
    public class JitCompilerException : Exception
    {
        public JitCompilerException(string message) : base(message)
        {
        }
    }
}