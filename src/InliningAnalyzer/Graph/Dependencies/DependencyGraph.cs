using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer.Graph.Dependencies
{
    public class DependencyGraph
    {
        private HashSet<DependencyMethod> _remainingNonRoots;
        private HashSet<DependencyMethod> _remainingRoots;

        public bool HasRemainingMethods => _remainingRoots.Count > 0 || _remainingNonRoots.Count > 0;

        public DependencyGraph(IEnumerable<DependencyMethod> allMethods)
        {
            _remainingNonRoots = new HashSet<DependencyMethod>();
            _remainingRoots = new HashSet<DependencyMethod>();

            foreach (var method in allMethods)
            {
                method.Graph = this;
                if (method.IsRoot)
                    _remainingRoots.Add(method);
                else
                    _remainingNonRoots.Add(method);
            }
        }
        
        internal DependencyMethod GetNextRoot()
        {
            return _remainingRoots.FirstOrDefault();
        }
        internal DependencyMethod GetNextNonRootMethod()
        {
            return _remainingNonRoots.FirstOrDefault();
        }

        internal void Remove(DependencyMethod method)
        {
            if (method.IsRoot)
                _remainingRoots.Remove(method);
            else
                _remainingNonRoots.Remove(method);
        }

        internal void PromoteToRoot(DependencyMethod method)
        {
            if (_remainingNonRoots.Remove(method))
                _remainingRoots.Add(method);
        }

        public bool Contains(DependencyMethod method)
        {
            if (_remainingRoots.Contains(method))
                return true;

            return _remainingNonRoots.Contains(method);
        }
    }
}
