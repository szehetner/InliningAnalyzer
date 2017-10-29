using InliningAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.CodeDom.Compiler;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Data;
using System.Diagnostics;

namespace Tests.Model
{
    public class RoslynCompiler
    {
        public static (SemanticModel SemanticModel, AssemblyCallGraph CallGraph, List<InliningEvent> Events) Run(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemData = MetadataReference.CreateFromFile(typeof(IDataReader).Assembly.Location);
            var compilation = CSharpCompilation.Create("UnitTestCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib, systemData }, 
                options:new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe:true, optimizationLevel:OptimizationLevel.Release));

            var tempFileName = Path.GetTempFileName();
            try
            {
                using (var stream = new FileStream(tempFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var result = compilation.Emit(stream);
                    if (!result.Success)
                    {
                        throw new Exception(string.Join("\r\n", result.Diagnostics.Select(d => d.ToString())));
                    }
                }

                using (var analyzer = new Analyzer(true))
                {
                    JitHostController jitController = new JitHostController(tempFileName, PlatformTarget.X64, null);
                    jitController.StartProcess();
                    jitController.Process.OutputDataReceived += JitHostOutputDataReceived;
                    jitController.Process.BeginOutputReadLine();

                    analyzer.StartEventTrace(jitController.Process.Id);
                    jitController.RunJitCompilation();
                    analyzer.StopEventTrace();

                    jitController.Process.OutputDataReceived -= JitHostOutputDataReceived;

                    return (compilation.GetSemanticModel(tree), analyzer.AssemblyCallGraph, analyzer.EventDetails);
                }
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }
        
        private static void JitHostOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        public static SemanticModel GetSemanticModel(string source)
        {
            var tree = CSharpSyntaxTree.ParseText(source);
            var Mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var compilation = CSharpCompilation.Create("UnitTestCompilation",
                syntaxTrees: new[] { tree }, references: new[] { Mscorlib });

            return compilation.GetSemanticModel(tree);
        }

        public static (AssemblyCallGraph CallGraph, List<InliningEvent> Events) Analyze(string source)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            var result = provider.CompileAssemblyFromSource(new CompilerParameters() { GenerateInMemory = true, CompilerOptions = "/unsafe" }, source);
            if (result.Errors.HasErrors)
                Assert.Fail(string.Join("\r\n", result.Errors.OfType<CompilerError>().Select(e => e.ToString())));

            var assembly = result.CompiledAssembly;

            return InprocessAnalyzer.Analyze(assembly);
        }
    }
}
