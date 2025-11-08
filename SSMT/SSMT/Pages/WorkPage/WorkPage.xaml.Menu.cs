using Microsoft.UI.Xaml;
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
    public partial class WorkPage
    {
        private void Menu_OpenGlobalConfigFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(GlobalConfig.Path_SSMT3GlobalConfigsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_SSMT3GlobalConfigsFolder);
            }
        }

        private void Menu_OpenAssetsFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(GlobalConfig.Path_AssetsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_AssetsFolder);
            }
        }

        public async void OpenWorkSpaceGenerateModFolder(object sender, RoutedEventArgs e)
        {

            if (Directory.Exists(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_CurrentWorkSpaceGeneratedModFolder);
            }
            else
            {
                await SSMTMessageHelper.Show("您还未生成任何Mod", "You have not generate any mod yet");
            }
        }

        public async void OpenModsFolder(object sender, RoutedEventArgs e)
        {
            string modsFolder = Path.Combine(GlobalConfig.Path_3DmigotoLoaderFolder, "Mods\\");
            if (Directory.Exists(modsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(modsFolder);
            }
            else
            {
                await SSMTMessageHelper.Show("此目录不存在，请检查您的Mods文件夹是否设置正确：" + modsFolder, "This path didn't exists, please check if your Mods folder is correct");
            }
        }

        public async void OpenLatestFrameAnalysisFolder(object sender, RoutedEventArgs e)
        {

            string latestFrameAnalysisFolder = GlobalConfig.Path_LatestFrameAnalysisFolder;
            Debug.WriteLine("latestFrameAnalysisFolder: " + latestFrameAnalysisFolder);
            if (latestFrameAnalysisFolder.Trim() == "\\")
            {
                await SSMTMessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
            }
            else
            {
                if (!string.IsNullOrEmpty(latestFrameAnalysisFolder))
                {
                    SSMTCommandHelper.ShellOpenFolder(latestFrameAnalysisFolder);
                }
                else
                {
                    await SSMTMessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
                }
            }
        }



        public async void OpenLatestFrameAnalysisLogTxtFile(object sender, RoutedEventArgs e)
        {
            string LatestFrameAnalysisFolderLogTxtFilePath = GlobalConfig.Path_LatestFrameAnalysisLogTxt;

            if (LatestFrameAnalysisFolderLogTxtFilePath != "")
            {
                if (File.Exists(LatestFrameAnalysisFolderLogTxtFilePath))
                {
                    await SSMTCommandHelper.ShellOpenFile(LatestFrameAnalysisFolderLogTxtFilePath);
                }
            }
            else
            {
                await SSMTMessageHelper.Show("没有找到任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
            }
        }

        public async void OpenLatestFrameAnalysisDedupedFolder(object sender, RoutedEventArgs e)
        {
            string LatestFrameAnalysisDedupedFolder = GlobalConfig.Path_LatestFrameAnalysisDedupedFolder;
            if (!string.IsNullOrEmpty(LatestFrameAnalysisDedupedFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(LatestFrameAnalysisDedupedFolder);
            }
            else
            {
                await SSMTMessageHelper.Show("目标目录没有任何FrameAnalysis文件夹", "Target directory didn't have any FrameAnalysisFolder.");
            }
        }


        public void OpenSSMTPackageFolder(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(GlobalConfig.SSMTCacheFolderPath))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.SSMTCacheFolderPath);
            }
            else
            {
                _ = SSMTMessageHelper.Show("您当前还未设置SSMT缓存文件夹的路径，请先前往设置页面中进行设置。");
            }
        }

        public void OpenLogsFolder(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(GlobalConfig.Path_LogsFolder))
            {
                SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_LogsFolder);

            }
            else
            {
                _ = SSMTMessageHelper.Show(GlobalConfig.Path_LogsFolder + " 不存在，请至少执行任意一项任务后生成此日志文件夹。");
            }
        }


        public void OpenLatestLogFile(object sender, RoutedEventArgs e)
        {
            string LogFilePath = GlobalConfig.Path_LatestDBMTLogFile;
            if (File.Exists(LogFilePath))
            {
                _ = SSMTCommandHelper.ShellOpenFile(LogFilePath);
            }
        }

   

        private async void OpenD3dxIniFile(object sender, RoutedEventArgs e)
        {
            await SSMTCommandHelper.ShellOpenFile(GlobalConfig.Path_D3DXINI);
        }

        private void Open3DmigotoFolder(object sender, RoutedEventArgs e)
        {

            SSMTCommandHelper.ShellOpenFolder(GlobalConfig.Path_3DmigotoLoaderFolder);
        }

        private void OpenShaderFixesFolder(object sender, RoutedEventArgs e)
        {

            SSMTCommandHelper.ShellOpenFolder(Path.Combine(GlobalConfig.Path_3DmigotoLoaderFolder, "ShaderFixes\\"));
        }


        public void OpenCurrentWorkSpaceFolder(object sender, RoutedEventArgs e)
        {

            string WorkSpaceOutputFolder = GlobalConfig.Path_CurrentGameTotalWorkSpaceFolder + ComboBoxWorkSpaceSelection.Text + "\\";
            if (!string.IsNullOrEmpty(WorkSpaceOutputFolder))
            {
                if (Directory.Exists(WorkSpaceOutputFolder))
                {
                    SSMTCommandHelper.ShellOpenFolder(WorkSpaceOutputFolder);
                }
                else
                {
                    _ = SSMTMessageHelper.Show("此目录不存在，请检查您的工作空间是否设置正确", "This folder doesn't exists,please check if your WorkSpace is correct.");
                }
            }
        }

        private void SetD3dxConfig_DisableMods()
        {
            //直接修改d3dx.ini的include部分
            string[] D3DxIniLineList = File.ReadAllLines(GlobalConfig.Path_D3DXINI);
            List<string> NewD3DxIniLineList = new List<string>();

            foreach (string iniLine in D3DxIniLineList)
            {
                if (iniLine.ToLower().Trim().StartsWith("include_recursive"))
                {
                    IniEqual iniEqual = new IniEqual(iniLine);
                    if (iniEqual.RightValueTrim == "Mods")
                    {
                        NewD3DxIniLineList.Add(";include_recursive = Mods");
                    }
                    else
                    {
                        NewD3DxIniLineList.Add(iniLine);
                    }
                }
                else
                {
                    NewD3DxIniLineList.Add(iniLine);
                }
            }

            File.WriteAllLines(GlobalConfig.Path_D3DXINI, NewD3DxIniLineList);
        }

        private void SetD3dxConfig_EnableMods()
        {
            string[] D3DxIniLineList = File.ReadAllLines(GlobalConfig.Path_D3DXINI);
            List<string> NewD3DxIniLineList = new List<string>();

            foreach (string iniLine in D3DxIniLineList)
            {
                if (iniLine.ToLower().Trim().StartsWith(";include_recursive"))
                {
                    IniEqual iniEqual = new IniEqual(iniLine);
                    if (iniEqual.RightValueTrim == "Mods")
                    {
                        NewD3DxIniLineList.Add("include_recursive = Mods");
                    }
                    else
                    {
                        NewD3DxIniLineList.Add(iniLine);
                    }
                }
                else
                {
                    NewD3DxIniLineList.Add(iniLine);
                }
            }

            File.WriteAllLines(GlobalConfig.Path_D3DXINI, NewD3DxIniLineList);
        }

        private void Menu_DisableModsFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetD3dxConfig_DisableMods();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }

        private void Menu_EnableModsFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetD3dxConfig_EnableMods();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }


        private void Menu_DisableModsAndSpecificIBListConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetD3dxConfig_DisableMods();
                SetD3dxConfig_DumpSpecificIBListConfig();
                _ = SSMTMessageHelper.Show("已成功禁用Mods文件夹，并启用特定IB列表Dump配置");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }


        }

        private void Menu_EnableModsAndGlobalDumpConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetD3dxConfig_EnableMods();
                SetD3dxConfig_RecoverGlobalDumpConfig();

                _ = SSMTMessageHelper.Show("已成功启用Mods文件夹，并恢复全局Dump配置");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }

        }




    }
}
