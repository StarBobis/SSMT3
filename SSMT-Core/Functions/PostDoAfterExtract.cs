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
    public partial class CoreFunctions
    {
        /// <summary>
        /// 读取每个TrianglelistTextures里的贴图文件对应Deduped文件保存到Json文件供后续贴图设置使用。
        /// </summary>
        /// <param name="ReverseExtract"></param>
        public static void Generate_TrianglelistDedupedFileName_Json(bool ReverseExtract = false)
        {
            LOG.Info("Generate_TrianglelistDedupedFileName_Json::Start");
            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }
                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                LOG.Info("FAInfo.FolderPath: " + FAInfo.FolderPath);
                List<string> TrianglelistTextureFileNameList = TextureConfig.Get_TrianglelistTexturesFileNameList(FAInfo.FolderPath, DrawIB, ReverseExtract);
                LOG.Info("TrianglelistTextureFileNameList Size: " + TrianglelistTextureFileNameList.Count.ToString());

                JObject Trianglelist_DedupedFileName_JObject = DBMTJsonUtils.CreateJObject();
                foreach (string TrianglelistTextureFileName in TrianglelistTextureFileNameList)
                {
                    string Hash = DBMTStringUtils.GetFileHashFromFileName(TrianglelistTextureFileName);
                    string DedupedTextureFileName = Hash + "_" + FrameAnalysisLogUtilsV2.Get_DedupedFileName(TrianglelistTextureFileName, FAInfo.FolderPath, FAInfo.LogFilePath);
                    string FADedupedFileName = FrameAnalysisDataUtils.GetDedupedTextureFileName(FAInfo.FolderPath, TrianglelistTextureFileName);
                    LOG.Info("Hash: " + Hash);
                    LOG.Info("DedupedTextureFileName: " + DedupedTextureFileName);

                    if (FADedupedFileName.Trim() != "")
                    {
                        FADedupedFileName = Hash + "_" + FADedupedFileName;
                    }

                    JObject TextureProperty = DBMTJsonUtils.CreateJObject();
                    TextureProperty["FALogDedupedFileName"] = DedupedTextureFileName;
                    TextureProperty["FADataDedupedFileName"] = FADedupedFileName;

                    Trianglelist_DedupedFileName_JObject[TrianglelistTextureFileName] = TextureProperty;
                }

                string TrianglelistDedupedFileNameJsonName = "TrianglelistDedupedFileName.json";
                string TrianglelistDedupedFileNameJsonPath = Path.Combine(PathManager.Path_CurrentWorkSpaceFolder + DrawIB + "\\", TrianglelistDedupedFileNameJsonName);
                DBMTJsonUtils.SaveJObjectToFile(Trianglelist_DedupedFileName_JObject, TrianglelistDedupedFileNameJsonPath);
            }

            LOG.Info("Generate_TrianglelistDedupedFileName_Json::End");

        }

        /// <summary>
        /// 读取每个Component的DrawIndexList并保存到Json文件供贴图设置页面使用。
        /// </summary>
        /// <param name="ReverseExtract"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, List<string>>> Generate_ComponentName_DrawCallIndexList_Json(bool ReverseExtract = false)
        {
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::Start");
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = new Dictionary<string, Dictionary<string, List<string>>>();

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    continue;
                }

                FrameAnalysisInfo FAInfo = new FrameAnalysisInfo(DrawIB);

                Dictionary<string, UInt64> ComponentName_MatchFirstIndex_Dict = FrameAnalysisDataUtils.Read_ComponentName_MatchFirstIndex_Dict(FAInfo.FolderPath, DrawIB);

                JObject ComponentName_DrawIndexList_JObject = DBMTJsonUtils.CreateJObject();

                Dictionary<string, List<string>> ComponentName_DrawCallIndexList = new Dictionary<string, List<string>>();

                foreach (var item in ComponentName_MatchFirstIndex_Dict)
                {
                    List<string> DrawCallIndexList = new List<string>();

                    if (ReverseExtract)
                    {
                        DrawCallIndexList = FrameAnalysisLogUtils.Get_DrawCallIndexList_ByMatchFirstIndex(DrawIB, item.Value);
                    }
                    else
                    {
                        DrawCallIndexList = FrameAnalysisDataUtils.Read_DrawCallIndexList(FAInfo.FolderPath, DrawIB, item.Key);
                    }

                    ComponentName_DrawIndexList_JObject[item.Key] = new JArray(DrawCallIndexList);
                    ComponentName_DrawCallIndexList[item.Key] = DrawCallIndexList;
                }

                string SaveFileName = "ComponentName_DrawCallIndexList.json";
                string SaveJsonFilePath = Path.Combine(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\"), SaveFileName);
                DBMTJsonUtils.SaveJObjectToFile(ComponentName_DrawIndexList_JObject, SaveJsonFilePath);

                DrawIB_ComponentName_DrawCallIndexList_Dict_Dict[DrawIB] = ComponentName_DrawCallIndexList;
            }
            LOG.Info("Get_ComponentName_DrawCallIndexList_Dict_FromJson::End");
            return DrawIB_ComponentName_DrawCallIndexList_Dict_Dict;
        }

        /// <summary>
        /// 在正向提取和逆向提取后都需要做的事情
        /// </summary>
        public static void PostDoAfterExtract(bool ReverseExtract = false)
        {
            CoreFunctions.ExtractDedupedTextures();

            
                //异步执行，我才懒得等它全部转换完毕才弹出文件夹
                LOG.Info("ConvertDedupedTexturesToTargetFormat:");
                _ = SSMTTextureHelper.ConvertDedupedTexturesToTargetFormat();

            List<string> DrawIBList = DrawIBConfig.GetDrawIBListFromConfig();

            //(1) 贴图标记功能前置1
            Dictionary<string, Dictionary<string, List<string>>> DrawIB_ComponentName_DrawCallIndexList_Dict_Dict = Generate_ComponentName_DrawCallIndexList_Json();

            //(2) 贴图标记功能前置2
            Generate_TrianglelistDedupedFileName_Json();


            //(3)自动检测贴图配置并自动上贴图
            //要自动检测贴图的前提条件是存在贴图配置文件夹
            if (!Directory.Exists(PathManager.Path_GameTextureConfigFolder))
            {
                return;
            }

            //检测贴图配置数量，如果一个都没有那就不用检测了
            Dictionary<string, JObject> TextureConfigName_JObject_Dict = TextureConfig.Get_TextureConfigName_JObject_Dict();
            if (TextureConfigName_JObject_Dict.Count == 0)
            {
                return;
            }

            //开始自动贴图识别流程，自动识别满足条件的第一个贴图配置，并将其应用到自动贴图。
            LOG.Info("开始自动贴图识别流程");
            foreach (string DrawIB in DrawIBList)
            {
                //如果这个DrawIB的文件夹存在，说明提取成功了，否则直接跳过
                if (!Directory.Exists(Path.Combine(PathManager.Path_CurrentWorkSpaceFolder, DrawIB + "\\")))
                {
                    LOG.Info("跳过DrawIB: " + DrawIB + "，因为没有提取成功。");
                    continue;
                }


                Dictionary<string, List<string>> ComponentName_DrawCallIndexList = DrawIB_ComponentName_DrawCallIndexList_Dict_Dict[DrawIB];
                LOG.Info("ComponentName_DrawCallIndexList:" + ComponentName_DrawCallIndexList.Count.ToString());

                foreach (var pair in ComponentName_DrawCallIndexList)
                {
                    //对每个ComponentName都进行处理:
                    string ComponentName = pair.Key;
                    List<string> DrawCallIndexList = pair.Value;

                    LOG.Info("ComponentName_DrawCallIndexList:" + ComponentName_DrawCallIndexList.Count.ToString());
                    LOG.Info("DrawCallIndexList:" + DrawCallIndexList.Count.ToString());

                    bool findMatchTextureConfig = false;
                    string MatchTextureConfigName = "";
                    List<ImageItem> MatchImageList = new List<ImageItem>();
                    foreach (string DrawCallIndex in DrawCallIndexList)
                    {
                        //TODO 这里获取到的ImageList是空的
                        List<ImageItem> ImageList = TextureConfig.Read_ImageItemList(DrawIB, DrawCallIndex);

                        //如果当前ImageList是空的，则不需要进行识别了，肯定识别不到
                        if (ImageList.Count == 0)
                        {
                            continue;
                        }
                        LOG.Info("DrawCall: " + DrawCallIndex + " ImageListSize: " + ImageList.Count.ToString());


                        List<string> MatchedTextureConfigNameList = TextureConfig.FindMatch_TextureConfigNameListV2(ImageList, TextureConfigName_JObject_Dict);
                        if (MatchedTextureConfigNameList.Count != 0)
                        {
                            //即使找到了多个，默认也只使用第一个
                            MatchTextureConfigName = MatchedTextureConfigNameList[0];
                            MatchImageList = ImageList;
                            findMatchTextureConfig = true;
                            break;
                        }
                    }

                    if (!findMatchTextureConfig)
                    {
                        LOG.Info("未找到任何匹配的贴图");
                        continue;
                    }

                    LOG.Info("找到了匹配的贴图配置: " + MatchTextureConfigName);
                    //根据MatchTextureConfigName读取MarkName

                    string TextureConfigSavePath = PathManager.Path_GameTextureConfigFolder + MatchTextureConfigName + ".json";
                    LOG.Info("TextureConfigSavePath: " + TextureConfigSavePath);
                    if (File.Exists(TextureConfigSavePath))
                    {
                        Dictionary<string, SlotObject> PixeSlot_SlotObject_Dict = TextureConfig.Read_PixelSlot_SlotObject_Dict(TextureConfigSavePath);

                        LOG.Info("Count: " + MatchImageList.Count.ToString());
                        for (int i = 0; i < MatchImageList.Count; i++)
                        {
                            ImageItem imageItem = MatchImageList[i];
                            if (PixeSlot_SlotObject_Dict.ContainsKey(imageItem.PixelSlot))
                            {
                                SlotObject sobj = PixeSlot_SlotObject_Dict[imageItem.PixelSlot];
                                string MarkName = sobj.MarkName;

                                imageItem.MarkName = MarkName;
                                imageItem.MarkStyle = sobj.MarkStyle;


                                MatchImageList[i] = imageItem;

                                LOG.Info(MatchImageList[i].MarkName);
                            }

                        }
                    }

                    //执行到这里说明此ComponentName已匹配到对应的贴图，那么直接应用。
                    TextureConfig.ApplyTextureConfig(MatchImageList, DrawIB, ComponentName);

                }

            }


        }


    }
}
