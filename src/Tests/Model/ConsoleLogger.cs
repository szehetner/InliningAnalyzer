using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VsExtension.Shell;

namespace Tests.Model
{
    public class ConsoleLogger : ILogger
    {
        public void WriteText(string text)
        {
            Console.WriteLine(text);
        }
    }
}
