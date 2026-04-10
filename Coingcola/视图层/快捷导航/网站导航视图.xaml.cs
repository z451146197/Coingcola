using Coingcola.模型;
using Coingcola.服务;
using Coingcola.系统工具;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Coingcola.视图层.快捷导航
{
    public partial class 网站导航视图 : UserControl
    {
        private readonly 快捷导航服务 _快捷导航服务 = new();
        private 网址识别结果? _当前识别结果;

        public 网站导航视图()
        {
            InitializeComponent();
            Loaded += 网站导航视图_Loaded;
        }

        private void 网站导航视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新网站列表();
        }

        private void 输入框_TextChanged(object sender, TextChangedEventArgs e)
        {
            string 输入内容 = 输入框.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(输入内容))
            {
                _当前识别结果 = null;
                识别结果文本.Text = "等待输入...";
                识别说明文本.Text = "";
                return;
            }

            try
            {
                _当前识别结果 = _快捷导航服务.预识别输入(输入内容);

                识别结果文本.Text =
                    $"最终地址：{_当前识别结果.最终地址}\n" +
                    $"建议名称：{_当前识别结果.建议名称}\n" +
                    $"识别来源：{_当前识别结果.来源类型}";

                识别说明文本.Text = _当前识别结果.说明;
            }
            catch (Exception ex)
            {
                _当前识别结果 = null;
                识别结果文本.Text = "识别失败";
                识别说明文本.Text = ex.Message;
            }
        }

        private void 打开按钮_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string 输入内容 = 输入框.Text?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(输入内容))
                {
                    MessageBox.Show("请输入网址、域名或关键字。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                网址识别结果 结果 = _快捷导航服务.打开输入内容(输入内容);

                _当前识别结果 = 结果;
                识别结果文本.Text =
                    $"最终地址：{结果.最终地址}\n" +
                    $"建议名称：{结果.建议名称}\n" +
                    $"识别来源：{结果.来源类型}";
                识别说明文本.Text = 结果.说明;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void 添加按钮_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string 输入内容 = 输入框.Text?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(输入内容))
                {
                    MessageBox.Show("请输入网址、域名或关键字。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                导航网址项 新项 = _快捷导航服务.新增网站(输入内容);

                MessageBox.Show($"已添加网站：{新项.名称}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);

                输入框.Clear();
                _当前识别结果 = null;
                识别结果文本.Text = "等待输入...";
                识别说明文本.Text = "";

                刷新网站列表();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"添加失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新网站列表();
        }

        private void 列表打开按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            string? id = button.Tag?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            try
            {
                List<导航网址项> 列表 = _快捷导航服务.获取网站列表();
                导航网址项? 目标项 = 列表.Find(x => x.Id == id);

                if (目标项 == null)
                {
                    MessageBox.Show("未找到对应的网站项。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _快捷导航服务.打开网址(目标项.地址);
                _快捷导航服务.标记最近使用(id);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void 列表删除按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }

            string? id = button.Tag?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "确定要删除这个网站吗？",
                "确认删除",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                _快捷导航服务.删除网站(id);
                刷新网站列表();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"删除失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void 网站列表框_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void 刷新网站列表()
        {
            List<导航网址项> 列表 = _快捷导航服务.获取网站列表();
            网站列表框.ItemsSource = null;
            网站列表框.ItemsSource = 列表;

            网站数量文本.Text = $"共 {列表.Count} 个网站";
        }
    }
}