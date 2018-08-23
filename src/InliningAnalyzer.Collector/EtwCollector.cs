using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace InliningAnalyzer
{
    public class EtwCollector : IDisposable
    {
        private const int EVENTID_JITTING_STARTED = 145;
        private const int EVENTID_INLINING_SUCCEEDED = 185;
        private const int EVENTID_INLINING_FAILED = 186;
        private const int EVENTID_INLINING_FAILEDANSI = 189;

        public const string EVENTSOURCENAME = "InliningAnalyzerSource";
        private const int EVENTID_START_COMPILERRUN = 1;
        private const int EVENTID_STOP_COMPILERRUN = 2;
        private const int EVENTID_ASYNCMETHOD_START = 3;
        private const int EVENTID_ASYNCMETHOD_STOP = 4;
        
        private readonly AssemblyCallGraph _assemblyCallGraph;
        private TraceEventSession _session;

        public AssemblyCallGraph AssemblyCallGraph => _assemblyCallGraph;

        private bool _isEnabled;
        private ManualResetEventSlim _isFinished;

        private readonly bool _recordEventDetails;

        private Method _lastMethod;
        private Method _currentAsyncMethod;

        public EtwCollector(bool recordEventDetails)
        {
            _assemblyCallGraph = new AssemblyCallGraph();
            _recordEventDetails = recordEventDetails;
            if (_recordEventDetails)
                _assemblyCallGraph.EventDetails = new List<InliningEvent>();
        }

        public void StartEventTrace(int processId)
        {
            _isFinished = new ManualResetEventSlim(false);

            _session = new TraceEventSession("InliningAnalyzerSession");
            
            _session.Source.Clr.MethodInliningSucceeded += Clr_MethodInliningSucceeded;
            _session.Source.Clr.MethodInliningFailed += Clr_MethodInliningFailed;
            _session.Source.Clr.MethodInliningFailedAnsi += Clr_MethodInliningFailedAnsi;
            _session.Source.Clr.MethodJittingStarted += Clr_MethodJittingStarted;
            _session.Source.Dynamic.All += Dynamic_All;
            
            _session.EnableProvider(ClrTraceEventParser.ProviderGuid, TraceEventLevel.Verbose, (ulong)(ClrTraceEventParser.Keywords.Jit | ClrTraceEventParser.Keywords.JitTracing), 
                                        new TraceEventProviderOptions
                                        {
                                            ProcessIDFilter = new List<int> { processId },
                                            EventIDsToEnable = new List<int> { EVENTID_JITTING_STARTED, EVENTID_INLINING_SUCCEEDED, EVENTID_INLINING_FAILED, EVENTID_INLINING_FAILEDANSI }
                                        });
            _session.EnableProvider(EVENTSOURCENAME, TraceEventLevel.Informational, options:
                                        new TraceEventProviderOptions
                                        {
                                            ProcessIDFilter = new List<int> { processId }
                                        });

            Task.Run(() => _session.Source.Process());
        }

        public void StopEventTrace()
        {
            _session.Flush();
            _isFinished.Wait(TimeSpan.FromSeconds(5));
        }

        private void Clr_MethodJittingStarted(Microsoft.Diagnostics.Tracing.Parsers.Clr.MethodJittingStartedTraceData data)
        {
            if (!_isEnabled)
                return;

            var method = _assemblyCallGraph.GetOrAddMethod(data.MethodNamespace, data.MethodName, data.MethodSignature);
            method.ILSize = data.MethodILSize;

            _lastMethod = method;
            // not called for NGENed assemblies!
        }

        private void Dynamic_All(TraceEvent traceEvent)
        {
            if (traceEvent.ProviderName != EVENTSOURCENAME)
                return;

            if ((int)traceEvent.ID == EVENTID_START_COMPILERRUN)
            {
                _isEnabled = true;
            }
            if ((int)traceEvent.ID == EVENTID_STOP_COMPILERRUN)
            {
                _isEnabled = false;
                _isFinished.Set();
            }
            if ((int)traceEvent.ID == EVENTID_ASYNCMETHOD_START)
            {
                _currentAsyncMethod = _lastMethod;
            }
            if ((int)traceEvent.ID == EVENTID_ASYNCMETHOD_STOP)
            {
                _currentAsyncMethod = null;
            }
        }

        private void Clr_MethodInliningFailed(Microsoft.Diagnostics.Tracing.Parsers.Clr.MethodJitInliningFailedTraceData data)
        {
            if (!_isEnabled)
                return;

            var inlinerMethod = _assemblyCallGraph.GetOrAddMethod(data.InlinerNamespace, data.InlinerName, data.InlinerNameSignature);
            if (_currentAsyncMethod != null)
                inlinerMethod = _currentAsyncMethod;

            var calledMethod = _assemblyCallGraph.GetOrAddMethod(data.InlineeNamespace, data.InlineeName, data.InlineeNameSignature);

            var methodCall = new MethodCall
                {
                    Source = inlinerMethod,
                    Target = calledMethod,
                    IsInlined = false,
                    FailReason = data.FailReason
                };
            inlinerMethod.MethodCalls.Add(methodCall);
            calledMethod.CalledBy.Add(methodCall);

            if (data.FailAlways)
                calledMethod.InlineFailsAlways = true;

            if (_recordEventDetails)
                _assemblyCallGraph.EventDetails.Add(new InliningEvent(data));

            _lastMethod = inlinerMethod;
        }

        private void Clr_MethodInliningFailedAnsi(MethodJitInliningFailedAnsiTraceData data)
        {
            if (!_isEnabled)
                return;

            var inlinerMethod = _assemblyCallGraph.GetOrAddMethod(data.InlinerNamespace, data.InlinerName, data.InlinerNameSignature);
            if (_currentAsyncMethod != null)
                inlinerMethod = _currentAsyncMethod;

            var calledMethod = _assemblyCallGraph.GetOrAddMethod(data.InlineeNamespace, data.InlineeName, data.InlineeNameSignature);

            var methodCall = new MethodCall
            {
                Source = inlinerMethod,
                Target = calledMethod,
                IsInlined = false,
                FailReason = data.FailReason
            };
            inlinerMethod.MethodCalls.Add(methodCall);
            calledMethod.CalledBy.Add(methodCall);

            if (data.FailAlways)
                calledMethod.InlineFailsAlways = true;

            if (_recordEventDetails)
                _assemblyCallGraph.EventDetails.Add(new InliningEvent(data));

            _lastMethod = inlinerMethod;
        }

        private void Clr_MethodInliningSucceeded(Microsoft.Diagnostics.Tracing.Parsers.Clr.MethodJitInliningSucceededTraceData data)
        {
            if (!_isEnabled)
                return;

            var inlinerMethod = _assemblyCallGraph.GetOrAddMethod(data.InlinerNamespace, data.InlinerName, data.InlinerNameSignature);
            if (_currentAsyncMethod != null)
                inlinerMethod = _currentAsyncMethod;

            var calledMethod = _assemblyCallGraph.GetOrAddMethod(data.InlineeNamespace, data.InlineeName, data.InlineeNameSignature);

            var methodCall = new MethodCall
                {
                    Source = inlinerMethod,
                    Target = calledMethod,
                    IsInlined = true
                };
            inlinerMethod.MethodCalls.Add(methodCall);
            calledMethod.CalledBy.Add(methodCall);

            if (_recordEventDetails)
                _assemblyCallGraph.EventDetails.Add(new InliningEvent(data));

            _lastMethod = inlinerMethod;
        }

        public void Dispose()
        {
            _session?.Dispose();
            _isFinished?.Dispose();
        }
    }
}
