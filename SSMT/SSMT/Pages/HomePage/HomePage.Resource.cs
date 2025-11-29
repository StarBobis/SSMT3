using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using SSMT_Core;
using SSMT_Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using WinUI3Helper;

namespace SSMT
{
    public partial class HomePage
    {

       


        private async void Button_CheckMigotoPackageUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProgressRing_PackageUpdateRing.IsActive)
                {
                    return;
                }

                ProgressRing_PackageUpdateRing.IsActive = true;

                string PackageName = ComboBox_MigotoPackage.SelectedItem.ToString();
                RepositoryInfo repositoryInfo = GithubUtils.GetCurrentRepositoryInfo(PackageName);

                if (repositoryInfo.OwnerName == "")
                {
                    _ = SSMTMessageHelper.Show("当前选中的执行逻辑暂不支持自动下载更新3Dmigoto包", "Current LogicName doesn't support check update.");
                    ProgressRing_PackageUpdateRing.IsActive = false;
                    return;
                }

                var helper = new GitHubApiHelper();
                var latestRelease = await helper.GetLatestRelease(repositoryInfo.OwnerName, repositoryInfo.RepositoryName);

                string Version = latestRelease.TagName;
                string Description = latestRelease.Body;

                bool ConfirmUpdate = await SSMTMessageHelper.ShowConfirm("从Github检查到了一个新版本，您是否需要下载更新?\r\n\r\n版本号: \r\n" + Version + "  \r\n\r\n更新描述: \r\n" + Description);

                if (!ConfirmUpdate)
                {
                    ProgressRing_PackageUpdateRing.IsActive = false;
                    return;
                }

                var asset = latestRelease.Assets.FirstOrDefault(a => a.Name.EndsWith(".zip"));
                if (asset == null)
                {
                    _ = SSMTMessageHelper.Show("No ZIP file found in the latest release.");
                    ProgressRing_PackageUpdateRing.IsActive = false;
                    return;
                }

                //确保默认的SSMT-Package路径存在
                string SSMTPackage3DMigotoFolder = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "3Dmigoto\\");
                Directory.CreateDirectory(SSMTPackage3DMigotoFolder);

                string CurrentGameMigotoFolder = Path.Combine(SSMTPackage3DMigotoFolder, GlobalConfig.CurrentGameName);
                if (!Directory.Exists(CurrentGameMigotoFolder))
                {
                    Directory.CreateDirectory(CurrentGameMigotoFolder);
                }

                //如果手动设置了当前3Dmigoto的路径，则使用手动设置的路径
                string SelectedMigotoFolderPath = TextBox_3DmigotoPath.Text.Trim();
                if (Directory.Exists(SelectedMigotoFolderPath) && SelectedMigotoFolderPath != "")
                {
                    CurrentGameMigotoFolder = SelectedMigotoFolderPath;
                }

                //创建一个Mods文件夹防止Mod没地方存放
                string ModsFolder = Path.Combine(CurrentGameMigotoFolder, "\\Mods\\");
                if (!Directory.Exists(ModsFolder))
                {
                    Directory.CreateDirectory(ModsFolder);
                }

                //解压到的路径
                var zipPath = await helper.DownloadReleaseZip(asset.BrowserDownloadUrl, SSMTPackage3DMigotoFolder);

                //解压Zip文件
                helper.ExtractZip(zipPath, CurrentGameMigotoFolder);

                //删除Zip文件
                helper.DeleteZip(zipPath);

                TextBox_3DmigotoPath.Text = CurrentGameMigotoFolder;

                InstallBasicDllFileTo3DmigotoFolder();

                _ = SSMTMessageHelper.Show("加载器已成功更新到 " + Version);

                ProgressRing_PackageUpdateRing.IsActive = false;

                //写入一个配置文件到当前的3Dmigoto文件夹下面
                //这个配置我们做成一个专门的类吧
                GameConfig gameConfig = new GameConfig();
                gameConfig.GithubPackageVersion = Version;
                gameConfig.SaveConfig();

                UpdatePackageVersionLink();
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
                ProgressRing_PackageUpdateRing.IsActive = false;
            }
        }






       

        private async void Button_CheckBGUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgressRing_AutoUpdateBackground.Visibility = Visibility.Visible;
                Button_AutoUpdateBackground.IsEnabled = false;
                await MainWindow.CurrentWindow.AutoUpdateBackgroundPicture(ComboBox_GameName.SelectedItem.ToString(),ComboBox_LogicName.SelectedItem.ToString());
                _ = SSMTMessageHelper.Show("背景图更新成功");
                Button_AutoUpdateBackground.IsEnabled = true;
                ProgressRing_AutoUpdateBackground.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                //出错的话得让按钮能再次按下
                Button_AutoUpdateBackground.IsEnabled = true;

                //也不能继续转圈圈
                ProgressRing_AutoUpdateBackground.Visibility = Visibility.Collapsed;

                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }


        private async void Button_SelectBackgroundFile_Click(object sender, RoutedEventArgs e)
        {
            string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png;.mp4;.webp;.webm");
            if (string.IsNullOrWhiteSpace(filepath))
                return;

            try
            {
                string folder = PathManager.Path_CurrentGamesFolder;

                // 清理旧背景文件
                foreach (var file in new[] { "Background.webp", "Background.png", "Background.mp4", "Background.webm" })
                {
                    string fullPath = Path.Combine(folder, file);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                string ext = Path.GetExtension(filepath).ToLowerInvariant();

                if (ext == ".png" || ext == ".webp")
                {
                    string NewBackgroundPath = Path.Combine(folder, "Background" + ext);
                    File.Copy(filepath, NewBackgroundPath, true);

                    MainWindow.CurrentWindow.ShowBackgroundPicture(NewBackgroundPath,GlobalConfig.CurrentGameName);
                }
                else if (ext == ".mp4" || ext == ".webm")
                {
                    string NewBackgroundPath = Path.Combine(folder, "Background" + ext);
                    File.Copy(filepath, NewBackgroundPath, true);

                    MainWindow.CurrentWindow.ShowBackgroundVideo(NewBackgroundPath,GlobalConfig.CurrentGameName);
                }
                else
                {
                    await SSMTMessageHelper.Show($"不支持的背景文件类型：{ext}");
                }
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }


     
    }
}
