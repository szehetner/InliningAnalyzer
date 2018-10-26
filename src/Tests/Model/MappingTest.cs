using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VsExtension.Model;
using VsExtension;
using System.Linq;

namespace Tests.Model
{
    [TestClass]
    public class MappingTest
    {
        [TestMethod]
        public void TestBasicInline()
        {
            RunMappingTest("Tests.Model.Samples.BasicInline.cs");
        }

        [TestMethod]
        public void TestStaticClass()
        {
            RunMappingTest("Tests.Model.Samples.StaticClass.cs");
        }

        [TestMethod]
        public void TestArraySignature()
        {
            RunMappingTest("Tests.Model.Samples.ArraySignature.cs");
        }

        [TestMethod]
        public void TestInterfaceCalls()
        {
            RunMappingTest("Tests.Model.Samples.InterfaceCalls.cs");
        }

        [TestMethod]
        public void TestWrapper()
        {
            RunMappingTest("Tests.Model.Samples.WrappedReader.cs");
        }

        [TestMethod]
        public void TestGenerics()
        {
            RunMappingTest("Tests.Model.Samples.GenericClass.cs");
        }

        [TestMethod]
        public void TestMultipleCalls()
        {
            RunMappingTest("Tests.Model.Samples.MultipleCalls.cs");
        }

        [TestMethod]
        public void TestIterator()
        {
            RunMappingTest("Tests.Model.Samples.Iterator.cs");
        }

        [TestMethod]
        public void TestIndexer()
        {
            RunMappingTest("Tests.Model.Samples.Indexer.cs");
        }
        
        [TestMethod]
        public void TestAsyncFunction()
        {
            RunMappingTest("Tests.Model.Samples.Async.cs");
        }

        [TestMethod]
        public void TestOutParameters()
        {
            RunMappingTest("Tests.Model.Samples.OutParameters.cs");
        }

        [TestMethod]
        public void TestRefReturnMethods()
        {
            RunMappingTest("Tests.Model.Samples.RefReturnMethods.cs");
        }

        private void RunMappingTest(string resourceFileName)
        {
            RunMappingTest(resourceFileName, Platform.X86);
            RunMappingTest(resourceFileName, Platform.X64);
        }

        private void RunMappingTest(string resourceFileName, Platform platform)
        {
            var analyzerResult = RoslynCompiler.Run(resourceFileName, platform);

            AnalyzerModel analyzerModel = new AnalyzerModel() { CallGraph = analyzerResult.CallGraph };
            CodeModel codeModel = new CodeModel() { AnalyzerModel = analyzerModel };
            var cache = new Cache() { SemanticModel = analyzerResult.SemanticModel, SyntaxRoot = analyzerResult.SemanticModel.SyntaxTree.GetRoot() };

            foreach (SyntaxNode node in analyzerResult.SemanticModel.SyntaxTree.GetRoot().DescendantNodes())
            {
                var symbol = cache.SemanticModel.GetSymbolInfo(node).Symbol;
                if (symbol == null || (symbol.Kind != SymbolKind.Method && symbol.Kind != SymbolKind.Property && symbol.Kind != SymbolKind.NamedType))
                    continue;

                if ((symbol as IMethodSymbol)?.MethodKind == MethodKind.BuiltinOperator)
                    continue;

                if (symbol.Kind == SymbolKind.NamedType && node.FirstAncestorOrSelf<ObjectCreationExpressionSyntax>() == null || node.FirstAncestorOrSelf<TypeArgumentListSyntax>() != null)
                    continue;

                var method = codeModel.GetMethodCall(cache, node.Span);
                if (method == null)
                {
                    Console.WriteLine("Platform " + platform);
                    Console.WriteLine("No mapped Method found for " + node.ToString());
                    Console.WriteLine("Events:\r\n");
                    foreach (var e in analyzerResult.CallGraph.EventDetails)
                        Console.WriteLine(e.ToString());

                    Assert.Fail("No mapped Method found for " + node.ToString());
                }
            }
        }
    }
}
