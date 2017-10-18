using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class InterfaceCalls
    {
        public void Main()
        {
            Tester tester = new Tester();
            tester.Test();
        }

        public void Main2()
        {
            ITestable tester = new Tester();
            tester.Test();

            UseTester(tester);
        }

        public void UseTester(ITestable tester)
        {
            tester.Test();
        }
    }

    public interface ITestable
    {
        bool Test();
    }

    public class Tester : ITestable
    {
        public bool Test()
        {
            return true;
        }
    }
}
