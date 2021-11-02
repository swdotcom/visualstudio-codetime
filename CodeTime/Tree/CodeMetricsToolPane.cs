namespace CodeTime
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Runtime.InteropServices;

    [Guid("B9ADECFD-3D3C-451D-AE3A-90994DB55AA4")]
    public class CodeMetricsToolPane : ToolWindowPane
    {
        private long lastMetricsRebuild = 0;
        private long lastMenuRebuild = 0;
        private static long TREE_REBUILD_THRESHOLD_SECONDS = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeMetricsToolPane"/> class.
        /// </summary>
        public CodeMetricsToolPane() : base(null)
        {
            this.Caption = "CodeTime";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new CodeMetricsTree();
        }

        public void RebuildTree()
        {
            if (this.Content != null)
            {
                _ = ((CodeMetricsTree)Content).RebuildAccountButtons();
                _ = ((CodeMetricsTree)Content).RebuildFlowButtonsAsync();
                _ = ((CodeMetricsTree)Content).RebuildStatsButtonsAsync();
            }
        }

        public void RebuildAccountButtons()
        {
            if (this.Content != null)
            {
                long now = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (now - lastMenuRebuild > TREE_REBUILD_THRESHOLD_SECONDS)
                {
                    _ = ((CodeMetricsTree)Content).RebuildAccountButtons();
                    lastMenuRebuild = now;
                }
            }
        }

        public void RebuildFlowButtons()
        {
            if (this.Content != null)
            {
                _ = ((CodeMetricsTree)Content).RebuildFlowButtonsAsync();
            }
        }

        public void RebuildStatsButtons()
        {
            if (this.Content != null)
            {
                long now = DateTimeOffset.Now.ToUnixTimeSeconds();
                if (now - lastMetricsRebuild > TREE_REBUILD_THRESHOLD_SECONDS)
                {
                    _ = ((CodeMetricsTree)Content).RebuildStatsButtonsAsync();
                    lastMetricsRebuild = now;
                }
            }
        }

        public void ToggleClickHandler()
        {
            StatusBarManager.showingStatusbarMetrics = !StatusBarManager.showingStatusbarMetrics;
            long now = DateTimeOffset.Now.ToUnixTimeSeconds();
            if (this.Content != null)
            {
                _ = ((CodeMetricsTree)Content).RebuildAccountButtons();
            }
        }
    }
}
