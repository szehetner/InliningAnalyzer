using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class UnorderedMethodProvider : IMethodProvider
    {
        private readonly Assembly _assembly;

        public UnorderedMethodProvider(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<MethodBase> GetMethods()
        {
            Type[] types = _assembly.GetTypes();
            foreach (Type type in types)
            {
                if (IsIgnored(type))
                    continue;

                BindingFlags filterFlags = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

                MethodBase[] methods = type.GetMethods(filterFlags);
                MethodBase[] constructors = type.GetConstructors(filterFlags);

                var allMethods = methods.Concat(constructors);

                foreach (MethodBase method in allMethods)
                {
                    if (IsIgnored(method))
                        continue;

                    yield return method;
                }
            }
        }

        public static bool IsIgnored(MethodBase method)
        {
            return method == null ||
                   method.IsAbstract ||
                   method.ContainsGenericParameters;
        }

        public bool IsIgnored(Type type)
        {
            if (typeof(IAsyncStateMachine).IsAssignableFrom(type))
                return true; // will be called explicitly when compiling the corresponding async method

            return false;
        }
    }
}
