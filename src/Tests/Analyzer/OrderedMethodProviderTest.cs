using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Model;
using VsExtension.Shell.Runner;

namespace Tests.Analyzer
{
    [TestClass]
    public class OrderedMethodProviderTest
    {
        [TestMethod]
        public void Test()
        {
            var assemblyFile = RoslynCompiler.CreateAssembly("Tests.Model.Samples.MethodOrdering.cs");

            JitRunner runner = new JitRunner(assemblyFile, InliningAnalyzer.PlatformTarget.X64, null, new ConsoleLogger(), true);
            var assemblyCallGraph = runner.Run();

            Console.WriteLine("Events:\r\n");
            foreach (var e in assemblyCallGraph.EventDetails)
                Console.WriteLine(e.ToString());

        }
    }
    
}
