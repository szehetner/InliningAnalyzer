using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    [EventSource(Name = EventSourceName)]
    public class InliningAnalyzerSource : EventSource
    {
        public const string EventSourceName = "InliningAnalyzerSource";

        public class Keywords
        {
            public const EventKeywords JitCompiler = (EventKeywords)1;
        }

        public class Tasks
        {
            public const EventTask CompilerRun = (EventTask)1;
        }

        public class EventId
        {
            public const int StartCompilerRun = 1;
            public const int StopCompilerRun = 2;
        }

        private static InliningAnalyzerSource _log = new InliningAnalyzerSource();
        private InliningAnalyzerSource() { }
        public static InliningAnalyzerSource Log { get { return _log; } }

        [Event(EventId.StartCompilerRun, Level = EventLevel.Informational, Keywords = Keywords.JitCompiler, Opcode = EventOpcode.Start, Task = Tasks.CompilerRun)]
        internal void StartCompilerRun()
        {
            WriteEvent(EventId.StartCompilerRun);
        }

        [Event(EventId.StopCompilerRun, Level = EventLevel.Informational, Keywords = Keywords.JitCompiler, Opcode = EventOpcode.Stop, Task = Tasks.CompilerRun)]
        internal void StopCompilerRun()
        {
            WriteEvent(EventId.StopCompilerRun);
        }
    }
}
