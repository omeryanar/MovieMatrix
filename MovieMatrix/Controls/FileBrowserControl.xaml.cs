using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Grid;
using DevExpress.Xpf.Grid.TreeList;
using MovieMatrix.Helper;

namespace MovieMatrix.Controls
{
    public partial class FileBrowserControl : TreeListControl
    {
        public FileBrowserControl()
        {
            InitializeComponent();

            View.NodeImageSelector = new TreeImageSelector();
            View.RowDoubleClick += (s, e) =>
            {
                try
                {
                    if (CurrentItem != null)
                        Process.Start(CurrentItem.ToString());
                }
                catch { }
            };
        }
    }

    public class TreeImageSelector : TreeListNodeImageSelector
    {
        public override ImageSource Select(TreeListRowData rowData)
        {
            if (rowData?.Node?.Content != null)
            {
                if (FileHelper.ImageFiles.Any(x => rowData.Node.Content.ToString().EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    return Image;
                if (FileHelper.VideoFiles.Any(x => rowData.Node.Content.ToString().EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    return Video;
            }

            return Folder;
        }

        private static BitmapImage Image = new BitmapImage(new Uri("pack://application:,,,/MovieMatrix;component/Assets/Icon/Image.png"));

        private static BitmapImage Video = new BitmapImage(new Uri("pack://application:,,,/MovieMatrix;component/Assets/Icon/Video.png"));

        private static BitmapImage Folder = new BitmapImage(new Uri("pack://application:,,,/MovieMatrix;component/Assets/Icon/Folder.png"));
    }
}
