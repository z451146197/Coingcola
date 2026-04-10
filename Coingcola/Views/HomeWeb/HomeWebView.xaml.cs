using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Coingcola.Views.HomeWeb
{
    public partial class HomeWebView : UserControl
    {
        public HomeWebView()
        {
            InitializeComponent();
            Loaded += HomeWebView_Loaded;
        }

        private void HomeWebView_Loaded(object sender, RoutedEventArgs e)
        {
            string indexPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "Web",
                "Home",
                "index.html"
            );

            if (File.Exists(indexPath))
            {
                Browser.Source = new Uri(indexPath);
            }
        }
    }
}
