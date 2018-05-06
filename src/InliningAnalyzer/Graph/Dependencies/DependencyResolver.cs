using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer.Graph.Dependencies
{
    /// <summary>
    /// Orders a method call graph to ensure callers come before callees, which is necessary for RuntimeHelpers.PrepareMethod() to work correctly
    /// 
    /// Outline:
    /// The source DependencyGraph already contains root and non-root methods. Roots are methods with no callers or methods that can't be inlined anyway (as detected by first JIT run)
    /// * Iterate over all roots
    ///     ** Add root to the result list
    ///     ** Remove root from the graph and from all callees
    ///     ** recursive depth first search if any callee of root has no more other callers left -> promote callee to root and insert into result list as well
    /// * If only non-roots remain, there must be a cycle somewhere
    ///     ** pick a random non-root and search its callers upwards until a cycle is detected
    ///     ** break the cycle by sorting all cycle members by IL descending (larger methods are less likely to be inlined) and then method name (just to be more deterministic),
    ///        first method in sort order is treated like a root (this should promote others to root as well, if not -> repeat cycle search)
    /// </summary>
    public class DependencyResolver
    {
        private readonly DependencyGraph _callGraph;
        private readonly MethodCompilationList _result;

        public DependencyResolver(DependencyGraph callGraph)
        {
            _callGraph = callGraph;
            _result = new MethodCompilationList();
        }

        public MethodCompilationList GetOrderedMethodList()
        {
            while (_callGraph.HasRemainingMethods)
            {
                var rootMethod = _callGraph.GetNextRoot();
                while (rootMethod != null)
                {
                    //  root method -> insert into target, remove from list
                    AddToResultList(rootMethod);

                    rootMethod = _callGraph.GetNextRoot();
                }

                // if no roots left -> circular dependency
                var nonRootMethod = _callGraph.GetNextNonRootMethod();
                while (nonRootMethod != null)
                {
                    // find cycle
                    // get all members of cycle
                    List<DependencyMethod> cycleMethods = nonRootMethod.FindCycle();

                    // sort by IL Size, then name -> treat first as root and remove
                    var firstInCycle = cycleMethods.OrderByDescending(m => m.ILSize).ThenBy(m => m.MethodName).FirstOrDefault();
                    AddToResultList(firstInCycle);
                    // TODO: log warning?

                    nonRootMethod = _callGraph.GetNextNonRootMethod();
                }
            }

            return _result;
        }

        private void AddToResultList(DependencyMethod method)
        {
            if (!_callGraph.Contains(method))
                return; // has already been added to result / removed from graph

            _result.Add(method);
            method.RemoveFromGraph();

            // depth first search if callers remain (if not -> recursive remove + search)
            foreach (var child in method.Calls)
            {
                if (child.IsRoot)
                    AddToResultList(child);
            }
        }
    }
}
