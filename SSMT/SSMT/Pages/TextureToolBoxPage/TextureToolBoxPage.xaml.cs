using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using SSMT_Core;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using SSMT.SSMTHelper;
using SSMT.Pages.TextureToolBoxPage;

namespace SSMT
{
    public enum FpsOption
    {
        Fps30,
        Fps60
    }

    public sealed partial class TextureToolBoxPage : Page
    {
        // Backing fields
        private string SelectedTextureFilePath;
        private string SelectedVideoFilePath;
        private string DynamicTextureModGenerateFolderPath;
        private FpsOption SelectedFpsOption = FpsOption.Fps30;

        public TextureToolBoxPage()
        {
            this.InitializeComponent();

            // Load config
            var config = TextToolBoxConfig.Load();
            SelectedTextureFilePath = config.SelectedTextureFilePath;
            SelectedVideoFilePath = config.SelectedVideoFilePath;
            DynamicTextureModGenerateFolderPath = config.DynamicTextureModGenerateFolderPath;
            SelectedFpsOption = (FpsOption)config.SelectedFpsOption;

            // Initialize UI values
            TextBox_OriginalTextureFile.Text = SelectedTextureFilePath;
            TextBox_VideoFile.Text = SelectedVideoFilePath;
            TextBox_DynamicTextureModGenerateFolder.Text = DynamicTextureModGenerateFolderPath;
            ComboBox_FpsOption.SelectedIndex = SelectedFpsOption == FpsOption.Fps30 ?0 :1;
        }

        private void SaveConfig()
        {
            var config = new TextToolBoxConfig
            {
                SelectedTextureFilePath = this.SelectedTextureFilePath,
                SelectedVideoFilePath = this.SelectedVideoFilePath,
                DynamicTextureModGenerateFolderPath = this.DynamicTextureModGenerateFolderPath,
                SelectedFpsOption = (int)this.SelectedFpsOption
            };
            config.Save();
        }

        // Event handlers
        private async void Button_ChooseOriginalTextureFile_Click(object sender, RoutedEventArgs e)
        {
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(".dds");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedTextureFilePath = path;
                TextBox_OriginalTextureFile.Text = path;
                SaveConfig();
            }
        }

        private async void Button_ChooseVideoFile_Click(object sender, RoutedEventArgs e)
        {
            var supportedFormats = new List<string> { ".mp4", ".avi", ".mov", ".mkv", ".flv", ".webm", ".wmv", ".gif" };
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(supportedFormats);
            if (!string.IsNullOrEmpty(path))
            {
                SelectedVideoFilePath = path;
                TextBox_VideoFile.Text = path;
                SaveConfig();
            }
        }

