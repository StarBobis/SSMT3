using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DocumentPage : Page
    {
        private const string DocsUrl = "https://starbobis.github.io/SSMT-Documents/";
        private bool _initialized;

        public DocumentPage()
        {
            InitializeComponent();
            // Cache this page instance to avoid re-creating and reloading the WebView each navigation
            NavigationCacheMode = NavigationCacheMode.Required;
            Loaded += DocumentPage_Loaded;


        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            // 导航到此页面时取消静音（允许播放声音）
            TryUnmuteWebView();
        }
        private void DocumentPage_Unloaded(object sender, RoutedEventArgs e)
        {
            // 页面卸载时静音
            TryMuteWebView();
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            // 导航离开页面时静音
            TryMuteWebView();
        }

        private void TryMuteWebView()
        {
            try
            {
                if (DocWebView?.CoreWebView2 != null)
                {
                    DocWebView.CoreWebView2.IsMuted = true;
                }
                else
                {
                    // 如果尚未初始化则异步尝试初始化后静音
                    _ = MuteWhenReadyAsync();
                }
            }
            catch { }
        }

        private void TryUnmuteWebView()
        {
            try
            {
                if (DocWebView?.CoreWebView2 != null)
                {
                    DocWebView.CoreWebView2.IsMuted = false;
                }
                else
                {
                    // 如果尚未初始化则异步尝试初始化后取消静音
                    _ = UnmuteWhenReadyAsync();
                }
            }
            catch { }
        }

        private async System.Threading.Tasks.Task UnmuteWhenReadyAsync()
        {
            try
            {
                await DocWebView.EnsureCoreWebView2Async();
                if (DocWebView.CoreWebView2 != null)
                {
                    DocWebView.CoreWebView2.IsMuted = false;
                }
            }
            catch { }
        }

        private async System.Threading.Tasks.Task MuteWhenReadyAsync()
        {
            try
            {
                await DocWebView.EnsureCoreWebView2Async();
                if (DocWebView.CoreWebView2 != null)
                {
                    DocWebView.CoreWebView2.IsMuted = true;
                }
            }
            catch { }
        }

        private async void DocumentPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Prevent repeated initialization when the page is revisited
            if (_initialized)
            {
                return;
            }

            try
            {
                await DocWebView.EnsureCoreWebView2Async();
                DocWebView.Source = new Uri(DocsUrl);
                _initialized = true;
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
