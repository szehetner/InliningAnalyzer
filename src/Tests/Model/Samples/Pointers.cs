using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public unsafe class Pointers
    {
        public void Main()
        {
            PointerMethod(null, 1);
        }

        public static unsafe void PointerMethod(float* a, int length)
        {
            var x = int.Parse("1");
        }
    }
}