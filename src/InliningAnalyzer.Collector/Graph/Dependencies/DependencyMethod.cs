using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer.Graph.Dependencies
{
    [DebuggerDisplay("{MethodName}")]
    public class DependencyMethod
    {
        public string FullTypename { get; set; }
        public string MethodName { get; set; }
        public string Signature { get; set; }
        public List<DependencyMethod> CalledBy { get; set; } = new List<DependencyMethod>();
        public List<DependencyMethod> Calls { get; set; } = new List<DependencyMethod>();
        public DependencyGraph Graph { get; set; }
        public bool CanBeInlined { get; set; } = true;
        public int ILSize { get; set; }

        public int NumberOfCallers => CalledBy.Count;
        public bool HasCallers => NumberOfCallers > 0;
        public bool IsRoot => !HasCallers || !CanBeInlined;

        public void RemoveFromGraph()
        {
            Graph.Remove(this);

            if (HasCallers)
            {
                foreach (var parent in CalledBy)
                {
                    parent.RemoveFromCalls(this);
                }
            }

            foreach (var child in Calls)
            {
                child.RemoveFromCalledBy(this);
            }
        }

        public void RemoveFromCalls(DependencyMethod calledMethod)
        {
            Calls.Remove(calledMethod);
        }

        public void RemoveFromCalledBy(DependencyMethod calledMethod)
        {
            CalledBy.Remove(calledMethod);
            if (!HasCallers)
                Graph.PromoteToRoot(this);
        }

        public List<DependencyMethod> FindCycle()
        {
            return FindCycle(new List<DependencyMethod>());
        }

        private List<DependencyMethod> FindCycle(List<DependencyMethod> callPath)
        {
            callPath.Add(this);

            // follow graph upwards until we find a method that we have visited before -> cycle detected
            var firstCaller = CalledBy.First();
            int callerIndex = callPath.IndexOf(firstCaller);
            if (callerIndex != -1)
            {
                // the original method and some of its callers may not be part of the cycle
                // -> trim callPath to contain only cycle members
                return callPath.GetRange(callerIndex, callPath.Count - callerIndex);
            }
            return firstCaller.FindCycle(callPath);
        }
    }
}
