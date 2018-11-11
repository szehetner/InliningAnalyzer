namespace InliningAnalyzer
{
    public enum ScopeType
    {
        AssemblyFile,
        Class,
        Method
    }

    public class TargetScope
    {
        public ScopeType ScopeType { get; set; }
        public string Name { get; set; }

        public TargetScope(ScopeType type, string name)
        {
            ScopeType = type;
            Name = name;
        }
    }
}