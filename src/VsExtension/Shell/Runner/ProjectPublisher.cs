using InliningAnalyzer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsExtension.Shell.Runner
{
    public class ProjectPublisher 
    {
        private readonly JitTarget _jitTarget;
        private readonly string _configurationName;
        private readonly IProjectPropertyProvider _propertyProvider;
        private readonly ILogger _logger;

        public string PublishPath { get; private set; }

        public ProjectPublisher(JitTarget jitTarget, string configurationName, IProjectPropertyProvider propertyProvider, ILogger logger)
        {
            _jitTarget = jitTarget;
            _configurationName = configurationName;
            _propertyProvider = propertyProvider;
            _logger = logger;
        }

        public bool Publish()
        {
            DeterminePublishPath();
            _logger.WriteText("Publishing project to " + PublishPath);

            return RunPublisher();
        }

        private bool RunPublisher()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = JitHostController.GetDotnetExecutablePath(_jitTarget),
                Arguments = BuildArguments(),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = _propertyProvider.ProjectPath
            };
            using (var process = Process.Start(startInfo))
            {
                process.OutputDataReceived += OutputDataReceived;
                process.ErrorDataReceived += OutputDataReceived;
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                process.OutputDataReceived -= OutputDataReceived;
                process.ErrorDataReceived -= OutputDataReceived;

                return process.ExitCode == 0;
            }
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                _logger.WriteText(e.Data);
        }

        private string BuildArguments()
        {
            return $"publish --no-build --output \"{PublishPath}\" -f \"{_propertyProvider.TargetFramework}\" -c \"{_configurationName}\"";
        }
        
        private string DeterminePublishPath()
        {
            string tempRoot = Path.GetTempPath();
            string tempFolder = "InliningAnalyzer_";

            int i = 1;
            do
            {
                PublishPath = Path.Combine(tempRoot, tempFolder + i);
                i++;
            }
            while (Directory.Exists(PublishPath));

            return PublishPath;
        }

        public static bool IsPublishingNecessary(IProjectPropertyProvider propertyProvider)
        {
            return propertyProvider.TargetFramework.StartsWith("netstandard") || propertyProvider.TargetFramework.StartsWith("netcore");
        }
    }
}
