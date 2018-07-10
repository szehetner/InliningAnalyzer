using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InliningAnalyzer
{
    public class JitHostController
    {
        private readonly string _assemblyFile;
        private readonly JitTarget _jitTarget;
        private readonly TargetScope _targetScope;
        private readonly string _methodListFile;

        public Process Process { get; private set; }
        
        public JitHostController(string assemblyFile, JitTarget jitTarget, TargetScope targetScope, string methodListFile)
        {
            _assemblyFile = assemblyFile;
            _jitTarget = jitTarget;
            _targetScope = targetScope;
            _methodListFile = methodListFile;
        }

        private string GetJitHostExePath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string extensionPath = Path.GetDirectoryName(path);

            return Path.Combine(extensionPath, GetJitHostExe());
        }

        public void StartProcess()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = GetJitHostExePath(),
                Arguments = BuildArguments(),
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };
            Process = Process.Start(startInfo);
        }

        private string BuildArguments()
        {
            if (_jitTarget.Runtime == TargetRuntime.NetCore)
            {
                string jitHostDll = _jitTarget.Platform == TargetPlatform.X64
                    ? "JitHost.Core.x64.dll "
                    : "JitHost.Core.x86.dll ";

                return jitHostDll + BuildRawArguments();
            }

            return BuildRawArguments();
        }

        private string BuildRawArguments()
        {
            if (_methodListFile != null)
                return "\"" + _assemblyFile + "\" /l:\"" + _methodListFile + "\"";

            if (_targetScope == null)
                return "\"" + _assemblyFile + "\"";

            if (_targetScope.ScopeType == ScopeType.Class)
                return "\"" + _assemblyFile + "\" /c:\"" + _targetScope.Name + "\"";

            if (_targetScope.ScopeType == ScopeType.Method)
                return "\"" + _assemblyFile + "\" /m:\"" + _targetScope.Name + "\"";

            throw new InvalidOperationException();
        }
        
        public void RunJitCompilation()
        {
            Process.StandardInput.WriteLine();
            Process.WaitForExit();
        }

        private string GetJitHostExe()
        {
            if (_jitTarget.Runtime == TargetRuntime.NetCore)
            {
                var dotnetExecutable = GetDotnetExecutablePath();
                CheckDotnetExeVersion(dotnetExecutable);
                return dotnetExecutable;
            }

            switch (_jitTarget.Platform)
            {
                case TargetPlatform.X86:
                    return "JitHost.x86.exe";
                case TargetPlatform.X64:
                    return "JitHost.x64.exe";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CheckDotnetExeVersion(string dotnetExecutable)
        {
            string versionString;
            int major;
            int minor;
            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = dotnetExecutable,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }))
                {
                    versionString = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    string[] parts = versionString.Trim().Split('.');
                    if (parts.Length < 2)
                        return;

                    major = int.Parse(parts[0]);
                    minor = int.Parse(parts[1]);
                }
            }
            catch (Exception)
            {
                return;
            }

            if (major < 2 || minor < 1)
                throw new JitCompilerException(".Net Core 2.1+ is required to run the Inlining Analyzer on .NetCore projects.\r\nCurrent .Net Core Version: " + versionString);
        }

        private string GetDotnetExecutablePath()
        {
            string dotnetExecutable;
            if (_jitTarget.Platform == TargetPlatform.X64)
                dotnetExecutable = Environment.ExpandEnvironmentVariables(@"%ProgramW6432%\dotnet\dotnet.exe");
            else
                dotnetExecutable = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\dotnet\dotnet.exe");
            
            if (!File.Exists(dotnetExecutable))
                throw new JitCompilerException($"dotnet.exe could not be found (expected under {dotnetExecutable}).\r\nPlease verify that the correct .Net Core Runtime is installed!");

            return dotnetExecutable;
        }
    }
}
