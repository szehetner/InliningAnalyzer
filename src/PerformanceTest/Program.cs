using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsExtension.Shell.Runner;

namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string assemblyFile = @"";

            JitRunner jitRunner = new JitRunner(assemblyFile, InliningAnalyzer.PlatformTarget.X64, null, new NullLogger());
            jitRunner.Run();
        }
    }
}
