using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using InliningAnalyzer;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using VsExtension.Model;

namespace VsExtension.QuickInfo
{
    public class QuickInfoSource : IQuickInfoSource
    {
        private QuickInfoSourceProvider m_provider;
        private ITextBuffer m_subjectBuffer;
        private Cache _cache;
        private ICodeModel _codeModel;

        public QuickInfoSource(QuickInfoSourceProvider provider, ITextBuffer subjectBuffer, ICodeModel codeModel)
        {
            m_provider = provider;
            m_subjectBuffer = subjectBuffer;

            _codeModel = codeModel;
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;
            // Map the trigger point down to our buffer.
            SnapshotPoint? subjectTriggerPoint = session.GetTriggerPoint(m_subjectBuffer.CurrentSnapshot);
            if (!subjectTriggerPoint.HasValue)
            {
                return;
            }

            ITextSnapshot currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            SnapshotSpan querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);

            if (_cache == null || _cache.Snapshot != currentSnapshot)
            {
                var task = Cache.Resolve(m_subjectBuffer, currentSnapshot);
                try
                {
                    task.Wait();
                }
                catch (Exception)
                {
                    // TODO: report this to someone.
                    return;
                }
                _cache = task.Result;
                if (_cache == null)
                {
                    // TODO: report this to someone.
                    return;
                }
            }

            ITextStructureNavigator navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
            TextExtent extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);
            string searchText = extent.Span.GetText();
            var textSpan = TextSpan.FromBounds(extent.Span.Start, extent.Span.End);

            var methodCall = _codeModel.GetMethodCall(_cache, textSpan);
            if (methodCall == null)
                return;

            string ilSize = "IL Size: " + methodCall.Target.ILSize + " Bytes";
            if (!methodCall.IsInlined)
            {
                qiContent.Add("Inlining Analyzer: " + methodCall.FailReason +  
                    (methodCall.Target.ILSize > 0 ? "\r\n" + ilSize : ""));
            }
            else
            {
                if (methodCall.Target.ILSize > 0)
                    qiContent.Add(ilSize);
            }
        }

        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}
