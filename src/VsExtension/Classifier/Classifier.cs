using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Tagging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.CSharp;
using System.Threading.Tasks;
using InliningAnalyzer;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VsExtension.Model;

namespace VsExtension
{
    public class Classifier : ITagger<IClassificationTag>
    {
        private readonly ITextBuffer _theBuffer;
        private readonly IClassificationType _succededFormat;
        private readonly IClassificationType _failedFormat;

        private Cache _cache;
#pragma warning disable CS0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore CS0067

        private IAnalyzerModel _analyzerModel;
        private ICodeModel _codeModel;
        private ITextSnapshot _currentSnapshot;

        internal Classifier(ITextBuffer buffer, IClassificationTypeRegistryService registry, IAnalyzerModel analyzerModel, ICodeModel codeModel)
        {
            _theBuffer = buffer;
            _succededFormat = registry.GetClassificationType("InlineSucceeded");
            _failedFormat = registry.GetClassificationType("InlineFailed");

            _analyzerModel = analyzerModel;
            _codeModel = codeModel;

            _analyzerModel.HasChanged += AnalyzerModel_HasChanged;
        }

        private void AnalyzerModel_HasChanged(object sender, EventArgs e)
        {
            OnTagsChanged();
        }

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            _currentSnapshot = spans[0].Snapshot;

            if (spans.Count == 0 || !_analyzerModel.IsHighlightingEnabled)
            {
                return Enumerable.Empty<ITagSpan<IClassificationTag>>();
            }
            if (_cache == null || _cache.Snapshot != spans[0].Snapshot)
            {
                var task = Cache.Resolve(_theBuffer, spans[0].Snapshot);
                try
                {
                    task.Wait();
                }
                catch (Exception)
                {
                    // TODO: report this to someone.
                    return Enumerable.Empty<ITagSpan<IClassificationTag>>();
                }
                _cache = task.Result;
                if (_cache == null)
                {
                    // TODO: report this to someone.
                    return Enumerable.Empty<ITagSpan<IClassificationTag>>();
                }
            }
            return GetTagsImpl(_cache, spans);
        }

        private IEnumerable<ITagSpan<IClassificationTag>> GetTagsImpl(Cache doc, NormalizedSnapshotSpanCollection spans)
        {
            var snapshot = spans[0].Snapshot;

            IEnumerable<ClassifiedSpan> identifiers = GetIdentifiersInSpans(doc.Workspace, doc.SemanticModel, spans);

            foreach (var id in identifiers)
            {
                MethodCall methodCall = null;
                try
                {
                    methodCall = _codeModel.GetMethodCall(doc, id.TextSpan);
                }
                catch (Exception)
                {
                    // TODO: log this to output window
                }

                if (methodCall == null)
                    continue;

                if (methodCall.IsInlined)
                    yield return id.TextSpan.ToTagSpan(snapshot, _succededFormat);
                else
                    yield return id.TextSpan.ToTagSpan(snapshot, _failedFormat);
            }
        }

        private IEnumerable<ClassifiedSpan> GetIdentifiersInSpans(
              Workspace workspace, SemanticModel model,
              NormalizedSnapshotSpanCollection spans)
        {
            var comparer = StringComparer.InvariantCultureIgnoreCase;
            var classifiedSpans =
              spans.SelectMany(span => {
                  var textSpan = TextSpan.FromBounds(span.Start, span.End);
                  return Microsoft.CodeAnalysis.Classification.Classifier.GetClassifiedSpans(model, textSpan, workspace);
              });

            return from cs in classifiedSpans
                   where comparer.Compare(cs.ClassificationType, "identifier") == 0 || comparer.Compare(cs.ClassificationType, "class name") == 0
                   select cs;
        }

        private void OnTagsChanged()
        {
            if (_currentSnapshot != null)
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(_currentSnapshot, 0, _currentSnapshot.Length)));
        }
    }

    public class Cache
    {
        public Workspace Workspace { get; set; }
        public Document Document { get; set; }
        public SemanticModel SemanticModel { get; set; }
        public SyntaxNode SyntaxRoot { get; set; }
        public ITextSnapshot Snapshot { get; set; }

        public static async Task<Cache> Resolve(ITextBuffer buffer, ITextSnapshot snapshot)
        {
            var workspace = buffer.GetWorkspace();
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();
            if (document == null)
                return null;

            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
            var syntaxRoot = await document.GetSyntaxRootAsync().ConfigureAwait(false);
            return new Cache
            {
                Workspace = workspace,
                Document = document,
                SemanticModel = semanticModel,
                SyntaxRoot = syntaxRoot,
                Snapshot = snapshot
            };
        }
    }
}
