namespace InliningAnalyzer
{
    public struct JitTarget
    {
        public TargetPlatform Platform { get; }
        public TargetRuntime Runtime { get; }

        public JitTarget(TargetPlatform platform, TargetRuntime runtime)
        {
            Platform = platform;
            Runtime = runtime;
        }
    }
}