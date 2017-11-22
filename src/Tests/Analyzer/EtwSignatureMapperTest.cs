using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Model;
using System.Reflection;
using InliningAnalyzer;
using System.Linq;

namespace Tests.Analyzer
{
    [TestClass]
    public class EtwSignatureMapperTest
    {
        private Type _overloadType;

        public EtwSignatureMapperTest()
        {
            var assemblyFile = RoslynCompiler.CreateAssembly("Tests.Model.Samples.Overloads.cs");
            var assembly = Assembly.LoadFrom(assemblyFile);
            _overloadType = assembly.GetType("Tests.Model.Samples.Overloads");
        }

        [TestMethod]
        public void ResolveOverloadsFromEtwSignatures()
        {
            TestOverload("instance void  ()");
            TestOverload("instance void  (bool)", "Boolean");
            TestOverload("instance void  (bool,int32)", "Boolean", "Int32");
            TestOverload("instance void  (class System.String,int32)", "String", "Int32");
            TestOverload("instance void  (value class System.Decimal)", "Decimal");
            TestOverload("instance void  (class Tests.Model.Samples.DummyClass,class Tests.Model.Samples.DummyClass)", "DummyClass", "DummyClass");
            TestOverload("instance void  (value class Tests.Model.Samples.DummyStruct)", "DummyStruct");
            
            TestOverload("instance void  (unsigned int8)", "Byte");
            TestOverload("instance void  (int8)", "SByte");
            TestOverload("instance void  (int64)", "Int64");
            TestOverload("instance void  (float32)", "Single");
            TestOverload("instance void  (float64)", "Double");
            TestOverload("instance void  (unsigned int16)", "UInt16");
            TestOverload("instance void  (unsigned int32)", "UInt32");
            TestOverload("instance void  (unsigned int64)", "UInt64");
            TestOverload("instance void  (int)", "IntPtr");
            TestOverload("instance void  (unsigned int)", "UIntPtr");
            TestOverload("instance void  (value class System.DateTime)", "DateTime");
            TestOverload("instance void  (value class Tests.Model.Samples.DummyStruct*)", "DummyStruct*");
            TestOverload("instance void  (int32[])", "Int32[]");
            TestOverload("instance void  (int32[][])", "Int32[][]");
            TestOverload("instance void  (class System.Collections.Generic.List`1<class System.String>)", "List`1");
            //TestOverload("instance void  (class System.Collections.Generic.Dictionary`2<class System.String,int32>)", "Dictionary`2");
        }

        private void TestOverload(string signature, params string[] parameterNames)
        {
            var candidates = EtwSignatureMapper.GetMethodCandidates(_overloadType, "A");
            var overload = EtwSignatureMapper.SelectOverload(candidates, signature);

            Assert.IsNotNull(overload, "Overload not found: " + signature);
            var actualParameters = overload.GetParameters();
            Assert.AreEqual(parameterNames.Length, actualParameters.Length, "Actual Parameters: " + string.Join(",", actualParameters.Select(p => p.ParameterType.Name)));
            for (int i = 0; i < parameterNames.Length; i++)
            {
                var expectedParameterName = parameterNames[i];
                var actualParameterName = actualParameters[i].ParameterType.Name;
                Assert.AreEqual(expectedParameterName, actualParameterName);
            }
        }
    }
}
