using System.ComponentModel;
using InliningAnalyzer;
using Microsoft.VisualStudio.Shell;

namespace VsExtension.Shell
{
    public class InliningAnalyzerOptionsPage : DialogPage
    {
        [Category("Inlining Analyzer")]
        [DisplayName("Preferred Runtime")]
        [Description("This runtime is used as default for .Net Standard projects and projects with multiple target frameworks.")]
        public TargetRuntime PreferredRuntime { get; set; } = TargetRuntime.NetCore;
    }
}