using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public static class InprocessAnalyzer
    {
        public static (AssemblyCallGraph, List<InliningEvent>) Analyze(Assembly assembly)
        {
            using (var analyzer = new Analyzer(true))
            {
                JitCompiler jitCompiler = new JitCompiler(assembly);

                analyzer.StartEventTrace(Process.GetCurrentProcess().Id);
                jitCompiler.PreJITMethods();
                analyzer.StopEventTrace();

                return (analyzer.AssemblyCallGraph, analyzer.EventDetails);
            }
        }
    }
}
