using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Globalization;
using SSMT_Core;

namespace SSMT
{
    
    public static class GlobalConfig
    {
        public static string analyse_options = "deferred_ctx_immediate dump_rt dump_cb dump_vb dump_ib buf txt dds dump_tex dds";

        public static string SSMT_Version = "V3.0.5";

        public static string GIPluginName = "GoodWorkGI.exe";

        //程序窗口名称
        public static string SSMT_Title = "SSMT " + SSMT_Version;
        public static string CurrentGameName { get; set; } = "ZZZ";
        public static string CurrentWorkSpace { get; set; } = "";
        public static string SSMTCacheFolderPath { get; set; } = "";
        public static bool Theme { get; set; } = true;
        public static bool Chinese { get; set; } = true;

        //窗口大小
        public static double WindowWidth { get; set; } = 1280;
        public static double WindowHeight { get; set; } = 720;
        public static double WindowLuminosityOpacity { get; set; } = 0.65f;

        public static int WindowPositionX { get; set; } = -1;
        public static int WindowPositionY { get; set; } = -1;

        //Others
        public static bool AutoCleanFrameAnalysisFolder { get; set; } = true;
        public static int FrameAnalysisFolderReserveNumber { get; set; } = 1;
 

        /// <summary>
        /// 打开页面后跳转到工作台页面
        /// </summary>
        public static bool OpenToWorkPage { get; set; } = false;

        /// <summary>
        /// 是否显示数据类型管理页面
        /// </summary>
        public static bool ShowGameTypePage { get; set; } = false;
        /// <summary>
        /// 是否显示Mod管理页面
        /// </summary>
        public static bool ShowModManagePage { get; set; } = false;
       
        /// <summary>
        /// 是否显示Mod保护页面
        /// </summary>
        public static bool ShowModProtectPage { get; set; } = false;
        /// <summary>
        /// 是否显示贴图工具箱页面，因为这个页面用的人非常少。
        /// </summary>
        public static bool ShowTextureToolBoxPage { get; set; } = false;

        //// 配置文件路径
        public static string Path_MainConfig
        {
            get { return Path.Combine(Path_ConfigsFolder, PathManager.Name_GlobalConfigFileName); }
        }

        public static string Path_MainConfig_Global
        {
            get { return Path.Combine(Path_AppDataLocal, PathManager.Name_GlobalConfigFileName); }
        }


