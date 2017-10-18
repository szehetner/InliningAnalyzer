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
        private string _assemblyPath;
        private Assembly _assembly;

        public JitCompiler(string assemblyFile)
        {
            _assemblyPath = Path.GetDirectoryName(assemblyFile);
            _assembly = Assembly.LoadFile(assemblyFile);
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

            Type[] types = _assembly.GetTypes();
            foreach (Type type in types)
            {
                BindingFlags filterFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

                MethodBase[] methods = type.GetMethods(filterFlags);
                MethodBase[] constructors = type.GetConstructors(filterFlags);

                var allMethods = methods.Concat(constructors);

                foreach (MethodBase method in allMethods)
                {
                    if (method == null ||
                        method.IsAbstract ||
                        method.ContainsGenericParameters)
                        continue;

                    try
                    {
                        //if (method.ContainsGenericParameters)
                        //{
                        //    //var genericTypes = method.GetGenericArguments().Select(t => t.TypeHandle).ToArray();
                        //    var genericTypes = new[] { typeof(object).TypeHandle };

                        //    RuntimeHelpers.PrepareMethod(method.MethodHandle, genericTypes);
                        //}
                        //else
                        //{
                            RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        //}
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(type.Name + "." + method.Name + ":" + ex.Message);
                    }
                }
            }

            InliningAnalyzerSource.Log.StopCompilerRun();
        }
    }
}
