using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using InliningAnalyzer;
using System.Reflection;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class SingleMethodProviderTest
    {
        [TestMethod]
        public void TestSingleMethod()
        {
            var provider = new SingleMethodProvider(Assembly.GetExecutingAssembly(), "Tests.MethodProviderTestClass.Method1()");
            var method = provider.GetMethods().Single();
            Assert.AreEqual("Method1", method.Name);
        }

        [TestMethod]
        public void TestOverloads()
        {
            TestOverload("Tests.MethodProviderTestClass.Method1()");
            TestOverload("Tests.MethodProviderTestClass.Method2(System.Int32)");
            TestOverload("Tests.MethodProviderTestClass.Method3(System.Int32,System.String)");
            TestOverload("Tests.MethodProviderTestClass.Method4(Tests.MethodProviderTestClass)");
            TestOverload("Tests.MethodProviderTestClass.Method5(System.Int32[],System.Collections.Generic.List`1[System.String])");
            TestOverload("Tests.MethodProviderTestClass.Method6(System.Collections.Generic.Dictionary`2[System.DateTime;System.Boolean])");

            TestOverload("Tests.MethodProviderTestClass.ctor()");
            TestOverload("Tests.MethodProviderTestClass.ctor(System.Int32)");

            TestOverload("Tests.MethodProviderTestClass.get_Name()");
            TestOverload("Tests.MethodProviderTestClass.set_Name(System.String)");
        }

        private void TestOverload(string methodSpecifier)
        {
            var provider = new SingleMethodProvider(Assembly.GetExecutingAssembly(), methodSpecifier);
            var method = provider.GetMethods().SingleOrDefault();
            if (method == null)
                Assert.Fail("No Method Overload found for " + methodSpecifier);
        }
    }

    public class MethodProviderTestClass
    {
        public void Method1() { }
        public void Method2() { }
        public void Method2(int arg1) { }
        public void Method3() { }
        public void Method3(int arg1, string arg2) { }
        public void Method4() { }
        public void Method4(MethodProviderTestClass arg1) { }
        public void Method5() { }
        public void Method5(int[] arg1, List<string> arg2) { }
        public void Method6() { }
        public void Method6(Dictionary<DateTime, bool> arg1) { }

        public MethodProviderTestClass() { }
        public MethodProviderTestClass(int arg) { }

        public string Name
        {
            get { return null; }
            set { }
        }
    }
}
