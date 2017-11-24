using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using InliningAnalyzer;

namespace Tests.Analyzer
{
    [TestClass]
    public class EtwSignatureParserTest
    {
        [TestMethod]
        public void TestEmpty()
        {
            TestGetParameters("instance void  ()");
        }

        [TestMethod]
        public void TestSingleBool()
        {
            TestGetParameters("instance void  (bool)", "System.Boolean");
        }

        [TestMethod]
        public void TestStaticLong()
        {
            TestGetParameters("void  (int64)", "System.Int64");
        }

        [TestMethod]
        public void TestBoolAndInt()
        {
            TestGetParameters("instance void  (bool,int32)", "System.Boolean", "System.Int32");
        }

        [TestMethod]
        public void TestStringAndInt()
        {
            TestGetParameters("instance void  (class System.String,int32)", "System.String", "System.Int32");
        }

        [TestMethod]
        public void TestDecimal()
        {
            TestGetParameters("instance void  (value class System.Decimal)", "System.Decimal");
        }

        [TestMethod]
        public void TestCustomClasses()
        {
            TestGetParameters("instance void  (class Tests.Model.Samples.DummyClass,class Tests.Model.Samples.DummyClass)", 
                "Tests.Model.Samples.DummyClass", "Tests.Model.Samples.DummyClass");
        }

        [TestMethod]
        public void TestCustomStruct()
        {
            TestGetParameters("instance void  (value class Tests.Model.Samples.DummyStruct)", "Tests.Model.Samples.DummyStruct");
        }

        [TestMethod]
        public void TestIntArray()
        {
            TestGetParameters("instance void  (int32[])", "System.Int32[]");
        }

        [TestMethod]
        public void TestIntArrayRank2()
        {
            TestGetParameters("instance void  (int32[][])", "System.Int32[][]");
        }

        [TestMethod]
        public void TestList()
        {
            TestGetParameters("instance void  (class System.Collections.Generic.List`1<class System.String>)", "System.Collections.Generic.List`1[System.String]");
        }

        [TestMethod]
        public void TestDictionary()
        {
            TestGetParameters("instance void  (class System.Collections.Generic.Dictionary`2<class System.String,int32>)", "System.Collections.Generic.Dictionary`2[System.String,System.Int32]");
        }

        [TestMethod]
        public void TestDictionaryWithNestedList()
        {
            TestGetParameters("instance void  (class System.Collections.Generic.Dictionary`2<class System.String,class System.Collections.Generic.List`1<class System.String>>)",
                "System.Collections.Generic.Dictionary`2[System.String,System.Collections.Generic.List`1[System.String]]");
        }

        [TestMethod]
        public void TestDictionaryWithNestedListFirst()
        {
            TestGetParameters("instance void  (class System.Collections.Generic.Dictionary`2<class System.Collections.Generic.List`1<class System.String>,class System.String>)",
                "System.Collections.Generic.Dictionary`2[System.Collections.Generic.List`1[System.String],System.String]");
        }

        [TestMethod]
        public void TestDictionaryWithNestedTupleList()
        {
            TestGetParameters("instance void  (class System.Collections.Generic.Dictionary`2<class System.String,class System.Collections.Generic.List`1<class System.Tuple`5<int32,value class System.Decimal,int32[],class System.Collections.Generic.Dictionary`2<class System.String,int32>,bool>>>)",
                "System.Collections.Generic.Dictionary`2[System.String,System.Collections.Generic.List`1[System.Tuple`5[System.Int32,System.Decimal,System.Int32[],System.Collections.Generic.Dictionary`2[System.String,System.Int32],System.Boolean]]]");
        }
        
        private static void TestGetParameters(string rawSignature, params string[] expectedParameters)
        {
            string[] parameters = EtwSignatureParser.GetParameters(rawSignature);
            CollectionAssert.AreEqual(expectedParameters, parameters);
        }
    }
}
