using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace InliningAnalyzer
{
    public class InliningEvent
    {
        public bool Succeeded { get; set; }
        public string InlinerNamespace { get; set; }
        public string InlinerName { get; set; }
        public string InlinerNameSignature { get; set; }
        public string InlineeNamespace { get; set; }
        public string InlineeName { get; set; }
        public string InlineeNameSignature { get; set; }
        public string FailReason { get; set; }
        public bool FailAlways { get; set; }

        public InliningEvent(MethodJitInliningFailedTraceData data)
        {
            Succeeded = false;
            InlinerNamespace = data.InlinerNamespace;
            InlinerName = data.InlinerName;
            InlinerNameSignature = data.InlinerNameSignature;
            InlineeNamespace = data.InlineeNamespace;
            InlineeName = data.InlineeName;
            InlineeNameSignature = data.InlineeNameSignature;
            FailReason = data.FailReason;
            FailAlways = data.FailAlways;
        }

        public InliningEvent(MethodJitInliningFailedAnsiTraceData data)
        {
            Succeeded = false;
            InlinerNamespace = data.InlinerNamespace;
            InlinerName = data.InlinerName;
            InlinerNameSignature = data.InlinerNameSignature;
            InlineeNamespace = data.InlineeNamespace;
            InlineeName = data.InlineeName;
            InlineeNameSignature = data.InlineeNameSignature;
            FailReason = data.FailReason;
            FailAlways = data.FailAlways;
        }

        public InliningEvent(MethodJitInliningSucceededTraceData data)
        {
            Succeeded = true;
            InlinerNamespace = data.InlinerNamespace;
            InlinerName = data.InlinerName;
            InlinerNameSignature = data.InlinerNameSignature;
            InlineeNamespace = data.InlineeNamespace;
            InlineeName = data.InlineeName;
            InlineeNameSignature = data.InlineeNameSignature;
        }

        public override string ToString()
        {
            return (Succeeded ? "Succeeded " : "Failed ")
                + $"InlinerNamespace:{InlinerNamespace}, InlinerName:{InlinerName}, InlinerNameSignature:{InlinerNameSignature}, "
                + $"InlineeNamespace:{InlineeNamespace}, InlineeName:{InlineeName}, InlineeNameSignature:{InlineeNameSignature}, "
                + $"FailReason:{FailReason}, FailAlways:{FailAlways}";
        }
    }
}
