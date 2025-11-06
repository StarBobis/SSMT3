using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class SettingsPage
    {
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ✅ 每次进入页面都会执行，适合刷新 UI
            // 因为开启了缓存模式之后，是无法刷新页面语言的，只能在这里执行来刷新
            TranslatePage();
        }

        public void TranslatePage()
        {

            if (GlobalConfig.Chinese)
            {
                TextBlock_SSMTCacheFolder.Text = "SSMT缓存文件存放路径设置";
                Button_ChooseSSMTPackageFolder.Content = "选择缓存文件存放路径";

                TextBlock_OpenToWorkPage.Text = "打开SSMT后默认进入工作台页面";
                ToolTipService.SetToolTip(TextBlock_OpenToWorkPage, "开启后每次只要运行SSMT就会进入工作台页面而不是主页，适合已经熟练使用SSMT的Mod作者");

                ToggleSwitch_OpenToWorkPage.OnContent = "当前:打开SSMT后进入工作台页面";
                ToggleSwitch_OpenToWorkPage.OffContent = "当前:打开SSMT后进入主页";

                TextBlock_Theme.Text = "主题颜色";
                ToggleSwitch_Theme.OnContent = "曜石黑";
                ToggleSwitch_Theme.OffContent = "晨曦白";

                TextBlock_Language.Text = "语言";
                ToggleSwitch_Chinese.OnContent = "简体中文";
                ToggleSwitch_Chinese.OffContent = "英语";

                TextBlock_AutoCleanFrameAnalysisFolder.Text = "退出SSMT之前自动清理当前游戏Dump生成的FrameAnalysis文件夹";
                ToolTipService.SetToolTip(TextBlock_AutoCleanFrameAnalysisFolder, "建议开启，这样就不用担心Dump的FrameAnalysis文件夹一直积累了，适合已经熟练使用SSMT的Mod作者。");

                ToggleSwitch_AutoCleanFrameAnalysisFolder.OnContent = "当前:退出SSMT之前自动清理FrameAnalysis文件夹";
                ToggleSwitch_AutoCleanFrameAnalysisFolder.OffContent = "当前:退出SSMT之前不清理FrameAnalysis文件夹";

                TextBlock_FrameAnalysisFolderReserveNumber.Text = "自动清理时保留的FrameAnalysis文件夹数量";
                ToolTipService.SetToolTip(TextBlock_FrameAnalysisFolderReserveNumber, "有经验的Mod作者通常设为2，保留上一次和上上次的Dump文件方便使用的同时不会留下太多文件");

                TextBlock_About.Text = "关于";

                HyperlinkButton_SubmitIssueAndFeedback.Content = "提交错误报告与使用反馈建议";
                
                TextBlock_Help.Text = "帮助";
                HyperlinkButton_SSMTDocuments.Content = "SSMT使用文档";
                HyperlinkButton_SSMTPluginTheHerta.Content = "SSMT的Blender插件TheHerta";
                HyperlinkButton_SSMTDiscord.Content = "SSMT Discord交流群";
                HyperlinkButton_SSMTQQGroup.Content = "SSMT QQ公开群 169930474";

                Run_SponsorSupport.Text = "赞助支持";
                HyperlinkButton_SSMTTechCommunity.Content = "SSMT技术社群";
                HyperlinkButton_AFDianNicoMico.Content = "爱发电:NicoMico";

                TextBlock_CheckForUpdates.Text = "检查版本更新";
                Button_AutoUpdate.Content = "自动检查新版本并更新";
                TextBlock_UpdateProgressing.Text = "自动更新下载进度:";

                TextBlock_WindowOpacitySetting.Text = "窗口透明度调整";
                Slider_LuminosityOpacity.Header = "透光度";

                TextBlock_ShowPagesSetting.Text = "页面显示设置";

                //ToggleSwitch_ShowGameTypePage.Header = "是否显示数据类型管理页面";
                ToggleSwitch_ShowGameTypePage.OnContent = "显示数据类型管理页面";
                ToggleSwitch_ShowGameTypePage.OffContent = "不显示数据类型管理页面";

                //ToggleSwitch_ShowModManagePage.Header = "是否显示Mod管理页面";
                ToggleSwitch_ShowModManagePage.OnContent = "显示 Mod 管理页面";
                ToggleSwitch_ShowModManagePage.OffContent = "不显示 Mod 管理页面";

                //ToggleSwitch_ShowModProtectPage.OnContent = "显示 Mod 保护页面";
                //ToggleSwitch_ShowModProtectPage.OffContent = "隐藏 Mod 保护页面";

                ToggleSwitch_ShowTextureToolBoxPage.OnContent = "显示贴图工具箱页面";
                ToggleSwitch_ShowTextureToolBoxPage.OffContent = "隐藏贴图工具箱页面";


            }
            else
            {
                TextBlock_SSMTCacheFolder.Text = "SSMT Cache Folder";
                Button_ChooseSSMTPackageFolder.Content = "Choose Cache Folder";

                TextBlock_OpenToWorkPage.Text = "Jump To WorkPage Immediately After Open SSMT";

                ToggleSwitch_OpenToWorkPage.OnContent = "Current: Jump To WorkPage After Open SSMT";
                ToggleSwitch_OpenToWorkPage.OffContent = "Current: Jump To HomePage After Open SSMT";

                TextBlock_Theme.Text = "Theme Color";

                ToggleSwitch_Theme.OnContent = "Dark";
                ToggleSwitch_Theme.OffContent = "Light";

                TextBlock_Language.Text = "Language";
                ToggleSwitch_Chinese.OnContent = "Chinese(zh-CN)";
                ToggleSwitch_Chinese.OffContent = "English(en-US)";

                TextBlock_AutoCleanFrameAnalysisFolder.Text = "Auto Delete Current Game's FrameAnalysis Folder Before Quit SSMT";

                ToggleSwitch_AutoCleanFrameAnalysisFolder.OnContent = "Current: Auto Delete FrameAnalysis Folder Before Quit SSMT";
                ToggleSwitch_AutoCleanFrameAnalysisFolder.OffContent = "Current: Do Not Delete FrameAnalysis Folder Before Quit SSMT";

                TextBlock_FrameAnalysisFolderReserveNumber.Text = "The Number Of FrameAnalysis Folder To Keep When Auto Deleting";

               

                TextBlock_About.Text = "About";

                HyperlinkButton_SubmitIssueAndFeedback.Content = "Submit Issue And Feedback";

                TextBlock_Help.Text = "Help";
                HyperlinkButton_SSMTDocuments.Content = "SSMT Documents";
                HyperlinkButton_SSMTPluginTheHerta.Content = "SSMT's Blender Plugin: TheHerta";
                HyperlinkButton_SSMTDiscord.Content = "SSMT Discord Server";
                HyperlinkButton_SSMTQQGroup.Content = "SSMT QQGroup 169930474";

                Run_SponsorSupport.Text = "Sponsor Support";
                HyperlinkButton_SSMTTechCommunity.Content = "SSMT Tech Community";
                HyperlinkButton_AFDianNicoMico.Content = "afdian: NicoMico";

                TextBlock_CheckForUpdates.Text = "Check Version Update";
                Button_AutoUpdate.Content = "Auto Update To Latest Version";
                TextBlock_UpdateProgressing.Text = "Auto Update Download Progress:";

                TextBlock_WindowOpacitySetting.Text = "Window Opacity";
                Slider_LuminosityOpacity.Header = "Luminosity Opacity";

                TextBlock_ShowPagesSetting.Text = "Pages Show Setting";

                ToggleSwitch_ShowGameTypePage.OnContent = "Show Game Type Management Page";
                ToggleSwitch_ShowGameTypePage.OffContent = "Hide Game Type Management Page";

                ToggleSwitch_ShowModManagePage.OnContent = "Show Mod Management Page";
                ToggleSwitch_ShowModManagePage.OffContent = "Hide Mod Management Page";

                //ToggleSwitch_ShowModProtectPage.OnContent = "Show Mod Protection Page";
                //ToggleSwitch_ShowModProtectPage.OffContent = "Hide Mod Protection Page";

                ToggleSwitch_ShowTextureToolBoxPage.OnContent = "Show Texture Toolbox Page";
                ToggleSwitch_ShowTextureToolBoxPage.OffContent = "Hide Texture Toolbox Page";


            }

        }

    }
}
