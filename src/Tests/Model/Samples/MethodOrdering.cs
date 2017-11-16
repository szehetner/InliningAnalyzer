using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class Root
    {
        public void Main()
        {
            var x = Name;
            A();
            B();
        }

        public void C()
        {
        }

        public string Name { get; set; }

        public void A()
        {
            Name = "a";
            B();
        }

        public void B()
        {
            C();
        }
    }
}
