using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Coingcola.视图层.软件中心
{
    /// <summary>
    /// 软件更新视图。
    /// </summary>
    public partial class 软件更新视图 : UserControl
    {
        private readonly 软件更新服务 _软件更新服务 = new();

        private List<软件更新项> _当前列表 = new();
        private string _当前分类 = "全部";

        public 软件更新视图()
        {
            InitializeComponent();
            Loaded += 软件更新视图_Loaded;
        }

        private void 软件更新视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        public void 刷新页面()
        {
            _当前分类 = "全部";
            搜索输入框.Text = string.Empty;
            _当前列表 = _软件更新服务.获取更新列表();

            绑定列表(_当前列表);
            更新分类按钮样式();
            刷新摘要();

            页面状态文本.Text = _软件更新服务.系统支持Winget()
                ? "当前系统已检测到 winget，可直接发起部分软件更新。"
                : "当前系统未检测到 winget，系统更新类软件将自动降级为官网更新。";
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        private void 全部系统更新按钮_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "将尝试发起全部系统软件更新。\n\n此操作会拉起更新窗口并保留输出，是否继续？",
                "确认更新",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            var result = _软件更新服务.执行全部系统更新();
            页面状态文本.Text = result.提示;

            if (!result.成功)
            {
                MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void 搜索输入框_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                执行搜索();
            }
        }

        private void 检索按钮_Click(object sender, RoutedEventArgs e)
        {
            执行搜索();
        }

        private void 重置按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        private void 全部分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("全部");
        private void 浏览器分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("浏览器");
        private void 办公分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("办公");
        private void 通讯分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("通讯");
        private void 工具分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("工具");
        private void 压缩分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("压缩");
        private void 开发分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("开发");
        private void 播放器分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("播放器");

        private void 执行按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            string id = button.Tag?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            软件更新项? item = _当前列表.Find(x => x.Id == id);
            if (item == null)
            {
                return;
            }

            string confirmText = item.更新方式 == "系统更新"
                ? $"将尝试更新“{item.名称}”。\n\n若系统支持 winget，会拉起更新窗口并保留输出。\n是否继续？"
                : $"将打开“{item.名称}”官网。\n是否继续？";

            var confirm = MessageBox.Show(
                confirmText,
                "确认操作",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }

            var result = _软件更新服务.执行更新(id);
            页面状态文本.Text = result.提示;

            if (!result.成功)
            {
                MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void 执行搜索()
        {
            string key = 搜索输入框.Text?.Trim() ?? string.Empty;
            _当前列表 = _软件更新服务.搜索更新项(key, _当前分类);
            绑定列表(_当前列表);
        }

        private void 切换分类(string 分类)
        {
            _当前分类 = 分类;
            执行搜索();
            更新分类按钮样式();
        }

        private void 绑定列表(List<软件更新项> 列表)
        {
            更新列表控件.ItemsSource = null;
            更新列表控件.ItemsSource = 列表;

            结果数量文本.Text = $"共 {列表.Count} 项";
            空结果文本.Visibility = 列表.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void 刷新摘要()
        {
            var 全部 = _软件更新服务.获取更新列表();

            var 摘要列表 = new List<摘要卡片项>
            {
                new 摘要卡片项
                {
                    标题 = "更新入口",
                    数值 = 全部.Count.ToString(),
                    说明 = "当前已接入的软件更新入口数量。"
                },
                new 摘要卡片项
                {
                    标题 = "系统更新",
                    数值 = _软件更新服务.获取系统更新数量().ToString(),
                    说明 = "可通过系统命令直接发起更新的软件数。"
                },
                new 摘要卡片项
                {
                    标题 = "官网更新",
                    数值 = _软件更新服务.获取官网更新数量().ToString(),
                    说明 = "当前通过官网处理更新的软件数。"
                }
            };

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = 摘要列表;
        }

        private void 更新分类按钮样式()
        {
            设置分类按钮样式(全部分类按钮, _当前分类 == "全部");
            设置分类按钮样式(浏览器分类按钮, _当前分类 == "浏览器");
            设置分类按钮样式(办公分类按钮, _当前分类 == "办公");
            设置分类按钮样式(通讯分类按钮, _当前分类 == "通讯");
            设置分类按钮样式(工具分类按钮, _当前分类 == "工具");
            设置分类按钮样式(压缩分类按钮, _当前分类 == "压缩");
            设置分类按钮样式(开发分类按钮, _当前分类 == "开发");
            设置分类按钮样式(播放器分类按钮, _当前分类 == "播放器");
        }

        private void 设置分类按钮样式(Button 按钮, bool 是否选中)
        {
            if (是否选中)
            {
                按钮.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2563EB"));
                按钮.Foreground = Brushes.White;
                按钮.BorderBrush = Brushes.Transparent;
                按钮.BorderThickness = new Thickness(0);
            }
            else
            {
                按钮.Background = Brushes.White;
                按钮.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                按钮.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                按钮.BorderThickness = new Thickness(1);
            }
        }

        private void 根滚动容器_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer viewer)
            {
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private void 更新列表控件_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (根滚动容器 != null)
            {
                根滚动容器.ScrollToVerticalOffset(根滚动容器.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private class 摘要卡片项
        {
            public string 标题 { get; set; } = "";
            public string 数值 { get; set; } = "";
            public string 说明 { get; set; } = "";
        }
    }
}