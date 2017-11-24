using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public unsafe class Overloads
    {
        public void A() {}
        public void A(bool arg1) { }
        public void A(bool arg1, int arg2) { }
        public void A(string arg1, int arg2) { }
        public void A(string arg1, int arg2, bool arg3) { }
        public void A(decimal arg1) { }
        public void A(DummyClass arg1, DummyClass arg2) { }
        public void A(DummyStruct arg1) { }
        public void A(byte arg1) { }
        public void A(sbyte arg1) { }
        public void A(long arg1) { }
        public void A(float arg1) { }
        public void A(double arg1) { }
        public void A(UInt16 arg1) { }
        public void A(uint arg1) { }
        public void A(UInt64 arg1) { }
        public void A(IntPtr arg1) { }
        public void A(UIntPtr arg1) { }
        public void A(DateTime arg1) { }
        public void A(DummyStruct* arg1) { }
        public void A(int[] arg1) { }
        public void A(int[][] arg1) { }
        public void A(List<string> arg1) { }
        public void A(Dictionary<string, int> arg1) { }
        public void A(Dictionary<string, List<string>> arg1) { }
        public void A(Dictionary<string, List<Tuple<int, decimal, int[], Dictionary<string, int>, bool>>> arg1) { }
    }

    public class DummyClass { }

    public struct DummyStruct { }
}
