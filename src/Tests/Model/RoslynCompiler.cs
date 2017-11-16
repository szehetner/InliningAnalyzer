﻿using InliningAnalyzer;
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
        public static (SemanticModel SemanticModel, AssemblyCallGraph CallGraph) Run(string resourceFileName)
        {
            string source = GetEmbeddedResource(resourceFileName);

            var tree = CSharpSyntaxTree.ParseText(source);
            CSharpCompilation compilation = CreateCSharpCompilation(tree);

            var tempFileName = Path.GetTempFileName();
            try
            {
                CreateAssembly(compilation, tempFileName);

                using (var etwCollector = new EtwCollector(true))
                {
                    JitHostController jitController = new JitHostController(tempFileName, PlatformTarget.X64, null, null);
                    jitController.StartProcess();
                    jitController.Process.OutputDataReceived += JitHostOutputDataReceived;
                    jitController.Process.BeginOutputReadLine();

                    etwCollector.StartEventTrace(jitController.Process.Id);
                    jitController.RunJitCompilation();
                    etwCollector.StopEventTrace();

                    jitController.Process.OutputDataReceived -= JitHostOutputDataReceived;

                    return (compilation.GetSemanticModel(tree), etwCollector.AssemblyCallGraph);
                }
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        public static string CreateAssembly(string resourceFileName)
        {
            string source = GetEmbeddedResource(resourceFileName);
            var tree = CSharpSyntaxTree.ParseText(source);
            CSharpCompilation compilation = CreateCSharpCompilation(tree);

            var tempFileName = Path.GetTempFileName();
            CreateAssembly(compilation, tempFileName);
            return tempFileName;
        }

        private static CSharpCompilation CreateCSharpCompilation(SyntaxTree tree)
        {
            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemData = MetadataReference.CreateFromFile(typeof(IDataReader).Assembly.Location);
            var compilation = CSharpCompilation.Create("UnitTestCompilation",
                syntaxTrees: new[] { tree }, references: new[] { mscorlib, systemData },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));
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
}
