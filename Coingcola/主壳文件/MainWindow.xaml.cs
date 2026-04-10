using Coingcola.Services.Search;
using Coingcola.模型;
using Coingcola.服务;
using Coingcola.视图层.开始使用;
using Coingcola.视图层.快捷导航;
using Coingcola.视图层.电脑优化;
using Coingcola.视图层.软件中心;
using Coingcola.视图层.此电脑;
using Coingcola.视图层.关于;
using System.Collections.Generic;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Coingcola
{
    public partial class MainWindow : Window
    {
        private readonly 菜单配置服务 _菜单配置服务 = new();

        private List<一级菜单项> _一级菜单列表 = new();
        private 一级菜单项? _当前一级菜单;
        private 二级菜单项? _当前二级菜单;

        private readonly 开始使用视图 _开始使用视图 = new();
        private readonly 网站导航视图 _网站导航视图 = new();
        private readonly WPS快捷键视图 _wps快捷键视图 = new();

        private readonly 看看这台电脑视图 _看看这台电脑视图 = new();
        private readonly 详细配置视图 _详细配置视图 = new();
        private readonly 硬件监控视图 _硬件监控视图 = new();
        private readonly 更多信息视图 _更多信息视图 = new();

        private readonly 优化体验视图 _优化体验视图 = new();
        private readonly 让电脑更顺手视图 _让电脑更顺手视图 = new();
        private readonly 驱动与激活视图 _驱动与激活视图 = new();
        private readonly 常见修复视图 _常见修复视图 = new();
        private readonly 高级设置视图 _高级设置视图 = new();

        private readonly 安装常用软件视图 _安装常用软件视图 = new();
        private readonly 查看全部软件视图 _查看全部软件视图 = new();
        private readonly 软件更新视图 _软件更新视图 = new();

        private readonly 程序简介视图 _程序简介视图 = new();
        private readonly 版本更新视图 _版本更新视图 = new();

        public MainWindow()
        {
            InitializeComponent();
            EverythingRuntimeHost.TryEnsureStarted();
_开始使用视图.请求跳转到功能页 += 开始使用视图_请求跳转到功能页;
            _优化体验视图.请求跳转到功能页 += 开始使用视图_请求跳转到功能页;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _一级菜单列表 = _菜单配置服务.获取菜单配置();

            更新权限状态();

            切换一级菜单("开始使用");
        }

        private void 开始使用视图_请求跳转到功能页(string 一级菜单名称, string 二级菜单名称)
        {
            切换一级菜单(一级菜单名称);
            切换二级菜单(二级菜单名称);
        }

        private string 规范一级菜单名称(string 名称)
        {
            return 名称 switch
            {
                "此电脑" => "我的电脑",
                _ => 名称
            };
        }

        private string 规范二级菜单名称(string 一级菜单名称, string 二级菜单名称)
        {
            一级菜单名称 = 规范一级菜单名称(一级菜单名称);

            if (一级菜单名称 == "我的电脑")
            {
                return 二级菜单名称 switch
                {
                    "看看这台电脑" => "电脑概览",
                    _ => 二级菜单名称
                };
            }

            if (一级菜单名称 == "电脑优化")
            {
                return 二级菜单名称 switch
                {
                    "一键优化体检" => "一键优化",
                    "让电脑更顺手" => "常用设置",
                    _ => 二级菜单名称
                };
            }

            if (一级菜单名称 == "软件中心")
            {
                return 二级菜单名称 switch
                {
                    "查看全部软件" => "软件目录",
                    _ => 二级菜单名称
                };
            }

            if (一级菜单名称 == "开始使用")
        {
            return 二级菜单名称 switch
            {
                "网站导航" => "首页",
                "管理常用网站" => "首页",
                _ => 二级菜单名称
            };
        }

            return 二级菜单名称;
        }

        private void 切换一级菜单(string 一级菜单名称)
        {
            一级菜单名称 = 规范一级菜单名称(一级菜单名称);

            _当前一级菜单 = _一级菜单列表.Find(x => 规范一级菜单名称(x.名称) == 一级菜单名称);

            if (_当前一级菜单 == null)
            {
                return;
            }

            模块简介文本.Text = _当前一级菜单.简介;

            更新一级菜单样式(一级菜单名称);
            生成功能分区按钮(_当前一级菜单);

            if (_当前一级菜单.功能分区.Count > 0)
            {
                切换二级菜单(_当前一级菜单.功能分区[0].名称);
            }
        }

        private void 切换二级菜单(string 二级菜单名称)
        {
            if (_当前一级菜单 == null)
            {
                return;
            }

            string 当前一级菜单名称 = 规范一级菜单名称(_当前一级菜单.名称);
            二级菜单名称 = 规范二级菜单名称(当前一级菜单名称, 二级菜单名称);

            _当前二级菜单 = _当前一级菜单.功能分区.Find(x =>
                规范二级菜单名称(当前一级菜单名称, x.名称) == 二级菜单名称);

            if (_当前二级菜单 == null)
            {
                return;
            }

            更新二级菜单样式(二级菜单名称);
            更新右侧内容(_当前二级菜单);
        }

        private void 生成功能分区按钮(一级菜单项 一级菜单)
        {
            功能分区按钮容器.Children.Clear();

            string 当前一级菜单名称 = 规范一级菜单名称(一级菜单.名称);

            foreach (var 分区 in 一级菜单.功能分区)
            {
                string 显示名称 = 规范二级菜单名称(当前一级菜单名称, 分区.名称);

                var button = new Button
                {
                    Content = 显示名称,
                    Height = 44,
                    Margin = new Thickness(0, 0, 0, 12),
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Tag = 显示名称
                };

                button.Click += 功能分区按钮_Click;
                功能分区按钮容器.Children.Add(button);
            }
        }

        
        private void 应用首页内容布局(bool 首页模式)
        {
            页面内容区容器.Margin = 首页模式
                ? new Thickness(5)
                : new Thickness(0, 24, 0, 0);

            主内容边框.Padding = 首页模式
                ? new Thickness(15)
                : new Thickness(24);

            页面内容容器.HorizontalAlignment = HorizontalAlignment.Stretch;
            页面内容容器.VerticalAlignment = VerticalAlignment.Stretch;
            页面内容容器.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            页面内容容器.VerticalContentAlignment = VerticalAlignment.Stretch;
        }
        private void 更新右侧内容(二级菜单项 分区)
        {
            if (_当前一级菜单 == null)
            {
                return;
            }

            string 当前一级菜单名称 = 规范一级菜单名称(_当前一级菜单.名称);
            string 当前二级菜单名称 = 规范二级菜单名称(当前一级菜单名称, 分区.名称);

            bool 首页模式 = 当前一级菜单名称 == "开始使用" && 当前二级菜单名称 == "首页";

            页面标题文本.Text = 当前二级菜单名称;
            页面简介文本.Text = 分区.简介;

            功能指引标题文本.Text = "功能指引";
            功能指引内容文本.Text = 分区.功能指引;
            当前状态文本.Text = 分区.当前状态; 应用首页内容布局(首页模式);
            功能指引卡片.Visibility = 首页模式 ? Visibility.Collapsed : Visibility.Visible;

            if (首页模式)
            {
                _开始使用视图.刷新首页数据();
                显示真实页面(_开始使用视图);
                状态文本.Text = "状态：首页已加载";
                return;
            }

            

            if (当前一级菜单名称 == "开始使用" && 当前二级菜单名称 == "WPS快捷键")
            {
                _wps快捷键视图.刷新页面();
                显示真实页面(_wps快捷键视图);
                状态文本.Text = "状态：WPS 快捷键知识库已加载";
                return;
            }

            if (当前一级菜单名称 == "我的电脑" && 当前二级菜单名称 == "电脑概览")
            {
                _看看这台电脑视图.刷新页面();
                显示真实页面(_看看这台电脑视图);
                状态文本.Text = "状态：电脑概览页面已加载";
                return;
            }

            if (当前一级菜单名称 == "我的电脑" && 当前二级菜单名称 == "详细配置")
            {
                _详细配置视图.刷新页面();
                显示真实页面(_详细配置视图);
                状态文本.Text = "状态：详细配置页面已加载";
                return;
            }

            if (当前一级菜单名称 == "我的电脑" && 当前二级菜单名称 == "运行状态")
            {
                _硬件监控视图.刷新页面();
                显示真实页面(_硬件监控视图);
                状态文本.Text = "状态：运行状态页面已加载";
                return;
            }

            if (当前一级菜单名称 == "我的电脑" && 当前二级菜单名称 == "更多信息")
            {
                _更多信息视图.刷新页面();
                显示真实页面(_更多信息视图);
                状态文本.Text = "状态：更多信息页面已加载";
                return;
            }

            if (当前一级菜单名称 == "电脑优化" && 当前二级菜单名称 == "一键优化")
            {
                _优化体验视图.刷新页面();
                显示真实页面(_优化体验视图);
                状态文本.Text = "状态：一键优化页面已加载";
                return;
            }

            if (当前一级菜单名称 == "电脑优化" && 当前二级菜单名称 == "常用设置")
            {
                _让电脑更顺手视图.刷新页面();
                显示真实页面(_让电脑更顺手视图);
                状态文本.Text = "状态：常用设置页面已加载";
                return;
            }

            if (当前一级菜单名称 == "电脑优化" && 当前二级菜单名称 == "常见修复")
            {
                _常见修复视图.刷新页面();
                显示真实页面(_常见修复视图);
                状态文本.Text = "状态：常见修复页面已加载";
                return;
            }

            if (当前一级菜单名称 == "电脑优化" && 当前二级菜单名称 == "驱动与激活")
            {
                _驱动与激活视图.刷新页面();
                显示真实页面(_驱动与激活视图);
                状态文本.Text = "状态：驱动与激活页面已加载";
                return;
            }

            if (当前一级菜单名称 == "电脑优化" && 当前二级菜单名称 == "高级设置")
            {
                _高级设置视图.刷新页面();
                显示真实页面(_高级设置视图);
                状态文本.Text = "状态：高级设置页面已加载";
                return;
            }

            if (当前一级菜单名称 == "软件中心" && 当前二级菜单名称 == "安装常用软件")
            {
                _安装常用软件视图.刷新页面();
                显示真实页面(_安装常用软件视图);
                状态文本.Text = "状态：安装常用软件页面已加载";
                return;
            }

            if (当前一级菜单名称 == "软件中心" && 当前二级菜单名称 == "软件目录")
            {
                _查看全部软件视图.刷新页面();
                显示真实页面(_查看全部软件视图);
                状态文本.Text = "状态：软件目录页面已加载";
                return;
            }

            if (当前一级菜单名称 == "软件中心" && 当前二级菜单名称 == "软件更新")
            {
                _软件更新视图.刷新页面();
                显示真实页面(_软件更新视图);
                状态文本.Text = "状态：软件更新页面已加载";
                return;
            }

            if (当前一级菜单名称 == "关于" && 当前二级菜单名称 == "程序简介")
            {
                _程序简介视图.刷新页面();
                显示真实页面(_程序简介视图);
                状态文本.Text = "状态：程序简介页面已加载";
                return;
            }

            if (当前一级菜单名称 == "关于" && 当前二级菜单名称 == "版本更新")
            {
                显示真实页面(_版本更新视图);
                状态文本.Text = "状态：版本更新页面已加载";
                return;
            }

            显示占位内容(
                $"这里将放置“{当前二级菜单名称}”的真实页面内容。\n\n" +
                $"当前阶段已接入首页、WPS快捷键、电脑概览、详细配置、运行状态、更多信息、一键优化、常用设置、常见修复、驱动与激活、高级设置、安装常用软件、软件目录、软件更新、程序简介与版本更新。");

            状态文本.Text = $"状态：当前已进入“{当前二级菜单名称}”";
        }

        private void 显示真实页面(UserControl 页面)
        {
            页面标题文本.Visibility = Visibility.Collapsed;
            页面简介文本.Visibility = Visibility.Collapsed;

            页面内容容器.Content = 页面;
            页面内容容器.Visibility = Visibility.Visible;

            占位内容边框.Visibility = Visibility.Collapsed;
        }

        private void 显示占位内容(string 说明文本)
        {
            页面标题文本.Visibility = Visibility.Visible;
            页面简介文本.Visibility = Visibility.Visible;

            页面内容容器.Content = null;
            页面内容容器.Visibility = Visibility.Collapsed;

            内容占位说明文本.Text = 说明文本;
            占位内容边框.Visibility = Visibility.Visible;
        }

        private void 更新一级菜单样式(string 当前名称)
        {
            当前名称 = 规范一级菜单名称(当前名称);

            var 按钮列表 = new List<Button>
            {
                开始使用按钮,
                此电脑按钮,
                电脑优化按钮,
                软件中心按钮,
                关于按钮
            };

            foreach (var 按钮 in 按钮列表)
            {
                string 名称 = 规范一级菜单名称(按钮.Content?.ToString() ?? "");

                if (名称 == 当前名称)
                {
                    按钮.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));
                    按钮.Foreground = Brushes.White;
                    按钮.BorderThickness = new Thickness(0);
                    按钮.BorderBrush = Brushes.Transparent;
                }
                else
                {
                    按钮.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1F5F9"));
                    按钮.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                    按钮.BorderBrush = Brushes.Transparent;
                    按钮.BorderThickness = new Thickness(0);
                }
            }
        }

        private void 更新二级菜单样式(string 当前名称)
        {
            if (_当前一级菜单 == null)
            {
                return;
            }

            string 当前一级菜单名称 = 规范一级菜单名称(_当前一级菜单.名称);
            当前名称 = 规范二级菜单名称(当前一级菜单名称, 当前名称);

            foreach (var child in 功能分区按钮容器.Children)
            {
                if (child is Button 按钮)
                {
                    string 名称 = 规范二级菜单名称(当前一级菜单名称, 按钮.Tag?.ToString() ?? "");

                    if (名称 == 当前名称)
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
                        按钮.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E2E8F0"));
                        按钮.BorderThickness = new Thickness(1);
                    }
                }
            }
        }

        private void 更新权限状态()
        {
            bool isAdmin = 是否为管理员运行();

            权限文本.Text = isAdmin ? "权限：管理员模式" : "权限：普通模式";
            权限文本.Foreground = isAdmin ? Brushes.ForestGreen : Brushes.IndianRed;
        }

        private bool 是否为管理员运行()
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new(identity);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void 开始使用按钮_Click(object sender, RoutedEventArgs e)
        {
            切换一级菜单("开始使用");
        }

        // 事件名保留不改，避免改 XAML；内部统一跳转到新口径
        private void 此电脑按钮_Click(object sender, RoutedEventArgs e)
        {
            切换一级菜单("我的电脑");
        }

        private void 电脑优化按钮_Click(object sender, RoutedEventArgs e)
        {
            切换一级菜单("电脑优化");
        }

        private void 软件中心按钮_Click(object sender, RoutedEventArgs e)
        {
            切换一级菜单("软件中心");
        }

        private void 关于按钮_Click(object sender, RoutedEventArgs e)
        {
            切换一级菜单("关于");
        }

        private void 功能分区按钮_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string 名称 = button.Tag?.ToString() ?? "";
                切换二级菜单(名称);
            }
        }
    }
}
