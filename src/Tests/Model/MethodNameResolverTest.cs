using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp;
using VsExtension.Model;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Tests.Model
{
    [TestClass]
    public class MethodNameResolverTest
    {
        [TestMethod]
        public void TestGetSymbolByLocation()
        {
            string source = 
@"namespace Test
{
    public class TestClass
    {
        public bool Method1(int arg)
        {
            return true;
        }
    }
}
";
            var model = RoslynCompiler.GetSemanticModel(source);
            
            var methodSymbol = model.SyntaxTree.GetRoot().DescendantNodes().Select(n => model.GetDeclaredSymbol(n)).First(s => s != null && s.Kind == SymbolKind.Method);

            Assert.AreNotEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 4, 0));
            Assert.AreEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 5, 0));
            Assert.AreEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 5, 25));
            Assert.AreEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 5, 37));
            Assert.AreEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 7, 0));
            Assert.AreEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 8, 0));
            Assert.AreNotEqual(methodSymbol, MethodNameResolver.GetSymbolByLocation(model, model.SyntaxTree, 9, 0));
        }

        [TestMethod]
        public void TestGetMethodName()
        {
            string source =
@"using System.Collections.Generic; using System;
namespace Test
{
    public class TestClass : IDisposable
    {
        public void Method1() {} // line 5
        public void Method2(int arg) {}
        public void Method2(string arg) {}
        public void Method2(int arg1, string arg2) {}
        public void Method3(TestClass arg) {}
        public void Method4(int[] arg) {}
        public void Method5(List<string> arg) {}
        public void Method6(Dictionary<System.DateTime, bool> arg) {}

        public TestClass() {}
        public TestClass(int arg) {}
        
        public string Name
        {
            get { return null; }
            set { }
        }

        public void Dispose() {}
        public virtual void BaseTest() {}
    }

    public class TestClass2 : TestClass
    {
        public override void BaseTest() {}
    }

    public class TestClass3 : IDisposable, IIndexer
    {
        void IDisposable.Dispose() {}
        string this[string name] => name;
        int IIndexer.this[int i] => i;
    }

    public interface IIndexer
    {
        int this[int i] { get; }
    }
}
";
            var model = RoslynCompiler.GetSemanticModel(source);
            
            Assert.AreEqual("Test.TestClass|Method1()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 6, 0));
            Assert.AreEqual("Test.TestClass|Method2(System.Int32)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 7, 0));
            Assert.AreEqual("Test.TestClass|Method2(System.String)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 8, 0));
            Assert.AreEqual("Test.TestClass|Method2(System.Int32,System.String)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 9, 0));
            Assert.AreEqual("Test.TestClass|Method3(Test.TestClass)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 10, 0));
            Assert.AreEqual("Test.TestClass|Method4(System.Int32[])", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 11, 0));
            Assert.AreEqual("Test.TestClass|Method5(System.Collections.Generic.List`1[System.String])", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 12, 0));
            Assert.AreEqual("Test.TestClass|Method6(System.Collections.Generic.Dictionary`2[System.DateTime;System.Boolean])", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 13, 0));

            Assert.AreEqual("Test.TestClass|ctor()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 15, 0));
            Assert.AreEqual("Test.TestClass|ctor(System.Int32)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 16, 0));

            Assert.AreEqual("Test.TestClass|get_Name()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 20, 0));
            Assert.AreEqual("Test.TestClass|set_Name(System.String)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 21, 0));

            Assert.AreEqual("Test.TestClass|Dispose()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 24, 0));
            Assert.AreEqual("Test.TestClass|BaseTest()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 25, 0));
            Assert.AreEqual("Test.TestClass2|BaseTest()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 30, 0));
            Assert.AreEqual("Test.TestClass3|System.IDisposable.Dispose()", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 35, 0));

            Assert.AreEqual("Test.TestClass3|get_Item(System.String)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 36, 0));
            Assert.AreEqual("Test.TestClass3|Test.IIndexer.get_Item(System.Int32)", MethodNameResolver.GetMethodName(model, model.SyntaxTree, 37, 0));
        }
    }
}
