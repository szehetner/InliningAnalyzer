using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{

    [ProtoContract]
    public class MethodCall
    {
        [ProtoMember(1, AsReference = true)]
        public Method Target { get; set; }

        [ProtoMember(2)]
        public bool IsInlined { get; set; }

        [ProtoMember(3)]
        public string FailReason { get; set; }

        public override string ToString()
        {
            return Target.ToString() + " " + (IsInlined ? "Inlined" : "");
        }
    }
}
