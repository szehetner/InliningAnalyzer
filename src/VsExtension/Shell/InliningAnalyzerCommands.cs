//------------------------------------------------------------------------------
// <copyright file="EnableHighlighting.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsExtension.Model;
using System.ComponentModel.Composition;
using EnvDTE80;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using InliningAnalyzer;
using VsExtension.Shell;
using System.Windows.Forms;
using System.Collections.Generic;
using EnvDTE;

namespace VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InliningAnalyzerCommands
    {
        public const int StartCommandId = 0x0101;
        public const int ToggleCommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("cbca2c69-1cc0-4b99-aa32-a621c99552e4");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private OutputWindowLogger _outputLogger;
        private StatusBarLogger _statusBarLogger;

        [Import]
        internal IAnalyzerModel AnalyzerModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="InliningAnalyzerCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private InliningAnalyzerCommands(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var sp = new ServiceProvider(dte2 as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
            var container = sp.GetService(typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel)) as Microsoft.VisualStudio.ComponentModelHost.IComponentModel;
            container.DefaultCompositionService.SatisfyImportsOnce(this);

            _package = package;
            _outputLogger = new OutputWindowLogger(package);
            _statusBarLogger = new StatusBarLogger(package);

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                OleMenuCommand startMenuItem = new OleMenuCommand(StartMenuItemCallback, new CommandID(CommandSet, StartCommandId));
                startMenuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatusStart);
                startMenuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(startMenuItem);


                OleMenuCommand menuItem = new OleMenuCommand(ToggleMenuItemCallback, new CommandID(CommandSet, ToggleCommandId));
                menuItem.BeforeQueryStatus += new EventHandler(OnBeforeQueryStatusToggle);
                menuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(menuItem);
            }
        }

        private void OnBeforeQueryStatusToggle(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null == myCommand)
                return;

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (!dte2.Solution.IsOpen)
            {
                myCommand.Enabled = false;
                return;
            }

            myCommand.Enabled = true;
            myCommand.Text = AnalyzerModel.IsHighlightingEnabled ? "Hide Inlining Analyzer Coloring" : "Show Inlining Analyzer Coloring";
        }

        private void OnBeforeQueryStatusStart(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null == myCommand)
                return;

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (!dte2.Solution.IsOpen)
            {
                myCommand.Enabled = false;
                return;
            }

            myCommand.Enabled = true;
            try
            {
                myCommand.Text = "Run Inlining Analyzer on " + dte2.ActiveDocument?.ProjectItem?.ContainingProject?.Name;
            }
            catch(Exception ex)
            {
                myCommand.Text = "Run Inlining Analyzer on Current Project";
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static InliningAnalyzerCommands Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this._package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new InliningAnalyzerCommands(package);
        }
        
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void ToggleMenuItemCallback(object sender, EventArgs e)
        {
            AnalyzerModel.IsHighlightingEnabled = !AnalyzerModel.IsHighlightingEnabled;
        }

        private void StartMenuItemCallback(object sender, EventArgs e)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var project = dte2.ActiveDocument?.ProjectItem?.ContainingProject;
            if (project == null)
                return;

            var configurationManager = project.ConfigurationManager;
            var active = project.ConfigurationManager.ActiveConfiguration;
            var properties = project.ConfigurationManager.ActiveConfiguration.Properties;

            try
            {
                bool isOptimized = (bool)project.ConfigurationManager.ActiveConfiguration.Properties.Item("Optimize").Value;
                if (!isOptimized)
                {
                    ShowError("The current build configuration does not have the \"Optimize code\" flag set and is therefore not suitable for analysing the JIT compiler.\r\n\r\nPlease enable the the \"Optimize code\" flag (under Project Properties -> Build) or switch to a non-debug configuration (e.g. 'Release') before running the Inlining Analyzer.");
                    return;
                }
            }
            catch (Exception)
            {
            }

            _outputLogger.ActivateWindow();
            _outputLogger.WriteText("Building " + project.Name + "...");

            dte2.Solution.SolutionBuild.BuildProject(project.ConfigurationManager.ActiveConfiguration.ConfigurationName, project.UniqueName, true);
            if (dte2.Solution.SolutionBuild.LastBuildInfo != 0)
            {
                _outputLogger.WriteText("Build failed.");
                return;
            }

            string assemblyFile = GetAssemblyPath(project);
            PlatformTarget platformTarget = GetPlatformTarget(project);

            _statusBarLogger.SetText("Running Inlining Analyzer on " + project.Name);
            _statusBarLogger.StartProgressAnimation();

            _outputLogger.WriteText("Starting Inlining Analyzer...");
            _outputLogger.WriteText("Assembly: " + assemblyFile);
            _outputLogger.WriteText("Platform: " + platformTarget);
            _outputLogger.WriteText("");

            using (var analyzer = new Analyzer())
            {
                JitHostController jitController = new JitHostController(assemblyFile, platformTarget);
                jitController.StartProcess();
                jitController.Process.OutputDataReceived += JitHostOutputDataReceived;
                jitController.Process.BeginOutputReadLine();

                analyzer.StartEventTrace(jitController.Process.Id);
                jitController.RunJitCompilation();
                analyzer.StopEventTrace();

                jitController.Process.OutputDataReceived -= JitHostOutputDataReceived;

                AnalyzerModel.CallGraph = analyzer.AssemblyCallGraph;

                _outputLogger.WriteText("Finished Inlining Analyzer");
                _statusBarLogger.StopProgressAnimation();
                _statusBarLogger.Clear();
            }
        }

        private void JitHostOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _outputLogger.WriteText(e.Data);
        }

        private PlatformTarget GetPlatformTarget(EnvDTE.Project vsProject)
        {
            try
            {
                if (!Environment.Is64BitOperatingSystem)
                    return PlatformTarget.X86;

                var projectProperties = vsProject.ConfigurationManager.ActiveConfiguration.Properties;
                string target = projectProperties.Item("PlatformTarget").Value.ToString();
                if (target == "x64")
                    return PlatformTarget.X64;

                if (target == "x86")
                    return PlatformTarget.X86;

                if ((bool)projectProperties.Item("Prefer32bit").Value)
                    return PlatformTarget.X86;
            }
            catch (Exception) { }

            return GetFallbackPlatformTarget();
        }

        private PlatformTarget GetFallbackPlatformTarget()
        {
            return PlatformTarget.X64;
        }

        private static string GetAssemblyPath(EnvDTE.Project vsProject)
        {
            try
            {
                string fullPath = vsProject.Properties.Item("FullPath").Value.ToString();
                string outputPath = vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();
                string outputDir = Path.Combine(fullPath, outputPath);
                string outputFileName = vsProject.Properties.Item("OutputFileName").Value.ToString();
                string assemblyPath = Path.Combine(outputDir, outputFileName);

                if (!File.Exists(assemblyPath))
                    return GetUserSelectedAssemblyPath(vsProject);

                return assemblyPath;
            }
            catch (Exception)
            {
                return GetUserSelectedAssemblyPath(vsProject);
            }
        }

        private static string GetUserSelectedAssemblyPath(EnvDTE.Project vsProject)
        {
            string initialDirectory = null;
            try
            {
                initialDirectory = Path.Combine(vsProject.Properties.Item("FullPath").Value.ToString(), "bin");
            }
            catch (Exception) { }

            OpenFileDialog dialog = new OpenFileDialog()
                                    {
                                        Title = "Select Assembly to analyze",
                                        DefaultExt = "Assembly Files(*.DLL;*.EXE)|*.DLL;*.EXE|All files (*.*)|*.*",
                                        InitialDirectory = initialDirectory
                                    };
            if (dialog.ShowDialog() != DialogResult.OK)
                return null;

            return dialog.FileName;
        }

        private void ShowError(string message)
        {
            string title = "Inlining Analyzer";

            VsShellUtilities.ShowMessageBox(
                this.ServiceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
