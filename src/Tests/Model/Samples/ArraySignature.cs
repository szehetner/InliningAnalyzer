using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class ArraySignature
    {
        public void Main()
        {
            var x = HandleArray(new int[1]);
        }

        public int HandleArray(int[] values)
        {
            return values[0];
        }
    }
}
