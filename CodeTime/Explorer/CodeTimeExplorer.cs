using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace CodeTime
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("9efdc9a8-d40b-4fc6-8036-bf30294494f7")]
    public class CodeTimeExplorer : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeTimeExplorer"/> class.
        /// </summary>
        public CodeTimeExplorer() : base(null)
        {
            this.Caption = "Code Time";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CodeTimeExplorerControl();
        }


        public void RebuildTree()
        {
            if (this.Content != null)
            {
                ((CodeTimeExplorerControl)this.Content).RebuildAccountButtons();
                ((CodeTimeExplorerControl)this.Content).RebuildFlowButtonsAsync();
                ((CodeTimeExplorerControl)this.Content).RebuildStatsButtonsAsync();
            }
        }

        public void RebuildAccountButtons()
        {
            if (this.Content != null)
            {
                ((CodeTimeExplorerControl)this.Content).RebuildAccountButtons();
            }
        }

        public void RebuildFlowButtons()
        {
            if (this.Content != null)
            {
                ((CodeTimeExplorerControl)this.Content).RebuildFlowButtonsAsync();
            }
        }

        public void RebuildStatsButtons()
        {
            if (this.Content != null)
            {
                ((CodeTimeExplorerControl)this.Content).RebuildStatsButtonsAsync();
            }
        }

        public void ToggleClickHandler()
        {
            StatusBarManager.showingStatusbarMetrics = !StatusBarManager.showingStatusbarMetrics;
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (this.Content != null)
            {
                ((CodeTimeExplorerControl)this.Content).RebuildAccountButtons();
            }
        }
    }
}
