using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class MethodGroup
    {
        private Method _singleMethod;
        private List<Method> _methods;

        public bool HasOverloads
        {
            get { return _methods != null; }
        }

        public IEnumerable<Method> GetAllMethods()
        {
            if (_singleMethod != null)
            {
                yield return _singleMethod;
            }
            else
            {
                foreach (var method in _methods)
                {
                    yield return method;
                }
            }
        }

        public Method GetOverload(string signature)
        {
            if (_singleMethod != null && _singleMethod.Signature == signature)
                return _singleMethod;

            if (_methods == null)
                return null;

            return _methods.FirstOrDefault(m => m.Signature == signature);
        }

        public MethodGroup(Method method)
        {
            _singleMethod = method;
        }

        public void AddMethod(Method method)
        {
            if (HasOverloads)
            {
                _methods.Add(method);
            }
            else
            {
                _methods = new List<Method>();

                _methods.Add(_singleMethod);
                _singleMethod = null;

                _methods.Add(method);
            }
        }
    }
}
