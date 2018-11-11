using InliningAnalyzer;

namespace VsExtension.Shell
{
    public interface IOptionsProvider
    {
        TargetRuntime PreferredRuntime { get; }
        string PreferredTargetFramework { get; }
    }
}
