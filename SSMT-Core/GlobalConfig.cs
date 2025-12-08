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
        public static string CurrentGameName { get; set; } = "ZZZ";
        public static string CurrentWorkSpace { get; set; } = "";
        public static string SSMTCacheFolderPath { get; set; } = "";
        public static string GithubToken { get; set; } = "";
        public static bool Theme { get; set; } = true;
        public static bool Chinese { get; set; } = true;

        //窗口大小
        public static double WindowWidth { get; set; } = 1280;
        public static double WindowHeight { get; set; } = 720;
        public static double WindowLuminosityOpacity { get; set; } = 0.65f;

        /// <summary>
        /// 是否显示数据类型管理页面
        /// </summary>
        public static bool ShowGameTypePage { get; set; } = false;
        /// <summary>
        /// 是否显示Mod管理页面
        /// </summary>
        public static bool ShowModManagePage { get; set; } = false;
       
   
        /// <summary>
        /// 是否显示贴图工具箱页面，因为这个页面用的人非常少。
        /// </summary>
        public static bool ShowTextureToolBoxPage { get; set; } = false;
        public static bool UseTitleBar { get; set; } = false;
        public static bool UseGithubToken { get; set; } = false;


        /// <summary>
        /// 开启后减少Dump文件总大小，提高Dump速度
        /// 99%的磁盘格式都支持，但是1%的磁盘格式Dump下来天生文件损坏
        /// 所以默认是关闭状态的，作者有需要可以手动开启
        /// </summary>
        public static bool UseSymlinkFeature { get; set; } = false;






        /// <summary>
        /// 使用古法读取，不要自作聪明用C#的某些语法糖特性实现全自动
        /// </summary>
        public static void ReadConfig()
        {
            try
            {
                //只有存在这个全局配置文件时，才读取
                if (File.Exists(PathManager.Path_MainConfig_Global))
                {
                    //读取配置时优先读取全局的
                    JObject SettingsJsonObject = DBMTJsonUtils.ReadJObjectFromFile(PathManager.Path_MainConfig_Global);

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
                    //GithubToken
                    if (SettingsJsonObject.ContainsKey("GithubToken"))
                    {
                        GithubToken = (string)SettingsJsonObject["GithubToken"];
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


                    //WindowLuminosityOpacity
                    if (SettingsJsonObject.ContainsKey("WindowLuminosityOpacity"))
                    {
                        WindowLuminosityOpacity = (double)SettingsJsonObject["WindowLuminosityOpacity"];
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

                    //ShowTextureToolBoxPage
                    if (SettingsJsonObject.ContainsKey("ShowTextureToolBoxPage"))
                    {
                        ShowTextureToolBoxPage = (bool)SettingsJsonObject["ShowTextureToolBoxPage"];
                    }

                    //UseTitleBar
                    if (SettingsJsonObject.ContainsKey("UseTitleBar"))
                    {
                        UseTitleBar = (bool)SettingsJsonObject["UseTitleBar"];
                    }

                    //UseGithubToken
                    if (SettingsJsonObject.ContainsKey("UseGithubToken"))
                    {
                        UseGithubToken = (bool)SettingsJsonObject["UseGithubToken"];
                    }

                    //UseSymlinkFeature
                    if (SettingsJsonObject.ContainsKey("UseSymlinkFeature"))
                    {
                        UseSymlinkFeature = (bool)SettingsJsonObject["UseSymlinkFeature"];
                    }


                }


            }
            catch (Exception ex) {
                //如果全局的配置文件读取错误的话，直接删掉重新保存一个全局的配置文件
                //这是因为蓝屏的时候这里的配置文件会直接被损坏。
                ex.ToString();
                File.Delete(PathManager.Path_MainConfig_Global);
                GlobalConfig.SaveConfig();
            }
        }

        /// <summary>
        /// 使用古法保存，不要自作聪明用C#的某些语法糖特性实现全自动
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                //古法保存
                JObject SettingsJsonObject = new JObject();

                SettingsJsonObject["CurrentGameName"] = CurrentGameName;
                SettingsJsonObject["CurrentWorkSpace"] = CurrentWorkSpace;
                SettingsJsonObject["DBMTWorkFolder"] = SSMTCacheFolderPath;
                SettingsJsonObject["GithubToken"] = GithubToken;
                SettingsJsonObject["WindowWidth"] = WindowWidth;
                SettingsJsonObject["WindowHeight"] = WindowHeight;
                SettingsJsonObject["WindowLuminosityOpacity"] = WindowLuminosityOpacity;
                SettingsJsonObject["Theme"] = Theme;
                SettingsJsonObject["Chinese"] = Chinese;
                SettingsJsonObject["ShowGameTypePage"] = ShowGameTypePage;
                SettingsJsonObject["ShowModManagePage"] = ShowModManagePage;
                SettingsJsonObject["ShowTextureToolBoxPage"] = ShowTextureToolBoxPage;
                SettingsJsonObject["UseTitleBar"] = UseTitleBar;
                SettingsJsonObject["UseGithubToken"] = UseGithubToken;
                SettingsJsonObject["UseSymlinkFeature"] = UseSymlinkFeature;

                //写出内容
                string WirteStirng = SettingsJsonObject.ToString();

                //保存配置时，全局配置也顺便保存一份
                File.WriteAllText(PathManager.Path_MainConfig_Global, WirteStirng);
            }
            catch (Exception ex)
            {
                //保存失败就算了，也不是非得保存不可。
                //很难想象没法在AppData\\Local下面写文件的情况会发生。
                ex.ToString();
            }
        }


    }
}
