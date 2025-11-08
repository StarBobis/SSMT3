using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSMT;
using SSMT_Core;
using SSMT_Core.Utils;

namespace SSMT_Core
{
    public class SSMTTextureHelper
    {

        public static void ConvertAllTextureFilesToTargetFolder(string SourceFolderPath, string TargetFolderPath)
        {
            Debug.Write("ConvertAllTextureFilesToTargetFolder::");
            if (!Directory.Exists(TargetFolderPath))
            {
                Directory.CreateDirectory(TargetFolderPath);
            }

            string[] filePathArray = Directory.GetFiles(SourceFolderPath);
            foreach (string ddsFilePath in filePathArray)
            {
                //只转换dds格式和png格式贴图
                if (ddsFilePath.EndsWith(".dds"))
                {
                    string TextureFormatString = "jpg";
                    SSMTCommandUtils.ConvertTexture(ddsFilePath, TextureFormatString, TargetFolderPath);
                }
                else if (ddsFilePath.EndsWith(".jpg") || ddsFilePath.EndsWith(".png"))
                {
                    Debug.Write("Copy: " + ddsFilePath + " To: " + TargetFolderPath);
                    File.Copy(ddsFilePath, Path.Combine(TargetFolderPath, Path.GetFileName(ddsFilePath)), true);
                }

            }
        }

        public static void ConvertAllTextureFilesToTargetFolderReverse(string SourceFolderPath, string TargetFolderPath, string TextureSuffix)
        {
            Debug.Write("ConvertAllTextureFilesToTargetFolder::");
            if (!Directory.Exists(TargetFolderPath))
            {
                Directory.CreateDirectory(TargetFolderPath);
            }

            string[] filePathArray = Directory.GetFiles(SourceFolderPath);
            foreach (string ddsFilePath in filePathArray)
            {
                //只转换dds格式和png格式贴图
                if (ddsFilePath.EndsWith(".dds"))
                {
                    SSMTCommandUtils.ConvertTexture(ddsFilePath, TextureSuffix, TargetFolderPath);
                }
                else if (ddsFilePath.EndsWith(".jpg") || ddsFilePath.EndsWith(".png"))
                {
                    Debug.Write("Copy: " + ddsFilePath + " To: " + TargetFolderPath);
                    File.Copy(ddsFilePath, Path.Combine(TargetFolderPath, Path.GetFileName(ddsFilePath)), true);
                }

            }
        }

        public static void ConvertAllTexturesIntoConvertedTexturesReverse(string TargetConvertFolderPath, string TextureSuffix, string ConvertedToFolderPath = "")
        {
            List<string> result = DBMTFileUtils.FindDirectoriesWithImages(TargetConvertFolderPath);
            foreach (string TextureFolder in result)
            {
                //如果要转换到的目标目录不存在或者没填写，则自动转换到当前目录下。
                if (ConvertedToFolderPath == "")
                {
                    string TargetTexturesFolderPath = TextureFolder + "/ConvertedTextures/";
                    //MessageBox.Show(TargetTexturesFolderPath);
                    Directory.CreateDirectory(TargetTexturesFolderPath);
                    ConvertAllTextureFilesToTargetFolderReverse(TextureFolder, TargetTexturesFolderPath, TextureSuffix);
                }
                else
                {
                    if (!Directory.Exists(ConvertedToFolderPath))
                    {
                        Directory.CreateDirectory(ConvertedToFolderPath);
                    }
                    ConvertAllTextureFilesToTargetFolderReverse(TextureFolder, ConvertedToFolderPath, TextureSuffix);
                }

            }
        }

        public static void ConvertAllTexturesIntoConvertedTextures(string TargetConvertFolderPath)
        {
            List<string> result = DBMTFileUtils.FindDirectoriesWithImages(TargetConvertFolderPath);
            foreach (string TextureFolder in result)
            {
                string TargetTexturesFolderPath = TextureFolder + "/ConvertedTextures/";
                //MessageBox.Show(TargetTexturesFolderPath);
                Directory.CreateDirectory(TargetTexturesFolderPath);
                ConvertAllTextureFilesToTargetFolder(TextureFolder, TargetTexturesFolderPath);
            }
        }

        public static void ConvertTexturesInMod(string ModIniFilePath)
        {
            if (!string.IsNullOrEmpty(ModIniFilePath))
            {
                string ModFolderPath = Path.GetDirectoryName(ModIniFilePath);
                ConvertAllTexturesIntoConvertedTextures(ModFolderPath);
                

                string ModFolderName = Path.GetFileName(ModFolderPath);
                string ModFolderParentPath = Path.GetDirectoryName(ModFolderPath);
                string ModReverseFolderPath = ModFolderParentPath + "\\" + ModFolderName + "-Reverse\\";

                SSMTCommandUtils.ShellOpenFolder(ModReverseFolderPath);
            }
        }



        public static async Task ConvertDedupedTexturesToTargetFormat()
        {

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();
            foreach (string DrawIB in DrawIBList)
            {
                //在这里把所有output目录下的dds转为jpg格式
                string DedupedTexturesFolderPath = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder ,DrawIB + "\\DedupedTextures\\");
                if (!Directory.Exists(DedupedTexturesFolderPath))
                {
                    throw new Exception("无法找到DedupedTextures文件夹: " + DedupedTexturesFolderPath);
                }

                string DedupedTexturesConvertFolderPath = TextureConfig.GetConvertedTexturesFolderPath(DrawIB);
                SSMTTextureHelper.ConvertAllTextureFilesToTargetFolder(DedupedTexturesFolderPath, DedupedTexturesConvertFolderPath);
            }
        }


        




    }
}
