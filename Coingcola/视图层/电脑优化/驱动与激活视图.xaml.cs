using Coingcola.模型;
using Coingcola.服务;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Coingcola.视图层.电脑优化
{
    /// <summary>
    /// 驱动与激活视图。
    /// 
    /// 本版优化重点：
    /// - 页面分阶段异步加载，先出激活状态，再扫驱动
    /// - 激活状态支持“待确认”显示，不再误伤用户
    /// - 刷新按钮可强制刷新，绕过缓存
    /// </summary>
    public partial class 驱动与激活视图 : UserControl
    {
        private readonly 驱动与激活服务 _驱动与激活服务 = new();

        private 激活状态信息 _激活状态 = new();
        private List<驱动问题项> _驱动问题列表 = new();

        private bool _正在加载 = false;

        public 驱动与激活视图()
        {
            InitializeComponent();
            Loaded += 驱动与激活视图_Loaded;
        }

        private void 驱动与激活视图_Loaded(object sender, RoutedEventArgs e)
        {
            刷新页面();
        }

        /// <summary>
        /// 对外刷新入口。
        /// 默认使用缓存；手动刷新时可强制刷新。
        /// </summary>
        public async void 刷新页面(bool 强制刷新 = false)
        {
            if (_正在加载)
            {
                return;
            }

            _正在加载 = true;
            页面状态文本.Text = "正在读取系统激活状态...";

            try
            {
                // 第一步：先读取激活状态，优先让用户看到核心信息
                _激活状态 = await Task.Run(() => _驱动与激活服务.获取激活状态(强制刷新));
                应用激活状态();

                页面状态文本.Text = "激活状态已读取，正在扫描驱动问题...";

                // 第二步：再读取驱动问题
                _驱动问题列表 = await Task.Run(() => _驱动与激活服务.获取驱动问题列表(强制刷新));
                应用驱动问题();

                页面状态文本.Text = "已完成系统激活状态与驱动问题扫描。";
            }
            catch (System.Exception ex)
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
            刷新页面(true);
        }

        private void 打开激活设置按钮_Click(object sender, RoutedEventArgs e)
        {
            _驱动与激活服务.打开激活设置();
        }

        private void 打开设备管理器按钮_Click(object sender, RoutedEventArgs e)
        {
            _驱动与激活服务.打开设备管理器();
        }

        /// <summary>
        /// 将激活状态写入界面。
        /// </summary>
        private void 应用激活状态()
        {
            激活状态标题文本.Text = _激活状态.状态标题;
            激活状态说明文本.Text = _激活状态.状态说明;
            激活状态标签文本.Text = _激活状态.状态标签;

            系统名称文本.Text = _激活状态.系统名称;
            系统版本文本.Text = _激活状态.版本名称;

            授权名称文本.Text = string.IsNullOrWhiteSpace(_激活状态.授权名称)
                ? "授权名称：未读取到"
                : $"授权名称：{_激活状态.授权名称}";

            部分密钥文本.Text = string.IsNullOrWhiteSpace(_激活状态.部分产品密钥)
                ? "部分密钥：数字许可证或未读取到"
                : $"部分密钥：{_激活状态.部分产品密钥}";

            if (_激活状态.是否已激活)
            {
                激活状态主卡.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5"));
                激活状态主卡.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A7F3D0"));
                激活状态标签边框.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5"));
                激活状态标签文本.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46"));
            }
            else if (_激活状态.状态标签.Contains("宽限"))
            {
                激活状态主卡.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF7ED"));
                激活状态主卡.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FED7AA"));
                激活状态标签边框.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEDD5"));
                激活状态标签文本.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A3412"));
            }
            else if (_激活状态.状态标签.Contains("待确认") || _激活状态.状态标签.Contains("未知"))
            {
                激活状态主卡.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFF6FF"));
                激活状态主卡.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BFDBFE"));
                激活状态标签边框.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DBEAFE"));
                激活状态标签文本.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1D4ED8"));
            }
            else
            {
                激活状态主卡.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2"));
                激活状态主卡.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FECACA"));
                激活状态标签边框.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                激活状态标签文本.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B"));
            }
        }

        /// <summary>
        /// 将驱动问题写入界面。
        /// </summary>
        private void 应用驱动问题()
        {
            驱动问题数量文本.Text = _驱动问题列表.Count.ToString();

            if (_驱动问题列表.Count == 0)
            {
                驱动问题说明文本.Text = "当前未检测到异常设备。";
                无问题文本.Visibility = Visibility.Visible;
            }
            else
            {
                驱动问题说明文本.Text = "当前检测到异常设备，建议进入设备管理器进一步确认。";
                无问题文本.Visibility = Visibility.Collapsed;
            }

            驱动问题列表控件.ItemsSource = null;
            驱动问题列表控件.ItemsSource = _驱动问题列表;
        }
    }
}