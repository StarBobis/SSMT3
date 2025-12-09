using Newtonsoft.Json.Linq;
using SSMT_Core;
using System;
using System.IO;

namespace SSMT.Pages.TextureToolBoxPage
{
    public class TextToolBoxConfig
    {
        public string SelectedTextureFilePath { get; set; } = string.Empty;
        public string SelectedVideoFilePath { get; set; } = string.Empty;
        public string DynamicTextureModGenerateFolderPath { get; set; } = string.Empty;
        public int SelectedFpsOption { get; set; } = 0;

        public static string ConfigPath => PathManager.Path_TextToolBoxConfig;

        public TextToolBoxConfig() { }

        public static TextToolBoxConfig Load()
        {
            if (File.Exists(ConfigPath))
            {
                JObject jobj = DBMTJsonUtils.ReadJObjectFromFile(ConfigPath);
                return new TextToolBoxConfig
                {
                    SelectedTextureFilePath = jobj["SelectedTextureFilePath"]?.ToString() ?? string.Empty,
                    SelectedVideoFilePath = jobj["SelectedVideoFilePath"]?.ToString() ?? string.Empty,
                    DynamicTextureModGenerateFolderPath = jobj["DynamicTextureModGenerateFolderPath"]?.ToString() ?? string.Empty,
                    SelectedFpsOption = jobj["SelectedFpsOption"]?.ToObject<int>() ?? 0
                };
            }
            return new TextToolBoxConfig();
        }

        public void Save()
        {
            JObject jobj = DBMTJsonUtils.CreateJObject();
            jobj["SelectedTextureFilePath"] = SelectedTextureFilePath;
            jobj["SelectedVideoFilePath"] = SelectedVideoFilePath;
            jobj["DynamicTextureModGenerateFolderPath"] = DynamicTextureModGenerateFolderPath;
            jobj["SelectedFpsOption"] = SelectedFpsOption;
            DBMTJsonUtils.SaveJObjectToFile(jobj, ConfigPath);
        }
    }
}
