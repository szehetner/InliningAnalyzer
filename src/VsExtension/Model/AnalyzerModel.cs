using InliningAnalyzer;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Model
{
    public interface IAnalyzerModel
    {
        AssemblyCallGraph CallGraph { get; set; }
        bool IsHighlightingEnabled { get; set; }
        event EventHandler HasChanged;
    }

    [Export(typeof(IAnalyzerModel))]
    public class AnalyzerModel : IAnalyzerModel
    {
        private AssemblyCallGraph _callGraph;
        public AssemblyCallGraph CallGraph
        {
            get { return _callGraph; }
            set
            {
                _callGraph = value;
                HasChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _isHighlightingEnabled;
        public bool IsHighlightingEnabled
        {
            get { return _isHighlightingEnabled; }
            set
            {
                _isHighlightingEnabled = value;
                HasChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler HasChanged;

        public AnalyzerModel()
        {
            IsHighlightingEnabled = true;
        }

        public void StartAnalyzer()
        {
        }
    }
}
