using EnvDTE;

namespace VsExtension.Common
{
    public interface ICommonProjectPropertyProviderFactory
    {
        IProjectPropertyProvider Create(Project vsProject, IOptionsProvider optionsProvider);
        bool IsNewProjectFormat(Project vsProject);
    }
}
