using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public static class CallGraphPostProcessor
    {
        public static void Process(AssemblyCallGraph callGraph)
        {
            foreach (var type in callGraph.Types.Values)
            {
                foreach (var method in type.Methods.Values)
                {
                    foreach (var overload in method.GetAllMethods())
                    {
                        ProcessMethod(overload);
                    }
                }
            }
        }

        private static void ProcessMethod(Method method)
        {
            Method moveNextMethod = null;
            foreach (var methodCall in method.MethodCalls)
            {
                if (IsIteratorTypeForMethod(methodCall.Target, method.Name))
                {
                    moveNextMethod = methodCall.Target.ParentType.GetMethod("MoveNext", "instance bool  ()");
                    break;
                }
            }

            if (moveNextMethod != null)
                method.MoveCallsFrom(moveNextMethod);
        }

        private static bool IsIteratorTypeForMethod(Method target, string sourceMethodname)
        {
            if (target.Name != ".ctor")
                return false;

            int innerTypeStart = target.TypeName.IndexOf("+<");
            if (innerTypeStart == -1)
                return false;

            int innerTypEnd = target.TypeName.IndexOf(">", innerTypeStart + 2);
            if (innerTypEnd == -1)
                return false;

            string innerTypename = target.TypeName.Substring(innerTypeStart + 2, innerTypEnd - innerTypeStart - 2);
            return innerTypename == sourceMethodname;
        }
    }
}
