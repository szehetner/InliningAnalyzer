using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class MethodCall
    {
        public Method Source { get; set; }

        public Method Target { get; set; }

        public bool IsInlined { get; set; }

        public string FailReason { get; set; }

        public override string ToString()
        {
            return Target.ToString() + " " + (IsInlined ? "Inlined" : "");
        }
    }
}
