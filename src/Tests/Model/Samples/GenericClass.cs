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
            var test = new List<string>();
            test.Add("123");

            var container = new Container<int, bool>();
            container.Add(1, true);

            var container2 = new Container<int, List<string>>();
            container2.Add(1, new List<string>());
        }
    }

    public class Container<TKey, TValue>
    {
        private TKey _key;
        private TValue _value;

        public Container()
        {
        }

        public void Add(TKey key, TValue value)
        {
            _key = key;
            _value = value;
        }
    }
}