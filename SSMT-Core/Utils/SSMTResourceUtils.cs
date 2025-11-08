using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using SSMT_Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SSMT
{
    public class SSMTResourceUtils
    {
        public static void InitializeWorkFolder(bool OverwriteMigotoFiles = false)
        {
            if (Directory.Exists(GlobalConfig.SSMTCacheFolderPath) && GlobalConfig.SSMTCacheFolderPath != "")
            {
                

                if (!Directory.Exists(GlobalConfig.Path_LogsFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_LogsFolder);
                }

                if (!Directory.Exists(GlobalConfig.Path_TotalWorkSpaceFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_TotalWorkSpaceFolder);
                }



                //自动补全Plugins文件夹
                if (!Directory.Exists(GlobalConfig.Path_PluginsFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_PluginsFolder);
                }
                string OurPluginsPath = Path.Combine(GlobalConfig.Path_BaseFolder, "Plugins\\");
                string[] FileList = Directory.GetFiles(OurPluginsPath);
                foreach (string PluginFilePath in FileList)
                {
                    string PluginFileName = Path.GetFileName(PluginFilePath);
                    string TargetFileLocation = Path.Combine(GlobalConfig.Path_PluginsFolder, PluginFileName);

                    if (!File.Exists(TargetFileLocation))
                    {
                        File.Copy(PluginFilePath, TargetFileLocation, false);
                    }
                }

                //自动补全Games文件夹
                if (!Directory.Exists(GlobalConfig.Path_GamesFolder))
                {
                    Directory.CreateDirectory(GlobalConfig.Path_GamesFolder);
                }
                string OurGamesPath = Path.Combine(GlobalConfig.Path_BaseFolder, "Games\\");

                DBMTFileUtils.CopyDirectory(OurGamesPath, GlobalConfig.Path_GamesFolder, true, false);


                //自动补全3Dmigoto文件夹
                string Target3DmigotoFolderPath = Path.Combine(GlobalConfig.SSMTCacheFolderPath, "3Dmigoto\\");
                string Our3DmigotoPath = Path.Combine(GlobalConfig.Path_BaseFolder, "3Dmigoto\\");

                if (!Directory.Exists(Target3DmigotoFolderPath))
                {
                    Directory.CreateDirectory(Target3DmigotoFolderPath);
                }

                DBMTFileUtils.CopyDirectory(Our3DmigotoPath, Target3DmigotoFolderPath, true, OverwriteMigotoFiles);


            }
        }
        public static async Task<string> DownloadLatestBackground(string GameId)
        {
            // API 地址
            string apiUrl = "https://hyp-api.mihoyo.com/hyp/hyp-connect/api/getAllGameBasicInfo?launcher_id=jGHBHlcOq1&language=zh-cn&game_id=" + GameId;

            // 创建 HttpClient
            using HttpClient client = new HttpClient();

            // 添加 User-Agent 头，模拟浏览器请求
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // 发送 GET 请求
            Console.WriteLine("正在请求API...");
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            // 读取响应内容
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("API请求成功，开始解析JSON...");

            // 解析 JSON
            using JsonDocument document = JsonDocument.Parse(responseBody);
            JsonElement root = document.RootElement;

            // 获取第一个 background 的 URL
            JsonElement firstBackground = root
                .GetProperty("data")
                .GetProperty("game_info_list")[0]
                .GetProperty("backgrounds")[0]
                .GetProperty("background")
                .GetProperty("url");

            string imageUrl = firstBackground.GetString();
            Console.WriteLine($"获取到图片URL: {imageUrl}");

            // 下载图片
            Console.WriteLine("开始下载图片...");
            HttpResponseMessage imageResponse = await client.GetAsync(imageUrl);
            imageResponse.EnsureSuccessStatusCode();

            // 获取图片数据
            byte[] imageData = await imageResponse.Content.ReadAsByteArrayAsync();

            // 创建保存路径
            string fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            string ext = Path.GetExtension(fileName);
            string savePath = Path.Combine(GlobalConfig.Path_GamesFolder, GlobalConfig.CurrentGameName + "\\Background" + ext);

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }
            // 保存图片
            await File.WriteAllBytesAsync(savePath, imageData);
            Console.WriteLine($"图片已保存到: {savePath}");

            return savePath;
        }

        public static List<string> GetFrameAnalysisFileNameList()
        {
            string[] directories = Directory.GetDirectories(GlobalConfig.Path_3DmigotoLoaderFolder);
            List<string> frameAnalysisFileList = new List<string>();
            foreach (string directory in directories)
            {
                string directoryName = Path.GetFileName(directory);

                if (directoryName.StartsWith("FrameAnalysis-"))
                {
                    frameAnalysisFileList.Add(directoryName);
                }
            }
            return frameAnalysisFileList;
        }

        /// <summary>
        /// 此方法基于GetGameIconList来获取当前DBMT支持的所有游戏
        /// </summary>
        /// <returns></returns>
        public static List<string> GetGameNameList()
        {
            List<GameIconItem> gameIconItems = GetGameIconItems();

            List<string> GameNameList = [];
            foreach (GameIconItem gameIconItem in gameIconItems)
            {
                GameNameList.Add(gameIconItem.GameName);
            }

            return GameNameList;
        }

        public static List<GameIconItem> GetGameIconItems()
        {
            //LOG.Info("GetGameIconItems::Start");

            List<GameIconItem> gameIconItems = [];

            if (!Directory.Exists(GlobalConfig.Path_GamesFolder))
            {
                return gameIconItems;
            }

            string[] GamesFolderList = Directory.GetDirectories(GlobalConfig.Path_GamesFolder);

            foreach (string GameFolderPath in GamesFolderList)
            {
                //LOG.Info("GameFolder: " + GameFolderPath);
                if (Directory.Exists(GameFolderPath))
                {
                    string GameName = Path.GetFileName(GameFolderPath);
                    //LOG.Info(GameName);

                    string GameIconImage = Path.Combine(GlobalConfig.Path_GamesFolder,GameName + "\\Icon.png");
                    if (!File.Exists(GameIconImage))
                    {
                        GameIconImage = Path.Combine(GlobalConfig.Path_GamesFolder, "DefaultIcon.png");
                    }
                    //LOG.Info(GameIconImage);

                    string GameBackGroundImage = Path.Combine(GlobalConfig.Path_GamesFolder, GameName + "\\Background.png");
                    if (!File.Exists(GameBackGroundImage))
                    {
                        GameBackGroundImage = Path.Combine(GlobalConfig.Path_GamesFolder, "DefaultBackground.png");
                    }
                    //LOG.Info(GameBackGroundImage);

                    gameIconItems.Add(new GameIconItem
                    {
                        GameIconImage = GameIconImage,
                        GameName = GameName,
                        GameBackGroundImage = new BitmapImage(new Uri(GameBackGroundImage))
                    });
                }
            }
            //LOG.Info("GetGameIconItems::End");

            return gameIconItems;
        }


        

    }
}
