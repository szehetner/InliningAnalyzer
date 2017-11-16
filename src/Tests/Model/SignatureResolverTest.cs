using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VsExtension.Model;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CSharp;
using InliningAnalyzer;
using System.Reflection;
using System.IO;
using System.CodeDom.Compiler;

namespace Tests.Model
{
    [TestClass]
    public class SignatureResolverTest
    {
        [TestMethod]
        public void ResolvedSignaturesMatchEtwSignatures()
        {
            string source = GetSampleSource();

            var model = RoslynCompiler.GetSemanticModel(source);
            var analyzerResult = RoslynCompiler.Run("Tests.Model.SignatureResolverSamples.cs");

            var jitType = analyzerResult.CallGraph.GetJitType("Tests.Model.SignatureResolverSamples");
            var innerType = analyzerResult.CallGraph.GetJitType("Tests.Model.SignatureResolverSamples+InnerClass");

            foreach (SyntaxNode node in model.SyntaxTree.GetRoot().DescendantNodes())
            {
                if (!(node is ConstructorDeclarationSyntax || node is MethodDeclarationSyntax))
                    continue;

                var methodSymbol = model.GetDeclaredSymbol(node);

                string signature = SignatureResolver.BuildSignature((IMethodSymbol)methodSymbol);

                MethodGroup jitMethodGroup;
                if (jitType.Methods.ContainsKey(methodSymbol.Name))
                {
                    jitMethodGroup = jitType.Methods[methodSymbol.Name];
                }
                else
                {
                    jitMethodGroup = innerType.Methods[methodSymbol.Name];
                }
                var overload = jitMethodGroup.GetAllMethods().FirstOrDefault(m => m.Signature == signature);
                if (overload == null)
                {
                    var allMethods = jitMethodGroup.GetAllMethods().ToList();
                    string expected;
                    if (allMethods.Count == 1)
                        expected = allMethods[0].Signature;
                    else
                        expected = string.Join("\r\n", allMethods.Select(m => m.Signature));

                    Assert.Fail("Method: " + methodSymbol.Name + "\r\nExpected / Actual:\r\n" + expected + "\r\n" + signature + "\r\n");
                }
            }
        }

        private string GetSampleSource()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Tests.Model.SignatureResolverSamples.cs"))
            {
                StreamReader reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}
