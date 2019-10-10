using InliningAnalyzer;

namespace VsExtension.Common
{
    public interface IOptionsProvider
    {
        TargetRuntime PreferredRuntime { get; }
        string PreferredTargetFramework { get; }
        string NetCoreVersion { get; }
    }
}
