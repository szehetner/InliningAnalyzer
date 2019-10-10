namespace InliningAnalyzer
{
    public struct JitTarget
    {
        public TargetPlatform Platform { get; }
        public TargetRuntime Runtime { get; }
        public string NetCoreVersion { get; }
        public bool HasSpecificNetCoreVersion => !string.IsNullOrWhiteSpace(NetCoreVersion);

        public JitTarget(TargetPlatform platform, TargetRuntime runtime, string netCoreVersion)
        {
            Platform = platform;
            Runtime = runtime;
            NetCoreVersion = netCoreVersion;
        }
    }
}