using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using SSMT.Pages;
using SSMT.SSMTHelper;
using SSMT_Core;
using SSMT_Core.InfoItemClass;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Graphics.Display;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.WindowManagement;
using WinRT;
using WinRT.Interop;
using WinUI3Helper;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_MAXIMIZE = 3;
        private const int SW_RESTORE = 9;

        /// <summary>
        /// 使用原生 Win32 最大化窗口（保留右上角按钮）。
        /// </summary>
        public void NativeMaximize()
        {
            try
            {
                var hwnd = WindowNative.GetWindowHandle(this);
                ShowWindow(hwnd, SW_MAXIMIZE);
            }
            catch { }
        }

        /// <summary>
        /// 使用原生 Win32 恢复窗口（取消最大化）。
        /// </summary>
        public void NativeRestore()
        {
            try
            {
                var hwnd = WindowNative.GetWindowHandle(this);
                ShowWindow(hwnd, SW_RESTORE);
            }
            catch { }
        }

        //public Image MainWindowImageBrushW;
        /// <summary>
        /// 视觉效果组件
        /// </summary>
        private Visual imageVisual;

        /// <summary>
        /// 背景透明效果控制器
        /// </summary>
        public DesktopAcrylicController _controller;

        public static MainWindow CurrentWindow;

        public Image MainWindowImageBrushW;

        //Nico: IsLoopingEnabled有个严重的问题就是循环播放时，会卡顿一瞬间
        //但是米哈游启动器就不卡，这个基本上就是WinUI3这个MediaPlayer实现的问题
        //暂时先不解决，不碰底层的情况下，不是轻易能解决的。
        //咱先解决有没有的问题，再解决好不好的问题。
        MediaPlayer BackgroundMediaPlayer = new MediaPlayer
        {
            IsLoopingEnabled = true,
        };


        public MainWindow()
        {
            
            this.InitializeComponent();

            // 初始化Composition组件
            // 获取Image控件的Visual对象
            imageVisual = ElementCompositionPreview.GetElementVisual(MainWindowImageBrush);
            BackgroundWebView.CoreWebView2Initialized += BackgroundWebView_CoreWebView2Initialized;

            //全局配置文件夹不存在就创建一个
            if (!Directory.Exists(PathManager.Path_SSMT3GlobalConfigsFolder))
            {
                Directory.CreateDirectory(PathManager.Path_SSMT3GlobalConfigsFolder);
            }

            CurrentWindow = this;

            
            // 1. 把窗口变成可以挂系统背景的目标
            var target = this.As<ICompositionSupportsSystemBackdrop>();

            // 2. 创建 Acrylic-Thin 控制器
            _controller = new DesktopAcrylicController()
            {

                Kind = DesktopAcrylicKind.Thin,   // ← 这就是“Thin透明”的关键
                LuminosityOpacity = 0.65f,  //遮挡光线程度
                TintOpacity = 0.1f //moh
            };

            // 3. 挂到窗口并激活
            _controller.AddSystemBackdropTarget(target);
            _controller.SetSystemBackdropConfiguration(new SystemBackdropConfiguration { IsInputActive = true });

            try
            {
                GlobalConfig.ReadConfig();

                if (GlobalConfig.Theme)
                {
                    WindowHelper.SetTheme(this, ElementTheme.Dark);
                    _controller.TintColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
                }
                else
                {
                    WindowHelper.SetTheme(this, ElementTheme.Light);
                    _controller.TintColor = Windows.UI.Color.FromArgb(255, 245, 245, 245);
                }
            }
            catch (Exception ex)
            {
                LOG.Info("Parse Error");
                ex.ToString();
            }

            _controller.LuminosityOpacity = (float)GlobalConfig.WindowLuminosityOpacity;

            GlobalConfig.SaveConfig();

            this.ExtendsContentIntoTitleBar = !GlobalConfig.UseTitleBar;

            //设置标题和宽高
            this.Title = ConstantsManager.SSMT_Title;
            //设置图标
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");


            //启动的时候就检查SSMT缓存文件夹是否设置正确，如果不正确就去设置，直到正确了才允许跳转其它页面
            if (GlobalConfig.SSMTCacheFolderPath == "" || !Directory.Exists(GlobalConfig.SSMTCacheFolderPath))
            {
                //如果不存在，那没办法了，只能创建一个了

                DirectoryInfo dir = new DirectoryInfo(PathManager.Path_BaseFolder);
                DirectoryInfo parentDir = dir.Parent;
                string parentPath = parentDir.FullName;
                string DefaultCacheLocation = System.IO.Path.Combine(parentPath,"SSMTDefaultCacheFolder\\");
                if (!Directory.Exists(DefaultCacheLocation))
                {
                    Directory.CreateDirectory(DefaultCacheLocation);
                }

                GlobalConfig.SSMTCacheFolderPath = DefaultCacheLocation;
                GlobalConfig.SaveConfig();

            }

            SSMTResourceUtils.InitializeWorkFolder(false);


            //默认选中主页界面
            contentFrame.Navigate(typeof(HomePage));



            //设置页面是否显示
            this.SetGameTypePageVisibility(GlobalConfig.ShowGameTypePage);
            this.SetModManagePageVisibility(GlobalConfig.ShowModManagePage);
            this.SetTextureToolBoxPageVisibility(GlobalConfig.ShowTextureToolBoxPage);


            

            TranslatePage();

            ResetWindow();

		}


        private void ResetWindow() {
			double logicalWidth = GlobalConfig.WindowWidth;
			double logicalHeight = GlobalConfig.WindowHeight;

			WindowHelper.SetSmartSizeAndMoveToCenter(AppWindow, (int)(logicalWidth), (int)(logicalHeight));
		}

		
		private void TranslatePage()
        {
            if (GlobalConfig.Chinese)
            {
                NavigationViewItem_StarterPage.Content = "主页";
                NavigationViewItem_WorkPage.Content = "工作台";
                NavigationViewItem_TexturePage.Content = "贴图标记";
                NavigationViewItem_TextureToolBoxPage.Content = "贴图工具箱";
                NavigationViewItem_GameTypePage.Content = "数据类型管理";
                NavigationViewItem_ModManagePage.Content = "Mod管理";
            }
            else
            {
                NavigationViewItem_StarterPage.Content = "Starter";
                NavigationViewItem_WorkPage.Content = "Work";
                NavigationViewItem_TexturePage.Content = "Mark Texture";
                NavigationViewItem_TextureToolBoxPage.Content = "Texture ToolBox";
                NavigationViewItem_GameTypePage.Content = "GameType Management";
                NavigationViewItem_ModManagePage.Content = "Mod Management";
            }
        }









        private void nvSample_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // PxncAcd: 添加重复点击设置页则回退到前一页的功能
            if (args.IsSettingsInvoked)
            {
                // 如果当前页面就是设置页，则返回上一页
                
                if (contentFrame.CurrentSourcePageType == typeof(SettingsPage))
                {
                    if (contentFrame.CanGoBack)
                    {
                        contentFrame.GoBack();
                    }
                    return;
                }

                // 否则导航到设置页
                contentFrame.Navigate(typeof(SettingsPage));

                ResetBackground();

                // 取消最大化（恢复到普通窗口）
                NativeRestore();

            }
            else if (args.InvokedItemContainer is NavigationViewItem item)
            {
                var pageTag = item.Tag.ToString();
                Type pageType = null;

                 

                switch (pageTag)
                {
                    case "HomePage":
                        pageType = typeof(HomePage);

                        break;
                    case "WorkPage":
                        pageType = typeof(WorkPage);
                        break;
                    case "TexturePage":
                        pageType = typeof(TexturePage);
                        break;
                    case "TextureToolBoxPage":
                        pageType = typeof(TextureToolBoxPage);
                        break;
                    case "GameTypePage":
                        pageType = typeof(GameTypePage);
                        break;
                    case "ModManagePage":
                        pageType = typeof(ModManagePage);
                        break;
                    case "DocumentPage":
                        pageType = typeof(DocumentPage);
                        
                        break;
                }

                if (pageType == typeof(DocumentPage))
                {
                    // 最大化窗口
                    //AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

                    NativeMaximize();
                }
                else
                {
                    //AppWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
                    NativeRestore();
                }


                if (pageType != null)
                {

                    if (pageType != typeof(HomePage))
                    {
                        ResetBackground();
                    }

                    if (contentFrame.Content?.GetType() != pageType)
                    {
                        //如果当前点击的页面不是当前页面，就跳转到目标页面
                        contentFrame.Navigate(pageType);
                    }

                }

                
            }

        }


        private void Window_Closed(object sender, WindowEventArgs args)
        {
            NativeRestore();

            //退出程序时，保存窗口大小
            //用户反馈蓝屏的时候，全局配置文件会损坏导致SSMT无法启动，启动后闪退。
            //所以不管是保存还是读取配置都应该有TryCatch，
            //咱们已经有了，但是这玩意高低也算个小坑，特此记录。
            GlobalConfig.WindowWidth = App.m_window.AppWindow.Size.Width;
            GlobalConfig.WindowHeight = App.m_window.AppWindow.Size.Height;
            GlobalConfig.SaveConfig();

            //不释放资源就会出现那个0x0000005的内存访问异常
            //但是没有任何文档对此有所说明
            //可恶的WinUI3
			try
			{
				_controller?.RemoveAllSystemBackdropTargets();
				_controller?.Dispose();
				_controller = null;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Backdrop cleanup failed: {ex}");
			}
		}

        private void BackgroundWebView_CoreWebView2Initialized(object sender, CoreWebView2InitializedEventArgs e)
        {
            if (e.Exception is null)
            {
                // 将虚拟主机名映射到 SSMT 缓存文件夹的根目录
                // 这样可以确保所有背景视频（无论在哪个子文件夹）都可以通过相对路径访问
                BackgroundWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "assets.ssmt.local",
                    GlobalConfig.SSMTCacheFolderPath,
                    CoreWebView2HostResourceAccessKind.Allow);
            }
        }


        private void nvSample_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (contentFrame.CanGoBack)
            {
                contentFrame.GoBack();
            }
        }

        private void contentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            //此函数的作用是，在Frame每次Navigate调用后，自动设置选中项为当前跳转到的页面。
            //这样就不需要手动设置了

            // 确保返回按钮状态正确更新
            nvSample.IsBackEnabled = contentFrame.CanGoBack;

            // 假设页面类名和 Tag 有对应关系，如 "HomePage" -> "HomePage"
            // 为了让这种方法能够很方便的生效，以后都要符合这种命名约定
            string pageName = contentFrame.SourcePageType.Name;
            string tag = pageName; // 或者根据需要进行转换

            //if (tag != "HomePage")
            //{
            //    MainWindowImageBrush.Source = null;
               
            //}

            nvSample.SelectedItem = nvSample.MenuItems.OfType<NavigationViewItem>()
                .FirstOrDefault(item => item.Tag?.ToString() == tag) ?? null;

  
        }

        public void SetGameTypePageVisibility(bool Visible)
        {
            if (Visible)
            {
                NavigationViewItem_GameTypePage.Visibility = Visibility.Visible;
            }
            else
            {
                NavigationViewItem_GameTypePage.Visibility = Visibility.Collapsed;
            }
        }

        public void SetModManagePageVisibility(bool Visible)
        {
            if (Visible)
            {
                NavigationViewItem_ModManagePage.Visibility = Visibility.Visible;
            }
            else
            {
                NavigationViewItem_ModManagePage.Visibility = Visibility.Collapsed;
            }
        }

     


        public void SetTextureToolBoxPageVisibility(bool Visible)
        {
            if (Visible)
            {
                NavigationViewItem_TextureToolBoxPage.Visibility = Visibility.Visible;
            }
            else
            {
                NavigationViewItem_TextureToolBoxPage.Visibility = Visibility.Collapsed;
            }

        }




    }
}
