using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model
{
    public unsafe class SignatureResolverSamples
    {
        public void EmptyVoid() { }

        public static void StaticEmptyVoid() { }

        public string EmptyString() { return null; }

        public void SingleString(string test) { }

        public void SingleInt(int test) { }

        public void SingleDecimal(decimal test) { }

        public void SingleBool(bool test) { }
        public void SingleLong(long test) { }
        public void SingleByte(byte test) { }
        public void SingleSByte(sbyte test) { }
        public void SingleChar(char test) { }
        public void SingleInt16(Int16 test) { }
        public void SingleUint16(UInt16 test) { }
        public void SingleUint32(uint test) { }
        public void SingleUint64(UInt64 test) { }
        public void SingleFloat(float test) { }
        public void SingleDouble(double test) { }
        public void SingleDateTime(DateTime test) { }
        public void SingleTimeSpan(TimeSpan test) { }
        public void SingleStruct(CustomStruct test) { }
        public void SingleStructPointer(CustomStruct* test) { }
        public void SingleIntPointer(int* test) { }
        public void SingleFloatPointer(float* test) { }
        public void Multiple(int test, string test2, bool test3) { }
        public CustomStruct MultipleWithReturn(CustomStruct test, string test2) { return default(CustomStruct); }

        public SignatureResolverSamples() { }

        public SignatureResolverSamples(int test) { }
        public SignatureResolverSamples(CustomStruct test) { }

        public void Array(int[] values) { }
        public void Array(int[][] values) { }
        public void Array(CustomStruct[] values) { }

        public void OutParam(out int value) { value = 1; }
        public void OutParamArray(out int value, out int[] values) { value = 1; values = null; }

        public object this[string name] => "";

        private int i;
        public ref int RefReturnMethod() { return ref i; }
        public ref int RefMethod(ref bool input) { return ref i; }
                
        private class InnerClass
        {
            public void InnerMethod() { }
        }
    }

    public struct CustomStruct
    {
        public int Value { get; set; }
    }
}
