using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    class InMethods
    {
        public void Main()
        {
            Process(DateTime.UtcNow);
            Process(1);
        }

        private void Process(in DateTime value)
        {
        }

        private void Process(int value)
        {
        }
    }
}
