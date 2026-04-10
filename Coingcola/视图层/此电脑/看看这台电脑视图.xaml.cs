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
    /// 看看这台电脑视图。
    /// </summary>
    public partial class 看看这台电脑视图 : UserControl
    {
        private readonly 设备概览服务 _设备概览服务 = new();
        private bool _正在加载 = false;

        public 看看这台电脑视图()
        {
            InitializeComponent();
            Loaded += 看看这台电脑视图_Loaded;
        }

        private void 看看这台电脑视图_Loaded(object sender, RoutedEventArgs e)
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
            页面状态文本.Text = "正在读取这台电脑的概览信息...";

            try
            {
                设备概览信息 info = await Task.Run(() => _设备概览服务.获取设备概览());
                应用设备信息(info);
                页面状态文本.Text = "已完成设备概览读取。";
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

        private void 应用设备信息(设备概览信息 info)
        {
            设备名称文本.Text = info.设备名称;
            系统名称文本.Text = info.系统名称;
            系统版本文本.Text = info.系统版本;
            当前用户文本.Text = info.当前用户;
            权限状态文本.Text = $"权限状态：{info.权限状态}";
            运行时长文本.Text = $"运行时长：{info.运行时长}";
            上次启动时间文本.Text = $"上次启动：{info.上次启动时间}";
            运行结论文本.Text = info.运行结论;

            系统信息名称文本.Text = info.系统名称;
            系统信息版本文本.Text = info.系统版本;
            系统类型文本.Text = info.系统类型;
            设备厂商文本.Text = info.设备厂商;
            设备型号文本.Text = info.设备型号;

            主板信息文本.Text = info.主板信息;
            BIOS信息文本.Text = info.BIOS信息;
            CPU信息文本.Text = info.CPU名称;
            显卡信息文本.Text = 拼接多行文本(info.显卡列表, info.显卡名称);
            系统盘信息文本.Text = info.系统盘信息;

            刷新关键指标卡(info);
        }

        private void 刷新关键指标卡(设备概览信息 info)
        {
            var 列表 = new List<关键指标卡项>
            {
                new 关键指标卡项
                {
                    标题 = "CPU",
                    数值 = info.CPU名称,
                    说明 = "当前识别到的处理器信息。"
                },
                new 关键指标卡项
                {
                    标题 = "内存",
                    数值 = info.内存总量,
                    说明 = "当前设备总物理内存。"
                },
                new 关键指标卡项
                {
                    标题 = "显卡",
                    数值 = 拼接多行文本(info.显卡列表, info.显卡名称),
                    说明 = info.显卡列表.Count > 1 ? $"当前共识别到 {info.显卡列表.Count} 个显卡设备。" : "当前识别到的显卡信息。"
                },
                new 关键指标卡项
                {
                    标题 = "系统盘",
                    数值 = info.系统盘信息,
                    说明 = "系统所在磁盘及容量概览。"
                },
                new 关键指标卡项
                {
                    标题 = "主机名",
                    数值 = info.主机名,
                    说明 = "当前设备名称。"
                },
                new 关键指标卡项
                {
                    标题 = "运行时长",
                    数值 = info.运行时长,
                    说明 = "从上次开机到现在的累计运行时间。"
                }
            };

            关键指标卡列表.ItemsSource = null;
            关键指标卡列表.ItemsSource = 列表;
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

        private class 关键指标卡项
        {
            public string 标题 { get; set; } = "";
            public string 数值 { get; set; } = "";
            public string 说明 { get; set; } = "";
        }
    }
}