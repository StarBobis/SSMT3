using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Newtonsoft.Json.Linq;
using SSMT.SSMTHelper;
using SSMT_Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.WindowManagement;
using WinRT;
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
        //public Image MainWindowImageBrushW;

        public DesktopAcrylicController _controller;

        public static MainWindow CurrentWindow;
        

        public MainWindow()
        {

            this.InitializeComponent();

            CurrentWindow = this;

            //为了最大程度提升用户体验，这个标题栏必须的显示出来，不能隐藏
            //因为右上角的几个按钮，经常会由于使用透明背景等等原因，和背景融为一体
            //导致用户无法找到关闭按钮等，体验极差。
            //SSMT首先是一个生产力工具，其次才是一个炫酷的工具。
            //this.ExtendsContentIntoTitleBar = true;

            //MainWindowImageBrushW = MainWindowImageBrush;


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


            //设置标题和宽高
            this.Title = GlobalConfig.SSMT_Title;
            //设置图标
            this.AppWindow.SetIcon("Assets/XiaoMai.ico");


            //启动的时候就检查SSMT缓存文件夹是否设置正确，如果不正确就去设置，直到正确了才允许跳转其它页面
            if (GlobalConfig.SSMTCacheFolderPath == "" || !Directory.Exists(GlobalConfig.SSMTCacheFolderPath))
            {
                //如果不存在，那没办法了，只能创建一个了

                DirectoryInfo dir = new DirectoryInfo(GlobalConfig.Path_BaseFolder);
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
            if (nvSample.MenuItems.Count > 0)
            {
                //一开始就设为透明的
                if (GlobalConfig.CurrentGameMigotoFolder != "" && Directory.Exists(GlobalConfig.CurrentGameMigotoFolder))
                {
                    if (GlobalConfig.OpenToWorkPage)
                    {
                        contentFrame.Navigate(typeof(WorkPage));
                    }
                    else
                    {
                        contentFrame.Navigate(typeof(HomePage));
                    }
                }
                else
                {
                    contentFrame.Navigate(typeof(HomePage));
                }

            }

            double logicalWidth = GlobalConfig.WindowWidth;
            double logicalHeight = GlobalConfig.WindowHeight;

            int actualWidth = (int)(logicalWidth);
            int actualHeight = (int)(logicalHeight);

            if (actualHeight < 720)
            {
                actualHeight = 720;
            }

            if (actualWidth < 1280)
            {
                actualWidth = 1280;
            }

            //设置页面是否显示
            this.SetGameTypePageVisibility(GlobalConfig.ShowGameTypePage);
            this.SetModManagePageVisibility(GlobalConfig.ShowModManagePage);
            this.SetTextureToolBoxPageVisibility(GlobalConfig.ShowTextureToolBoxPage);


            WindowHelper.SetWindowSizeWithNavigationView(AppWindow, actualWidth, actualHeight);
            WindowHelper.MoveWindowToCenter(AppWindow);

            TranslatePage();
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
                }

                if (pageType != null && contentFrame.Content?.GetType() != pageType)
                {
                    contentFrame.Navigate(pageType);
                }
            }

        }


        private void Window_Closed(object sender, WindowEventArgs args)
        {
            //退出程序时，保存窗口大小
            //用户反馈蓝屏的时候，全局配置文件会损坏导致SSMT无法启动，启动后闪退。
            //所以不管是保存还是读取配置都应该有TryCatch，
            //咱们已经有了，但是这玩意高低也算个小坑，特此记录。
            GlobalConfig.WindowWidth = App.m_window.AppWindow.Size.Width;
            GlobalConfig.WindowHeight = App.m_window.AppWindow.Size.Height;
            GlobalConfig.SaveConfig();
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
