using System.Windows.Controls;
using System.Windows.Input;

namespace Coingcola.视图层.关于
{
    /// <summary>
    /// 版本更新视图。
    /// </summary>
    public partial class 版本更新视图 : UserControl
    {
        public 版本更新视图()
        {
            InitializeComponent();
            当前版本文本.Text = "v0.1.0";
            版本结论文本.Text = "当前版本已经从早期骨架阶段，推进到多条主链路可演示、可操作、可继续扩展的状态。";
        }

        private void 根滚动容器_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer viewer)
            {
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}