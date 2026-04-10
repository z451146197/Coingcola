using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Coingcola.视图层.电脑优化
{
    /// <summary>
    /// 常见修复视图。
    /// </summary>
    public partial class 常见修复视图 : UserControl
    {
        private readonly 常见修复服务 _常见修复服务 = new();

        private List<修复动作项> _修复动作列表 = new();

        public 常见修复视图()
        {
            InitializeComponent();
            Loaded += 常见修复视图_Loaded;
        }

        private void 常见修复视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        /// <summary>
        /// 对外刷新入口。
        /// </summary>
        public void 刷新页面()
        {
            _修复动作列表 = _常见修复服务.获取修复动作列表();

            修复动作列表控件.ItemsSource = null;
            修复动作列表控件.ItemsSource = _修复动作列表;

            刷新摘要();
            页面状态文本.Text = "当前页已加载常见修复动作。建议优先从低风险动作开始。";
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        private void 执行修复按钮_Click(object sender, RoutedEventArgs e)
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

            修复动作项? 动作项 = _修复动作列表.Find(x => x.Id == id);
            if (动作项 == null)
            {
                return;
            }

            if (动作项.风险级别 == "管理员权限")
            {
                var confirm = MessageBox.Show(
                    "该操作会调用管理员权限命令，是否继续？",
                    "确认执行",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirm != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            var 结果 = _常见修复服务.执行修复(id);
            页面状态文本.Text = 结果.提示;

            if (!结果.成功)
            {
                MessageBox.Show(
                    结果.提示,
                    "提示",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 生成摘要卡片。
        /// </summary>
        private void 刷新摘要()
        {
            int 总数 = _修复动作列表.Count;
            int 低风险数 = _修复动作列表.FindAll(x => x.风险级别 == "低风险").Count;
            int 管理员数 = _修复动作列表.FindAll(x => x.风险级别 == "管理员权限").Count;

            var 摘要列表 = new List<摘要卡片项>
            {
                new 摘要卡片项
                {
                    标题 = "修复动作",
                    数值 = 总数.ToString(),
                    说明 = "当前页已提供的修复动作数量。"
                },
                new 摘要卡片项
                {
                    标题 = "低风险",
                    数值 = 低风险数.ToString(),
                    说明 = "建议优先尝试这些低风险动作。"
                },
                new 摘要卡片项
                {
                    标题 = "需管理员权限",
                    数值 = 管理员数.ToString(),
                    说明 = "执行前需要额外确认。"
                }
            };

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = 摘要列表;
        }

        /// <summary>
        /// 轻量摘要项。
        /// </summary>
        private class 摘要卡片项
        {
            public string 标题 { get; set; } = "";
            public string 数值 { get; set; } = "";
            public string 说明 { get; set; } = "";
        }
    }
}