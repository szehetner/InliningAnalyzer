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
            public const EventTask AsyncMethod = (EventTask)2;
        }

        public class EventId
        {
            public const int StartCompilerRun = 1;
            public const int StopCompilerRun = 2;
            public const int AsyncMethodStart = 3;
            public const int AsyncMethodStop = 4;
        }

        private InliningAnalyzerSource() { }
        public static InliningAnalyzerSource Log { get; } = new InliningAnalyzerSource();

        [Event(EventId.StartCompilerRun, Level = EventLevel.Informational, Keywords = Keywords.JitCompiler, Opcode = EventOpcode.Start, Task = Tasks.CompilerRun)]
        public void StartCompilerRun()
        {
            WriteEvent(EventId.StartCompilerRun);
        }

        [Event(EventId.StopCompilerRun, Level = EventLevel.Informational, Keywords = Keywords.JitCompiler, Opcode = EventOpcode.Stop, Task = Tasks.CompilerRun)]
        public void StopCompilerRun()
        {
            WriteEvent(EventId.StopCompilerRun);
        }

        [Event(EventId.AsyncMethodStart, Level = EventLevel.Informational, Keywords = Keywords.JitCompiler, Opcode = EventOpcode.Start, Task = Tasks.AsyncMethod)]
        public void AsyncMethodStart(string methodName)
        {
            WriteEvent(EventId.AsyncMethodStart);
        }

        [Event(EventId.AsyncMethodStop, Level = EventLevel.Informational, Keywords = Keywords.JitCompiler, Opcode = EventOpcode.Stop, Task = Tasks.AsyncMethod)]
        public void AsyncMethodStop(string methodName)
        {
            WriteEvent(EventId.AsyncMethodStop);
        }
    }
}
