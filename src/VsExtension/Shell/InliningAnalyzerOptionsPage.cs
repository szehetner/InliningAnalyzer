using System.ComponentModel;
using InliningAnalyzer;
using Microsoft.VisualStudio.Shell;

namespace VsExtension.Shell
{
    public class InliningAnalyzerOptionsPage : DialogPage, IOptionsProvider
    {
        [Category("Inlining Analyzer")]
        [DisplayName("Preferred Runtime")]
        [Description("This runtime is used to run the analyzer for .Net Standard projects and projects with multiple target frameworks.")]
        public TargetRuntime PreferredRuntime { get; set; } = TargetRuntime.NetCore;

        [Category("Inlining Analyzer")]
        [DisplayName("Preferred TargetFramework")]
        [Description("The output assembly for this target framework is used for projects with multiple target frameworks. If this conflicts with PreferredRuntime, this setting takes preference.")]
        [DefaultValue("netstandard2.0")]
        public string PreferredTargetFramework { get; set; }
    }
}