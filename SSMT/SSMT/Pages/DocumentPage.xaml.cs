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
