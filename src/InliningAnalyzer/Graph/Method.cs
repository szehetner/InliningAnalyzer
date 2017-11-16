using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class Method
    {
        public string TypeName { get; set; }

        public string Name { get; set; }

        public string Signature { get; set; }

        public List<MethodCall> MethodCalls { get; set; } = new List<MethodCall>();

        public bool InlineFailsAlways { get; set; }

        public int ILSize { get; set; }

        public HashSet<MethodCall> CalledBy { get; set; } = new HashSet<MethodCall>();

        public Method()
        {
        }

        public Method(string typeName, string name, string signature)
        {
            TypeName = typeName;
            Name = name;
            Signature = signature;
        }

        public MethodCall GetMethodCall(string typeName, string targetName, string signature)
        {
            List<MethodCall> candidates = MethodCalls.Where(m => m.Target.TypeName == typeName && m.Target.Name == targetName).ToList();
            if (candidates.Count == 0)
                return null;

            if (candidates.Count == 1)
                return candidates[0];

            return candidates.FirstOrDefault(m => signature == null || m.Target.Signature == signature);
        }

        public override string ToString()
        {
            return TypeName + "." + Name;
        }
    }
}
