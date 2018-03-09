using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class Async
    {
        public void Main()
        {
            var values = GetValues();    
        }

        public async Task<int> GetValues()
        {
            await Task.Delay(1);
            return GetValue();
        }

        private int GetValue()
        {
            return 1;
        }
    }
}
