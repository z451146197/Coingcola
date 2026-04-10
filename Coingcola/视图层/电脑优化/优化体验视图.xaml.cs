using Coingcola.模型;
using Coingcola.服务;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Coingcola.视图层.电脑优化
{
    /// <summary>
    /// 一键优化体检视图。
    /// 当前仍沿用“优化体验视图”类名，减少你本地改动成本。
    /// </summary>
    public partial class 优化体验视图 : UserControl
    {
        private readonly 优化体检服务 _优化体检服务 = new();
        private bool _正在加载 = false;

        public event Action<string, string>? 请求跳转到功能页;

        public 优化体验视图()
        {
            InitializeComponent();
            Loaded += 优化体验视图_Loaded;
        }

        private void 优化体验视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        public async void 刷新页面()
        {
            if (_正在加载)
            {
                return;
            }

            _正在加载 = true;
            页面状态文本.Text = "正在读取体检总览...";

            try
            {
                优化体检页面数据 data = await Task.Run(() => _优化体检服务.获取页面数据());
                应用页面数据(data);
                页面状态文本.Text = "已完成体检总览读取。";
            }
            catch (Exception ex)
            {
                页面状态文本.Text = $"读取失败：{ex.Message}";
            }
            finally
            {
                _正在加载 = false;
            }
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        private void 应用页面数据(优化体检页面数据 data)
        {
            页面结论文本.Text = data.页面结论;
            副结论文本.Text = data.副结论;

            设备名称文本.Text = data.设备名称;
            系统名称文本.Text = data.系统名称;
            当前用户文本.Text = $"当前用户：{data.当前用户}";
            权限状态文本.Text = $"权限状态：{data.权限状态}";
            Winget状态文本.Text = $"Winget 状态：{data.Winget状态}";

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = data.摘要卡列表;

            建议列表控件.ItemsSource = null;
            建议列表控件.ItemsSource = data.建议列表;

            快捷入口列表控件.ItemsSource = null;
            快捷入口列表控件.ItemsSource = data.快捷入口列表;
        }

        private void 建议动作按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                触发跳转(button.Tag?.ToString() ?? "");
            }
        }

        private void 快捷入口按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                触发跳转(button.Tag?.ToString() ?? "");
            }
        }

        private void 触发跳转(string 跳转标识)
        {
            if (string.IsNullOrWhiteSpace(跳转标识))
            {
                return;
            }

            string[] parts = 跳转标识.Split('|');
            if (parts.Length != 2)
            {
                return;
            }

            请求跳转到功能页?.Invoke(parts[0], parts[1]);
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