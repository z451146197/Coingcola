using Coingcola.模型;
using Coingcola.服务;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Coingcola.视图层.此电脑
{
    /// <summary>
    /// 硬件监控视图。
    /// 当前版本定位为“运行状态快照页”。
    /// </summary>
    public partial class 硬件监控视图 : UserControl
    {
        private readonly 运行状态服务 _运行状态服务 = new();
        private bool _正在加载 = false;

        public 硬件监控视图()
        {
            InitializeComponent();
            Loaded += 硬件监控视图_Loaded;
        }

        private void 硬件监控视图_Loaded(object sender, RoutedEventArgs e)
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
            页面状态文本.Text = "正在读取运行状态...";

            try
            {
                运行状态信息 data = await Task.Run(() => _运行状态服务.获取运行状态());
                应用页面数据(data);
                页面状态文本.Text = "已完成运行状态读取。";
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

        private void 应用页面数据(运行状态信息 data)
        {
            页面结论文本.Text = data.页面结论;
            系统信息文本.Text = $"{data.系统名称} · {data.系统版本}";
            运行时长文本.Text = data.运行时长;
            最近启动时间文本.Text = $"最近启动：{data.最近启动时间}";

            分组列表控件.ItemsSource = null;
            分组列表控件.ItemsSource = data.分组列表;

            刷新摘要卡(data);
        }

        private void 刷新摘要卡(运行状态信息 data)
        {
            var 列表 = new List<摘要卡项>
            {
                new 摘要卡项
                {
                    标题 = "CPU 当前负载",
                    数值 = data.CPU当前负载,
                    说明 = data.CPU名称
                },
                new 摘要卡项
                {
                    标题 = "内存使用率",
                    数值 = data.内存使用率,
                    说明 = $"已用 {data.内存已用} / 总量 {data.内存总量}"
                },
                new 摘要卡项
                {
                    标题 = "系统盘使用率",
                    数值 = data.系统盘使用率,
                    说明 = $"{data.系统盘名称} 可用 {data.系统盘可用}"
                },
                new 摘要卡项
                {
                    标题 = "运行时长",
                    数值 = data.运行时长,
                    说明 = "从上次开机到现在的累计运行时间。"
                }
            };

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = 列表;
        }

        private void 根滚动容器_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer viewer)
            {
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }

        private class 摘要卡项
        {
            public string 标题 { get; set; } = "";
            public string 数值 { get; set; } = "";
            public string 说明 { get; set; } = "";
        }
    }
}