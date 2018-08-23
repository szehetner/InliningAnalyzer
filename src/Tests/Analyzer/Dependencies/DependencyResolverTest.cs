using InliningAnalyzer.Graph.Dependencies;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InliningAnalyzer.Collector;

namespace Tests.Dependencies
{
    [TestClass]
    public class DependencyResolverTest
    {
        private Dictionary<string, DependencyMethod> _methods = new Dictionary<string, DependencyMethod>();

        [TestInitialize]
        public void Setup()
        {
            _methods = new Dictionary<string, DependencyMethod>();
        }

        [TestMethod]
        public void TestSingleMethod()
        {
            CreateMethod("A");

            AssertOrder("A");
        }

        [TestMethod]
        public void TestSingleCall()
        {
            CreateMethod("A", callees:"B");

            AssertOrder("A", "B");
        }

        [TestMethod]
        public void TestTwoRootsCallSameMethod()
        {
            CreateMethod("R1", "M1");
            CreateMethod("R2", "M1");

            AssertOrder("R1", "R2", "M1");
        }

        [TestMethod]
        public void TestSimpleCycle()
        {
            CreateMethod("Root", "C1");
            CreateMethod("C1", 20, "C2");
            CreateMethod("C2", 10, "C1");

            AssertOrder("Root", "C1", "C2");
        }

        [TestMethod]
        public void TestSimpleCycleWithSwitchedIlSize()
        {
            CreateMethod("Root", "C1");
            CreateMethod("C1", 10, "C2");
            CreateMethod("C2", 20, "C1");

            AssertOrder("Root", "C2", "C1");
        }

        [TestMethod]
        public void TestSimpleCycleWithSameIlSize()
        {
            CreateMethod("Root", "Ca");
            CreateMethod("Cb", 10, "Ca");
            CreateMethod("Ca", 10, "Cb");

            AssertOrder("Root", "Ca", "Cb");
        }

        [TestMethod]
        public void TestDeepGraphWithCycles()
        {
            CreateMethod("L1a", "L2a", "L2b", "L2c");
            CreateMethod("L1b", "L2c");

            CreateMethod("L2b", 40, "L3a");
            CreateMethod("L2c", "L3a", "L3b", "L3c");

            CreateMethod("L3a", 30, "L3b", "L4a");
            CreateMethod("L3b", 50, "L3a", "L4b");

            CreateMethod("L4a", "L2a");

            AssertOrder(
                "L1a",
                "L2b",
                "L1b",
                "L2c",
                "L3c",
                "L3b",
                "L3a",
                "L4a",
                "L2a",
                "L4b"
                );
        }

        private void AssertOrder(params string[] methodNames)
        {
            var methodList = ResolveGraph();
            try
            {

                Assert.IsNotNull(methodList, "MethodList");
                Assert.AreEqual(methodNames.Length, methodList.Methods.Count, "Number of Methods");

                for (int i = 0; i < methodNames.Length; i++)
                {
                    Assert.AreEqual(methodNames[i], methodList.Methods[i].MethodName);
                }
            }
            catch (AssertFailedException)
            {
                Console.WriteLine("Expected: " + string.Join("\r\n", methodNames));
                Console.WriteLine("\r\nActual: " + string.Join("\r\n", methodList.Methods.Select(m => m.MethodName)));

                throw;
            }
        }
        
        private MethodCompilationList ResolveGraph()
        {
            DependencyResolver resolver = new DependencyResolver(CreateGraph());
            return resolver.GetOrderedMethodList();
        }

        private DependencyGraph CreateGraph()
        {
            return new DependencyGraph(_methods.Values.ToList());
        }

        private DependencyMethod CreateMethod(string name)
        {
            if (_methods.TryGetValue(name, out DependencyMethod result))
                return result;

            var method = new DependencyMethod() { MethodName = name };
            _methods.Add(name, method);
            return method;
        }

        private DependencyMethod CreateMethod(string name, params string[] callees)
        {
            return CreateMethod(name, 1, callees);
        }

        private DependencyMethod CreateMethod(string name, int ilSize, params string[] callees)
        {
            DependencyMethod result;
            if (!_methods.TryGetValue(name, out result))
            {
                result = new DependencyMethod() { MethodName = name };
                _methods.Add(name, result);
            }
            result.ILSize = ilSize;

            foreach (string callee in callees)
            {
                var method = CreateMethod(callee);
                if (!result.Calls.Contains(method))
                {
                    result.Calls.Add(method);
                    method.CalledBy.Add(result);
                }
            }
            
            return result;
        }
    }
}
