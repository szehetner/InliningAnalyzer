//------------------------------------------------------------------------------
// <copyright file="EnableHighlighting.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsExtension.Model;
using System.ComponentModel.Composition;
using EnvDTE80;
using System.IO;
using InliningAnalyzer;
using VsExtension.Shell;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using VsExtension.Shell.Runner;
using Task = System.Threading.Tasks.Task;

namespace VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InliningAnalyzerCommands
    {
        public const int StartCommandId = 0x0101;
        public const int ToggleCommandId = 0x0100;
        public const int OpenOptionsCommandId = 0x0102;
        public const int StartSingleMethodCommandId = 0x0131;
        
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("cbca2c69-1cc0-4b99-aa32-a621c99552e4");
        public static readonly Guid CommandSetContextMenu = new Guid("B790E7BC-2D80-45AA-BF6C-6807582F1D32");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private OutputWindowLogger _outputLogger;
        private StatusBarLogger _statusBarLogger;

        [Import]
        internal IAnalyzerModel AnalyzerModel;

        [Import]
        internal VisualStudioWorkspace Workspace;
        
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

                OleMenuCommand optionsMenuItem = new OleMenuCommand(OpenOptionsCallback, new CommandID(CommandSet, OpenOptionsCommandId));
                commandService.AddCommand(optionsMenuItem);
                
                OleMenuCommand contextMenuItem = new OleMenuCommand(StartSingleMethodMenuItemCallback, new CommandID(CommandSetContextMenu, StartSingleMethodCommandId));
                contextMenuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(contextMenuItem);
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
            catch(Exception)
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
        
        private void ToggleMenuItemCallback(object sender, EventArgs e)
        {
            AnalyzerModel.IsHighlightingEnabled = !AnalyzerModel.IsHighlightingEnabled;
        }
        
        private async void StartSingleMethodMenuItemCallback(object sender, EventArgs e)
        {
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            var project = dte2.ActiveDocument?.ProjectItem?.ContainingProject;
            if (project == null)
                return;

            var selection = (TextSelection)dte2.ActiveDocument.Selection;
            var filename = dte2.ActiveDocument.FullName;
            var documentId = Workspace.CurrentSolution.GetDocumentIdsWithFilePath(filename);
            if (documentId.IsEmpty)
                return;

            var document = Workspace.CurrentSolution.GetDocument(documentId[0]);
            var semanticModel = await document.GetSemanticModelAsync();
            var syntaxTree = await document.GetSyntaxTreeAsync();

            string methodName = MethodNameResolver.GetMethodName(semanticModel, syntaxTree, selection.CurrentLine, selection.CurrentColumn);
            if (methodName == null)
            {
                ShowError("Could not determine selected Method.");
                return;
            }

            await RunAnalyzer(methodName);
        }
        
        private async void StartMenuItemCallback(object sender, EventArgs e)
        {
            await RunAnalyzer(null);
        }
        
        private async Task RunAnalyzer(string methodName)
        {
            var dte2 = (DTE2) Package.GetGlobalService(typeof(SDTE));
            var project = dte2.ActiveDocument?.ProjectItem?.ContainingProject;
            if (project == null)
                return;

            var propertyProvider = CreateProjectPropertyProvider(project, PreferredRuntime);
            await propertyProvider.LoadProperties();

            try
            {
                if (!propertyProvider.IsOptimized)
                {
                    ShowError(
                        "The current build configuration does not have the \"Optimize code\" flag set and is therefore not suitable for analysing the JIT compiler.\r\n\r\nPlease enable the the \"Optimize code\" flag (under Project Properties -> Build) or switch to a non-debug configuration (e.g. 'Release') before running the Inlining Analyzer.");
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

            _outputLogger.ActivateWindow();

            string assemblyFile = GetAssemblyPath(propertyProvider);
            JitTarget jitTarget = new JitTarget(DetermineTargetPlatform(propertyProvider), propertyProvider.TargetRuntime);

            _statusBarLogger.SetText("Running Inlining Analyzer on " + project.Name);
            _statusBarLogger.StartProgressAnimation();

            _outputLogger.WriteText("Starting Inlining Analyzer...");
            _outputLogger.WriteText("Assembly: " + assemblyFile);
            _outputLogger.WriteText("Runtime: " + jitTarget.Runtime);
            _outputLogger.WriteText("Platform: " + jitTarget.Platform);
            if (methodName == null)
                _outputLogger.WriteText("Method: All");
            else
                _outputLogger.WriteText("Method: " + methodName);
            _outputLogger.WriteText("");

            try
            {
                var runner = new JitRunner(assemblyFile, jitTarget, methodName, _outputLogger);
                AnalyzerModel.CallGraph = runner.Run();

                _outputLogger.WriteText("Finished Inlining Analyzer");
            }
            catch (JitCompilerException jitException)
            {
                ShowError(jitException.Message);
            }
            catch (Exception ex)
            {
                _outputLogger.WriteText(ex.ToString());
                ShowError("Jit Compilation failed with errors. Check the Inlining Analyzer Output Window for details.");
            }

            _statusBarLogger.StopProgressAnimation();
            _statusBarLogger.Clear();
        }

        private static IProjectPropertyProvider CreateProjectPropertyProvider(Project project,
            TargetRuntime preferredRuntime)
        {
            if (IsNewProjectFormat(project))
                return new CommonProjectPropertyProvider(project, preferredRuntime);

            return new LegacyProjectPropertyProvider(project);
        }

        private static bool IsNewProjectFormat(EnvDTE.Project vsProject)
        {
            return vsProject.Kind == "{9A19103F-16F7-4668-BE54-9A1E7A4F7556}";
        }
        
        private TargetPlatform DetermineTargetPlatform(IProjectPropertyProvider propertyProvider)
        {
            try
            {
                if (!Environment.Is64BitOperatingSystem)
                    return TargetPlatform.X86;

                string target = propertyProvider.PlatformTarget;
                if (target == "x64")
                    return TargetPlatform.X64;

                if (target == "x86")
                    return TargetPlatform.X86;

                if (propertyProvider.Prefer32Bit)
                    return TargetPlatform.X86;
            }
            catch (Exception) { }

            return TargetPlatform.X64;
        }

        private static string GetAssemblyPath(IProjectPropertyProvider propertyProvider)
        {
            try
            {
                string outputFilename = propertyProvider.OutputFilename;
                
                if (File.Exists(outputFilename))
                    return outputFilename;

                return GetUserSelectedAssemblyPath(propertyProvider);
            }
            catch (Exception)
            {
                return GetUserSelectedAssemblyPath(propertyProvider);
            }
        }

        private static string GetUserSelectedAssemblyPath(IProjectPropertyProvider propertyProvider)
        {
            string initialDirectory = null;
            try
            {
                initialDirectory = propertyProvider.OutputPath;
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

        private TargetRuntime PreferredRuntime
        {
            get
            {
                InliningAnalyzerOptionsPage optionsPage = (InliningAnalyzerOptionsPage) _package.GetDialogPage(typeof(InliningAnalyzerOptionsPage));
                return optionsPage.PreferredRuntime;
            }
        }

        private void OpenOptionsCallback(object sender, EventArgs e)
        {
            _package.ShowOptionPage(typeof(InliningAnalyzerOptionsPage));
        }
    }
}
