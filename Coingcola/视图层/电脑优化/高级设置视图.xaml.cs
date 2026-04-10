using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Coingcola.视图层.电脑优化
{
    /// <summary>
    /// 高级设置视图。
    /// </summary>
    public partial class 高级设置视图 : UserControl
    {
        private readonly 高级设置服务 _高级设置服务 = new();

        private List<高级设置入口项> _当前列表 = new();
        private string _当前分类 = "全部";

        public 高级设置视图()
        {
            InitializeComponent();
            Loaded += 高级设置视图_Loaded;
        }

        private void 高级设置视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        public void 刷新页面()
        {
            _当前分类 = "全部";
            搜索输入框.Text = string.Empty;
            _当前列表 = _高级设置服务.获取入口列表();

            绑定列表(_当前列表);
            更新分类按钮样式();
            刷新摘要();

            页面状态文本.Text = "当前页已加载高级设置入口。建议优先通过这里快速进入系统高级能力，而不是手动到处查找。";
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
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
        private void 系统分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("系统");
        private void 应用分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("应用");
        private void 硬件分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("硬件");
        private void 工具分类按钮_Click(object sender, RoutedEventArgs e) => 切换分类("工具");

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

            高级设置入口项? item = _当前列表.Find(x => x.Id == id);
            if (item == null)
            {
                return;
            }

            if (item.风险级别 == "需谨慎")
            {
                var confirm = MessageBox.Show(
                    $"将打开“{item.名称}”。\n\n该入口属于高级系统能力，请确认你知道自己要做什么。\n是否继续？",
                    "确认打开",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var result = _高级设置服务.打开入口(id);
            页面状态文本.Text = result.提示;

            if (!result.成功)
            {
                MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void 执行搜索()
        {
            string key = 搜索输入框.Text?.Trim() ?? string.Empty;
            _当前列表 = _高级设置服务.搜索入口(key, _当前分类);
            绑定列表(_当前列表);
        }

        private void 切换分类(string 分类)
        {
            _当前分类 = 分类;
            执行搜索();
            更新分类按钮样式();
        }

        private void 绑定列表(List<高级设置入口项> 列表)
        {
            入口列表控件.ItemsSource = null;
            入口列表控件.ItemsSource = 列表;

            结果数量文本.Text = $"共 {列表.Count} 项";
            空结果文本.Visibility = 列表.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void 刷新摘要()
        {
            var 全部 = _高级设置服务.获取入口列表();

            var 摘要列表 = new List<摘要卡片项>
            {
                new 摘要卡片项
                {
                    标题 = "入口总数",
                    数值 = 全部.Count.ToString(),
                    说明 = "当前已聚合的高级设置入口数量。"
                },
                new 摘要卡片项
                {
                    标题 = "低风险入口",
                    数值 = _高级设置服务.获取低风险数量().ToString(),
                    说明 = "适合直接打开查看的入口。"
                },
                new 摘要卡片项
                {
                    标题 = "需谨慎入口",
                    数值 = _高级设置服务.获取需谨慎数量().ToString(),
                    说明 = "建议明确目的后再进入。"
                }
            };

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = 摘要列表;
        }

        private void 更新分类按钮样式()
        {
            设置分类按钮样式(全部分类按钮, _当前分类 == "全部");
            设置分类按钮样式(系统分类按钮, _当前分类 == "系统");
            设置分类按钮样式(应用分类按钮, _当前分类 == "应用");
            设置分类按钮样式(硬件分类按钮, _当前分类 == "硬件");
            设置分类按钮样式(工具分类按钮, _当前分类 == "工具");
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

        private void 入口列表控件_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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