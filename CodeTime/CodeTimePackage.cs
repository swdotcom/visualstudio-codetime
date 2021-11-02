﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace CodeTime
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(CodeTimePackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideToolWindow(typeof(CodeMetricsToolPane), Window = ToolWindowGuids.SolutionExplorer, MultiInstances = false)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class CodeTimePackage : AsyncPackage
    {
        /// <summary>
        /// CodeTimePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "f8cb9ea8-4214-42d8-8b7f-5d6e6c5cf50e";

        private static CodeTimePackage package = null;
        private static bool initialized = false;

        public static DTE ObjDte = null;

        private Events2 events;
        private DocumentEvents _docEvents;
        private TextEditorEvents _textEditorEvents;
        private TextDocumentKeyPressEvents _textDocKeyEvents;
        private WindowVisibilityEvents _windowVisibilityEvents;

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // obtain the DTE service to track doc changes
            ObjDte = await GetServiceAsync(typeof(DTE)) as DTE;
            // events = (Events2)ObjDte.Events;

            // Intialize the document event handlers
            // _textEditorEvents = events.TextEditorEvents;
            // _textDocKeyEvents = events.TextDocumentKeyPressEvents;
            // _docEvents = events.DocumentEvents;
            // _windowVisibilityEvents = events.WindowVisibilityEvents;

            // init the package manager that will use the AsyncPackage to run main thread requests
            package = this;
            PackageManager.initialize(package, ObjDte);
            _ = Task.Delay(10000).ContinueWith((task) => { CheckSolutionActivation(); });
        }

        public async void CheckSolutionActivation()
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (!initialized)
            {
                // don't initialize the rest of the plugin until a project is loaded
                string solutionDir = await PackageManager.GetSolutionDirectory();
                if (string.IsNullOrEmpty(solutionDir))
                {
                    // no solution, try again later
                    _ = System.Threading.Tasks.Task.Delay(8000).ContinueWith((task) => { CheckSolutionActivation(); });
                }
                else
                {
                    // solution is activated or it's empty, initialize
                    _ = System.Threading.Tasks.Task.Delay(1000).ContinueWith((task) => { InitializePlugin(); });
                }
            }
        }

        private void InitializePlugin()
        {
            _ = PackageManager.InitializeStatusBar();

            _ = TreeViewCommand.InitializeAsync(this);

            initialized = true;
        }

            [STAThread]
        public static async Task OpenCodeMetricsPaneAsync()
        {
            if (package == null)
            {
                return;
            }

            try
            {
                await package.JoinableTaskFactory.SwitchToMainThreadAsync();
                ToolWindowPane window = package.FindToolWindow(typeof(CodeMetricsToolPane), 0, true);
                if (window != null && window.Frame != null)
                {
                    IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                    windowFrame.Show();
                }
            }
            catch (Exception) { }
        }

        #endregion
    }
}