using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
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

                string PackageName = ComboBox_LogicName.SelectedItem.ToString();
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
                if (!Directory.Exists(ModsFolder)) {
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

        private async void AutoUpdateBackgroundPicture(string SpecificLogicName = "")
        {
            /*
            https://github.com/Scighost/Starward/issues/833

            新版启动器背景图API
            https://hyp-api.mihoyo.com/hyp/hyp-connect/api/getAllGameBasicInfo?launcher_id=jGHBHlcOq1&language=zh-cn&game_id=64kMb5iAWu
            
            国际服
            https://sg-hyp-api.hoyoverse.com/hyp/hyp-connect/api/getAllGameBasicInfo?launcher_id=VYTpXlbWo8&language=zh-cn7D&game_id=4ziysqXOQ8

            替换game_id参数还有别的几个游戏背景图

            name biz game_id

            绝区零 nap_cn x6znKlJ0xK
            崩坏星穹铁道 hkrpg_cn 64kMb5iAWu
            原神 hk4e_cn 1Z8W5NHUQb
            崩坏3 bh3_cn osvnlOc0S8
            ZenlessZoneZero nap_global U5hbdsT9W7
            HonkaiStarRail hkrpg_global 4ziysqXOQ8
            GenshinImpact hk4e_global gopR6Cufr3
            HonkaiImpact3rd bh3_global 5TIVvvcwtM
             */
            string GameId = "";
            string LogicNameStr = SpecificLogicName;

            if(LogicNameStr == "")
            {
                LogicNameStr = ComboBox_LogicName.SelectedItem.ToString();
            }

            if (LogicNameStr == LogicName.GIMI)
            {
                GameId = "1Z8W5NHUQb";
            }
            else if (LogicNameStr == LogicName.SRMI)
            {
                GameId = "64kMb5iAWu";
            }
            else if (LogicNameStr == LogicName.HIMI)
            {
                GameId = "osvnlOc0S8";

            }
            else if (LogicNameStr == LogicName.ZZMI)
            {
                GameId = "x6znKlJ0xK";
            }

            if (GameId != "")
            {
                string NewBackGroundPath = await SSMTResourceUtils.DownloadLatestBackground(GameId);
                //MainWindowImageBrush.Source = new BitmapImage(new Uri(NewBackGroundPath));
                VisualHelper.CreateScaleAnimation(imageVisual);
                VisualHelper.CreateFadeAnimation(imageVisual);

                MainWindowImageBrush.Source = new BitmapImage(new Uri(NewBackGroundPath + "?t=" + DateTime.Now.Ticks));
            }
            else
            {
                _ = SSMTMessageHelper.Show("当前选择的执行逻辑: " + LogicNameStr + " 暂不支持自动更新背景图，请手动设置。");
            }
        }

        private void Button_CheckBGUpdate_Click(object sender, RoutedEventArgs e)
        {
            try {
                AutoUpdateBackgroundPicture();
                _ = SSMTMessageHelper.Show("背景图更新成功");
            }
            catch (Exception ex)
            {
                _ = SSMTMessageHelper.Show(ex.ToString());
            }
        }


        private async void Button_SelectBackgroundFile_Click(object sender, RoutedEventArgs e)
        {
            string filepath = await SSMTCommandHelper.ChooseFileAndGetPath(".png;.mp4");
            if (string.IsNullOrWhiteSpace(filepath))
                return;

            try
            {
                string folder = GlobalConfig.Path_CurrentGamesFolder;

                // 清理旧背景文件
                foreach (var file in new[] { "Background.webp", "Background.png", "Background.mp4" })
                {
                    string fullPath = Path.Combine(folder, file);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                string ext = Path.GetExtension(filepath).ToLowerInvariant();

                if (ext == ".png")
                {
                    string NewBackgroundPath = Path.Combine(folder, "Background.png");
                    File.Copy(filepath, NewBackgroundPath, true);

                    BackgroundVideo.Visibility = Visibility.Collapsed;
                    MainWindowImageBrush.Visibility = Visibility.Visible;

                    VisualHelper.CreateScaleAnimation(MainWindowImageBrush);
                    VisualHelper.CreateFadeAnimation(MainWindowImageBrush);
                    MainWindowImageBrush.Source = new BitmapImage(new Uri(NewBackgroundPath));
                }
                else if (ext == ".mp4")
                {
                    string NewBackgroundPath = Path.Combine(folder, "Background.mp4");
                    File.Copy(filepath, NewBackgroundPath, true);

                    MainWindowImageBrush.Visibility = Visibility.Collapsed;
                    BackgroundVideo.Visibility = Visibility.Visible;

                    var player = new MediaPlayer
                    {
                        Source = MediaSource.CreateFromUri(new Uri(NewBackgroundPath)),
                        IsLoopingEnabled = true
                    };
                    BackgroundVideo.SetMediaPlayer(player);
                    player.Play();

                    VisualHelper.CreateFadeAnimation(BackgroundVideo);
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
