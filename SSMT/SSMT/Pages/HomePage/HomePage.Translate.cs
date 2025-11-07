using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class HomePage
    {

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // ✅ 每次进入页面都会执行，适合刷新 UI
            // 因为开启了缓存模式之后，是无法刷新页面语言的，只能在这里执行来刷新
            TranslatePage();
        }


        public void TranslatePage() {
            if (GlobalConfig.Chinese)
            {

                TextBlock_GameName.Text = "游戏基础设置";

                ComboBox_GameName.Header = "游戏名称";
                Button_CreateNewGame.Content = "创建自定义游戏名称";
                Button_DeleteSelectedGame.Content = "删除当前选中游戏名称";

                ComboBox_LogicName.Header = "执行逻辑";
                ToolTipService.SetToolTip(ComboBox_LogicName, "执行逻辑会影响到下载的3Dmigoto包来源，自动更新的背景图来源，提取模型逻辑，以及生成Mod逻辑等等，每个游戏都有对应唯一的执行逻辑");
                ComboBox_GameTypeFolder.Header = "数据类型文件夹";
                ToolTipService.SetToolTip(ComboBox_GameTypeFolder, "数据类型影响到提取模型时使用的数据类型，每个游戏都有对应唯一的数据类型文件夹");


                ToggleSwitch_ShowIcon.OnContent = "当前显示图标";
                ToggleSwitch_ShowIcon.OffContent = "当前隐藏图标";


                Button_ChooseGameIcon.Content = "选择图标文件";

                Button_AutoUpdateBackground.Content = "检查背景图更新";


                Button_SelectBackgroundFile.Content = "选择背景图文件";

                TextBlock_3DmigotoFolderPath.Text = "3Dmigoto设置";
                
                Button_Choose3DmigotoPath.Content = "选择3Dmigoto文件夹路径";
                Button_Open3DmigotoFolder.Content = "打开当前3Dmigoto文件夹";

                Button_CheckMigotoPackageUpdate.Content = "从Github检查更新并自动下载最新3Dmigoto加载器包";

                
                TextBox_TargetPath.Header = "进程路径";
                Button_ChooseProcessFile.Content = "选择进程文件路径";

                TextBox_LaunchPath.Header = "启动路径";

                Button_ChooseLaunchFile.Content = "选择启动文件路径";

                TextBox_LaunchArgsPath.Header = "启动参数";


                ToggleSwitch_Symlink.OnContent = "当前开启Symlink特性";
                ToggleSwitch_Symlink.OffContent = "当前关闭Symlink特性";

                ToggleSwitch_AutoSetAnalyseOptions.OnContent = "analyse_options：自动重置";
                ToggleSwitch_AutoSetAnalyseOptions.OffContent = "analyse_options：不自动重置";

                ToggleSwitch_ShowWarning.OnContent = "当前隐藏左上角红字报错显示";
                ToggleSwitch_ShowWarning.OffContent = "当前显示左上角红字报错";


                ToggleSwitch_DllMode.OffContent = "d3d11.dll版本：Dev";
                ToggleSwitch_DllMode.OnContent = "d3d11.dll版本：Play";

                Button_Run3DmigotoLoader.Content = " 启动3Dmigoto";
                ToolTipService.SetToolTip(Button_Run3DmigotoLoader, "执行高级启动设置并运行3Dmigoto加载器");

                Button_RunLaunchPath.Content = " 开始游戏";
                ToolTipService.SetToolTip(Button_RunLaunchPath, "运行启动路径中填写的游戏进程路径");


                NumberBox_DllInitializationDelay.Header = "d3d11.dll初始化延迟";
                ToolTipService.SetToolTip(NumberBox_DllInitializationDelay,"d3d11.dll初始化时的延迟，单位为毫秒，一般WWMI填200，若仍然闪退则以每次100为单位增加此值直到不闪退\n鸣潮在2.4版本更新后至少需要50ms的延迟以确保启动时不会闪退\n此外，如果要让Reshade和3Dmigoto一起使用，至少需要150ms的延迟");

                SettingsExpander_DllRelatedSettings.Header = "DLL相关设置";
                SettingsExpander_DllRelatedSettings.Description = "与d3d11.dll相关的高级设置选项";

                SettingsCard_DllPreProcess.Header = "DLL预处理";
                SettingsCard_DllPreProcess.Description = "在启动3Dmigoto加载器之前对d3d11.dll进行预处理的选项";

                ComboBox_DllPreProcess_None.Content = "无";
                ComboBox_DllPreProcess_PackWithUPX.Content = "使用UPX默认选项加壳";
            }
            else
            {
                TextBlock_GameName.Text = "Game Basic Settings";

                ComboBox_GameName.Header = "Game Name";
                Button_CreateNewGame.Content = "Create New GameName";
                Button_DeleteSelectedGame.Content = "Delete Selected GameName";

                ComboBox_LogicName.Header = "LogicName";
                ComboBox_GameTypeFolder.Header = "GameType Folder";

                ToggleSwitch_ShowIcon.OnContent = "Current: Show Icon";
                ToggleSwitch_ShowIcon.OffContent = "Current: Hide Icon";

                Button_ChooseGameIcon.Content = "Choose Game Icon File";

                Button_AutoUpdateBackground.Content = "Auto Check And Update Background";

                Button_SelectBackgroundFile.Content = "Choose Background File";

                TextBlock_3DmigotoFolderPath.Text = "3Dmigoto Settings";

                Button_Choose3DmigotoPath.Content = "Choose 3Dmigoto Folder Path";
                Button_Open3DmigotoFolder.Content = "Open Current 3Dmigoto Folder";

                Button_CheckMigotoPackageUpdate.Content = "Check Update & Instal Latest 3Dmigoto Package From Github";


                TextBox_TargetPath.Header = "Target Process Path";
                Button_ChooseProcessFile.Content = "Choose Target Process File Path";

                TextBox_LaunchPath.Header = "Launch Process Path";

                Button_ChooseLaunchFile.Content = "Choose Launch Process File Path";

                TextBox_LaunchArgsPath.Header = "Launch Arguments";


                ToggleSwitch_Symlink.OnContent = "Current Open And Use Symlink Feature";
                ToggleSwitch_Symlink.OffContent = "Current Close And Not Use Symlink Feature";

                ToggleSwitch_AutoSetAnalyseOptions.OnContent = "Auto Reset analyse_options";
                ToggleSwitch_AutoSetAnalyseOptions.OffContent = "Not Auto Reset analyse_options";

                ToggleSwitch_ShowWarning.OnContent = "Current Hide Top Left Red Warnings";
                ToggleSwitch_ShowWarning.OffContent = "Current Show Top Left Red Warnings";

                ToggleSwitch_DllMode.OnContent = "Start With Dev Version d3d11.dll";
                ToggleSwitch_DllMode.OnContent = "Start With Play Version d3d11.dll";

                Button_Run3DmigotoLoader.Content = " Start 3Dmigoto";
                Button_RunLaunchPath.Content = " Start Game";

                NumberBox_DllInitializationDelay.Header = "d3d11.dll Initialization Delay";
                ToolTipService.SetToolTip(NumberBox_DllInitializationDelay, "Delay in milliseconds for DLL initialization. We'll go with 200ms for WWMI:\nWuthering Waves requires at least 50ms delay since 2.4 update to not crash on startup.\nAlso, to inject Reshade along with 3dmigoto, 150ms delay is required.");


                SettingsExpander_DllRelatedSettings.Header = "Dll Related Settings";
                SettingsExpander_DllRelatedSettings.Description = "Various High Level Settings For d3d11.dll";

                SettingsCard_DllPreProcess.Header = "DLL Pre-Process";
                SettingsCard_DllPreProcess.Description = "Do something for d3d11.dll before inject";

                ComboBox_DllPreProcess_None.Content = "None";
                ComboBox_DllPreProcess_PackWithUPX.Content = "Pack With UPX";
            }
        }

    }
}
