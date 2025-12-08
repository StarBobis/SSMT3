using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SSMT_Core;
using SSMT_Core.InfoClass;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSMT
{
    public partial class HomePage
    {

        private void ShellRun()
        {

        }

        private void MigotoRun()
        {

        }


        private async void Button_RunLaunchPath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {
                    // 禁用按钮，防止多次点击误触导致启动多次
                    button.IsEnabled = false;

                    GameConfig gameConfig = new GameConfig();

                    string CurrentGameMigotoLoaderExePath = Path.Combine(gameConfig.MigotoPath, PathManager.Name_3DmigotoLoaderExe);

                    //如果存在旧的，就需要强制删除防止d3dxSkinManager以及XXMI带来的污染问题，光替换是没用的
                    if (File.Exists(CurrentGameMigotoLoaderExePath))
                    {
                        File.Delete(CurrentGameMigotoLoaderExePath);
                    }

                    //每次启动前强制替换LOD.exe 防止被其它工具污染
                    File.Copy(PathManager.Path_Default3DmigotoLoaderExe, CurrentGameMigotoLoaderExePath, true);

                    //当前3Dmigoto d3d11.dll目标路径
                    string MigotoTargetDll = Path.Combine(gameConfig.MigotoPath, "d3d11.dll");

                    //确保d3d11.dll是最新的
                    try
                    {
                        //这个函数只会在初始化的时候调用，所以默认复制Dev版本的d3d11.dll
                        string DllModeFolderName = "ReleaseX64Dev";

                        if (ComboBox_DllReplace.SelectedIndex == 1)
                        {
                            //如果是Play版本，则复制Play版本的d3d11.dll
                            DllModeFolderName = "ReleaseX64Play";
                        }

                        string MigotoSourceDll = Path.Combine(PathManager.Path_AssetsFolder, DllModeFolderName + "\\d3d11.dll");


                        //0是Dev 1是Play 2是None，所以只有0和1时才替换d3d11.dll
                        if (ComboBox_DllReplace.SelectedIndex == 0 || ComboBox_DllReplace.SelectedIndex == 1)
                        {
                            File.Copy(MigotoSourceDll, MigotoTargetDll, true);
                        }

                    }
                    catch (Exception ex)
                    {

                        _ = SSMTMessageHelper.Show(ex.ToString());
                        //这里不return
                        //因为经常会被占用，此时弹框提醒就行了。
                    }


                    //使用UPX压缩DLL，避开最基础的md5识别，当然目前原神热更新已经修复了这个，所以大部分情况下不管用了
                    //但是不是每个游戏都有反作弊，都能意识到这一点，所以保留此功能，万一有用，呵呵
                    if (ComboBox_DllPreProcess.SelectedIndex == 1)
                    {
                        SSMTCommandHelper.RunUPX(MigotoTargetDll, false);
                    }


                    //强制设置analyse_options 使用deferred_ctx_immediate确保IdentityV和YYSLS都能正确Dump出东西
                    string analyse_options = ConstantsManager.analyse_options;

                    if (GlobalConfig.UseSymlinkFeature)
                    {
                        analyse_options = analyse_options + " symlink";
                    }

                    if (ComboBox_AutoSetAnalyseOptions.SelectedIndex == 0)
                    {
                        D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "analyse_options", analyse_options);
                    }

                    string target_path = gameConfig.TargetPath;
                    if (target_path.Trim() == "")
                    {
                        target_path = TextBox_TargetPath.Text;
                        if (target_path.Trim() == "")
                        {
                            _ = SSMTMessageHelper.Show("启动前请先填写进程路径", "Please set your target path before start");
                            return;
                        }
                    }



                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "target", target_path);

                    //这里咱们根本不填写launch和launch_args，因为咱们用的是shell启动，所以这里直接就是强制设为空，防止预料之外的行为干扰。
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch", "");
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch_args","");

                    int dllInitializationDelay = (int)NumberBox_DllInitializationDelay.Value;

                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[system]", "dll_initialization_delay", dllInitializationDelay.ToString());

                    //强制设置hunting为2
                    //老外那边是默认关闭的，但是咱们首先是Mod制作工具，其次才是玩Mod的工具，为了兼顾还是设为2比较好
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "hunting", "2");


                    List<RunInfo> RunFilePathList = new List<RunInfo>();

                    //只有不勾选纯净游戏模式时，才启用3Dmigoto
                    if (!gameConfig.PureGameMode)
                    {
                        RunFilePathList.Add(new RunInfo { RunPath = CurrentGameMigotoLoaderExePath });
                    }

                    if (File.Exists(gameConfig.LaunchPath.Trim()))
                    {
                        LOG.Info(gameConfig.LaunchPath + " 添加到启动列表");
                        RunFilePathList.Add(new RunInfo
                        {
                            RunPath = gameConfig.LaunchPath,
                            RunWithArguments = gameConfig.LaunchArgs,
                            UseShell = true
                        });
                    }

                    if (RunFilePathList.Count == 0)
                    {
                        _ = SSMTMessageHelper.Show("您当前的配置没什么可以启动的，请仔细检查您的配置","your config can't start anything, please check your config again");
                        return;
                    }

                    await SSMTCommandHelper.LaunchSequentiallyAsyncV2(RunFilePathList);

                    // 等待1秒后重新启用
                    await Task.Delay(3000);
                }
                catch (Exception ex)
                {
                    // 确保按钮最终被重新启用
                    button.IsEnabled = true;
                    _ = SSMTMessageHelper.Show(ex.ToString());
                }
                finally
                {
                    // 确保按钮最终被重新启用
                    button.IsEnabled = true;
                }
            }



        }



        //private async void Button_RunLaunchPath_Without3DM_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button button)
        //    {
        //        try
        //        {
        //            // 禁用按钮，防止重复点击
        //            button.IsEnabled = false;

        //            GameConfig gameConfig = new GameConfig();

        //            string launchPath = gameConfig.LaunchPath;
                    
        //            if (string.IsNullOrWhiteSpace(launchPath))
        //            {
        //                _ = SSMTMessageHelper.Show("请先填写启动路径", "Please set your target path before start");
        //                return;
        //            }

        //            if (!File.Exists(launchPath))
        //            {
        //                _ = SSMTMessageHelper.Show("启动路径指向的文件不存在");
        //                return;
        //            }

        //            // 准备启动信息
        //            List<RunInfo> runList = new List<RunInfo>();

        //            runList.Add(new RunInfo
        //            {
        //                RunPath = launchPath,
        //                RunWithArguments = gameConfig.LaunchArgs,
        //                UseShell = true
        //            });

        //            LOG.Info(launchPath + " (仅启动游戏模式) 添加到启动列表");

        //            // 启动（这里使用和上面一样的统一流程）
        //            await SSMTCommandHelper.LaunchSequentiallyAsyncV2(runList);

        //            // 等待一会再启用按钮
        //            await Task.Delay(3000);
        //        }
        //        catch (Exception ex)
        //        {
        //            button.IsEnabled = true;
        //            _ = SSMTMessageHelper.Show(ex.ToString());
        //        }
        //        finally
        //        {
        //            // 确保按钮恢复
        //            button.IsEnabled = true;
        //        }
        //    }
        //}


    }
}
