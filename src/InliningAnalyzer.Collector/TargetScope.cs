namespace InliningAnalyzer
{
    public enum ScopeType
    {
        Project,
        AssemblyFile,
        Class,
        Method
    }

    public class TargetScope
    {
        public ScopeType ScopeType { get; set; }
        public string Name { get; set; }

        public TargetScope(ScopeType type, string name = null)
        {
            ScopeType = type;
            Name = name;
        }

        public bool RequiresBuild => ScopeType != ScopeType.AssemblyFile;

        public override string ToString()
        {
            if (ScopeType == ScopeType.Project || ScopeType == ScopeType.AssemblyFile)
                return "Scope: All Types and Methods";
            else
                return $"Scope ({ScopeType}): {Name}";
        }
    }
}