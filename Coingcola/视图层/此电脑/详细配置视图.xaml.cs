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
    /// 详细配置视图。
    /// </summary>
    public partial class 详细配置视图 : UserControl
    {
        private readonly 详细配置服务 _详细配置服务 = new();
        private bool _正在加载 = false;

        public 详细配置视图()
        {
            InitializeComponent();
            Loaded += 详细配置视图_Loaded;
        }

        private void 详细配置视图_Loaded(object sender, RoutedEventArgs e)
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
            页面状态文本.Text = "正在读取详细配置...";

            try
            {
                详细配置页面数据 data = await Task.Run(() => _详细配置服务.获取页面数据());
                应用页面数据(data);
                页面状态文本.Text = "已完成详细配置读取。";
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

        private void 应用页面数据(详细配置页面数据 data)
        {
            分组列表控件.ItemsSource = null;
            分组列表控件.ItemsSource = data.分组列表;

            刷新摘要卡(data);
        }

        private void 刷新摘要卡(详细配置页面数据 data)
        {
            var 列表 = new List<摘要卡项>
            {
                new 摘要卡项
                {
                    标题 = "系统",
                    数值 = data.系统名称,
                    说明 = data.系统版本
                },
                new 摘要卡项
                {
                    标题 = "CPU",
                    数值 = data.CPU名称,
                    说明 = data.CPU核心信息
                },
                new 摘要卡项
                {
                    标题 = "内存",
                    数值 = data.内存总量,
                    说明 = "当前设备总物理内存。"
                },
                new 摘要卡项
                {
                    标题 = "显卡",
                    数值 = 拼接多行文本(data.显卡列表, data.显卡名称),
                    说明 = data.显卡列表.Count > 1 ? $"当前共识别到 {data.显卡列表.Count} 个显卡设备。" : "当前识别到的显卡信息。"
                },
                new 摘要卡项
                {
                    标题 = "系统盘",
                    数值 = data.系统盘信息,
                    说明 = "当前系统所在磁盘容量概览。"
                },
                new 摘要卡项
                {
                    标题 = "磁盘概览",
                    数值 = 拼接多行文本(data.磁盘列表, data.磁盘概览),
                    说明 = data.磁盘列表.Count > 1 ? $"当前共识别到 {data.磁盘列表.Count} 个固定磁盘概览项。" : "当前磁盘概览。"
                }
            };

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = 列表;
        }

        private string 拼接多行文本(List<string> 列表, string 默认值)
        {
            if (列表 == null || 列表.Count == 0)
            {
                return 默认值;
            }

            return string.Join(Environment.NewLine, 列表);
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