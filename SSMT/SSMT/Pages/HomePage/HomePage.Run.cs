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
                string OriginalLoaderExePath = Path.Combine(PathManager.Path_AssetsFolder, LoaderExeName);
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

				//使用UPX压缩DLL，避开最基础的md5识别，当然目前原神热更新已经修复了这个，所以大部分情况下不管用了
                //但是不是每个游戏都有反作弊，都能意识到这一点，所以保留此功能，万一有用，呵呵
				if (ComboBox_DllPreProcess.SelectedIndex == 1)
                {
                    SSMTCommandHelper.RunUPX(MigotoTargetDll, false);
                }


                //强制设置analyse_options 使用deferred_ctx_immediate确保IdentityV和YYSLS都能正确Dump出东西
                string analyse_options = ConstantsManager.analyse_options;

                if (ComboBox_Symlink.SelectedIndex == 0)
                {
                    analyse_options = analyse_options + " symlink";
                }

                if (ComboBox_AutoSetAnalyseOptions.SelectedIndex == 0)
                {
                    D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "analyse_options", analyse_options);
                }

                string target_path = gameConfig.TargetPath;
                if (target_path.Trim() == "") {
                    target_path = TextBox_TargetPath.Text;
                    if (target_path.Trim() == "")
                    {
                        _ = SSMTMessageHelper.Show("启动前请先填写进程路径","Please set your target path before start");
                        return;
                    }
			    }

                

				D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "target", target_path);

                D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch", "");
                //D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[loader]", "launch_args", gameConfig.LaunchArgs);

                int dllInitializationDelay = (int)NumberBox_DllInitializationDelay.Value;

                D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[system]", "dll_initialization_delay", dllInitializationDelay.ToString());

                //强制设置hunting
                D3dxIniConfig.SaveAttributeToD3DXIni(PathManager.Path_D3DXINI, "[hunting]", "hunting", "2");

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
                        _ = SSMTMessageHelper.Show("您当前并未正确设置启动路径，或启动路径文件并不存在，请重新设置。");
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
