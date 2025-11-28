using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using SSMT;
using System.Collections.ObjectModel;
using SSMT.Configs;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SSMT_Core;
using System;
using System.IO;

namespace SSMT.ViewModels
{
    public enum FpsOption
    {
        Fps30,
        Fps60
    }

    public partial class TextureToolBoxPageViewModel : ObservableObject
    {
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

        private string _selectedTextureFilePath;
        public string SelectedTextureFilePath
        {
            get => _selectedTextureFilePath;
            set
            {
                if (SetProperty(ref _selectedTextureFilePath, value))
                {
                    SaveConfig();
                }
            }
        }

        private string _selectedVideoFilePath;
        public string SelectedVideoFilePath
        {
            get => _selectedVideoFilePath;
            set
            {
                if (SetProperty(ref _selectedVideoFilePath, value))
                {
                    SaveConfig();
                }
            }
        }

        private string _dynamicTextureModGenerateFolderPath;
        public string DynamicTextureModGenerateFolderPath
        {
            get => _dynamicTextureModGenerateFolderPath;
            set
            {
                if (SetProperty(ref _dynamicTextureModGenerateFolderPath, value))
                {
                    SaveConfig();
                }
            }
        }

        private FpsOption _selectedFpsOption = FpsOption.Fps30;
        public FpsOption SelectedFpsOption
        {
            get => _selectedFpsOption;
            set
            {
                if (SetProperty(ref _selectedFpsOption, value))
                {
                    SaveConfig();
                }
            }
        }

        public ObservableCollection<FpsOption> FpsOptions { get; } = new ObservableCollection<FpsOption>
        {
            FpsOption.Fps30,
            FpsOption.Fps60
        };

        public TextureToolBoxPageViewModel()
        {
            // 加载配置
            var config = TextToolBoxConfig.Load();
            SelectedTextureFilePath = config.SelectedTextureFilePath;
            SelectedVideoFilePath = config.SelectedVideoFilePath;
            DynamicTextureModGenerateFolderPath = config.DynamicTextureModGenerateFolderPath;
            SelectedFpsOption = (FpsOption)config.SelectedFpsOption;
        }

        [RelayCommand]
        public async Task ChooseOriginalTextureFileAsync()
        {
            // Use existing helper to show picker and get path (handles window handle initialization)
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(".dds");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedTextureFilePath = path;
            }
        }

        [RelayCommand]
        public async Task ChooseVideoFileAsync()
        {
            string path = await SSMTCommandHelper.ChooseFileAndGetPath(".mp4,.avi,.mov,.mkv,.flv,.webm,.wmv,.gif");
            if (!string.IsNullOrEmpty(path))
            {
                SelectedVideoFilePath = path;
            }
        }

        [RelayCommand]
        public async Task ChooseDynamicTextureModGenerateFolderAsync()
        {
            string folderPath = await SSMTCommandHelper.ChooseFolderAndGetPath();
            if (!string.IsNullOrEmpty(folderPath))
            {
                DynamicTextureModGenerateFolderPath = folderPath;
            }
        }

        // Drag-and-drop handlers for MVVM: receive file path string from attached behavior
        [RelayCommand]
        public void TextureFileDropped(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                SelectedTextureFilePath = path;
            }
        }

        [RelayCommand]
        public void VideoFileDropped(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                SelectedVideoFilePath = path;
            }
        }

        [RelayCommand]
        public async Task GenerateDynamicTextureMod()
        {
            try
            {
                LOG.Info("GenerateDynamicTextureMod: start");

                // 验证原始贴图文件
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

                // 验证视频文件
                if (string.IsNullOrWhiteSpace(SelectedVideoFilePath) || !File.Exists(SelectedVideoFilePath))
                {
                    await SSMTMessageHelper.Show("请先选择视频文件（.mp4）。", "Please choose the video file (.mp4) first.");
                    return;
                }

                // 验证目标文件夹
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

                //1. 获取DDS属性
                var (width, height, format) = GetDdsInfo(SelectedTextureFilePath);

                //2. 用ffmpeg转为PNG序列并翻转
                string tempPngDir = Path.Combine(Path.GetTempPath(), "SSMT_TempPngFrames");
                if (Directory.Exists(tempPngDir)) Directory.Delete(tempPngDir, true);
                Directory.CreateDirectory(tempPngDir);
                ExtractAndFlipFrames(SelectedVideoFilePath, tempPngDir);

                //3. PNG批量转DDS
                string outputDir = DynamicTextureModGenerateFolderPath;
                ConvertPngToDds(tempPngDir, outputDir, width, height, format);

                LOG.Info("GenerateDynamicTextureMod: done");
                await SSMTMessageHelper.Show("动态贴图Mod生成完成。", "Dynamic texture mod generation completed.");
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
                Arguments = $"-l -nologo \"{ddsPath}\"", // -l只读，-nologo去除头部版权
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            //只处理reading行，忽略其它所有输出
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("reading") && trimmed.Contains("(") && trimmed.Contains(")"))
                {
                    int l = trimmed.IndexOf('(');
                    int r = trimmed.IndexOf(')', l +1);
                    if (l >=0 && r > l)
                    {
                        var info = trimmed.Substring(l +1, r - l -1); //1024x1024 BC1_UNORM_SRGB2D
                        var parts = info.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >=2)
                        {
                            var wh = parts[0].Split('x');
                            if (wh.Length ==2 && int.TryParse(wh[0], out int width) && int.TryParse(wh[1], out int height))
                            {
                                string format = parts[1];
                                // 使用项目的日志工具记录解析到的属性
                                LOG.Info($"GetDdsInfo: path={ddsPath}, width={width}, height={height}, format={format}");
                                return (width, height, format);
                            }
                        }
                    }
                }
            }

            // 如果无法解析，记录完整输出到日志（不抛出Debug输出）并抛异常
            LOG.Info($"GetDdsInfo failed to parse. texconv stdout:\n{output}\n texconv stderr:\n{error}");
            throw new Exception("无法解析DDS属性: " + (output + error));
        }

        private void ExtractAndFlipFrames(string videoPath, string outputDir)
        {
            var psi = new ProcessStartInfo
            {
                FileName = PathManager.Path_Plugin_FFMPEG,
                Arguments = $"-i \"{videoPath}\" -vf \"vflip,hflip\" -q:v1 \"{outputDir}\\frame_%05d.png\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi);
            proc.WaitForExit();
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