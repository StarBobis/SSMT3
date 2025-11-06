using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using SSMT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Velopack;
using Velopack.Sources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SSMT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        bool ReadOver = false;

        bool CheckUpdateIng = false;

        public SettingsPage()
        {
            this.InitializeComponent();
            
            HyperlinkButton_SSMTVersion.Content = GlobalConfig.SSMT_Title;

            try
            {
                ReadSettingsFromConfig();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show("Error: " + ex.ToString());
            }
        }


        public void SaveSettingsToConfig()
        {
            try
            {
                Debug.WriteLine("保存配置");
                GlobalConfig.SSMTCacheFolderPath = TextBox_SSMTPackagePath.Text;

                GlobalConfig.AutoCleanFrameAnalysisFolder = ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn;
                GlobalConfig.FrameAnalysisFolderReserveNumber = (int)NumberBox_FrameAnalysisFolderReserveNumber.Value;

                GlobalConfig.WindowLuminosityOpacity = Slider_LuminosityOpacity.Value;

                GlobalConfig.OpenToWorkPage = ToggleSwitch_OpenToWorkPage.IsOn;
                GlobalConfig.Chinese = ToggleSwitch_Chinese.IsOn;
                GlobalConfig.Theme = ToggleSwitch_Theme.IsOn;

                GlobalConfig.ShowGameTypePage = ToggleSwitch_ShowGameTypePage.IsOn;
                GlobalConfig.ShowModManagePage = ToggleSwitch_ShowModManagePage.IsOn;
                GlobalConfig.ShowTextureToolBoxPage = ToggleSwitch_ShowTextureToolBoxPage.IsOn;

                GlobalConfig.SaveConfig();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
            
        }

        public void ReadSettingsFromConfig()
        {
            ReadOver = false;
            //防止程序启动时没正确读取，这里冗余读取一次，后面看情况可以去掉
            GlobalConfig.ReadConfig();

            TextBox_SSMTPackagePath.Text = GlobalConfig.SSMTCacheFolderPath;

            ToggleSwitch_AutoCleanFrameAnalysisFolder.IsOn = GlobalConfig.AutoCleanFrameAnalysisFolder;
            NumberBox_FrameAnalysisFolderReserveNumber.Value = GlobalConfig.FrameAnalysisFolderReserveNumber;


            Slider_LuminosityOpacity.Value = GlobalConfig.WindowLuminosityOpacity;

            ToggleSwitch_OpenToWorkPage.IsOn = GlobalConfig.OpenToWorkPage;
            ToggleSwitch_Theme.IsOn = GlobalConfig.Theme;
            ToggleSwitch_Chinese.IsOn = GlobalConfig.Chinese;

            ToggleSwitch_ShowGameTypePage.IsOn = GlobalConfig.ShowGameTypePage;
            ToggleSwitch_ShowModManagePage.IsOn = GlobalConfig.ShowModManagePage;
            ToggleSwitch_ShowTextureToolBoxPage.IsOn = GlobalConfig.ShowTextureToolBoxPage;

            ReadOver = true;
        }


        private async void Button_ChooseSSMTPackageFolder_Click(object sender, RoutedEventArgs e)
        {
            string FolderPath = await SSMTCommandHelper.ChooseFolderAndGetPath();

            if (FolderPath == "")
            {
                return;
            }

            if (Directory.Exists(FolderPath))
            {

                TextBox_SSMTPackagePath.Text = FolderPath;
                GlobalConfig.SSMTCacheFolderPath = FolderPath;
                GlobalConfig.SaveConfig();
            }

            SSMTResourceUtils.InitializeWorkFolder(true);
            _ = SSMTMessageHelper.Show("缓存文件夹路径设置成功!", "Success");
        }

        private void TextBox_SSMTPackageFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(TextBox_SSMTPackagePath.Text))
            {
                GlobalConfig.SSMTCacheFolderPath = TextBox_SSMTPackagePath.Text;
                GlobalConfig.SaveConfig();

                SSMTResourceUtils.InitializeWorkFolder(true);
            }

        }

        /// <summary>
        /// 任何设置项被改变后，都应该立刻调用这个方法，否则无法同步状态。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshSettings(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
            }
        }






        private async void Button_AutoUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUpdateIng)
            {
                _ = SSMTMessageHelper.Show("请稍等，正在检查是否有新版本中，速度取决于你的电脑联通Github的速度，无需重复检测。");
                return;
            }

            try
            {
                CheckUpdateIng = true;

                var source = new GithubSource("https://github.com/StarBobis/SSMT3/", null, false);
                var mgr = new UpdateManager(source);
                // check for new version
                ProgressRing_UpdateRing.Visibility = Visibility.Visible;
                var newVersion = await mgr.CheckForUpdatesAsync();
                ProgressRing_UpdateRing.Visibility = Visibility.Collapsed;
                CheckUpdateIng = false;

                if (newVersion == null)
                {


                    await SSMTMessageHelper.Show("您当前的版本已经是最新版本","Your version is already latest version.");
                    return; // no update available
                }
                else
                {
                    bool UpdateConfirm = await SSMTMessageHelper.ShowConfirm("检测到可用的新版本! 是否开始自动更新?","Available new version detected, do you want to update?");

                    if (UpdateConfirm)
                    {
                        // 创建进度回调（使用Action<int>）
                        Action<int> progress = p =>
                        {
                            // 使用Dispatcher确保UI线程更新
                            ProgressBar_AutoUpdate.DispatcherQueue.TryEnqueue(() =>
                            {
                                ProgressBar_AutoUpdate.Value = p; // 直接使用整数值
                            });
                        };

                        // download new version
                        await mgr.DownloadUpdatesAsync(newVersion, progress);

                        // install new version and restart app
                        mgr.ApplyUpdatesAndRestart(newVersion);
                    }

                }
                

            }
            catch (Exception ex)
            {
                ProgressRing_UpdateRing.Visibility = Visibility.Collapsed;
                CheckUpdateIng = false;

                string message = null;

                // 检查是否是 Octokit.ApiException（GitHub API 错误）
                if (ex.GetType().Name == "ApiException" && ex.Message.Contains("401"))
                {
                    message = "更新失败：GitHub 拒绝访问（401 Unauthorized）。\n可能是 GitHub API 限流或未授权访问导致，请稍后重试或检查网络代理。";
                }
                // 检查是否为普通 HttpRequestException
                else if (ex is HttpRequestException httpEx && httpEx.Message.Contains("401"))
                {
                    message = "更新失败：网络请求返回 401 未授权。\n请检查您的网络环境或代理设置。";
                }
                // 检查是否为 Octokit 的 rate limit（403）
                else if (ex.Message.Contains("403") && ex.Message.Contains("rate"))
                {
                    message = "更新失败：GitHub 访问频率受限（403 Rate Limit）。\n请稍后再试。";
                }
                // 其他常见错误（DNS、网络中断）
                else if (ex is HttpRequestException || ex.Message.Contains("NameResolutionFailure"))
                {
                    message = "无法连接到 GitHub，请检查网络连接或系统代理。";
                }

                if (message == null)
                {
                    // 默认兜底
                    message = "自动更新失败。\n详细错误信息如下，请联系开发者添加报错处理：\n" + ex.Message;
                }

                await SSMTMessageHelper.Show(message);
            }

        }



        private void ToggleSwitch_Theme_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                if (ToggleSwitch_Theme.IsOn)
                {
                    if (this.XamlRoot?.Content is FrameworkElement root)
                    {
                        root.RequestedTheme = ElementTheme.Dark;
                        MainWindow.CurrentWindow._controller.TintColor = Windows.UI.Color.FromArgb(255, 0, 0, 0);
                    }
                }
                else
                {
                    if (this.XamlRoot?.Content is FrameworkElement root)
                    {
                        root.RequestedTheme = ElementTheme.Light;
                        MainWindow.CurrentWindow._controller.TintColor = Windows.UI.Color.FromArgb(255, 245, 245, 245);
                    }

                }
                SaveSettingsToConfig();
            }
        }

        private void ToggleSwitch_Chinese_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();
                //Frame.Navigate(typeof(SettingsPage));
                TranslatePage();
            }
        }

      

        private void Slider_LuminosityOpacity_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ReadOver)
            {
                MainWindow.CurrentWindow._controller.LuminosityOpacity = (float)Slider_LuminosityOpacity.Value;
            }
        }




     

        private void Slider_LuminosityOpacity_LostFocus(object sender, RoutedEventArgs e)
        {
            SaveSettingsToConfig();

        }

        private void ToggleSwitch_ShowGameTypePage_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();

                MainWindow.CurrentWindow.SetGameTypePageVisibility(ToggleSwitch_ShowGameTypePage.IsOn);
            }
        }

        private void ToggleSwitch_ShowModManagePage_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();

                MainWindow.CurrentWindow.SetModManagePageVisibility(ToggleSwitch_ShowModManagePage.IsOn);
            }
        }

        

    

        private void ToggleSwitch_ShowTextureToolBoxPage_Toggled(object sender, RoutedEventArgs e)
        {
            if (ReadOver)
            {
                SaveSettingsToConfig();

                MainWindow.CurrentWindow.SetTextureToolBoxPageVisibility(ToggleSwitch_ShowTextureToolBoxPage.IsOn);
            }
        }
    }
}
