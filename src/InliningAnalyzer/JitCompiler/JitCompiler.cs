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
        private readonly IMethodProvider _methodProvider;

        public JitCompiler(string assemblyPath, IMethodProvider methodProvider)
        {
            _assemblyPath = assemblyPath;
            _methodProvider = methodProvider;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            
            // ensure methods are jitted before compilation starts to not interfere with actual jitting events
            RuntimeHelpers.PrepareMethod(GetType().GetMethod(nameof(PreJITMethods), BindingFlags.Instance | BindingFlags.Public).MethodHandle);
            InliningAnalyzerSource.Log.AsyncMethodStart(null);
            InliningAnalyzerSource.Log.AsyncMethodStop(null);
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

        public void PreJITMethods()
        {
            InliningAnalyzerSource.Log.StartCompilerRun();

            foreach (MethodBase method in _methodProvider.GetMethods())
            {
                try
                {
                    var asyncAttribute = (AsyncStateMachineAttribute)method.GetCustomAttribute(typeof(AsyncStateMachineAttribute));
                    if (asyncAttribute == null)
                    {
                        CompileSyncMethod(method);
                    }
                    else
                    {
                        CompileAsyncMethod(method, asyncAttribute);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(method.DeclaringType.Name + "." + method.Name + ":" + ex.Message);
                }
            }
            
            InliningAnalyzerSource.Log.StopCompilerRun();
        }

        private static void CompileSyncMethod(MethodBase method)
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle);
        }

        private void CompileAsyncMethod(MethodBase method, AsyncStateMachineAttribute asyncAttribute)
        {
            RuntimeHelpers.PrepareMethod(method.MethodHandle);

            InliningAnalyzerSource.Log.AsyncMethodStart(method.Name);

            var moveNextMethod = asyncAttribute.StateMachineType.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
            if (moveNextMethod != null)
                RuntimeHelpers.PrepareMethod(moveNextMethod.MethodHandle);

            InliningAnalyzerSource.Log.AsyncMethodStop(method.Name);
        }
    }
}