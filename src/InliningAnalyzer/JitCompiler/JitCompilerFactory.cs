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
                throw new ArgumentOutOfRangeException("Expected arg contents: assemblyFile and optional method name or method list");

            string assemblyFile = args[0];
            string assemblyPath = Path.GetDirectoryName(assemblyFile);
            var assembly = Assembly.LoadFile(assemblyFile);

            IMethodProvider methodProvider;
            if (args.Length == 2)
            {
                if (args[1].StartsWith("/m:"))
                {
                    methodProvider = new SingleMethodProvider(assembly, args[1].Substring(3).Trim('"'));
                }
                else if (args[1].StartsWith("/c:"))
                {
                    methodProvider = new TypeMethodProvider(assembly, args[1].Substring(3).Trim('"'));
                }
                else if (args[1].StartsWith("/l:"))
                {
                    methodProvider = new OrderedMethodProvider(assembly, args[1].Substring(3).Trim('"'));
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Expected method name or method list");
                }
            }
            else
            {
                methodProvider = new UnorderedMethodProvider(assembly);
            }
            return new JitCompiler(assemblyPath, methodProvider);
        }
    }
}
