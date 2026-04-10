using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Coingcola.视图层.电脑优化
{
    public partial class 让电脑更顺手视图 : UserControl
    {
        private readonly 电脑优化服务 _电脑优化服务 = new();
        private List<系统开关项> _当前列表 = new();

        public 让电脑更顺手视图()
        {
            InitializeComponent();
            Loaded += 让电脑更顺手视图_Loaded;
        }

        private void 让电脑更顺手视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        public void 刷新页面()
        {
            _当前列表 = _电脑优化服务.获取让电脑更顺手项列表();

            设置总数文本.Text = _当前列表.Count.ToString();
            已符合文本.Text = _当前列表.Count(x => x.当前是否开启 == x.推荐是否开启).ToString();
            待处理文本.Text = _当前列表.Count(x => x.当前是否开启 != x.推荐是否开启).ToString();
            需重启文本.Text = _当前列表.Count.ToString();

            int 待处理数 = _当前列表.Count(x => x.当前是否开启 != x.推荐是否开启);
            总状态文本.Text = 待处理数 == 0
                ? "当前所有高频项都已经符合建议，可以继续保持。"
                : $"当前有 {待处理数} 个高频项不符合建议，建议先处理这些项目。";

            var 建议项 = _当前列表
                .Where(x => x.当前是否开启 != x.推荐是否开启)
                .Select(转换展示项)
                .ToList();

            建议区状态文本.Text = 建议项.Count == 0
                ? "当前无需优先处理"
                : $"共 {建议项.Count} 项待优先处理";

            空建议文本.Visibility = 建议项.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            建议列表控件.ItemsSource = 建议项;
            全部设置列表控件.ItemsSource = _当前列表.Select(转换展示项).ToList();
        }

        private void 刷新按钮_Click(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        private void 一键建议处理按钮_Click(object sender, RoutedEventArgs e)
        {
            var result = _电脑优化服务.应用全部推荐设置();
            MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            刷新页面();
        }

        private void 重启资源管理器按钮_Click(object sender, RoutedEventArgs e)
        {
            var result = _电脑优化服务.重启资源管理器();
            MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, result.成功 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            刷新页面();
        }

        private void 按建议处理按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            var result = _电脑优化服务.应用推荐设置(id);
            MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, result.成功 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            刷新页面();
        }

        private void 手动切换按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string id)
            {
                return;
            }

            var result = _电脑优化服务.切换设置(id);
            MessageBox.Show(result.提示, "提示", MessageBoxButton.OK, result.成功 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            刷新页面();
        }

        private 设置项展示项 转换展示项(系统开关项 item)
        {
            return new 设置项展示项
            {
                Id = item.Id,
                名称 = item.名称,
                说明 = item.说明,
                推荐说明 = item.推荐说明,
                状态文本 = item.当前是否开启 ? "当前：已开启" : "当前：已关闭",
                推荐文本 = item.推荐是否开启 ? "建议：开启" : "建议：关闭"
            };
        }

        private class 设置项展示项
        {
            public string Id { get; set; } = string.Empty;
            public string 名称 { get; set; } = string.Empty;
            public string 说明 { get; set; } = string.Empty;
            public string 推荐说明 { get; set; } = string.Empty;
            public string 状态文本 { get; set; } = string.Empty;
            public string 推荐文本 { get; set; } = string.Empty;
        }
    }
}
