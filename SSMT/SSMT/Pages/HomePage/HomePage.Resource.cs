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

        //TODO IsLoopingEnabled有个严重的问题就是循环播放时，会卡顿一瞬间
        //但是米哈游启动器就不卡，这个基本上就是WinUI3这个MediaPlayer实现的问题
        //暂时先不解决，不碰底层的情况下，不是轻易能解决的。
        //咱先解决有没有的问题，再解决好不好的问题。
        MediaPlayer BackgroundMediaPlayer = new MediaPlayer
        {
            IsLoopingEnabled = true,
        };


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

        public void ShowBackgroundVideo(string path)
        {
            ResetBackground();

            BackgroundVideo.Visibility = Visibility.Visible;

            // 刷新 Uri，避免 WinUI3 缓存旧版本
            var uri = new Uri(path + "?t=" + DateTime.Now.Ticks);

            BackgroundMediaPlayer.Source = MediaSource.CreateFromUri(uri);
            BackgroundVideo.SetMediaPlayer(BackgroundMediaPlayer);

            BackgroundMediaPlayer.Play();

            VisualHelper.CreateFadeAnimation(BackgroundVideo);
        }


        public void ShowBackgroundPicture(string path)
        {
            ResetBackground();

            MainWindowImageBrush.Visibility = Visibility.Visible;

            VisualHelper.CreateScaleAnimation(imageVisual);
            VisualHelper.CreateFadeAnimation(imageVisual);

            // 强制刷新图片链接（你之前已有相同逻辑）
            MainWindowImageBrush.Source =
                new BitmapImage(new Uri(path + "?t=" + DateTime.Now.Ticks));
        }




        private async Task AutoUpdateBackgroundPicture(string SpecificLogicName = "")
        {
            ResetBackground();
            string GameId = HoyoBackgroundUtils.GetGameId(SpecificLogicName, GlobalConfig.Chinese);

            if (GameId == "")
            {
                _ = SSMTMessageHelper.Show("当前选择的执行逻辑: " + SpecificLogicName + " 暂不支持自动更新背景图，请手动设置。");
                return;
            }

            string BaseUrl = HoyoBackgroundUtils.GetBackgroundUrl(GameId, GlobalConfig.Chinese);
            //webm不一定存在，所以直接try catch，出错就懒得管了

            bool UseWebmBackground = false;

            //TODO 注意，绝区零的背景图视频有毛病，虽然都是.webm格式，但是只能在浏览器中播放，无法使用本地的媒体播放器播放
            //这意味着我们必须添加ffmpeg转码，下载下来之后执行转换，变为mp4视频，然后再应用为背景图，因为我实际测试发现WinUI3也是无法播放的
            //因为WinUI3用的就是系统的解码，底层都是一个东西导致的。
            //有点麻烦了，而且动态背景图本身就存在循环播放一瞬间卡顿的问题，而且暂时没法解决
            //综合来说，解决这个问题收益不大，暂时不实现了，随缘等一个热爱ZZZ的开发者提PR实现一下

            // PxncAcd: 您猜怎么着, 解决了


            try
            {
                string NewWebmBackgroundPath = await HoyoBackgroundUtils.DownloadLatestWebmBackground(BaseUrl);

                if (File.Exists(NewWebmBackgroundPath))
                {
                    UseWebmBackground = true;
                }

                string finalVideoPath = NewWebmBackgroundPath;

                if (SpecificLogicName == LogicName.ZZMI)
                {
                    // For ZZZ: cannot decode .webm → must transcode to mp4 via ffmpeg.

                    try
                    {
                        string ffmpegPath = Path.Combine(AppContext.BaseDirectory, "Plugins", "ffmpeg.exe");
                        if (!File.Exists(ffmpegPath))
                        {
                            LOG.Info("ffmpeg.exe 不存在，无法进行 ZZZ 背景视频转码");
                            throw new FileNotFoundException("ffmpeg.exe missing");
                        }

                        string mp4Output = Path.ChangeExtension(NewWebmBackgroundPath, ".mp4");

                        var psi = new ProcessStartInfo
                        {
                            FileName = ffmpegPath,
                            Arguments = $"-y -i \"{NewWebmBackgroundPath}\" -c:v libx264 -preset veryfast -pix_fmt yuv420p \"{mp4Output}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            RedirectStandardOutput = true
                        };

                        using (var p = new Process())
                        {
                            p.StartInfo = psi;

                            p.OutputDataReceived += (s, e) =>
                            {
                                if (!string.IsNullOrEmpty(e.Data))
                                    LOG.Info("[ffmpeg stdout] " + e.Data);
                            };

                            p.ErrorDataReceived += (s, e) =>
                            {
                                if (!string.IsNullOrEmpty(e.Data))
                                    LOG.Info("[ffmpeg stderr] " + e.Data);
                            };

                            p.Start();

                            p.BeginOutputReadLine();
                            p.BeginErrorReadLine();

                            await p.WaitForExitAsync();

                            if (p.ExitCode != 0 || !File.Exists(mp4Output))
                                throw new Exception($"ffmpeg failed, exit code {p.ExitCode}");

                            LOG.Info("ZZZ 背景视频已成功由 webm 转为 mp4");
                            finalVideoPath = mp4Output;
                            // delete webm
                            try
                            {
                                File.Delete(NewWebmBackgroundPath);
                            }
                            catch (Exception delEx)
                            {
                                LOG.Info("删除中间 webm 文件失败: " + delEx.Message);
                            }

                        }
                    }
                    catch (Exception innerEx)
                    {
                        LOG.Info("ZZZ 背景视频转码失败，fallback handler 触发: " + innerEx.Message);
                        // finalVideoPath = FigFallbackHandler(NewWebmBackgroundPath);
                    }
                }

                ShowBackgroundVideo(finalVideoPath);
                LOG.Info("设置好背景图视频了");
            }
            catch (Exception ex)
            {
                LOG.Info(ex.ToString());
            }






            //如果使用上了视频背景图，那就不用管后面的内容了
            if (UseWebmBackground)
            {
                LOG.Info("用上视频背景图了，后面内容不管了");
                return;
            }

            //否则使用普通背景图
            string NewWebpBackgroundPath = await HoyoBackgroundUtils.DownloadLatestWebpBackground(BaseUrl);
            if (File.Exists(NewWebpBackgroundPath))
            {
                ShowBackgroundPicture(NewWebpBackgroundPath);
            }

        }

        private async void Button_CheckBGUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProgressRing_AutoUpdateBackground.Visibility = Visibility.Visible;
                Button_AutoUpdateBackground.IsEnabled = false;
                await AutoUpdateBackgroundPicture(ComboBox_LogicName.SelectedItem.ToString());
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

                    ShowBackgroundPicture(NewBackgroundPath);
                }
                else if (ext == ".mp4" || ext == ".webm")
                {
                    string NewBackgroundPath = Path.Combine(folder, "Background" + ext);
                    File.Copy(filepath, NewBackgroundPath, true);

                    ShowBackgroundVideo(NewBackgroundPath);
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


        private void ResetBackground()
        {
            try
            {
                // 停止播放并清空媒体
                BackgroundMediaPlayer.Pause();
                BackgroundMediaPlayer.Source = null;
            }
            catch { }

            // 隐藏视频
            BackgroundVideo.Visibility = Visibility.Collapsed;

            // 清空静态图
            MainWindowImageBrush.Visibility = Visibility.Collapsed;
            MainWindowImageBrush.Source = null;
        }
    }
}
