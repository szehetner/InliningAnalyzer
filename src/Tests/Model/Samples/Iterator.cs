using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public class Iterator
    {
        public void Main()
        {
            var values = GetValues();    
        }

        public IEnumerable<int> GetValues()
        {
            yield return GetValue();
            yield return GetValue();
            yield return GetValue();
        }

        private int GetValue()
        {
            return 1;
        }
    }
}
