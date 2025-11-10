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

                SettingsCard_SymlinkFeature.Header = "符号链接选项";
                SettingsCard_SymlinkFeature.Description = "开启该选项后，可以显著减少F8得到的帧分析转储文件总大小，显著提升转储速度，非特殊情况建议保持开启。不过需要注意此时帧分析文件根目录 .\\FrameAnalysis 下的文件基本均为符号链接，指向去重文件目录 .\\FrameAnalysis\\Deduped 下的真正文件。";

                ComboBox_Symlink_On.Content = "开启";
                ComboBox_Symlink_Off.Content = "关闭";


                SettingsCard_AutoSetAnalyseOptions.Header = "自动设置analyse_options";
                SettingsCard_AutoSetAnalyseOptions.Description = "在启动3Dmigoto之前，自动设置d3dx.ini中的analyse_options为deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds";

                ComboboxItem_AutoSetAnalyseOptions_AutoSet.Content = "自动重置";
                ComboboxItem_AutoSetAnalyseOptions_DontSet.Content = "不进行重置";

                SettingsCard_ShowWarnings.Header = "左上角红字报错显示";
                SettingsCard_ShowWarnings.Description = "开启后将在游戏左上角显示3Dmigoto的红色报错信息，方便Mod作者调试和排查问题，仅使用Mod的Mod玩家用户推荐关闭";

                ComboBoxItem_ShowWarning_On.Content = "启用";
                ComboBoxItem_ShowWarning_Off.Content = "关闭";



                Button_Run3DmigotoLoader.Content = " 启动3Dmigoto";
                ToolTipService.SetToolTip(Button_Run3DmigotoLoader, "执行高级启动设置并运行3Dmigoto加载器");

                Button_RunLaunchPath.Content = " 开始游戏";
                ToolTipService.SetToolTip(Button_RunLaunchPath, "运行启动路径中填写的游戏进程路径");

                SettingsCard_DllInitializationDelay.Header = "d3d11.dll初始化延迟";
                SettingsCard_DllInitializationDelay.Description = "d3d11.dll初始化时的延迟，单位为毫秒，一般WWMI填200，若仍然闪退则以每次100为单位增加此值直到不闪退，鸣潮在2.4版本更新后至少需要50ms的延迟以确保启动时不会闪退。此外，如果要让Reshade和3Dmigoto一起使用，至少需要150ms的延迟";

                SettingsExpander_DllRelatedSettings.Header = "高级选项设置";
                SettingsExpander_DllRelatedSettings.Description = "进阶功能选项，如非必要无需更改";

                SettingsCard_DllPreProcess.Header = "DLL预处理";
                SettingsCard_DllPreProcess.Description = "在启动3Dmigoto加载器之前对d3d11.dll进行预处理的选项";

                ComboBox_DllPreProcess_None.Content = "无";
                ComboBox_DllPreProcess_PackWithUPX.Content = "使用UPX默认选项加壳";

                SettingsCard_DllReplace.Header = "DLL替换";
                SettingsCard_DllReplace.Description = "在启动3Dmigoto加载器之前使用自定义d3d11.dll替换3Dmigoto中现有的d3d11.dll的选项";

                ComboBoxItem_DllReplace_Dev.Content = "替换为Dev版本d3d11.dll";
                ComboBoxItem_DllReplace_Play.Content = "替换为Play版本d3d11.dll";
                ComboBoxItem_DllReplace_None.Content = "不进行任何替换";



             

                SettingsCard_ClearGICache.Description = "清理原神反作弊的运行报错日志，以避免由于缓存日志导致标记重点扫描从而更容易出现【与服务器的连接已断开】【15-4001】【10612-4001】的问题";
                SettingsCard_ClearGICache.Header = "清理原神Mod报错日志";
                Button_CleanGICache.Content = "清理原神缓存日志";

                SettingsCard_RunIgnoreGIError40.Description = "开启后，使用SSMT启动游戏使用Mod将可以避免由于Mod导致的网络卡顿而出现的【与服务器的连接已断开】【15-4001】【10612-4001】错误弹窗";
                SettingsCard_RunIgnoreGIError40.Header = "第四代Mod防报错插件: 网络加固插件";

                Button_RunIgnoreGIError40.Content = "启动网络加固插件";

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

                SettingsCard_SymlinkFeature.Header = "Symlink Feature";
                SettingsCard_SymlinkFeature.Description = "Open Symlink feature can make your F8 dump faster and total dump file size smaller";

                ComboBox_Symlink_On.Content = "Enable";
                ComboBox_Symlink_Off.Content = "Disable";

                SettingsCard_AutoSetAnalyseOptions.Header = "Auto Set analyse_options";
                SettingsCard_AutoSetAnalyseOptions.Description = "Auto Reset d3dx.ini's analyse_options to: deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds";


                ComboboxItem_AutoSetAnalyseOptions_AutoSet.Content = "Auto Reset analyse_options";
                ComboboxItem_AutoSetAnalyseOptions_DontSet.Content = "Not Reset analyse_options";

                SettingsCard_ShowWarnings.Header = "Show Warnings";
                SettingsCard_ShowWarnings.Description = "If Open, Top left conor will show mod's error info which can be used by mod author to fix mod, mod player does not need to open this";

                ComboBoxItem_ShowWarning_On.Content = "Enable";
                ComboBoxItem_ShowWarning_Off.Content = "Disable";



                Button_Run3DmigotoLoader.Content = " Start 3Dmigoto";
                Button_RunLaunchPath.Content = " Start Game";

                SettingsCard_DllInitializationDelay.Header = "d3d11.dll Initialization Delay";
                SettingsCard_DllInitializationDelay.Description = "Delay in milliseconds for DLL initialization. We'll go with 200ms for WWMI:\nWuthering Waves requires at least 50ms delay since 2.4 update to not crash on startup.\nAlso, to inject Reshade along with 3dmigoto, 150ms delay is required.";


                SettingsExpander_DllRelatedSettings.Header = "High Level Settings";
                SettingsExpander_DllRelatedSettings.Description = "Various High Level Settings,Don't Modifiy if you don't know how they work";

                SettingsCard_DllPreProcess.Header = "DLL Pre-Process";
                SettingsCard_DllPreProcess.Description = "Do something for d3d11.dll before inject";

                ComboBox_DllPreProcess_None.Content = "None";
                ComboBox_DllPreProcess_PackWithUPX.Content = "Pack With UPX";

                SettingsCard_DllReplace.Header = "Dll Replace";
                SettingsCard_DllReplace.Description = "Use Our d3d11.dll to replace 3Dmigoto folder's d3d11.dll before inject";

                ComboBoxItem_DllReplace_Dev.Content = "Replace with Dev version d3d11.dll";
                ComboBoxItem_DllReplace_Play.Content = "Replace with Play version d3d11.dll";
                ComboBoxItem_DllReplace_None.Content = "Not Replace";



                Button_RunIgnoreGIError40.Content = "Run GoodWorkGI.exe";

                SettingsCard_ClearGICache.Description = "Clear Error Log Files To Prevent Forever No-Condition Error";
                SettingsCard_ClearGICache.Header = "Clear GI Mod Error Log";
                Button_CleanGICache.Content = "Clean GI Log Cache Files";

                SettingsCard_RunIgnoreGIError40.Description = "The 4th Generation Mod Network Protect Technique";
                SettingsCard_RunIgnoreGIError40.Header = "Network Protect Plugin";


            }
        }

    }
}
