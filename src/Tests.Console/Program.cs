using InliningAnalyzer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var analyzer = new Analyzer())
            {
                JitCompiler compiler = new JitCompiler(@"K:\work\Open Source Projects\ravendb-3.5\ravendb-3.5\Raven.Voron\Voron\bin\Release\Voron.dll");

                analyzer.StartEventTrace(Process.GetCurrentProcess().Id);
                compiler.PreJITMethods();
                analyzer.StopEventTrace();

                var info = analyzer.AssemblyCallGraph.Types.Where(p => p.Key.Contains("Transaction")).ToList();
            }

            //using (var analyzer = new Analyzer())
            //{
            //    JitHostController jitController = new JitHostController(@"K:\work\Open Source Projects\ravendb-3.5\ravendb-3.5\Raven.Voron\Voron\bin\Release\Voron.dll", PlatformTarget.X64);
            //    jitController.StartProcess();

            //    analyzer.StartEventTrace(jitController.Process.Id);
            //    jitController.RunJitCompilation();
            //    analyzer.StopEventTrace();

            //    var graph = analyzer.AssemblyCallGraph;
            //}

            //Analyzer analyzer = new Analyzer(Assembly.LoadFrom());
            //var jitInfo = analyzer.AnalyzeAssembly();

            //jitInfo.SerializeToFile(@"C:\work\InliningAnalyzer\temp\Sample.bin");
            System.Console.WriteLine("finished");
            System.Console.ReadKey();
        }
    }
}