        /// <summary>
        /// 使用古法读取，不要自作聪明用C#的某些语法糖特性实现全自动
        /// </summary>
        public static void ReadConfig()
        {
            try
            {
                //读取配置时优先读取全局的
                JObject SettingsJsonObject = DBMTJsonUtils.CreateJObject();
                try
                {
                    if (File.Exists(GlobalConfig.Path_MainConfig_Global))
                    {
                        string json = File.ReadAllText(GlobalConfig.Path_MainConfig_Global);
                        SettingsJsonObject = JObject.Parse(json);
                    }
                }
                catch (Exception ex) {
                    //如果全局的配置文件读取错误的话，直接删掉重新保存一个全局的配置文件
                    //这是因为蓝屏的时候这里的配置文件会直接被损坏。
                    ex.ToString();
                    File.Delete(GlobalConfig.Path_MainConfig);
                    GlobalConfig.SaveConfig();
                }
                
                //古法读取
                if (SettingsJsonObject.ContainsKey("CurrentGameName"))
                {
                    CurrentGameName = (string)SettingsJsonObject["CurrentGameName"];
                }

                if (SettingsJsonObject.ContainsKey("CurrentWorkSpace"))
                {
                    CurrentWorkSpace = (string)SettingsJsonObject["CurrentWorkSpace"];
                }

                if (SettingsJsonObject.ContainsKey("DBMTWorkFolder"))
                {
                    SSMTCacheFolderPath = (string)SettingsJsonObject["DBMTWorkFolder"];
                }

                //WindowWidth
                if (SettingsJsonObject.ContainsKey("WindowWidth"))
                {
                    WindowWidth = (double)SettingsJsonObject["WindowWidth"];
                }

                //WindowHeight
                if (SettingsJsonObject.ContainsKey("WindowHeight"))
                {
                    WindowHeight = (double)SettingsJsonObject["WindowHeight"];
                }

                //WindowPositionX
                if (SettingsJsonObject.ContainsKey("WindowPositionX"))
                {
                    WindowPositionX = (int)SettingsJsonObject["WindowPositionX"];
                }

                //WindowPositionY
                if (SettingsJsonObject.ContainsKey("WindowPositionY"))
                {
                    WindowPositionY = (int)SettingsJsonObject["WindowPositionY"];
                }

                //AutoCleanFrameAnalysisFolder
                if (SettingsJsonObject.ContainsKey("AutoCleanFrameAnalysisFolder"))
                {
                    AutoCleanFrameAnalysisFolder = (bool)SettingsJsonObject["AutoCleanFrameAnalysisFolder"];
                }

                //FrameAnalysisFolderReserveNumber
                if (SettingsJsonObject.ContainsKey("FrameAnalysisFolderReserveNumber"))
                {
                    FrameAnalysisFolderReserveNumber = (int)SettingsJsonObject["FrameAnalysisFolderReserveNumber"];
                }

           


            

                //WindowLuminosityOpacity
                if (SettingsJsonObject.ContainsKey("WindowLuminosityOpacity"))
                {
                    WindowLuminosityOpacity = (double)SettingsJsonObject["WindowLuminosityOpacity"];
                }

                

                //OpenToWorkPage
                if (SettingsJsonObject.ContainsKey("OpenToWorkPage"))
                {
                    OpenToWorkPage = (bool)SettingsJsonObject["OpenToWorkPage"];
                }

                if (SettingsJsonObject.ContainsKey("Theme"))
                {
                    Theme = (bool)SettingsJsonObject["Theme"];
                }

                if (SettingsJsonObject.ContainsKey("Chinese"))
                {
                    Chinese = (bool)SettingsJsonObject["Chinese"];
                }


                //ShowGameTypePage
                if (SettingsJsonObject.ContainsKey("ShowGameTypePage"))
                {
                    ShowGameTypePage = (bool)SettingsJsonObject["ShowGameTypePage"];
                }

                //ShowModManagePage
                if (SettingsJsonObject.ContainsKey("ShowModManagePage"))
                {
                    ShowModManagePage = (bool)SettingsJsonObject["ShowModManagePage"];
                }

               

                //ShowModProtectPage
                if (SettingsJsonObject.ContainsKey("ShowModProtectPage"))
                {
                    ShowModProtectPage = (bool)SettingsJsonObject["ShowModProtectPage"];
                }

                //ShowTextureToolBoxPage
                if (SettingsJsonObject.ContainsKey("ShowTextureToolBoxPage"))
                {
                    ShowTextureToolBoxPage = (bool)SettingsJsonObject["ShowTextureToolBoxPage"];
                }

            }
            catch (Exception ex) {
                ex.ToString();
            }
        }

        /// <summary>
        /// 使用古法保存，不要自作聪明用C#的某些语法糖特性实现全自动
        /// </summary>
        public static void SaveConfig()
        {
            //古法保存
            JObject SettingsJsonObject = new JObject();

            SettingsJsonObject["CurrentGameName"] = CurrentGameName;
            SettingsJsonObject["CurrentWorkSpace"] = CurrentWorkSpace;
            SettingsJsonObject["DBMTWorkFolder"] = SSMTCacheFolderPath;
            SettingsJsonObject["WindowWidth"] = WindowWidth;
            SettingsJsonObject["WindowHeight"] = WindowHeight;
            SettingsJsonObject["WindowPositionX"] = WindowPositionX;
            SettingsJsonObject["WindowPositionY"] = WindowPositionY;
            SettingsJsonObject["AutoCleanFrameAnalysisFolder"] = AutoCleanFrameAnalysisFolder;
            SettingsJsonObject["FrameAnalysisFolderReserveNumber"] = FrameAnalysisFolderReserveNumber;
            SettingsJsonObject["WindowLuminosityOpacity"] = WindowLuminosityOpacity;
            SettingsJsonObject["OpenToWorkPage"] = OpenToWorkPage;
            SettingsJsonObject["Theme"] = Theme;
            SettingsJsonObject["Chinese"] = Chinese;
            SettingsJsonObject["ShowGameTypePage"] = ShowGameTypePage;
            SettingsJsonObject["ShowModManagePage"] = ShowModManagePage;
            SettingsJsonObject["ShowModProtectPage"] = ShowModProtectPage;
            SettingsJsonObject["ShowTextureToolBoxPage"] = ShowTextureToolBoxPage;

            //写出内容
            string WirteStirng = SettingsJsonObject.ToString();

            //保存配置时，全局配置也顺便保存一份
            File.WriteAllText(Path_MainConfig_Global, WirteStirng);
        }


        public static string Path_ModsFolder
        {
            get { return Path.Combine(Path_3DmigotoLoaderFolder, "Mods\\"); }
        }
       