        private async void Button_ChooseDynamicTextureModGenerateFolder_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (!string.IsNullOrEmpty(folderPath))
            {
                DynamicTextureModGenerateFolderPath = folderPath;
                TextBox_DynamicTextureModGenerateFolder.Text = folderPath;
                SaveConfig();
            }
        }

        private void Button_SetDynamicTextureModGenerateFolderToMods_Click(object sender, RoutedEventArgs e)
        {
            DynamicTextureModGenerateFolderPath = PathManager.Path_ModsFolder;
            TextBox_DynamicTextureModGenerateFolder.Text = DynamicTextureModGenerateFolderPath;
            SaveConfig();
        }

        private void TextBox_OriginalTextureFile_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = Task.Run(async () => await e.DataView.GetStorageItemsAsync()).Result;
                if (items.Count >0)
                {
                    var file = items[0] as Windows.Storage.StorageFile;
                    if (file != null)
                    {
                        var path = file.Path;
                        if (!string.IsNullOrEmpty(path))
                        {
                            SelectedTextureFilePath = path;
                            TextBox_OriginalTextureFile.Text = path;
                            SaveConfig();
                        }
                    }
                }
            }
        }

        private void TextBox_VideoFile_Drop(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = Task.Run(async () => await e.DataView.GetStorageItemsAsync()).Result;
                if (items.Count >0)
                {
                    var file = items[0] as Windows.Storage.StorageFile;
                    if (file != null)
                    {
                        var path = file.Path;
                        if (!string.IsNullOrEmpty(path))
                        {
                            SelectedVideoFilePath = path;
                            TextBox_VideoFile.Text = path;
                            SaveConfig();
                        }
                    }
                }
            }
        }

        private void TextBox_DragOver(object sender, Microsoft.UI.Xaml.DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private void ComboBox_FpsOption_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBox_FpsOption.SelectedIndex ==0) SelectedFpsOption = FpsOption.Fps30; else SelectedFpsOption = FpsOption.Fps60;
            SaveConfig();
        }

        private async void Button_GenerateDynamicTextureMod_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LOG.Info("GenerateDynamicTextureMod: start");

                if (string.IsNullOrWhiteSpace(SelectedTextureFilePath) || !File.Exists(SelectedTextureFilePath))
                {
                    await SSMTMessageHelper.Show("请先选择原始贴图文件（.dds）。", "Please choose the original texture file (.dds) first.");
                    return;
                }
                if (!SelectedTextureFilePath.EndsWith(".dds", StringComparison.OrdinalIgnoreCase))
                {
                    await SSMTMessageHelper.Show("请选择一个 .dds 格式的贴图文件。", "Please select a .dds texture file.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedVideoFilePath) || !File.Exists(SelectedVideoFilePath))
                {
                    await SSMTMessageHelper.Show("请先选择视频文件。", "Please choose the video file.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(DynamicTextureModGenerateFolderPath))
                {
                    await SSMTMessageHelper.Show("请先选择动态贴图Mod生成的文件夹位置。", "Please choose the output folder for generated dynamic texture mod.");
                    return;
                }
                if (!Directory.Exists(DynamicTextureModGenerateFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(DynamicTextureModGenerateFolderPath);
                    }
                    catch (Exception ex)
                    {
                        LOG.Info("GenerateDynamicTextureMod: create output folder failed: " + ex.Message);
                        await SSMTMessageHelper.Show("无法创建输出文件夹，请检查路径权限。", "Cannot create output folder, please check permissions.");
                        return;
                    }
                }

                string dynamicTextureModDir = Path.Combine(DynamicTextureModGenerateFolderPath, "DynamicTextureMod");
                if (Directory.Exists(dynamicTextureModDir))
                {
                    Directory.Delete(dynamicTextureModDir, true);
                }
                Directory.CreateDirectory(dynamicTextureModDir);

                var (width, height, format) = GetDdsInfo(SelectedTextureFilePath);

                string tempPngDir = Path.Combine(Path.GetTempPath(), "SSMT_TempPngFrames");
                if (Directory.Exists(tempPngDir)) Directory.Delete(tempPngDir, true);
                Directory.CreateDirectory(tempPngDir);
                ExtractAndFlipFrames(SelectedVideoFilePath, tempPngDir);

                ConvertPngToDds(tempPngDir, dynamicTextureModDir, width, height, format);

                int ddsFileCount = Directory.GetFiles(dynamicTextureModDir, "*.dds", SearchOption.TopDirectoryOnly).Length;
                LOG.Info($"DynamicTextureMod directory contains {ddsFileCount} DDS files.");

                string TextureHash = Path.GetFileName(SelectedTextureFilePath).Split("_")[0];

                CoreFunctions.GenerateDynamicTextureMod(dynamicTextureModDir, TextureHash, ".dds");

                SSMTCommandHelper.ShellOpenFolder(dynamicTextureModDir);
            }
            catch (Exception ex)
            {
                LOG.Info("GenerateDynamicTextureMod error: " + ex.ToString());
                await SSMTMessageHelper.Show("生成动态贴图Mod时发生错误：" + ex.Message, "Error occurred while generating dynamic texture mod: " + ex.Message);
            }
        }

        private (int width, int height, string format) GetDdsInfo(string ddsPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = PathManager.Path_TexconvExe,
                Arguments = $"-l -nologo \"{ddsPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("reading") && trimmed.Contains("(") && trimmed.Contains(")"))
                {
                    int l = trimmed.IndexOf('(');
                    int r = trimmed.IndexOf(')', l +1);
                    if (l >=0 && r > l)
                    {
                        var info = trimmed.Substring(l +1, r - l -1);
                        var parts = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >=2)
                        {
                            var wh = parts[0].Split('x');
                            if (wh.Length ==2 && int.TryParse(wh[0], out int width) && int.TryParse(wh[1], out int height))
                            {
                                string format = parts[1];
                                LOG.Info($"GetDdsInfo: path={ddsPath}, width={width}, height={height}, format={format}");
                                return (width, height, format);
                            }
                        }
                    }
                }
            }

            LOG.Info($"GetDdsInfo failed to parse. texconv stdout:\n{output}\n texconv stderr:\n{error}");
            throw new Exception("无法解析DDS属性: " + (output + error));
        }

        private void ExtractAndFlipFrames(string videoPath, string outputDir)
        {
            int fps = SelectedFpsOption == FpsOption.Fps30 ?30 :60;
            var psi = new ProcessStartInfo
            {
                FileName = PathManager.Path_Plugin_FFMPEG,
                Arguments = $"-i \"{videoPath}\" -vf \"fps={fps},scale=-1:-1,vflip,hflip\" \"{outputDir}\\frame_%05d.png\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            LOG.Info($"ExtractAndFlipFrames stdout:\n{output}");
            LOG.Info($"ExtractAndFlipFrames stderr:\n{error}");
        }

        private void ConvertPngToDds(string pngDir, string ddsDir, int width, int height, string format)
        {
            Directory.CreateDirectory(ddsDir);
            foreach (var png in Directory.GetFiles(pngDir, "frame_*.png"))
            {
                string fileName = Path.GetFileNameWithoutExtension(png);
                string ddsOut = Path.Combine(ddsDir, $"{fileName}.dds");
                var psi = new ProcessStartInfo
                {
                    FileName = PathManager.Path_TexconvExe,
                    Arguments = $"-f {format} -w {width} -h {height} -o \"{ddsDir}\" \"{png}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var proc = Process.Start(psi);
                proc.WaitForExit();
            }
        }
    }
}
