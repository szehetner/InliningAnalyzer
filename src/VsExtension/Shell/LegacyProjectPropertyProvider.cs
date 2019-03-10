using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using InliningAnalyzer;
using VsExtension.Common;

namespace VsExtension.Shell
{
    public class LegacyProjectPropertyProvider : IProjectPropertyProvider
    {
        private readonly Project _project;
        private readonly Configuration _activeConfiguration;

        public LegacyProjectPropertyProvider(Project project)
        {
            _project = project;
            _activeConfiguration = project.ConfigurationManager.ActiveConfiguration;
        }

        public Task LoadProperties()
        {
            return Task.CompletedTask;
        }

        public bool IsOptimized => (bool)_activeConfiguration.Properties.Item("Optimize").Value;
        public bool Prefer32Bit => (bool)_activeConfiguration.Properties.Item("Prefer32bit").Value;
        public string PlatformTarget => _activeConfiguration.Properties.Item("PlatformTarget").Value.ToString();
        public string ProjectPath => _project.Properties.Item("FullPath").Value.ToString();
        public string OutputPath => Path.Combine(ProjectPath, _activeConfiguration.Properties.Item("OutputPath").Value.ToString());
        public TargetRuntime TargetRuntime => TargetRuntime.NetFramework;
        public string TargetFramework => "";
        public bool IsWebSdkProject => false; // not needed

        public string GetOutputFilename(string publishPath)
        {
            string outputFileName = _project.Properties.Item("OutputFileName").Value.ToString();
            return Path.Combine(publishPath ?? OutputPath, outputFileName);
        }
    }
}