        public static string Path_ModRepoFolder
        {
            get { return Path_ModsFolder; }
        }

        public static string Path_GamesFolder
        {
            get { return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Games\\"); }
        }
        public static string Path_CurrentGamesFolder
        {
            get { return Path.Combine(GlobalConfig.Path_GamesFolder, GlobalConfig.CurrentGameName + "\\"); }
        }

        public static string Path_CurrentGameConfigJson
        {
            get { return Path.Combine(GlobalConfig.Path_CurrentGamesFolder,"Config.json"); }
        }

        public static string Path_GamesIconConfigJson
        {
            get { return Path.Combine(GlobalConfig.Path_GamesFolder,"GameIconConfig.json"); }
        }

        public static string Path_CurrentWorkSpaceGeneratedModFolder
        {
            get {
                string retpath = Path.Combine(GlobalConfig.Path_ModsFolder, "SSMTGeneratedMod\\Default\\Mod_" + GlobalConfig.CurrentWorkSpace + "\\");
                if (!Directory.Exists(retpath))
                {
                    Directory.CreateDirectory(retpath);
                }

                return retpath;
                    
            }
        }
        public static string CurrentGameMigotoFolder
        {
            get
            {
                GameConfig gameConfig = new GameConfig();
                return gameConfig.MigotoPath;
            }
        }

        public static string Path_3DmigotoLoaderFolder
        {
            get { return GlobalConfig.CurrentGameMigotoFolder; }
        }

        public static string Path_D3DXINI
        {
            get { return Path.Combine(GlobalConfig.CurrentGameMigotoFolder, "d3dx.ini"); }
        }


        //DumpIBListConfig
        public static string Path_DumpIBListConfig
        {
            get { return Path.Combine(GlobalConfig.CurrentGameMigotoFolder, "DumpIBListConfig.ini"); }
        }

        public static string Path_AssetsFolder
        {
            get { return Path.Combine(GlobalConfig.Path_BaseFolder, "Assets\\"); }
        }

        public static string Path_TextureConfigsFolder
        {
            get { return Path.Combine(Path_ConfigsFolder, "TextureConfigs\\"); }
        }

        public static string Path_GameTypeConfigsFolder
        {
            get { return Path.Combine(GlobalConfig.Path_BaseFolder, "GameTypeConfigs\\" ); }
        }

        public static string Path_CurrentGame_GameTypeFolder
        {
            get { return Path.Combine(GlobalConfig.Path_GameTypeConfigsFolder, GlobalConfig.CurrentGameName + "\\"); }
        }

        public static string Path_GameTextureConfigFolder
        {
            get { return Path.Combine(Path_TextureConfigsFolder, GlobalConfig.CurrentGameName + "\\"); }
        }


        //三种注入器的路径
        public static string Path_3Dmigoto_Loader_EXE
        {
            get { return Path.Combine(Path_3DmigotoLoaderFolder, "3Dmigoto Loader.exe"); }
        }

        public static string Path_3Dmigoto_Loader_PY
        {
            get { return Path.Combine(Path_3DmigotoLoaderFolder, "3Dmigoto Loader.py"); }
        }

        public static string Path_3Dmigoto_Loader_ByPassACE_EXE
        {
            get { return Path.Combine(Path_3DmigotoLoaderFolder, "3Dmigoto Loader-ByPassACE.exe"); }
        }

        public static string Path_PluginsFolder
        {
            get { return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Plugins\\"); }
        }





        public static string LatestFrameAnalysisFolderName
        {
            get
            {
                List<string> frameAnalysisFileList = SSMTResourceUtils.GetFrameAnalysisFileNameList();
                
                if (frameAnalysisFileList.Count > 0)
                {
                    return frameAnalysisFileList.Last();
                }

                return "";
            }
        }

        /// <summary>
        /// 最新的FrameAnalysis文件夹
        /// </summary>
        public static string Path_LatestFrameAnalysisFolder
        {
            get
            {

                if (LatestFrameAnalysisFolderName != "")
                {
                    return Path.Combine(Path_3DmigotoLoaderFolder, LatestFrameAnalysisFolderName + "\\");
                }
                else
                {
                    return "";
                }
            }
        }

        public static string Path_LatestFrameAnalysisLogTxt
        {
            get
            {
                return Path.Combine(Path_LatestFrameAnalysisFolder, "log.txt");
            }
        }
        

        //起别名方便使用
        public static string WorkFolder
        {
            get
            {
                return Path_LatestFrameAnalysisFolder;
            }
        }

        public static string Path_LatestFrameAnalysisDedupedFolder
        {
            get
            {
                return Path.Combine(Path_LatestFrameAnalysisFolder, "deduped\\");
            }
        }

        public static string Path_LogsFolder
        {
            get { return Path.Combine(GlobalConfig.SSMTCacheFolderPath,"Logs\\"); }
        }

     
    
        public static string Path_LatestDBMTLogFile
        {
            get
            {
                string logsPath = GlobalConfig.Path_LogsFolder;
                if (!Directory.Exists(logsPath))
                {
                    return "";
                }
                string[] logFiles = Directory.GetFiles(logsPath); ;
                List<string> logFileList = new List<string>();
                foreach (string logFile in logFiles)
                {
                    string logfileName = Path.GetFileName(logFile);
                    if (logfileName.EndsWith(".log") && logfileName.Length > 15)
                    {
                        logFileList.Add(logfileName);
                    }
                }

                logFileList.Sort();


                if (logFileList.Count == 0)
                {
                    return "";
                }
                else
                {
                    string LogFilePath = logsPath + "\\" + logFileList[^1];
                    return LogFilePath;
                }
            }
        }


        public static string Path_TotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "WorkSpace\\");
            }
        }

