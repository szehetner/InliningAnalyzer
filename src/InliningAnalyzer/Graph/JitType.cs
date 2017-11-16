using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class JitType
    {
        public string FullName { get; set; }

        public Dictionary<string, MethodGroup> Methods { get; set; } = new Dictionary<string, MethodGroup>();

        public JitType()
        {
        }

        public JitType(string fullName)
        {
            FullName = fullName;
        }

        public Method GetMethod(string name, string signature)
        {
            MethodGroup methodGroup;
            if (Methods.TryGetValue(name, out methodGroup))
                return methodGroup.GetOverload(signature);

            return null;
        }

        public Method GetOrAddMethod(string name, string signature)
        {
            MethodGroup methodGroup;
            if (Methods.TryGetValue(name, out methodGroup))
            {
                var overload = methodGroup.GetOverload(signature);
                if (overload != null)
                    return overload;

                overload = new Method(FullName, name, signature);
                methodGroup.AddMethod(overload);
                return overload;
            }

            var method = new Method(FullName, name, signature);
            Methods.Add(name, new MethodGroup(method));
            return method;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
