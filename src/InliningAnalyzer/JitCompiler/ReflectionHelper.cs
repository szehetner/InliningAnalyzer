using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class ReflectionHelper
    {
        public const BindingFlags SEARCH_BINDINGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public static MethodBase[] GetMethodCandidates(Type type, string methodName)
        {
            if (methodName == "ctor" || methodName == ".ctor")
                return type.GetConstructors(SEARCH_BINDINGS);

            return type.GetMethods(SEARCH_BINDINGS).Where(m => m.Name == methodName).ToArray();
        }
    }
}