        public static string Path_CurrentGameTotalWorkSpaceFolder
        {
            get
            {
                return Path.Combine(GlobalConfig.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\");
            }
        }

        public static string Path_CurrentWorkSpaceFolder
        {
            get{
                string CurrentWorkSpaceFolder = Path.Combine(GlobalConfig.Path_TotalWorkSpaceFolder, GlobalConfig.CurrentGameName + "\\" + GlobalConfig.CurrentWorkSpace + "\\");
                return CurrentWorkSpaceFolder;
            }
        }

        public static string Path_CurrentWorkSpace_ConfigJson
        {
            get
            {
                return Path.Combine(Path_CurrentWorkSpaceFolder,"Config.json");
            }
        }

        public static string Path_CurrentWorkSpace_SkipIBConfigJson
        {
            get
            {
                return Path.Combine(Path_CurrentWorkSpaceFolder, "SkipIBConfig.json");
            }
        }

        


        public static string Path_CurrentWorkSpace_VSCheckConfigJson
        {
            get
            {
                return Path.Combine(Path_CurrentWorkSpaceFolder, "VSCheckConfig.json");
            }
        }

        public static string Path_AppDataLocal
        {
            get
            { // 如果你需要非漫游配置文件路径（AppData\Local），可以这样做：
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return localAppDataPath;
            }
        }

        public static string Path_ConfigsFolder
        {
            get { return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Configs\\"); }
        }

        //ReverseResult.json
        public static string Path_ReverseResultConfig
        {
            get { return Path.Combine(GlobalConfig.Path_ConfigsFolder, "ReverseResult.json"); }
        }

        public static string Path_ModManageConfig
        {
            get { return Path.Combine(GlobalConfig.Path_ConfigsFolder, "ModManageConfig.json"); }
        }

        public static string Path_TexturePageIndexConfig
        {
            get { return Path.Combine(GlobalConfig.Path_ConfigsFolder, "TexturePageIndexConfig.json"); }
        }

        public static string Path_CurrentGameMainConfigJsonFile
        {
            get { return Path.Combine(Path_GamesFolder, GlobalConfig.CurrentGameName + "\\MainConfig.json"); }
        }

        public static string Path_SwordLv5Exe
        {
            get { return Path.Combine(GlobalConfig.Path_PluginsFolder, "3Dmigoto-Sword-Lv5.exe"); }
        }

        public static string Path_EncryptorExe
        {
            get { return Path.Combine(GlobalConfig.Path_PluginsFolder, "DBMT-Encryptor.exe"); }
        }

        public static string Path_ProtectExe
        {
            get { return Path.Combine(GlobalConfig.Path_PluginsFolder, "DBMT-Protect.exe"); }
        }
        public static string Path_RunResultJson
        {
            get { return Path.Combine(Path_ConfigsFolder, "RunResult.json"); }
        }

        public static string Path_RunInputJson
        {
            get { return Path.Combine(Path_ConfigsFolder, "RunInput.json"); }
        }

        public static string Path_ReversedFolder
        {
            get { return Path.Combine(GlobalConfig.SSMTCacheFolderPath, "Reversed\\"); }
        }

        public static string Path_BaseFolder
        {
            get
            {
                return AppContext.BaseDirectory;
            }
        }

        //public static string Path_AssetsFolder
        //{
        //    get
        //    {
        //        return Path.Combine(GlobalConfig.Path_BaseFolder,"Assets\\");
        //    }
        //}


    }
}
