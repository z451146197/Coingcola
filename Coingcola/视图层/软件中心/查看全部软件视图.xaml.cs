using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Coingcola.视图层.软件中心
{
    public partial class 查看全部软件视图 : UserControl
    {
        private readonly 软件中心服务 _软件中心服务 = new();
        private List<软件安装项> _当前列表 = new();
        private string _当前分类 = "全部";

        public 查看全部软件视图()
        {
            InitializeComponent();
            Loaded += 查看全部软件视图_Loaded;
        }

        private void 查看全部软件视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        public void 刷新页面()
        {
            搜索输入框.Text = string.Empty;
            _当前分类 = "全部";
            更新分类按钮样式();
            绑定数据();
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            绑定数据();
        }

        private void 重置按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        private void 搜索按钮_Click(object sender, RoutedEventArgs e)
        {
            绑定数据();
        }

        private void 搜索输入框_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                绑定数据();
                e.Handled = true;
            }
        }

        private void 清空按钮_Click(object sender, RoutedEventArgs e)
        {
            搜索输入框.Text = string.Empty;
            _当前分类 = "全部";
            更新分类按钮样式();
            绑定数据();
        }

        private void 分类按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                _当前分类 = button.Tag?.ToString() ?? "全部";
                更新分类按钮样式();
                绑定数据();
            }
        }

        private void 执行软件动作按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            var result = _软件中心服务.执行安装(id);
            MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, result.成功 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            状态文本.Text = result.提示;
            绑定数据();
        }

        private void 绑定数据()
        {
            string keyword = 搜索输入框.Text?.Trim() ?? string.Empty;
            _当前列表 = _软件中心服务.搜索软件(keyword, _当前分类);

            List<软件安装项> 全量列表 = _软件中心服务.获取软件列表();

            状态文本.Text = "目录已刷新。先按分类小范围，再决定执行安装或官网跳转。";

            目录总数文本.Text = 全量列表.Count.ToString();
            支持系统安装文本.Text = 全量列表.Count(x => x.来源类型 == "系统安装").ToString();
            官网跳转文本.Text = 全量列表.Count(x => x.来源类型 != "系统安装").ToString();
            当前命中文本.Text = _当前列表.Count.ToString();

            空结果文本.Visibility = _当前列表.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            结果总数文本.Text = $"共 {_当前列表.Count} 项";
            全部软件列表控件.ItemsSource = _当前列表.Select(转换展示项).ToList();
        }

        private 软件展示项 转换展示项(软件安装项 item)
        {
            return new 软件展示项
            {
                Id = item.Id,
                名称 = item.名称,
                分类 = item.分类,
                来源类型 = item.来源类型,
                说明 = item.说明,
                动作文案 = item.来源类型 == "系统安装" ? "立即安装" : "打开官网"
            };
        }

        private void 更新分类按钮样式()
        {
            var 默认背景 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            var 默认前景 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
            var 激活背景 = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
            var 激活前景 = new SolidColorBrush(Colors.White);

            foreach (var button in new[] { 全部按钮, 浏览器按钮, 办公按钮, 通讯按钮, 工具按钮, 压缩按钮, 开发按钮, 播放器按钮 })
            {
                bool active = (button.Tag?.ToString() ?? "全部") == _当前分类;
                button.Background = active ? 激活背景 : 默认背景;
                button.Foreground = active ? 激活前景 : 默认前景;
                button.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CBD5E1"));
                button.BorderThickness = new Thickness(active ? 0 : 1);
            }
        }

        private class 软件展示项
        {
            public string Id { get; set; } = string.Empty;
            public string 名称 { get; set; } = string.Empty;
            public string 分类 { get; set; } = string.Empty;
            public string 来源类型 { get; set; } = string.Empty;
            public string 说明 { get; set; } = string.Empty;
            public string 动作文案 { get; set; } = string.Empty;
        }
    }
}
