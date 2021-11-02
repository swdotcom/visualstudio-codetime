using System.Windows.Controls;

namespace CodeTime
{
    public class CodeMetricsTreeItem : TreeViewItem
    {

        public string ItemId { get; set; }
        public CodeMetricsTreeItem(string ItemId)
        {
            this.ItemId = ItemId;

        }
    }
}
