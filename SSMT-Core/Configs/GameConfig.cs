using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT_Core;

namespace SSMT
{
    public class LogicName
    {
        public static string UnityVS { get; set; } = "UnityVS";
        public static string UnityCS { get; set; } = "UnityCS";
        public static string UnityCPU { get; set; } = "UnityCPU";
        public static string GIMI { get; set; } = "GIMI";
        public static string HIMI { get; set; } = "HIMI";
        public static string SRMI { get; set; } = "SRMI";
        public static string ZZMI { get; set; } = "ZZMI";
        public static string WWMI { get; set; } = "WWMI";
        //public static string IdentityV { get; set; } = "IdentityV";
        public static string CTXMC { get; set; } = "CTXMC";
        public static string IdentityV2 { get; set; } = "IdentityV2";

        public static string YYSLS { get; set; } = "YYSLS";
        public static string AILIMIT { get; set; } = "AILIMIT";
        public static string HOK { get; set; } = "HOK";
        public static string NierR { get; set; } = "NierR";
        public static string SnowBreak { get; set; } = "SnowBreak";

    }
    public class GameConfig
    {

        public string MigotoPath { get; set; } = "";
        public string TargetPath { get; set; } = "";
        public string LaunchPath { get; set; } = "";
        public string LaunchArgs { get; set; } = "";

        public string LogicName { get; set; } = "GIMI";

        public string GameTypeName { get; set; } = "GIMI";

        public bool IgnoreErrorGI25 { get; set; } = false;

        public bool PlayVersionDll { get; set; } = false;

        public bool AutoSetAnalyseOptions { get; set; } = true;

        public string GithubPackageVersion { get; set; } = "";

        public int DllInitializationDelay { get; set; } = 500;
        public int DllReplaceSelectedIndex { get; set; } = 0;
        public int DllPreProcessSelectedIndex { get; set; } = 1;


        public List<LaunchItem> LaunchItemList { get; set; } = [];

        public GameConfig()
        {
            //读取并设置当前3Dmigoto路径
            if (File.Exists(GlobalConfig.Path_CurrentGameConfigJson))
            {
                JObject jobj = DBMTJsonUtils.ReadJObjectFromFile(GlobalConfig.Path_CurrentGameConfigJson);
                if (jobj.ContainsKey("3DmigotoPath"))
                {

                    string MigotoPath = jobj["3DmigotoPath"]?.ToString() ?? "";
                    this.MigotoPath = MigotoPath;
                }

                if (jobj.ContainsKey("LaunchPath"))
                {

                    string LaunchPath = jobj["LaunchPath"]?.ToString() ?? "";
                    this.LaunchPath = LaunchPath;
                }

                if (jobj.ContainsKey("LaunchArgs"))
                {

                    string LaunchArgs = jobj["LaunchArgs"]?.ToString() ?? "";
                    this.LaunchArgs = LaunchArgs;
                }

                //TargetPath
                if (jobj.ContainsKey("TargetPath"))
                {
                    string TargetPath = jobj["TargetPath"]?.ToString() ?? "";
                    this.TargetPath = TargetPath;
                }

                if (jobj.ContainsKey("LogicName"))
                {

                    string LogicName = jobj["LogicName"]?.ToString() ?? "";
                    this.LogicName = LogicName;
                }

                //GameTypeName
                if (jobj.ContainsKey("GameTypeName"))
                {

                    string GameTypeName = jobj["GameTypeName"]?.ToString() ?? "";
                    this.GameTypeName = GameTypeName;
                }

                //GithubPackageVersion
                if (jobj.ContainsKey("GithubPackageVersion"))
                {

                    string GithubPackageVersion = jobj["GithubPackageVersion"]?.ToString() ?? "";
                    this.GithubPackageVersion = GithubPackageVersion;
                }

                if (jobj.ContainsKey("PlayVersionDll"))
                {

                    bool PlayVersionDll = (bool)jobj["PlayVersionDll"];
                    this.PlayVersionDll = PlayVersionDll;
                }

                //AutoSetAnalyseOptions
                if (jobj.ContainsKey("AutoSetAnalyseOptions"))
                {

                    bool AutoSetAnalyseOptions = (bool)jobj["AutoSetAnalyseOptions"];
                    this.AutoSetAnalyseOptions = AutoSetAnalyseOptions;
                }

                if (jobj.ContainsKey("IgnoreGameError"))
                {

                    bool IgnoreGameError = (bool)jobj["IgnoreGameError"];
                    this.IgnoreErrorGI25 = IgnoreGameError;
                }

                //DllInitializationDelay
                if (jobj.ContainsKey("DllInitializationDelay"))
                {

                    int DllInitializationDelay = (int)jobj["DllInitializationDelay"];
                    this.DllInitializationDelay = DllInitializationDelay;
                }

                //DllReplaceSelectedIndex
                if (jobj.ContainsKey("DllReplaceSelectedIndex"))
                {

                    int DllReplaceSelectedIndex = (int)jobj["DllReplaceSelectedIndex"];
                    this.DllReplaceSelectedIndex = DllReplaceSelectedIndex;
                }

                //DllPreProcessSelectedIndex
                if (jobj.ContainsKey("DllPreProcessSelectedIndex"))
                {

                    int DllPreProcessSelectedIndex = (int)jobj["DllPreProcessSelectedIndex"];
                    this.DllPreProcessSelectedIndex = DllPreProcessSelectedIndex;
                }

                //LaunchItems
                if (jobj.ContainsKey("LaunchItems"))
                {
                    JArray jobjArray = jobj["LaunchItems"] as JArray ?? new JArray();

                    if (jobjArray != null && jobjArray.Count > 0)
                    {
                        this.LaunchItemList.Clear();

                        foreach (JObject launchItemJobj in jobjArray)
                        {
                            string LaunchExePath = launchItemJobj["LaunchExePath"]?.ToString() ?? "";
                            string LaunchArgs = launchItemJobj["LaunchArgs"]?.ToString() ?? "";
                            LaunchItem newLaunchItem = new LaunchItem(LaunchExePath, LaunchArgs);
                            this.LaunchItemList.Add(newLaunchItem);
                        }

                    }
                }



            }
        }

