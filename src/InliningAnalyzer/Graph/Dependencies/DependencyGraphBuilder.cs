using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer.Graph.Dependencies
{
    public class DependencyGraphBuilder
    {
        public static DependencyGraph BuildFromCallGraph(AssemblyCallGraph assemblyCallGraph)
        {
            Dictionary<Method, DependencyMethod> methodPairs = new Dictionary<Method, DependencyMethod>();

            foreach (var jitType in assemblyCallGraph.Types.Values)
            {
                foreach (var methodGroup in jitType.Methods.Values)
                {
                    foreach (var method in methodGroup.GetAllMethods())
                    {
                        methodPairs.Add(method, CreateMethod(method));
                    }
                }
            }

            RewireCalls(methodPairs);

            return new DependencyGraph(methodPairs.Values);
        }

        private static void RewireCalls(Dictionary<Method, DependencyMethod> methodPairs)
        {
            foreach (var methodPair in methodPairs)
            {
                var source = methodPair.Key;
                var target = methodPair.Value;

                foreach (var methodCall in source.CalledBy)
                {
                    target.CalledBy.Add(methodPairs[methodCall.Source]);
                }

                foreach (var methodCall in source.MethodCalls)
                {
                    target.Calls.Add(methodPairs[methodCall.Target]);
                }
            }
        }

        private static DependencyMethod CreateMethod(Method method)
        {
            return new DependencyMethod()
            {
                FullTypename = method.TypeName,
                MethodName = method.Name,
                Signature = method.Signature,
                ILSize = method.ILSize,
                CanBeInlined = method.CalledBy.Any(c => c.IsInlined) || method.CalledBy.All(c => c.FailReason == "Method is marked as no inline or has a cached result." 
                                                                                              || c.FailReason == "noinline per IL/cached result")
            };
        }
    }
}
