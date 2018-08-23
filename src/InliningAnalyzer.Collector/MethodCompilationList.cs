using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer.Collector
{
    [ProtoContract]
    public class MethodCompilationList
    {
        [ProtoMember(1)]
        public List<MethodCompilationListItem> Methods { get; set; }

        public MethodCompilationList()
        {
            Methods = new List<MethodCompilationListItem>();
        }
    }

    [ProtoContract]
    [DebuggerDisplay("{MethodName}")]
    public class MethodCompilationListItem
    {
        [ProtoMember(1)]
        public string FullTypeName { get; set; }

        [ProtoMember(2)]
        public string MethodName { get; set; }

        [ProtoMember(3)]
        public string Signature { get; set; }
    }
}
