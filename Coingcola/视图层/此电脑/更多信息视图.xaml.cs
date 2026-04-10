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
    /// 更多信息视图。
    /// </summary>
    public partial class 更多信息视图 : UserControl
    {
        private readonly 更多信息服务 _更多信息服务 = new();
        private bool _正在加载 = false;

        public 更多信息视图()
        {
            InitializeComponent();
            Loaded += 更多信息视图_Loaded;
        }

        private void 更多信息视图_Loaded(object sender, RoutedEventArgs e)
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
            页面状态文本.Text = "正在读取更多信息...";

            try
            {
                更多信息页面数据 data = await Task.Run(() => _更多信息服务.获取页面数据());
                应用页面数据(data);
                页面状态文本.Text = "已完成更多信息读取。";
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

        private void 应用页面数据(更多信息页面数据 data)
        {
            页面结论文本.Text = data.页面结论;
            基础信息文本.Text = $"{data.设备名称} · {data.系统名称} · {data.当前用户}";

            分组列表控件.ItemsSource = null;
            分组列表控件.ItemsSource = data.分组列表;

            刷新摘要卡(data);
        }

        private void 刷新摘要卡(更多信息页面数据 data)
        {
            var 列表 = new List<摘要卡项>
            {
                new 摘要卡项
                {
                    标题 = "活动网卡",
                    数值 = data.活动网卡,
                    说明 = data.IP地址
                },
                new 摘要卡项
                {
                    标题 = "固定磁盘数量",
                    数值 = data.固定磁盘数量,
                    说明 = "当前已就绪的固定磁盘数量。"
                },
                new 摘要卡项
                {
                    标题 = ".NET 版本",
                    数值 = data.DotNet版本,
                    说明 = "当前进程环境识别到的 .NET 版本。"
                },
                new 摘要卡项
                {
                    标题 = "Winget 状态",
                    数值 = data.Winget状态,
                    说明 = "用于软件安装与更新能力判断。"
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