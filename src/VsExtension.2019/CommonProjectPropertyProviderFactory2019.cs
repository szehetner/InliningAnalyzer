using EnvDTE;
using System.Composition;
using VsExtension.Common;
using VsExtension.Shell;

namespace VsExtension.Vs2017
{
    [Export(typeof(ICommonProjectPropertyProviderFactory))]
    public class CommonProjectPropertyProviderFactory2019 : ICommonProjectPropertyProviderFactory
    {
        public IProjectPropertyProvider Create(Project vsProject, IOptionsProvider optionsProvider)
        {
            return new CommonProjectPropertyProvider(vsProject, optionsProvider);
        }

        public bool IsNewProjectFormat(Project vsProject)
        {
            return CommonProjectPropertyProvider.IsNewProjectFormat(vsProject);
        }
    }
}
