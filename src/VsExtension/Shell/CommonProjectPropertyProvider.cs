using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using InliningAnalyzer;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace VsExtension.Shell
{
    public class CommonProjectPropertyProvider : IProjectPropertyProvider
    {
        private readonly Project _project;
        private readonly TargetRuntime _preferredRuntime;
        private string _outputPath;
        private string _assemblyName;
        private string _targetFramework;
        
        public CommonProjectPropertyProvider(Project project, TargetRuntime preferredRuntime)
        {
            _project = project;
            _preferredRuntime = preferredRuntime;
        }

        public async Task LoadProperties()
        {
            var context = (IVsBrowseObjectContext)_project;
            var unconfiguredProject = context.UnconfiguredProject;
            var configuredProject = await unconfiguredProject.GetSuggestedConfiguredProjectAsync();
            var properties = configuredProject.Services.ProjectPropertiesProvider.GetCommonProperties();

            IsOptimized = bool.Parse(await properties.GetEvaluatedPropertyValueAsync("Optimize"));
            Prefer32Bit = bool.Parse(await properties.GetEvaluatedPropertyValueAsync("Prefer32bit"));
            PlatformTarget = await properties.GetEvaluatedPropertyValueAsync("PlatformTarget");

            _outputPath = await properties.GetEvaluatedPropertyValueAsync("OutputPath");
            _assemblyName = await properties.GetEvaluatedPropertyValueAsync("AssemblyName");

            string targetFramework = await properties.GetEvaluatedPropertyValueAsync("TargetFramework");
            string targetFrameworks = await properties.GetEvaluatedPropertyValueAsync("TargetFrameworks");

            List<string> frameworks = string.IsNullOrEmpty(targetFrameworks) ? new List<string> {targetFramework} : targetFrameworks.Split(';').ToList();
            _targetFramework = DetermineTargetFramework(frameworks);
        }

        private string DetermineTargetFramework(List<string> targetFrameworks)
        {
            if (targetFrameworks.Count == 1)
            {
                return targetFrameworks[0];
            }

            string preferredTarget = _preferredRuntime == TargetRuntime.NetCore ? "netcore" : "net4";
            string fallbackTarget = _preferredRuntime == TargetRuntime.NetCore ? "net4" : "netcore";

            var preferred = targetFrameworks.FirstOrDefault(t => t.StartsWith(preferredTarget));
            if (preferred != null)
                return preferred;

            var netStandardTarget = targetFrameworks.FirstOrDefault(t => t.StartsWith("netstandard"));
            if (netStandardTarget != null)
                return netStandardTarget;

            var secondary = targetFrameworks.FirstOrDefault(t => t.StartsWith(fallbackTarget));
            if (secondary != null)
                return secondary;
            
            throw new JitCompilerException("No compatible TargetFramework could be determined from the project file. At least one of netstandard*, netcore* or net* is required.");
        }

        public bool IsOptimized { get; private set; }
        public bool Prefer32Bit { get; private set; }
        public string PlatformTarget { get; private set; }
        public string OutputPath => Path.Combine(_project.Properties.Item("FullPath").Value.ToString(), _outputPath);

        public string OutputFilename
        {
            get
            {
                string assemblyName = _assemblyName + ".dll";
                if (OutputPath.Trim('\\').EndsWith(_targetFramework))
                    return Path.Combine(OutputPath, assemblyName);

                return Path.Combine(OutputPath, "..", _targetFramework, assemblyName);
            }
        }
        
        public TargetRuntime TargetRuntime
        {
            get
            {
                if (_targetFramework.StartsWith("netstandard"))
                    return _preferredRuntime;

                if (_targetFramework.StartsWith("netcore"))
                    return TargetRuntime.NetCore;

                return TargetRuntime.NetFramework;
            }
        }
    }
}