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
        private readonly IOptionsProvider _optionsProvider;
        private string _outputPath;
        private string _assemblyName;

        public CommonProjectPropertyProvider(Project project, IOptionsProvider optionsProvider)
        {
            _project = project;
            _optionsProvider = optionsProvider;
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
            TargetFramework = DetermineTargetFramework(frameworks);
        }

        private string DetermineTargetFramework(List<string> targetFrameworks)
        {
            if (targetFrameworks.Count == 1)
            {
                // only one targetFramework in project file
                return targetFrameworks[0];
            }

            // check if preferred framework from options dialog is in project file
            var configured = targetFrameworks.FirstOrDefault(t => t.StartsWith(_optionsProvider.PreferredTargetFramework ?? ""));
            if (configured != null)
                return configured;

            // derive preferred framework from preferred runtime
            string runtimePreferredTarget = _optionsProvider.PreferredRuntime == TargetRuntime.NetCore ? "netcore" : "net4";
            string fallbackTarget = _optionsProvider.PreferredRuntime == TargetRuntime.NetCore ? "net4" : "netcore";

            var runtimePreferred = targetFrameworks.FirstOrDefault(t => t.StartsWith(runtimePreferredTarget));
            if (runtimePreferred != null)
                return runtimePreferred;

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
        public string ProjectPath => _project.Properties.Item("FullPath").Value.ToString();
        public string OutputPath => Path.Combine(ProjectPath, _outputPath);
        public string TargetFramework { get; set; }

        public string GetOutputFilename(string publishPath)
        {
            string assemblyName = _assemblyName + ".dll";

            if (publishPath != null)
                return Path.Combine(publishPath, assemblyName);

            if (OutputPath.Trim('\\').EndsWith(TargetFramework))
                return Path.Combine(OutputPath, assemblyName);

            return Path.Combine(OutputPath, "..", TargetFramework, assemblyName);
        }
        
        public TargetRuntime TargetRuntime
        {
            get
            {
                if (TargetFramework.StartsWith("netstandard"))
                    return _optionsProvider.PreferredRuntime;

                if (TargetFramework.StartsWith("netcore"))
                    return TargetRuntime.NetCore;

                return TargetRuntime.NetFramework;
            }
        }
    }
}