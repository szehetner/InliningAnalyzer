using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsExtension.Shell;

namespace PerformanceTest
{
    public class NullLogger : ILogger
    {
        public void WriteText(string text)
        {
        }
    }
}
