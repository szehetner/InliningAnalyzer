using InliningAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JitHost.x64
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.ReadLine();
                JitCompilerHost compiler = JitCompilerFactory.Create(args);
                compiler.PreJITMethods();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
