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


                ToggleSwitch_Theme.OnContent = "曜石黑";
                ToggleSwitch_Theme.OffContent = "晨曦白";

                ToggleSwitch_Chinese.OnContent = "简体中文";
                ToggleSwitch_Chinese.OffContent = "英语";


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

            


                SettingsCard_LuminosityOpacity.Header = "窗口透明度设置";
                SettingsCard_LuminosityOpacity.Description = "窗口透明度的值越小越透明看起来越炫酷，值越大越不透明看起来越厚重";

                
                ToggleSwitch_ShowGameTypePage.OnContent = "显示";
                ToggleSwitch_ShowGameTypePage.OffContent = "隐藏";

                
                ToggleSwitch_ShowModManagePage.OnContent = "显示";
                ToggleSwitch_ShowModManagePage.OffContent = "隐藏";


                ToggleSwitch_ShowTextureToolBoxPage.OnContent = "显示";
                ToggleSwitch_ShowTextureToolBoxPage.OffContent = "隐藏";

                SettingsCard_ShowTextureToolBoxPage.Header = "贴图工具箱页面显示设置";
                SettingsCard_ShowTextureToolBoxPage.Description = "设置是否在侧边导航栏显示贴图工具箱页面的入口，贴图工具箱页面包含动态贴图Mod制作，贴图格式批量转换等功能，适合进阶Mod作者";

                SettingsCard_ShowGameTypePage.Header = "数据类型管理页面显示设置";
                SettingsCard_ShowGameTypePage.Description = "设置是否在侧边导航栏显示数据类型管理页面的入口，数据类型管理页面包含游戏数据类型的添加、删除与修改等功能，适合SSMT工具开发者以及高阶Mod作者使用";

                SettingsCard_ShowModManagePage.Header = "Mod管理页面显示设置";
                SettingsCard_ShowModManagePage.Description = "设置是否在侧边导航栏显示Mod管理页面的入口，Mod管理页面包含Mod的启用、禁用、安装与卸载等功能，适合懒得使用第三方管理器的Mod玩家使用";


                SettingsCard_Language.Header = "语言设置";
                SettingsCard_Language.Description = "设置SSMT的界面显示语言，仅支持中文和英文";

                SettingsCard_Theme.Header = "主题颜色设置";
                SettingsCard_Theme.Description = "设置SSMT的界面主题颜色，支持晨曦白和曜石黑两种主题颜色，推荐使用晨曦白";

                SettingsCard_UseGithubToken.Header = "使用Github Token";
                SettingsCard_UseGithubToken.Description = "在自动下载更新3Dmigoto包以及自动更新时，使用Github Token来访问Github以避免出现443、连接超时、目标计算机积极拒绝访问等问题";

                ComboBoxItem_UseGithubToken_Enable.Content = "开启";
                ComboBoxItem_UseGithubToken_Disable.Content = "关闭";
            
            }
            else
            {
                TextBlock_SSMTCacheFolder.Text = "SSMT Cache Folder";
                Button_ChooseSSMTPackageFolder.Content = "Choose Cache Folder";



                ToggleSwitch_Theme.OnContent = "Dark";
                ToggleSwitch_Theme.OffContent = "Light";

                ToggleSwitch_Chinese.OnContent = "Chinese(zh-CN)";
                ToggleSwitch_Chinese.OffContent = "English(en-US)";


               

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

         


                ToggleSwitch_ShowGameTypePage.OnContent = "Show";
                ToggleSwitch_ShowGameTypePage.OffContent = "Hide";

                ToggleSwitch_ShowModManagePage.OnContent = "Show";
                ToggleSwitch_ShowModManagePage.OffContent = "Hide";


                ToggleSwitch_ShowTextureToolBoxPage.OnContent = "Show";
                ToggleSwitch_ShowTextureToolBoxPage.OffContent = "Hide";


                SettingsCard_ShowTextureToolBoxPage.Header = "Texture Toolbox Page Show Setting" ;
                SettingsCard_ShowTextureToolBoxPage.Description = "Set whether to show the entrance of the Texture Toolbox Page on the main page. The Texture Toolbox Page includes functions such as Dynamic Texture Mod Creation and Batch Texture Format Conversion.";
                
                SettingsCard_ShowGameTypePage.Header = "Game Type Management Page Show Setting";
                SettingsCard_ShowGameTypePage.Description = "Set whether to show the entrance of the Game Type Management Page on the main page. The Game Type Management Page includes functions such as adding, deleting, and modifying game data types.";

                SettingsCard_ShowModManagePage.Header = "Mod Management Page Show Setting";
                SettingsCard_ShowModManagePage.Description = "Set whether to show the entrance of the Mod Management Page on the main page. The Mod Management Page includes functions such as enabling, disabling, installing, and uninstalling mods.";

                SettingsCard_Language.Header = "Language Setting";
                SettingsCard_Language.Description = "Decide what kind of language ssmt show,only support English and Chinese";

                SettingsCard_Theme.Header = "Theme Setting";
                SettingsCard_Theme.Description = "Decide what kind of theme color ssmt use, support Light and Dark theme, Light is recommended";

                SettingsCard_LuminosityOpacity.Header = "Window Opacity Setting";
                SettingsCard_LuminosityOpacity.Description = "The smaller the value, the more transparent the window looks cool; the larger the value, the more opaque the window looks heavy";

                SettingsCard_UseGithubToken.Header = "Use Github Token";
                SettingsCard_UseGithubToken.Description = "Use Github Token When Try To Download 3Dmigoto Package or Auto Update SSMT Version To Avoid Network Error";
                ComboBoxItem_UseGithubToken_Enable.Content = "Enable";
                ComboBoxItem_UseGithubToken_Disable.Content = "Disable";

            }

        }

    }
}
