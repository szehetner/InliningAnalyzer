using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class JitCompiler
    {
        private readonly string _assemblyPath;
        private readonly Assembly _assembly;
        private readonly IMethodProvider _methodProvider;

        public JitCompiler(string assemblyFile)
        {
            _assemblyPath = Path.GetDirectoryName(assemblyFile);
            _assembly = Assembly.LoadFile(assemblyFile);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public JitCompiler(Assembly assembly, string assemblyPath, IMethodProvider methodProvider)
        {
            _assemblyPath = assemblyPath;
            _assembly = assembly;
            _methodProvider = methodProvider;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);
            string assemblyFile = Path.Combine(_assemblyPath, assemblyName.Name + ".dll");
            if (File.Exists(assemblyFile))
            {
                return Assembly.LoadFile(assemblyFile);
            }
            return null;
        }

        public JitCompiler(Assembly assembly)
        {
            _assembly = assembly;

            RuntimeHelpers.PrepareMethod(GetType().GetMethod(nameof(PreJITMethods), BindingFlags.Instance | BindingFlags.Public).MethodHandle);
        }
       
        public void PreJITMethods()
        {
            InliningAnalyzerSource.Log.StartCompilerRun();

            foreach (MethodBase method in _methodProvider.GetMethods())
            {
                try
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(method.DeclaringType.Name + "." + method.Name + ":" + ex.Message);
                }
            }
            
            InliningAnalyzerSource.Log.StopCompilerRun();
        }
    }
}
