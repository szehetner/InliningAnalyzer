using System;

namespace InliningAnalyzer
{
    public interface IJitHostPathResolver
    {
        string GetPath(TargetRuntime targetRuntime);
    }

    public class JitHostPathResolver : IJitHostPathResolver
    {
        public string GetPath(TargetRuntime targetRuntime)
        {
            switch (targetRuntime)
            {
                case TargetRuntime.NetFramework:
                    return "JitHosts\\NetFramework";
                case TargetRuntime.NetCore:
                    return "JitHosts\\NetCore";
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetRuntime), targetRuntime, null);
            }
        }
    }
}