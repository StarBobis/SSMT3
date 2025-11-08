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
    public partial class CoreFunctions
    {

        public static void ExtractTextures(bool ReverseExtract = false)
        {
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();


            foreach (string DrawIB in DrawIBList)
            {

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                //在当前工作空间文件夹中创建每个DrawIB的输出文件夹
                string DrawIBOutputFolder = GlobalConfig.Path_CurrentWorkSpaceFolder + DrawIB + "\\";
                Directory.CreateDirectory(DrawIBOutputFolder);

                //创建TrianglelistTextures
                string TrianglelistTexturesFolderPath = DrawIBOutputFolder + "TrianglelistTextures\\";
                Directory.CreateDirectory(TrianglelistTexturesFolderPath);

                //创建DedupedTextures
                string DedupedTexturesFolderPath = DrawIBOutputFolder + "DedupedTextures\\";
                Directory.CreateDirectory(DedupedTexturesFolderPath);

                //创建RenderTextures
                string RenderTexturesFolderPath = DrawIBOutputFolder + "RenderTextures\\";
                Directory.CreateDirectory(RenderTexturesFolderPath);



                List<string> TrianglelistTexturesFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);

                foreach (string PsTextureFileName in TrianglelistTexturesFileNameList)
                {
                    string OriginalTextureFilePath = Path.Combine(FAInfo.FolderPath, PsTextureFileName);
                    string DedupedRenderFileName = FrameAnalysisDataUtils.GetDedupedTextureFileName(FAInfo.FolderPath, PsTextureFileName);
                    string TextureSpecialHash = DBMTStringUtils.GetFileHashFromFileName(PsTextureFileName);

                    string TargetRenderFilePath = RenderTexturesFolderPath + TextureSpecialHash + "_" + DedupedRenderFileName;
                    if (DedupedRenderFileName != "" && !File.Exists(TargetRenderFilePath))
                    {
                        File.Copy(OriginalTextureFilePath, TargetRenderFilePath, true);
                    }

                    //移动TrianglelistTextures
                    File.Copy(OriginalTextureFilePath, TrianglelistTexturesFolderPath + PsTextureFileName, true);

                    //移动DedupedTextures
                    string DedupedFileName = FrameAnalysisLogUtilsV2.Get_DedupedFileName(PsTextureFileName, FAInfo.FolderPath, FAInfo.LogFilePath);
                    string TargetFilePath = DedupedTexturesFolderPath + TextureSpecialHash + "_" + DedupedFileName;

                    if (DedupedFileName != "" && !File.Exists(TargetFilePath))
                    {
                        File.Copy(OriginalTextureFilePath, TargetFilePath, true);
                    }

                }

            }

            //提取贴图时生成这俩Json文件，这样就能直接在贴图设置里生成贴图Mod模板。
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = Generate_ComponentName_DrawCallIndexList_Json();
            Generate_TrianglelistDedupedFileName_Json();

        }


        public static void  ExtractDedupedTextures(bool ReverseExtract = false)
        {
            Debug.WriteLine("ExtractDedupedTextures::Start");
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //在当前工作空间文件夹中创建每个DrawIB的输出文件夹
                string DrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                Directory.CreateDirectory(DrawIBOutputFolder);

                //创建DedupedTextures
                string DedupedTexturesFolderPath = Path.Combine(DrawIBOutputFolder, "DedupedTextures\\");
                Directory.CreateDirectory(DedupedTexturesFolderPath);

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);
                LOG.Info("当前FrameAnalysisFolder: " + FAInfo.FolderPath);

                List<string> TrianglelistTexturesFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);
                LOG.Info("TrianglelistTexturesFileNameList Size: " + TrianglelistTexturesFileNameList.Count.ToString());

                foreach (string PsTextureFileName in TrianglelistTexturesFileNameList)
                {
                    LOG.Info("Copy: " + PsTextureFileName);
                    string OriginalTextureFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(PsTextureFileName, FAInfo.FolderPath, FAInfo.LogFilePath);
                    LOG.Info("OriginalTextureFilePath: " + OriginalTextureFilePath);

                    //移动DedupedTextures
                    string DedupedFileName = FrameAnalysisLogUtilsV2.Get_DedupedFileName(PsTextureFileName, FAInfo.FolderPath, FAInfo.LogFilePath);

                    string TextureSpecialHash = DBMTStringUtils.GetFileHashFromFileName(PsTextureFileName);
                    string TargetFilePath = DedupedTexturesFolderPath + TextureSpecialHash + "_" + DedupedFileName;

                    LOG.Info("TargetFilePath: " + TargetFilePath);

                    if (DedupedFileName != "" && !File.Exists(TargetFilePath))
                    {
                        if (File.Exists(OriginalTextureFilePath))
                        {
                            File.Copy(OriginalTextureFilePath, TargetFilePath, true);
                        }
                    }
                    LOG.NewLine();
                }

            }
            LOG.Info("ExtractDedupedTextures::End");
        }

        public static async Task ExtractRenderTextures(bool ReverseExtract = false)
        {
            LOG.Info("ExtractRenderTextures::Start");

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //在当前工作空间文件夹中创建每个DrawIB的输出文件夹
                string DrawIBOutputFolder = Path.Combine(GlobalConfig.Path_CurrentWorkSpaceFolder, DrawIB + "\\");
                Directory.CreateDirectory(DrawIBOutputFolder);


                //创建RenderTextures
                string RenderTexturesFolderPath = Path.Combine(DrawIBOutputFolder, "RenderTextures\\");
                Directory.CreateDirectory(RenderTexturesFolderPath);

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                List<string> TrianglelistTexturesFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);

                foreach (string PsTextureFileName in TrianglelistTexturesFileNameList)
                {
                    LOG.Info(FAInfo.FolderPath);
                    string DedupedRenderFileName = FrameAnalysisLogUtilsV2.Get_DedupedFileName(PsTextureFileName,FAInfo.FolderPath,FAInfo.LogFilePath );

                    string OriginalTextureFilePath = FrameAnalysisLogUtilsV2.Get_DedupedFilePath(PsTextureFileName,FAInfo.FolderPath, FAInfo.LogFilePath);

                    string TextureSpecialHash = DBMTStringUtils.GetFileHashFromFileName(PsTextureFileName);

                    string TargetRenderFilePath = RenderTexturesFolderPath + TextureSpecialHash + "_" + DedupedRenderFileName;
                    if (DedupedRenderFileName != "" && !File.Exists(TargetRenderFilePath))
                    {
                        if (File.Exists(OriginalTextureFilePath))
                        {
                            File.Copy(OriginalTextureFilePath, TargetRenderFilePath, true);
                        }
                    }

                }

            }
            LOG.Info("ExtractRenderTextures::End");
        }

        public static void ExtractTrianglelistTextures(bool ReverseExtract = false)
        {
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //在当前工作空间文件夹中创建每个DrawIB的输出文件夹
                string DrawIBOutputFolder = GlobalConfig.Path_CurrentWorkSpaceFolder + DrawIB + "\\";
                Directory.CreateDirectory(DrawIBOutputFolder);

                //创建TrianglelistTextures
                string TrianglelistTexturesFolderPath = DrawIBOutputFolder + "TrianglelistTextures\\";
                Directory.CreateDirectory(TrianglelistTexturesFolderPath);

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                List<string> TrianglelistTexturesFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);

                foreach (string PsTextureFileName in TrianglelistTexturesFileNameList)
                {
                    string OriginalTextureFilePath = Path.Combine(FAInfo.FolderPath, PsTextureFileName);
                    string DedupedRenderFileName = FrameAnalysisDataUtils.GetDedupedTextureFileName(FAInfo.FolderPath, PsTextureFileName);

                    if (File.Exists(OriginalTextureFilePath))
                    {
                        //移动TrianglelistTextures
                        File.Copy(OriginalTextureFilePath, TrianglelistTexturesFolderPath + PsTextureFileName, true);
                    }
                   

                }

            }
        }
    }
}
