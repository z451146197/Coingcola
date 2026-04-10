using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Coingcola.视图层.开始使用
{
    /// <summary>
    /// WPS 快捷键知识库页。
    /// 
    /// 当前交互规则：
    /// 1. 默认显示全部本地知识项
    /// 2. 支持分类筛选 + 本地检索
    /// 3. 未命中时，提示继续网络检索
    /// 4. 页面以紧凑卡片形式展示，优先提高扫读效率
    /// </summary>
    public partial class WPS快捷键视图 : UserControl
    {
        private readonly 操作速查服务 _操作速查服务 = new();

        private List<快捷键知识项> _当前列表 = new();
        private string _当前分类 = "全部";

        public WPS快捷键视图()
        {
            InitializeComponent();
            Loaded += WPS快捷键视图_Loaded;
        }

        private void WPS快捷键视图_Loaded(object sender, RoutedEventArgs e)
        {
            加载全部知识项();
            更新分类按钮样式();
        }

        /// <summary>
        /// 对外刷新入口。
        /// 主壳切换到本页时可调用。
        /// </summary>
        public void 刷新页面()
        {
            执行本地检索();
            更新分类按钮样式();
        }

        private void 搜索输入框_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                执行本地检索();
            }
        }

        private void 检索按钮_Click(object sender, RoutedEventArgs e)
        {
            执行本地检索();
        }

        private void 重置按钮_Click(object sender, RoutedEventArgs e)
        {
            搜索输入框.Clear();
            _当前分类 = "全部";
            加载全部知识项();
            更新分类按钮样式();
        }

        private void 网络检索按钮_Click(object sender, RoutedEventArgs e)
        {
            string 搜索词 = 搜索输入框.Text?.Trim() ?? string.Empty;
            string 地址 = _操作速查服务.构造WPS网页搜索地址(搜索词);

            Process.Start(new ProcessStartInfo
            {
                FileName = 地址,
                UseShellExecute = true
            });
        }

        private void 全部分类按钮_Click(object sender, RoutedEventArgs e)
        {
            _当前分类 = "全部";
            执行本地检索();
            更新分类按钮样式();
        }

        private void 通用分类按钮_Click(object sender, RoutedEventArgs e)
        {
            _当前分类 = "通用";
            执行本地检索();
            更新分类按钮样式();
        }

        private void 文字分类按钮_Click(object sender, RoutedEventArgs e)
        {
            _当前分类 = "文字";
            执行本地检索();
            更新分类按钮样式();
        }

        private void 表格分类按钮_Click(object sender, RoutedEventArgs e)
        {
            _当前分类 = "表格";
            执行本地检索();
            更新分类按钮样式();
        }

        private void 演示分类按钮_Click(object sender, RoutedEventArgs e)
        {
            _当前分类 = "演示";
            执行本地检索();
            更新分类按钮样式();
        }

        /// <summary>
        /// 加载全部本地知识项。
        /// </summary>
        private void 加载全部知识项()
        {
            _当前列表 = _操作速查服务.获取全部WPS快捷键();

            绑定列表(_当前列表);

            结果数量文本.Text = $"共 {_当前列表.Count} 条";
            结果说明文本.Text = "当前显示全部 WPS 常见快捷键。你可以按分类筛选，也可以输入功能名称、快捷键或关键字进行检索。";
        }

        /// <summary>
        /// 执行本地知识库检索。
        /// 只查本地，不直接联网。
        /// </summary>
        private void 执行本地检索()
        {
            string 输入内容 = 搜索输入框.Text?.Trim() ?? string.Empty;

            _当前列表 = _操作速查服务.搜索WPS快捷键(输入内容, _当前分类);

            绑定列表(_当前列表);

            string 分类描述 = _当前分类 == "全部" ? "全部分类" : _当前分类;

            if (string.IsNullOrWhiteSpace(输入内容))
            {
                结果数量文本.Text = $"共 {_当前列表.Count} 条";
                结果说明文本.Text = $"当前显示 {分类描述} 下的本地快捷键知识项。";
                return;
            }

            if (_当前列表.Count > 0)
            {
                结果数量文本.Text = $"命中 {_当前列表.Count} 条";
                结果说明文本.Text = $"已在本地知识库中命中“{输入内容}”，当前分类：{分类描述}。";
            }
            else
            {
                结果数量文本.Text = "本地未命中";
                结果说明文本.Text = $"本地知识库中暂未找到“{输入内容}”，当前分类：{分类描述}。你可以继续点击“网络检索”。";
            }
        }

        /// <summary>
        /// 统一绑定列表，并控制空状态。
        /// </summary>
        private void 绑定列表(List<快捷键知识项> 列表)
        {
            快捷键列表控件.ItemsSource = null;
            快捷键列表控件.ItemsSource = 列表;

            空结果文本.Visibility = 列表.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        /// <summary>
        /// 更新分类按钮样式。
        /// 当前分类高亮，其他分类保持普通态。
        /// </summary>
        private void 更新分类按钮样式()
        {
            设置分类按钮样式(全部分类按钮, _当前分类 == "全部");
            设置分类按钮样式(通用分类按钮, _当前分类 == "通用");
            设置分类按钮样式(文字分类按钮, _当前分类 == "文字");
            设置分类按钮样式(表格分类按钮, _当前分类 == "表格");
            设置分类按钮样式(演示分类按钮, _当前分类 == "演示");
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
    }
}