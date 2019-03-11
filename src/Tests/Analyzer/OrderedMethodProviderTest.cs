using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Model;
using VsExtension.Shell.Runner;
using System.Linq;
using InliningAnalyzer;

namespace Tests.Analyzer
{
    [TestClass]
    public class OrderedMethodProviderTest
    {
        [TestMethod]
        public void Test()
        {
            var assemblyFile = RoslynCompiler.CreateAssembly("Tests.Model.Samples.MethodOrdering.cs");

            JitRunner runner = new JitRunner(assemblyFile, new JitTarget(TargetPlatform.X64, TargetRuntime.NetFramework), new TargetScope(ScopeType.AssemblyFile), new ConsoleLogger(), new TestJitPathResolver(), true);
            var assemblyCallGraph = runner.Run();

            Console.WriteLine("Events:\r\n");
            foreach (var e in assemblyCallGraph.EventDetails)
                Console.WriteLine(e.ToString());
        }

        [TestMethod]
        public void TestOverloads()
        {
            var assemblyFile = RoslynCompiler.CreateAssembly("Tests.Model.Samples.Overloads.cs");

            JitRunner runner = new JitRunner(assemblyFile, new JitTarget(TargetPlatform.X64, TargetRuntime.NetFramework), new TargetScope(ScopeType.AssemblyFile), new ConsoleLogger(), new TestJitPathResolver(), true);
            var assemblyCallGraph = runner.Run();

            var overloadType = assemblyCallGraph.GetJitType("Tests.Model.Samples.Overloads");
            var methodGroup = overloadType.Methods["A"];
            var allMethods = methodGroup.GetAllMethods().ToList();
            
            try
            {
                Assert.AreEqual(26, allMethods.Count, "Actual Methods:\r\n" + string.Join("\r\n", allMethods.Select(m => m.Signature)));
            }
            catch (Exception)
            {
                Console.WriteLine("Events:\r\n");
                foreach (var e in runner.UnorderedCallGraph.EventDetails)
                    Console.WriteLine(e.ToString());

                Console.WriteLine("Unordered Methods:\r\n");
                foreach (var e in runner.UnorderedCallGraph.GetJitType("Tests.Model.Samples.Overloads").Methods["A"].GetAllMethods().Select(m => m.Signature))
                    Console.WriteLine(e.ToString());

                throw;
            }
        }
    }
}
