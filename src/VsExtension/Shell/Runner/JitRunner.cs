using InliningAnalyzer;
using InliningAnalyzer.Graph.Dependencies;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Shell.Runner
{
    public class JitRunner
    {
        private readonly string _assemblyFile;
        private readonly PlatformTarget _platformTarget;
        private readonly string _methodName;
        private readonly ILogger _outputLogger;
        private readonly bool _recordEventDetails;

        public AssemblyCallGraph UnorderedCallGraph { get; set; }

        public JitRunner(string assemblyFile, PlatformTarget platformTarget, string methodName, ILogger outputLogger, bool recordEventDetails = false)
        {
            _assemblyFile = assemblyFile;
            _platformTarget = platformTarget;
            _methodName = methodName;
            _outputLogger = outputLogger;
            _recordEventDetails = recordEventDetails;
        }

        public AssemblyCallGraph Run()
        {
            var callGraph = RunJitCompiler(CreateUnorderedController());
            if (_recordEventDetails)
                UnorderedCallGraph = callGraph;

            if (_methodName != null)
                return callGraph;

            var dependencyGraph = DependencyGraphBuilder.BuildFromCallGraph(callGraph);
            DependencyResolver resolver = new DependencyResolver(dependencyGraph);
            var methodList = resolver.GetOrderedMethodList();

            string methodListFile = SerializeMethodList(methodList);

            var orderedCallGraph = RunJitCompiler(CreateOrderedController(methodListFile));
            return orderedCallGraph;
        }

        public string SerializeMethodList(MethodCompilationList methodList)
        {
            string tempfile = Path.GetTempFileName();

            using (var file = File.OpenWrite(tempfile))
            {
                Serializer.Serialize(file, methodList);
            }
            return tempfile;
        }

        private JitHostController CreateUnorderedController()
        {
            return new JitHostController(_assemblyFile, _platformTarget, _methodName, null);
        }

        private JitHostController CreateOrderedController(string methodListFile)
        {
            return new JitHostController(_assemblyFile, _platformTarget, _methodName, methodListFile);
        }

        private AssemblyCallGraph RunJitCompiler(JitHostController jitController)
        {
            using (var etwCollector = new EtwCollector(_recordEventDetails))
            {
                jitController.StartProcess();
                jitController.Process.OutputDataReceived += JitHostOutputDataReceived;
                jitController.Process.BeginOutputReadLine();

                etwCollector.StartEventTrace(jitController.Process.Id);
                jitController.RunJitCompilation();
                etwCollector.StopEventTrace();

                jitController.Process.OutputDataReceived -= JitHostOutputDataReceived;

                return etwCollector.AssemblyCallGraph;
            }
        }

        private void JitHostOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _outputLogger.WriteText(e.Data);
        }
    }
}
