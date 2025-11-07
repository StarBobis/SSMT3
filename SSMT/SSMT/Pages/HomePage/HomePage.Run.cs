using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SSMT_Core;
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

        private async void Open3DmigotoLoaderExe(object sender, RoutedEventArgs e)
        {
            try
            {
                GameConfig gameConfig = new GameConfig();

                string LoaderExeName = "LOD.exe";
                string OriginalLoaderExePath = Path.Combine(GlobalConfig.Path_AssetsFolder, LoaderExeName);
                if (!File.Exists(OriginalLoaderExePath))
                {
                    _ = SSMTMessageHelper.Show("您的SSMT中自带的LOD.exe缺失了，请检查是否被杀软误删，或重新安装SSMT以解决此问题");
                }
                string MigotoLoaderExePath = Path.Combine(gameConfig.MigotoPath, LoaderExeName);

                //强制删除防止污染，光替换是没用的
                if (File.Exists(MigotoLoaderExePath))
                {
                    File.Delete(MigotoLoaderExePath);
                }

                //每次启动前强制替换LOD.exe 防止被其它工具污染
                File.Copy(OriginalLoaderExePath, MigotoLoaderExePath, true);

                //确保d3d11.dll是最新的
                SyncD3D11DllFile();

                string MigotoTargetDll = Path.Combine(gameConfig.MigotoPath, "d3d11.dll");
                
                //用户需要仪式感和掌控力，所以不能直接运行，必须检测到按钮开启才运行
                //必须是能看见的情况下才解决报错，否则不解决。
                if (ComboBox_DllPreProcess.SelectedIndex == 1)
                {
                    SSMTCommandHelper.RunUPX(MigotoTargetDll, false);
                }


                //强制设置analyse_options 使用deferred_ctx_immediate确保IdentityV和YYSLS都能正确Dump出东西
                string analyse_options = GlobalConfig.analyse_options;

                if (ToggleSwitch_Symlink.IsOn)
                {
                    analyse_options = analyse_options + " symlink";
                }

                if (ToggleSwitch_AutoSetAnalyseOptions.IsOn)
                {
                    D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "analyse_options", analyse_options);
                }

                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[loader]", "target", gameConfig.TargetPath);

                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[loader]", "launch", "");
                //D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[loader]", "launch_args", gameConfig.LaunchArgs);

                int dllInitializationDelay = (int)NumberBox_DllInitializationDelay.Value;

                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[system]", "dll_initialization_delay", dllInitializationDelay.ToString());

                //强制设置hunting
                D3dxIniConfig.SaveAttributeToD3DXIni(GlobalConfig.Path_D3DXINI, "[hunting]", "hunting", "2");

                await SSMTCommandHelper.ProcessRunFile(MigotoLoaderExePath);


            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }

        private async void Button_RunLaunchPath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                try
                {

                    // 禁用按钮
                    button.IsEnabled = false;

                    GameConfig gameConfig = new GameConfig();

                    if (gameConfig.LaunchPath == "" || !File.Exists(gameConfig.LaunchPath))
                    {
                        _ = SSMTMessageHelper.Show("您当前并未正确设置启动路径，或启动路径文件并不存在，请前往设置页面检查。");
                        return;
                    }


                    string LaunchDirectory = Path.GetDirectoryName(gameConfig.LaunchPath);
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        Arguments = gameConfig.LaunchArgs,
                        FileName = gameConfig.LaunchPath,
                        UseShellExecute = true,
                        WorkingDirectory = LaunchDirectory // 设置工作路径为程序所在路径
                    };
                    Process.Start(startInfo);


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

    }
}
