using System.ComponentModel;
using InliningAnalyzer;
using Microsoft.VisualStudio.Shell;
using VsExtension.Common;

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
        public string PreferredTargetFramework { get; set; } = "netstandard2.0";

        [Category("Inlining Analyzer")]
        [DisplayName(".NET Core Version")]
        [Description("The .NET Core version that will be used to to run the analyzer. Needs to be a 3-part version e.g. \"3.0.0\". If not set, the latest installed version will be used.")]
        public string NetCoreVersion { get; set; }
    }
}