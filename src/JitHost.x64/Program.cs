using InliningAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JitHost.x64
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            JitCompiler compiler = JitCompilerFactory.Create(args);
            compiler.PreJITMethods();
        }
    }
}
