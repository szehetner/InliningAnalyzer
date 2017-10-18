using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    class BasicInline
    {
        public void Main()
        {
            var ex = new Exception("test");
            var s = ex.ToString();

            var x = Add(1, 2);
            var y = int.Parse("1");
            var z = float.Parse("1");
        }

        public int Add(int i, int x)
        {
            return i + x;
        }

        public class InnerClass
        {
            public void Test1()
            {
                Test2();
            }

            private int Test2()
            {
                return 1;
            }
        }
    }

    public class Person
    {
        public string Name { get; set; }

        public string FullName
        {
            get
            {
                return Name;
            }
        }
    }
}