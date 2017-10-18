using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    class GenericClass
    {
        public void Main()
        {
            Test();
            Test();
        }

        public int Test()
        {
            var x = int.Parse("1");
            x.ToString();
            x.ToString();
            x.ToString();
            x.ToString();
            x.ToString();
            x.ToString();
            x.ToString();
            x.ToString();
            x.ToString();
            return x;
        }
    }
}