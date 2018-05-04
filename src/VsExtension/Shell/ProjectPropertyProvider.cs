using System.Threading.Tasks;
using InliningAnalyzer;

namespace VsExtension.Shell
{
    public interface IProjectPropertyProvider
    {
        Task LoadProperties();
        bool IsOptimized { get; }
        bool Prefer32Bit { get; }
        string PlatformTarget { get; }
        string OutputPath { get; }
        string OutputFilename { get; }
        TargetRuntime TargetRuntime { get; }
    }
}