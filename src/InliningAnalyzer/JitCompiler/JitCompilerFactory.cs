using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public static class JitCompilerFactory
    {
        public static JitCompiler Create(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentOutOfRangeException("Expected arg contents: assemblyFile and optional method name");

            string assemblyFile = args[0];
            string assemblyPath = Path.GetDirectoryName(assemblyFile);
            var assembly = Assembly.LoadFile(assemblyFile);

            IMethodProvider methodProvider;
            if (args.Length == 2)
            {
                methodProvider = new SingleMethodProvider(assembly, args[1].Trim('"'));
            }
            else
            {
                methodProvider = new UnorderedMethodProvider(assembly);
            }
            return new JitCompiler(assembly, assemblyPath, methodProvider);
        }
    }
}
