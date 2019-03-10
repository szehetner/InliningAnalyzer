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
using VsExtension.Common;

namespace VsExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class InliningAnalyzerCommands
    {
        public const int StartCommandId = 0x0101;
        public const int StartForAssemblyCommandId = 0x0103;
        public const int ToggleCommandId = 0x0100;
        public const int OpenOptionsCommandId = 0x0102;
        public const int StartForScopeCommandId = 0x0131;
        
        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("cbca2c69-1cc0-4b99-aa32-a621c99552e4");
        public static readonly Guid CommandSetContextMenu = new Guid("B790E7BC-2D80-45AA-BF6C-6807582F1D32");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage _package;
        private IServiceProvider _serviceProvider;

        private OutputWindowLogger _outputLogger;
        private StatusBarLogger _statusBarLogger;

        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable CS0649

        [Import]
        internal IAnalyzerModel AnalyzerModel;

        [Import]
        internal VisualStudioWorkspace Workspace;

        [Import]
        internal ICommonProjectPropertyProviderFactory CommonProjectPropertyProviderFactory;

#pragma warning restore CS0649

        private InliningAnalyzerCommands(AsyncPackage package)
        {
            if (package == null)
                throw new ArgumentNullException("package");

            _package = package;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InliningAnalyzerCommands"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        private async Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            _serviceProvider = new ServiceProvider(dte2 as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

            var container = await _package.GetServiceAsync(typeof(Microsoft.VisualStudio.ComponentModelHost.SComponentModel)) as Microsoft.VisualStudio.ComponentModelHost.IComponentModel;
            container.DefaultCompositionService.SatisfyImportsOnce(this);
            
            _outputLogger = new OutputWindowLogger(_package);
            _statusBarLogger = new StatusBarLogger(_package);

            OleMenuCommandService commandService = await _package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                OleMenuCommand startMenuItem = new OleMenuCommand(StartMenuItemCallback, new CommandID(CommandSet, StartCommandId));
                startMenuItem.BeforeQueryStatus += OnBeforeQueryStatusProject;
                startMenuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(startMenuItem);

                OleMenuCommand startForAssemblyMenuItem = new OleMenuCommand(StartForAssemblyMenuItemCallback, new CommandID(CommandSet, StartForAssemblyCommandId));
                startForAssemblyMenuItem.BeforeQueryStatus += OnBeforeQueryStatusEnabled;
                startForAssemblyMenuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(startForAssemblyMenuItem);

                OleMenuCommand menuItem = new OleMenuCommand(ToggleMenuItemCallback, new CommandID(CommandSet, ToggleCommandId));
                menuItem.BeforeQueryStatus += OnBeforeQueryStatusToggle;
                menuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(menuItem);

                OleMenuCommand optionsMenuItem = new OleMenuCommand(OpenOptionsCallback, new CommandID(CommandSet, OpenOptionsCommandId));
                commandService.AddCommand(optionsMenuItem);
                
                OleMenuCommand contextMenuItem = new OleMenuCommand(StartForScopeMenuItemCallback, new CommandID(CommandSetContextMenu, StartForScopeCommandId));
                contextMenuItem.BeforeQueryStatus += OnBeforeQueryStatusEnabled;
                contextMenuItem.Enabled = dte2.Solution.IsOpen;
                commandService.AddCommand(contextMenuItem);
            }
        }

        private async void OnBeforeQueryStatusToggle(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null == myCommand)
                return;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (!dte2.Solution.IsOpen)
            {
                myCommand.Enabled = false;
                return;
            }

            myCommand.Enabled = true;
            myCommand.Text = AnalyzerModel.IsHighlightingEnabled ? "Hide Inlining Analyzer Coloring" : "Show Inlining Analyzer Coloring";
        }

        private async void OnBeforeQueryStatusProject(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null == myCommand)
                return;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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

        private async void OnBeforeQueryStatusEnabled(object sender, EventArgs e)
        {
            var myCommand = sender as OleMenuCommand;
            if (null == myCommand)
                return;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte2 = (DTE2)Package.GetGlobalService(typeof(SDTE));
            if (!dte2.Solution.IsOpen)
            {
                myCommand.Enabled = false;
                return;
            }

            myCommand.Enabled = true;
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
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            Instance = new InliningAnalyzerCommands(package);
            await Instance.InitializeAsync();
        }
        
        private void ToggleMenuItemCallback(object sender, EventArgs e)
        {
            AnalyzerModel.IsHighlightingEnabled = !AnalyzerModel.IsHighlightingEnabled;
        }
        
        private async void StartForScopeMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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

                TargetScope targetScope = TargetScopeResolver.GetTargetScope(semanticModel, syntaxTree, selection.CurrentLine, selection.CurrentColumn);
                if (targetScope == null)
                {
                    await ShowError("Could not determine selected Scope (Method or Class).");
                    return;
                }

                await RunAnalyzer(targetScope);
            }
            catch(Exception ex)
            {
                await ShowError(ex.Message);
            }
        }
        
        private async void StartMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                await RunAnalyzer(new TargetScope(ScopeType.Project));
            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        }

        private async void StartForAssemblyMenuItemCallback(object sender, EventArgs e)
        {
            try
            {
                await RunAnalyzer(new TargetScope(ScopeType.AssemblyFile));
            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        }

        private async Task RunAnalyzer(TargetScope targetScope)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte2 = (DTE2) Package.GetGlobalService(typeof(SDTE));
            var project = dte2.ActiveDocument?.ProjectItem?.ContainingProject;
            if (project == null)
                return;

            var propertyProvider = CreateProjectPropertyProvider(project, OptionsProvider);
            await propertyProvider.LoadProperties();

            _outputLogger.ActivateWindow();
            string publishPath = null;
            JitTarget jitTarget;
            if (targetScope.RequiresBuild)
            {
                try
                {
                    if (!propertyProvider.IsOptimized)
                    {
                        await ShowError(
                            "The current build configuration does not have the \"Optimize code\" flag set and is therefore not suitable for analysing the JIT compiler.\r\n\r\nPlease enable the the \"Optimize code\" flag (under Project Properties -> Build) or switch to a non-debug configuration (e.g. 'Release') before running the Inlining Analyzer.");
                        return;
                    }
                }
                catch (Exception)
                {
                }
            
                _outputLogger.WriteText("Building " + project.Name + "...");

                string configurationName = project.ConfigurationManager.ActiveConfiguration.ConfigurationName;
                dte2.Solution.SolutionBuild.BuildProject(configurationName, project.UniqueName, true);
                if (dte2.Solution.SolutionBuild.LastBuildInfo != 0)
                {
                    _outputLogger.WriteText("Build failed.");
                    return;
                }
            
                _outputLogger.ActivateWindow();

                jitTarget = new JitTarget(DetermineTargetPlatform(propertyProvider), propertyProvider.TargetRuntime);
                
                if (ProjectPublisher.IsPublishingNecessary(propertyProvider))
                {
                    var publisher = new ProjectPublisher(jitTarget, configurationName, propertyProvider, _outputLogger);
                    if (!await Task.Run(() => publisher.Publish()))
                        return;
                    publishPath = publisher.PublishPath;
                }
            }
            else
            {
                // TODO: read target platform from assembly file
                jitTarget = new JitTarget(DetermineTargetPlatform(propertyProvider), OptionsProvider.PreferredRuntime);
            }
            string assemblyFile = GetAssemblyPath(propertyProvider, publishPath, targetScope);
            if (assemblyFile == null)
                return;

            _statusBarLogger.SetText("Running Inlining Analyzer on " + project.Name);
            _statusBarLogger.StartProgressAnimation();

            _outputLogger.WriteText("");
            _outputLogger.WriteText("Starting Inlining Analyzer...");
            if (!string.IsNullOrEmpty(propertyProvider.TargetFramework))
                _outputLogger.WriteText("TargetFramework: " + propertyProvider.TargetFramework);
            _outputLogger.WriteText("Assembly: " + assemblyFile);
            _outputLogger.WriteText("Runtime: " + jitTarget.Runtime);
            _outputLogger.WriteText("Platform: " + jitTarget.Platform);
            _outputLogger.WriteText(targetScope.ToString());
            _outputLogger.WriteText("");

            try
            {
                var runner = new JitRunner(assemblyFile, jitTarget, targetScope, _outputLogger, new JitHostPathResolver());
                AnalyzerModel.CallGraph = await Task.Run(() => runner.Run());

                _outputLogger.WriteText("Finished Inlining Analyzer");
            }
            catch (JitCompilerException jitException)
            {
                await ShowError(jitException.Message);
            }
            catch (Exception ex)
            {
                _outputLogger.WriteText(ex.ToString());
                await ShowError("Jit Compilation failed with errors. Check the Inlining Analyzer Output Window for details.");
            }
            finally
            {
                if (publishPath != null)
                {
                    try
                    {
                        Directory.Delete(publishPath, true);
                    }
                    catch (Exception) { }
                }
            }
            _statusBarLogger.StopProgressAnimation();
            _statusBarLogger.Clear();
        }

        private IProjectPropertyProvider CreateProjectPropertyProvider(Project project, IOptionsProvider optionsProvider)
        {
            if (CommonProjectPropertyProviderFactory.IsNewProjectFormat(project))
            {
                return CommonProjectPropertyProviderFactory.Create(project, optionsProvider);
            }

            return new LegacyProjectPropertyProvider(project);
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

        private static string GetAssemblyPath(IProjectPropertyProvider propertyProvider, string publishPath, TargetScope targetScope)
        {
            try
            {
                if (targetScope.ScopeType != ScopeType.AssemblyFile)
                {
                    string outputFilename = propertyProvider.GetOutputFilename(publishPath);

                    if (File.Exists(outputFilename))
                        return outputFilename;
                }

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

            OpenFileDialog dialog = new OpenFileDialog
                                    {
                                        Title = "Select Assembly to analyze",
                                        DefaultExt = "Assembly Files(*.DLL;*.EXE)|*.DLL;*.EXE|All files (*.*)|*.*",
                                        InitialDirectory = initialDirectory
                                    };
            if (dialog.ShowDialog() != DialogResult.OK)
                return null;

            return dialog.FileName;
        }

        private async Task ShowError(string message)
        {
            string title = "Inlining Analyzer";

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            VsShellUtilities.ShowMessageBox(
                _serviceProvider,
                message,
                title,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private IOptionsProvider OptionsProvider
        {
            get
            {
                InliningAnalyzerOptionsPage optionsPage = (InliningAnalyzerOptionsPage)_package.GetDialogPage(typeof(InliningAnalyzerOptionsPage));
                return optionsPage;
            }
        }
                       
        private async void OpenOptionsCallback(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _package.ShowOptionPage(typeof(InliningAnalyzerOptionsPage));
        }
    }
}
