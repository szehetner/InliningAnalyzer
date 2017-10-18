using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Model.Samples
{
    public static class StaticClass
    {
        public static void Main()
        {
            if (string.IsNullOrEmpty("abc"))
            {
            }
            if ("ab".IsSet())
            {
            }
        }
        public static bool IsSet(this string s)
        {
            return s != null;
        }
    }
}
