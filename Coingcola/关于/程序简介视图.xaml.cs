using Coingcola.模型;
using Coingcola.服务;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Coingcola.视图层.关于
{
    /// <summary>
    /// 程序简介视图。
    /// </summary>
    public partial class 程序简介视图 : UserControl
    {
        private readonly 程序简介服务 _程序简介服务 = new();
        private bool _正在加载 = false;

        public 程序简介视图()
        {
            InitializeComponent();
            Loaded += 程序简介视图_Loaded;
        }

        private void 程序简介视图_Loaded(object sender, RoutedEventArgs e)
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

            try
            {
                程序简介页面数据 data = await Task.Run(() => _程序简介服务.获取页面数据());
                应用页面数据(data);
            }
            catch (Exception ex)
            {
                页面结论文本.Text = $"读取失败：{ex.Message}";
            }
            finally
            {
                _正在加载 = false;
            }
        }

        private void 应用页面数据(程序简介页面数据 data)
        {
            产品名称文本.Text = data.产品名称;
            核心口号文本.Text = data.核心口号;
            页面结论文本.Text = data.页面结论;

            摘要卡片列表.ItemsSource = null;
            摘要卡片列表.ItemsSource = data.摘要卡列表;

            分组列表控件.ItemsSource = null;
            分组列表控件.ItemsSource = data.分组列表;
        }

        private void 根滚动容器_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer viewer)
            {
                viewer.ScrollToVerticalOffset(viewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}