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
        public static (SemanticModel SemanticModel, AssemblyCallGraph CallGraph) Run(string resourceFileName, Platform platform)
        {
            string source = GetEmbeddedResource(resourceFileName);

            var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp7_3));
            CSharpCompilation compilation = CreateCSharpCompilation(tree, platform);

            var tempFileName = Path.GetTempFileName();
            try
            {
                CreateAssembly(compilation, tempFileName);

                using (var etwCollector = new EtwCollector(true))
                {
                    var targetPlatform = platform == Platform.X64 ? TargetPlatform.X64 : TargetPlatform.X86;
                    JitHostController jitController = new JitHostController(tempFileName, new JitTarget(targetPlatform, TargetRuntime.NetFramework), null, null, new TestJitPathResolver());
                    
                    jitController.StartProcess();
                    jitController.Process.OutputDataReceived += JitHostOutputDataReceived;
                    jitController.Process.ErrorDataReceived += JitHostOutputDataReceived;
                    jitController.Process.BeginOutputReadLine();

                    etwCollector.StartEventTrace(jitController.Process.Id);
                    jitController.RunJitCompilation();
                    etwCollector.StopEventTrace();

                    jitController.Process.OutputDataReceived -= JitHostOutputDataReceived;
                    jitController.Process.ErrorDataReceived -= JitHostOutputDataReceived;

                    CallGraphPostProcessor.Process(etwCollector.AssemblyCallGraph);
                    return (compilation.GetSemanticModel(tree), etwCollector.AssemblyCallGraph);
                }
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        public static string CreateAssembly(string resourceFileName, Platform platform = Platform.X64)
        {
            string source = GetEmbeddedResource(resourceFileName);
            var tree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp7_3));
            CSharpCompilation compilation = CreateCSharpCompilation(tree, platform);

            var tempFileName = Path.GetTempFileName();
            CreateAssembly(compilation, tempFileName);
            return tempFileName;
        }

        private static int _assemblyIndex = 1;
        private static CSharpCompilation CreateCSharpCompilation(SyntaxTree tree, Platform platform)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemData = MetadataReference.CreateFromFile(typeof(IDataReader).Assembly.Location);
            var compilation = CSharpCompilation.Create("UnitTestCompilation" + _assemblyIndex++,
                syntaxTrees: new[] { tree }, references: new[] { mscorlib, systemData },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release, platform: platform));
            return compilation;
        }

        private static void CreateAssembly(CSharpCompilation compilation, string filename)
        {
            using (var stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var result = compilation.Emit(stream);
                if (!result.Success)
                {
                    throw new Exception(string.Join("\r\n", result.Diagnostics.Select(d => d.ToString())));
                }
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

        private static string GetEmbeddedResource(string resourceFileName)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceFileName))
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }

    public class TestJitPathResolver : IJitHostPathResolver
    {
        public string GetPath(TargetRuntime targetRuntime)
        {
            return ".";
        }
    }
}