        public void SaveConfig()
        {
            JArray jobjArray = new JArray();

            foreach (LaunchItem launchItem in this.LaunchItemList)
            {
                if (launchItem.LaunchExePath.Trim() == "")
                {
                    continue;
                }
                JObject launchItemJObj = DBMTJsonUtils.CreateJObject();
                launchItemJObj["LaunchExePath"] = launchItem.LaunchExePath;
                launchItemJObj["LaunchArgs"] = launchItem.LaunchArgs;
                jobjArray.Add(launchItemJObj);
            }

            JObject jobj = DBMTJsonUtils.CreateJObject();

            if (File.Exists(GlobalConfig.Path_CurrentGameConfigJson))
            {
                jobj = DBMTJsonUtils.ReadJObjectFromFile(GlobalConfig.Path_CurrentGameConfigJson);
            }

            jobj["TargetPath"] = this.TargetPath;
            jobj["3DmigotoPath"] = this.MigotoPath;
            jobj["LaunchPath"] = this.LaunchPath;
            jobj["LaunchArgs"] = this.LaunchArgs;
            jobj["LogicName"] = this.LogicName;
            jobj["GameTypeName"] = this.GameTypeName;
            jobj["IgnoreGameError"] = this.IgnoreErrorGI25;
            jobj["PlayVersionDll"] = this.PlayVersionDll;
            jobj["AutoSetAnalyseOptions"] = this.AutoSetAnalyseOptions;
            jobj["GithubPackageVersion"] = this.GithubPackageVersion;
            jobj["DllInitializationDelay"] = this.DllInitializationDelay;
            jobj["DllReplaceSelectedIndex"] = this.DllReplaceSelectedIndex;
            jobj["DllPreProcessSelectedIndex"] = this.DllPreProcessSelectedIndex;
            jobj["LaunchItems"] = jobjArray;
            DBMTJsonUtils.SaveJObjectToFile(jobj, GlobalConfig.Path_CurrentGameConfigJson);
        }

    }
}
