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
        private readonly PlatformTarget _platformTarget;
        private readonly string _methodName;
        private readonly string _methodListFile;

        public Process Process { get; private set; }
        
        public JitHostController(string assemblyFile, PlatformTarget platformTarget, string methodName, string methodListFile)
        {
            _assemblyFile = assemblyFile;
            _platformTarget = platformTarget;
            _methodName = methodName;
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
                UseShellExecute = false
            };
            Process = Process.Start(startInfo);
        }

        private string BuildArguments()
        {
            if (_methodListFile != null)
                return "\"" + _assemblyFile + "\" /l:\"" + _methodListFile + "\"";

            if (_methodName == null)
                return "\"" + _assemblyFile + "\"";

            return "\"" + _assemblyFile + "\" /m:\"" + _methodName + "\"";
        }
        
        public void RunJitCompilation()
        {
            Process.StandardInput.WriteLine();
            Process.WaitForExit();
        }

        private string GetJitHostExe()
        {
            switch(_platformTarget)
            {
                case PlatformTarget.X86:
                    return "JitHost.x86.exe";
                case PlatformTarget.X64:
                    return "JitHost.x64.exe";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
