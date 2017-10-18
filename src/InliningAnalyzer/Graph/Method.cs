using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    [ProtoContract]
    public class Method
    {
        [ProtoMember(1)]
        public string TypeName { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public string Signature { get; set; }

        [ProtoMember(4)]
        public List<MethodCall> MethodCalls { get; set; } = new List<MethodCall>();

        [ProtoMember(5)]
        public bool InlineFailsAlways { get; set; }

        [ProtoMember(6)]
        public int ILSize { get; set; }

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
