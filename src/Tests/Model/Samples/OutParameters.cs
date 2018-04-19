using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class OutParameters
    {
        public void Main()
        {
            MethodWithOutParameter(out var i);
        }

        public void MethodWithOutParameter(out int i)
        {
            i = GetValue();
        }

        private int GetValue()
        {
            return 1;
        }
    }
}
